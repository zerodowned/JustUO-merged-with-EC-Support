﻿#region References
using System;
using System.Linq;

using VitaNex.SuperGumps;
#endregion

namespace Server.Gumps.Compendium
{
	public class ElementListGump : SuperGump
	{
		private const int Z_ORDER_CHANGE_UP = 0;
		private const int Z_ORDER_CHANGE_DOWN = 1;
		private const int NUMBER_OF_ELEMENTS_PER_PAGE = 20;

		private readonly CompendiumEditorState m_editorState;

		public ElementListGump(CompendiumEditorState editorState)
			: base(editorState.Caller, null, 230, 0)
		{
			m_editorState = editorState;

			Closable = false;
			Disposable = false;
			Resizable = false;
			Dragable = true;
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			AddImage(1, 0, 2623, 1880); //Content Left Border 1
			AddImage(1, 165, 2623, 1880); //Content Left Border 2
			AddImage(2, 321, 2623, 1880); //Content Left Border 2
			AddImage(261, 0, 2625, 1880); //Content Right Border 1
			AddImage(261, 320, 2625, 1880); //Content Right Border 2
			AddImage(261, 165, 2625, 1880); //Content Right Border 2
			AddImage(6, 480, 2521, 2171); //Below Content Background 3
			AddImage(6, 512, 2627, 1880); //Bottom Border 3
			AddImage(106, 480, 2521, 2171); //Below Content Background 3
			AddImage(7, 0, 2621, 1880); //Content Top Border 3
			AddBackground(10, 11, 264, 474, 9300); //Background
			AddImageTiled(27, 45, 230, 1, 2621); //Top Line
			AddImageTiled(24, 456, 230, 1, 2621); //Bottom Line

			var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);

			var startingIdx = NUMBER_OF_ELEMENTS_PER_PAGE * m_editorState.ElementListGumpCurrentPageNumber;
			if (startingIdx > elements.Count() - 1)
			{
				startingIdx = 0;
				m_editorState.ElementListGumpCurrentPageNumber = 0;
			}

