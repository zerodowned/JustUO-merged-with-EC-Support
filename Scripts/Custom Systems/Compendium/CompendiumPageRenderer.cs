﻿#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
#endregion

//               Label    Image  Tiled Image Alpha Area Item HTML TextButton Button 
//+–––––––––––––+––––––––+––––––+–––––––––––+––––––––––+––––+––––+––––––––––+––––––+
//| X Offset    |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
//| Y Offset    |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
//| Color       |   x    |  x   |           |          | x  |    |    x     |      |
//| Graphics ID |        |  x   |     x     |          | x  |    |          |  x   |
//| Width       |        |      |     x     |    x     |    | x  |          |      |
//| Height      |        |      |     x     |    x     |    | x  |          |      |
//| Text        |   x    |      |           |          |    | x  |    x     |      |
//| Z           |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
//| Scrollbar   |        |      |           |          |    | x  |          |      |
//| Background  |        |      |           |          |    | x  |          |      |
//| Font Size   |        |      |           |          |    |    |    x     |      |
//| GumpID      |        |      |           |          |    |    |    x     |  x   |
//| GraphicsID2 +        +      +           +          +    +    +          +  x   +
//+ Type            x       x         x          x       x    x       x        x    
//                  6       6         7          6       6    9       8        7    
//

namespace Server.Gumps.Compendium
{
	public class CompendiumPageRenderer : ICloneable
	{
		public object Clone()
		{
			var renderer = new CompendiumPageRenderer
			{
				Name = (string)Name.Clone(),
				SelectedElement = SelectedElement == null ? null : (BaseCompendiumPageElement)SelectedElement.Clone()
			};

			foreach (var element in m_elements)
			{
				renderer.m_elements.Add((BaseCompendiumPageElement)element.Clone());
			}

			return renderer;
		}

		public CompendiumEditorState State { get; set; }
		public bool ShowEditorGrid { get; set; }
		public BaseCompendiumPageElement SelectedElement { get; set; }

		private const int NUMBER_OF_GRID_COLUMNS = 60;
		private const int NUMBER_OF_GRID_ROWS = 41;
		private const int GRID_BUTTON_HEIGHT = 13;
		private const int GRID_BUTTON_WIDTH = 13;
		private const int GRID_BUTTON_ID = 2511;

		private string m_compendiumPageFileSavePath = string.Empty;
		private readonly List<BaseCompendiumPageElement> m_elements = new List<BaseCompendiumPageElement>();
		public List<BaseCompendiumPageElement> Elements { get { return m_elements; } }

		public string Name { get; set; }

		public void Render(CompendiumPreviewPageGump gump)
		{
			Console.WriteLine("Rendering Preview Gump");
			try
			{
				//purple gump outline
				gump.AddImageTiled(0, 0, 790, 1, 1); //top
				gump.AddImageTiled(0, 1, 790, 1, 1); //top
				gump.AddImageTiled(0, 2, 790, 1, 1); //top
				gump.AddImageTiled(0, 3, 790, 1, 1); //top
				gump.AddImageTiled(0, 4, 790, 1, 1); //top

				gump.AddImageTiled(0, 541, 790, 1, 1); //bottom
				gump.AddImageTiled(0, 542, 790, 1, 1); //bottom
				gump.AddImageTiled(0, 543, 790, 1, 1); //bottom
				gump.AddImageTiled(0, 544, 790, 1, 1); //bottom
				gump.AddImageTiled(0, 545, 790, 1, 1); //bottom

				gump.AddImageTiled(0, 0, 1, 545, 1); //left
				gump.AddImageTiled(1, 0, 1, 545, 1); //left
				gump.AddImageTiled(2, 0, 1, 545, 1); //left
				gump.AddImageTiled(3, 0, 1, 545, 1); //left
				gump.AddImageTiled(4, 0, 1, 545, 1); //left

				gump.AddImageTiled(786, 0, 1, 545, 1); //right
				gump.AddImageTiled(787, 0, 1, 545, 1); //right
				gump.AddImageTiled(788, 0, 1, 545, 1); //right
				gump.AddImageTiled(789, 0, 1, 545, 1); //right
				gump.AddImageTiled(790, 0, 1, 545, 1); //right

				gump.AddAlphaRegion(1, 1, 789, 544);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			Render((CompendiumPageGump)gump);
		}

		public void Render(CompendiumPageGump gump)
		{
			Console.WriteLine("Rendering Compendium Gump");

			try
			{
				var elements = m_elements.OrderBy(element => element.Z);

				foreach (var element in elements)
				{
					element.Render(gump);
				}

				if (gump is CompendiumPreviewPageGump)
				{
					if (SelectedElement != null)
					{
						gump.AddImage(SelectedElement.X - 45, SelectedElement.Y - 42, 4503);
						SelectedElement.RenderOutline(gump);
					}

					if (ShowEditorGrid)
					{
						for (var x = 0; x < NUMBER_OF_GRID_COLUMNS; ++x)
						{
							for (var y = 0; y < NUMBER_OF_GRID_ROWS; ++y)
							{
								var param = (x * NUMBER_OF_GRID_ROWS) + y;

								gump.AddButton(
									(x * GRID_BUTTON_WIDTH),
									(y * GRID_BUTTON_HEIGHT),
									GRID_BUTTON_ID,
									GRID_BUTTON_ID,
									b => onGridButtonButtonClick(b, param));
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void onGridButtonButtonClick(GumpButton button, int param)
		{
			if (button != null && SelectedElement != null)
			{
				var x = (param / NUMBER_OF_GRID_ROWS) * GRID_BUTTON_WIDTH;
				var y = (param % NUMBER_OF_GRID_ROWS) * GRID_BUTTON_HEIGHT;

				SelectedElement.X = x;
				SelectedElement.Y = y;
				ShowEditorGrid = false;
			}

			State.Refresh();
		}

		public void Serialize()
		{
			var path = Path.Combine(Compendium.CompendiumRootPath, string.Format("{0}.xml", Name));

			var xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine;
			xml += "<CompendiumArticle>" + Environment.NewLine;
			xml += "  <Name>" + Name + "</Name>" + Environment.NewLine;

			foreach (var element in m_elements)
			{
				element.Serialize(ref xml, 1);
			}
			xml += "</CompendiumArticle>" + Environment.NewLine;

			Console.WriteLine("Serializing to: " + path);

			var rootPath = Path.GetDirectoryName(path);

			if (rootPath != null && !Directory.Exists(rootPath))
			{
				Directory.CreateDirectory(rootPath);
			}

			File.WriteAllText(path, xml);
		}

		public static CompendiumPageRenderer Deserialize(XDocument document)
		{
			var renderer = new CompendiumPageRenderer();

			var page = document.Descendants("CompendiumArticle").First();

			var compendiumElementsXml = page.Descendants("Element");

			renderer.Name = page.Descendants("Name").First().Value;

			foreach (var xElement in compendiumElementsXml)
			{
				try
				{
					renderer.m_elements.Add(BaseCompendiumPageElement.CreateElement(xElement));
				}
				catch
				{ }
			}

			if (renderer.m_elements.Count <= 0)
			{
				renderer = null;
			}

			return renderer;
		}
	}
}