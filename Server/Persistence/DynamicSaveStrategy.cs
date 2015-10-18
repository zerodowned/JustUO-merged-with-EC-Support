﻿#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] DynamicSaveStrategy.cs
// ************************************/
#endregion

#region References
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using CustomsFramework;

using Server.Guilds;
#endregion

namespace Server
{
	public sealed class DynamicSaveStrategy : SaveStrategy
	{
		private readonly ConcurrentBag<Item> _decayBag;

		private readonly BlockingCollection<QueuedMemoryWriter> _itemThreadWriters;
		private readonly BlockingCollection<QueuedMemoryWriter> _mobileThreadWriters;
		private readonly BlockingCollection<QueuedMemoryWriter> _guildThreadWriters;
		private readonly BlockingCollection<QueuedMemoryWriter> _dataThreadWriters;

		private SaveMetrics _metrics;

		private SequentialFileWriter _itemData, _itemIndex;
		private SequentialFileWriter _mobileData, _mobileIndex;
		private SequentialFileWriter _guildData, _guildIndex;
		private SequentialFileWriter _customData, _customIndex;

		public DynamicSaveStrategy()
		{
			_decayBag = new ConcurrentBag<Item>();
			_itemThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
			_mobileThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
			_guildThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
			_dataThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
		}

		public override string Name { get { return "Dynamic"; } }

		public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
		{
			_metrics = metrics;

			OpenFiles();

			var saveTasks = new Task[4];

			saveTasks[0] = SaveItems();
			saveTasks[1] = SaveMobiles();
			saveTasks[2] = SaveGuilds();
			saveTasks[3] = SaveData();

			SaveTypeDatabases();

			if (permitBackgroundWrite)
			{
				//This option makes it finish the writing to disk in the background, continuing even after Save() returns.
				Task.Factory.ContinueWhenAll(
					saveTasks,
					_ =>
					{
						CloseFiles();

						World.NotifyDiskWriteComplete();
					});
			}
			else
			{
				Task.WaitAll(saveTasks); //Waits for the completion of all of the tasks(committing to disk)
				CloseFiles();
			}
		}