			var buttonLabelYCoord = 47;
			for (var i = startingIdx; i < startingIdx + NUMBER_OF_ELEMENTS_PER_PAGE && i < elements.Count(); ++i)
			{
				try
				{
					this.AddHyperlink(
						new WebColoredHyperLink(
							new Point2D(28, buttonLabelYCoord),
							elements.ElementAt(i) == m_editorState.SelectedElement ? "#FF0000" : Compendium.CONTENT_TEXT_WEB_COLOR,
							false,
							false,
							elements.ElementAt(i) == m_editorState.SelectedElement,
							onSelectElementButtonClick,
							i,
							elements.ElementAt(i).Name));

					//clone button(s)
					AddButton(214, buttonLabelYCoord, 5411, 5411, b => onCloneButtonClick(b, i));
					AddButton(208, buttonLabelYCoord + 3, 5411, 5411, b => onCloneButtonClick(b, i));

					if (elements.Count() > 1)
					{
						if (i != 0)
						{
							AddButton(238, buttonLabelYCoord + 5, 2435, 2436, b => onElementZOrderChangeUpButtonClick(b, i));
							//Z Order Up Button
						}

						if (i != m_editorState.RendererToEdit.Elements.Count - 1)
						{
							AddButton(248, buttonLabelYCoord + 5, 2437, 2438, b => onElementZOrderChangeDownButtonClick(b, i));
							//Z Order Down Button
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				buttonLabelYCoord += 20;
			}

			#region Top, Delete, and Bottom Buttons
			var selectedElementIdx = elements.IndexOf(m_editorState.SelectedElement);
			if (m_editorState.SelectedElement != null && selectedElementIdx < elements.Count() - 1)
			{
				AddImage(186, 461, 2463, 2171);
				this.AddHyperlink(
					new WebColoredHyperLink(
						new Point2D(186, 460),
						Compendium.YELLOW_TEXT_WEB_COLOR,
						false,
						false,
						false,
						onBottomButtonClick,
						selectedElementIdx,
						" Bottom"));
			}

			if (m_editorState.SelectedElement != null && selectedElementIdx > 0)
			{
				AddImage(24, 461, 2463, 2171);
				this.AddHyperlink(
					new WebColoredHyperLink(
						new Point2D(24, 460),
						Compendium.YELLOW_TEXT_WEB_COLOR,
						false,
						false,
						false,
						onTopButtonClick,
						selectedElementIdx,
						"   Top"));
			}

			if (m_editorState.SelectedElement != null)
			{
				AddImage(105, 461, 2463, 2171);
				this.AddHyperlink(
					new WebColoredHyperLink(
						new Point2D(105, 460),
						Compendium.YELLOW_TEXT_WEB_COLOR,
						false,
						false,
						false,
						onDeleteButtonClick,
						selectedElementIdx,
						"  Delete  "));
			}
			#endregion

			#region Page Navigator
			if (m_editorState.ElementListGumpCurrentPageNumber > 0)
			{
				AddButton(12, 495, 5603, 5607, b => onPrevPageButtonClick(b, 0)); //Next page Button
				AddLabel(34, 492, 47, @"Previous"); //Previous Page Label
			}
			else
			{
				AddImage(12, 495, 5603, 901); //Previous Page Button
				AddLabel(34, 492, 901, @"Previous"); //Previous Page Label
			}

			var totalpages = (int)Math.Ceiling(m_editorState.RendererToEdit.Elements.Count / (double)NUMBER_OF_ELEMENTS_PER_PAGE);

			if (m_editorState.ElementListGumpCurrentPageNumber < totalpages - 1)
			{
				AddButton(255, 495, 5601, 5605, b => onNextPageButtonClick(b, 0)); //Next page Button
				AddLabel(222, 492, 47, @"Next"); //Next Page Label
			}
			else
			{
				AddImage(255, 495, 5601, 901); //Previous Page Button
				AddLabel(222, 492, 901, @"Next"); //Next Page Label
			}

			AddLabel(
				128,
				493,
				47,
				string.Format("{0} of {1}", m_editorState.ElementListGumpCurrentPageNumber + 1, Math.Max(1, totalpages)));
			//Page Number Label

			AddLabel(116, 18, 47, @"Z Order"); //Z Order heading Label
			#endregion
		}

		public void setPageBySelectedElement(BaseCompendiumPageElement selectedElement)
		{
			var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);
			var idx = elements.IndexOf(selectedElement);

			var page = idx / NUMBER_OF_ELEMENTS_PER_PAGE;
			var totalpages = (m_editorState.RendererToEdit.Elements.Count / NUMBER_OF_ELEMENTS_PER_PAGE) + 1;

			if (page < 0 || page > totalpages)
			{
				page = 0;
			}

			m_editorState.ElementListGumpCurrentPageNumber = page;
		}

		public void onDeleteButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				try
				{
					var idx = button.Param;
					var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);
					var elementToRemove = m_editorState.SelectedElement;
					BaseCompendiumPageElement elementToSelect = null;

					var count = elements.Count();

					if (count > 1)
					{
						if (idx < count - 1)
						{
							elementToSelect = elements.ElementAt(idx + 1);
						}
						else if (idx == count - 1)
						{
							elementToSelect = elements.ElementAt(idx - 1);
						}
					}

					m_editorState.SelectedElement = elementToSelect;
					m_editorState.RendererToEdit.SelectedElement = elementToSelect;
					m_editorState.RendererToEdit.Elements.Remove(elementToRemove);
				}
				catch (Exception e)
				{
					Console.WriteLine("An exception was caught while trying to delete a compendium element");
					Console.WriteLine(e);
				}
			}

