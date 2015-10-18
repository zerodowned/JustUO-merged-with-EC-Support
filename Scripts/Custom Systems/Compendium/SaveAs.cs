﻿#region References
using System.IO;

using VitaNex.SuperGumps;
#endregion

namespace Server.Gumps.Compendium
{
	public class SaveAsGump : SuperGump
	{
		private readonly CompendiumEditorState m_state;
		private string m_gumpName;
		private bool m_displayErrorMessage;

		public SaveAsGump(CompendiumEditorState state)
			: base(state.Caller)
		{
			m_state = state;
			Disposable = false;
			Closable = false;
			Resizable = false;
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			if (!m_displayErrorMessage)
			{
				AddBackground(10, 11, 264, 99, 9300); //Background
			}
			else
			{
				AddBackground(10, 11, 264, 121, 9300); //Background
				this.AddHtmlLabel(28, 103, FontHandling.FontSize.Medium, false, false, false, "#FF0000", "Invalid Name!");
			}

			AddImageTiled(27, 45, 230, 1, 2621); //Top Line
			AddImageTiled(24, 74, 230, 1, 2621); //Bottom Line
			AddLabel(116, 18, 47, @"Save As..."); //Save as heading Label
			AddImage(186, 80, 2463, 2171); //Save Button Background
			AddImage(113, 80, 2463, 2171); //Cancel Button Background
			AddBackground(26, 50, 229, 20, 9350); //Background 1

			AddTextEntry(30, 50, 224, 20, 0, m_state.RendererToEdit.Name, onGumpNameEntryUpdate);
			this.AddHyperlink(
				new WebColoredHyperLink(
					new Point2D(188, 78),
					Compendium.YELLOW_TEXT_WEB_COLOR,
					false,
					false,
					false,
					onSaveButtonClick,
					0,
					@" Save")); //Save Hyperlink
			this.AddHyperlink(
				new WebColoredHyperLink(
					new Point2D(115, 78),
					Compendium.YELLOW_TEXT_WEB_COLOR,
					false,
					false,
					false,
					onCancelButtonClick,
					0,
					@" Cancel")); //Save Hyperlink
		}

		public void onSaveButtonClick(GumpButton entry, int param)
		{
			if (!m_displayErrorMessage && !string.IsNullOrEmpty(m_gumpName) && !string.IsNullOrWhiteSpace(m_gumpName))
			{
				m_state.RendererToEdit.Name = m_gumpName;

				m_state.RendererToEdit.Serialize();

				if (Compendium.g_CompendiumRenderers.ContainsKey(m_state.RendererToEdit.Name))
				{
					Compendium.g_CompendiumRenderers[m_state.RendererToEdit.Name] = m_state.RendererToEdit;
				}
				else
				{
					Compendium.g_CompendiumRenderers.Add(m_state.RendererToEdit.Name, m_state.RendererToEdit);
				}

				m_state.Refresh();
			}
			else
			{
				Refresh();
			}
		}

		public void onGumpNameEntryUpdate(GumpTextEntry entry, string param)
		{
			if (entry != null)
			{
				var gumpname = entry.InitialText;
				if (gumpname.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					m_displayErrorMessage = true;
				}
				else
				{
					m_gumpName = gumpname;
					m_displayErrorMessage = false;
				}
			}
		}

		public void onCancelButtonClick(GumpButton entry, int param)
		{
			//m_state.Refresh();
		}
	}
}