		public override void ProcessDecay()
		{
			Item item;

			while (_decayBag.TryTake(out item))
			{
				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

		private Task StartCommitTask(
			BlockingCollection<QueuedMemoryWriter> threadWriter, SequentialFileWriter data, SequentialFileWriter index)
		{
			Task commitTask = Task.Factory.StartNew(
				() =>
				{
					while (!(threadWriter.IsCompleted))
					{
						QueuedMemoryWriter writer;

						try
						{
							writer = threadWriter.Take();
						}
						catch (InvalidOperationException)
						{
							//Per MSDN, it's fine if we're here, successful completion of adding can rarely put us into this state.
							break;
						}

						writer.CommitTo(data, index);
					}
				});

			return commitTask;
		}

		private Task SaveItems()
		{
			//Start the blocking consumer; this runs in background.
			Task commitTask = StartCommitTask(_itemThreadWriters, _itemData, _itemIndex);

			IEnumerable<Item> items = World.Items.Values;

			//Start the producer.
			Parallel.ForEach(
				items,
				() => new QueuedMemoryWriter(),
				(item, state, writer) =>
				{
					long startPosition = writer.Position;

					item.Serialize(writer);

					var size = (int)(writer.Position - startPosition);

					writer.QueueForIndex(item, size);

					if (item.Decays && item.Parent == null && item.Map != Map.Internal &&
						DateTime.UtcNow > (item.LastMoved + item.DecayTime))
					{
						_decayBag.Add(item);
					}

					if (_metrics != null)
					{
						_metrics.OnItemSaved(size);
					}

					return writer;
				},
				writer =>
				{
					writer.Flush();

					_itemThreadWriters.Add(writer);
				});

			_itemThreadWriters.CompleteAdding(); //We only get here after the Parallel.ForEach completes.  Lets our task 

			return commitTask;
		}

		private Task SaveMobiles()
		{
			//Start the blocking consumer; this runs in background.
			Task commitTask = StartCommitTask(_mobileThreadWriters, _mobileData, _mobileIndex);

			IEnumerable<Mobile> mobiles = World.Mobiles.Values;

			//Start the producer.
			Parallel.ForEach(
				mobiles,
				() => new QueuedMemoryWriter(),
				(mobile, state, writer) =>
				{
					long startPosition = writer.Position;

					mobile.Serialize(writer);

					var size = (int)(writer.Position - startPosition);

					writer.QueueForIndex(mobile, size);

					if (_metrics != null)
					{
						_metrics.OnMobileSaved(size);
					}

					return writer;
				},
				writer =>
				{
					writer.Flush();

					_mobileThreadWriters.Add(writer);
				});

			_mobileThreadWriters.CompleteAdding();
			//We only get here after the Parallel.ForEach completes.  Lets our task tell the consumer that we're done

			return commitTask;
		}

		private Task SaveGuilds()
		{
			//Start the blocking consumer; this runs in background.
			Task commitTask = StartCommitTask(_guildThreadWriters, _guildData, _guildIndex);

			IEnumerable<BaseGuild> guilds = BaseGuild.List.Values;

			//Start the producer.
			Parallel.ForEach(
				guilds,
				() => new QueuedMemoryWriter(),
				(guild, state, writer) =>
				{
					long startPosition = writer.Position;

					guild.Serialize(writer);

					var size = (int)(writer.Position - startPosition);

					writer.QueueForIndex(guild, size);

					if (_metrics != null)
					{
						_metrics.OnGuildSaved(size);
					}

					return writer;
				},
				writer =>
				{
					writer.Flush();

					_guildThreadWriters.Add(writer);
				});

			_guildThreadWriters.CompleteAdding(); //We only get here after the Parallel.ForEach completes.  Lets our task 

			return commitTask;
		}

		private Task SaveData()
		{
			Task commitTask = StartCommitTask(_dataThreadWriters, _customData, _customIndex);

			IEnumerable<SaveData> data = World.Data.Values;

			Parallel.ForEach(
				data,
				() => new QueuedMemoryWriter(),
				(saveData, state, writer) =>
				{
					long startPosition = writer.Position;

					saveData.Serialize(writer);

					var size = (int)(writer.Position - startPosition);

					writer.QueueForIndex(saveData, size);

					if (_metrics != null)
					{
						_metrics.OnDataSaved(size);
					}

					return writer;
				},
				writer =>
				{
					writer.Flush();

					_dataThreadWriters.Add(writer);
				});

			_dataThreadWriters.CompleteAdding();

			return commitTask;
		}

		private void OpenFiles()
		{
			_itemData = new SequentialFileWriter(World.ItemDataPath, _metrics);
			_itemIndex = new SequentialFileWriter(World.ItemIndexPath, _metrics);

			_mobileData = new SequentialFileWriter(World.MobileDataPath, _metrics);
			_mobileIndex = new SequentialFileWriter(World.MobileIndexPath, _metrics);

			_guildData = new SequentialFileWriter(World.GuildDataPath, _metrics);
			_guildIndex = new SequentialFileWriter(World.GuildIndexPath, _metrics);

			_customData = new SequentialFileWriter(World.DataBinaryPath, _metrics);
			_customIndex = new SequentialFileWriter(World.DataIndexPath, _metrics);

			WriteCount(_itemIndex, World.Items.Count);
			WriteCount(_mobileIndex, World.Mobiles.Count);
			WriteCount(_guildIndex, BaseGuild.List.Count);
			WriteCount(_customIndex, World.Data.Count);
		}

		private void CloseFiles()
		{
			_itemData.Close();
			_itemIndex.Close();

			_mobileData.Close();
			_mobileIndex.Close();

			_guildData.Close();
			_guildIndex.Close();

			_customData.Close();
			_customIndex.Close();
		}

		private void WriteCount(SequentialFileWriter indexFile, int count)
		{
			//Equiv to GenericWriter.Write( (int)count );
			var buffer = new byte[4];

			buffer[0] = (byte)(count);
			buffer[1] = (byte)(count >> 8);
			buffer[2] = (byte)(count >> 16);
			buffer[3] = (byte)(count >> 24);

			indexFile.Write(buffer, 0, buffer.Length);
		}

		private void SaveTypeDatabases()
		{
			SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
			SaveTypeDatabase(World.MobileTypesPath, World.m_MobileTypes);
			SaveTypeDatabase(World.DataTypesPath, World._DataTypes);
		}

		private void SaveTypeDatabase(string path, List<Type> types)
		{
			var bfw = new BinaryFileWriter(path, false);

			bfw.Write(types.Count);

			foreach (Type type in types)
			{
				bfw.Write(type.FullName);
			}

			bfw.Flush();
			bfw.Close();
		}
	}
}