			m_editorState.Refresh();
		}

		public void onNextPageButtonClick(GumpButton button, int param)
		{
			m_editorState.ElementListGumpCurrentPageNumber++;
			m_editorState.Refresh();
		}

		public void onPrevPageButtonClick(GumpButton button, int param)
		{
			m_editorState.ElementListGumpCurrentPageNumber--;
			m_editorState.Refresh();
		}

		public void onBottomButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				var idx = button.Param;
				var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);

				if (idx < elements.Count() - 1)
				{
					var lower = elements.ElementAt(elements.Count() - 1);
					m_editorState.SelectedElement.Z = lower.Z + 1;

					var changedElements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);
					var newIdx = changedElements.IndexOf(m_editorState.SelectedElement);
					m_editorState.ElementListGumpCurrentPageNumber = newIdx / NUMBER_OF_ELEMENTS_PER_PAGE;
				}
			}

			m_editorState.Refresh();
		}

		public void onTopButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				var idx = button.Param;
				var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);

				if (idx > 0)
				{
					foreach (var element in elements)
					{
						element.Z += 1;
					}

					var topElement = elements.ElementAt(0);

					m_editorState.SelectedElement.Z = topElement.Z - 1;

					m_editorState.ElementListGumpCurrentPageNumber = 0;
				}
			}

			m_editorState.Refresh();
		}

		public void onSelectElementButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				var idx = button.Param;
				if (m_editorState.RendererToEdit.Elements.Count > idx)
				{
					var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);

					if (m_editorState.SelectedElement == elements.ElementAt(idx))
					{
						m_editorState.SelectedElement = null;
						m_editorState.RendererToEdit.SelectedElement = null;
					}
					else
					{
						m_editorState.SelectedElement = elements.ElementAt(idx);
						m_editorState.RendererToEdit.SelectedElement = elements.ElementAt(idx);
					}
				}
			}

			m_editorState.Refresh();
		}

		public void onCloneButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);
				if (button.Param >= 0 && elements.Count() > button.Param)
				{
					var newElement = elements.ElementAt(button.Param).Clone() as BaseCompendiumPageElement;

					if (newElement != null)
					{
						m_editorState.RendererToEdit.Elements.Add(newElement);
						m_editorState.ElementListGumpCurrentPageNumber = m_editorState.RendererToEdit.Elements.Count /
																		 NUMBER_OF_ELEMENTS_PER_PAGE;
						m_editorState.SelectedElement = newElement;
						m_editorState.RendererToEdit.SelectedElement = newElement;
						var lastElement = elements.ElementAt(elements.Count() - 1);
						newElement.Z = lastElement.Z + 1.0;
					}
				}
			}

			m_editorState.Refresh();
		}

		public void onElementZOrderChangeUpButtonClick(GumpButton button, int param)
		{
			ChangeZOrder(button, Z_ORDER_CHANGE_UP);
		}

		public void onElementZOrderChangeDownButtonClick(GumpButton button, int param)
		{
			ChangeZOrder(button, Z_ORDER_CHANGE_DOWN);
		}

		public void ChangeZOrder(GumpButton button, int direction)
		{
			if (button != null)
			{
				var elements = m_editorState.RendererToEdit.Elements.OrderBy(element => element.Z);
				if (elements.Count() > 1)
				{
					if (direction == Z_ORDER_CHANGE_UP)
					{
						if (button.Param > 0 && elements.Count() > button.Param)
						{
							var upper = elements.ElementAt(button.Param - 1);
							var lower = elements.ElementAt(button.Param);

							if (upper.Z == lower.Z)
							{
								lower.Z += 0.000001;
							}

							var temp = upper.Z;
							upper.Z = lower.Z;
							lower.Z = temp;
						}
					}
					else if (direction == Z_ORDER_CHANGE_DOWN)
					{
						if (elements.Count() > button.Param + 1)
						{
							var upper = elements.ElementAt(button.Param);
							var lower = elements.ElementAt(button.Param + 1);

							if (upper.Z == lower.Z)
							{
								lower.Z += 0.000001;
							}

							var temp = upper.Z;
							upper.Z = lower.Z;
							lower.Z = temp;
						}
					}
				}
			}

			m_editorState.Refresh();
		}
	}
}