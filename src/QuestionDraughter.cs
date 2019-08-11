using System.Collections.Generic;
using System.Windows.Forms;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс отвечает за отображение вопроса в контроле GroupBox
	/// </summary>
	public static class QuestionDraughter
		{
		/// <summary>
		/// Метод выполняет отрисовку вопроса на контроле GroupBox
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="PositionNumber">Позиция в тесте, вопрос из которой необходимо отобразить</param>
		/// <param name="PreviewZone">Контрол, в котором требуется выполнить отображение</param>
		/// <param name="QuestionNumber">Номер вопроса в позиции</param>
		/// <param name="TestToView">Объект-тест, в котором содержится отображаемый вопрос</param>
		public static void Draw (GroupBox PreviewZone, Test TestToView, int PositionNumber, int QuestionNumber, ConfigAccessor CA)
			{
			List<uint> units = TestToView.GetQuestionsAtPosition (PositionNumber)[QuestionNumber].GetUnitsList ();

			PreviewZone.Controls.Clear ();
			for (int i = 0; i < units.Count; i++)
				{
				QuestionUnit qu = new QuestionUnit (CA.TestsPath + "\\" + TestToView.Name, units[i]);
				if (!qu.IsInited)
					{
					MessageBox.Show ("Ошибка доступа: ресурс с номером " + units[i].ToString () + " не найден или недоступен",
						"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

				switch (qu.UnitType)
					{
					case QuestionUnitTypes.Text:
						PreviewZone.Controls.Add (new Label ());
						((Label)PreviewZone.Controls[i]).Text = qu.UnitText;
						PreviewZone.Controls[i].Height = (int)((float)((Label)PreviewZone.Controls[i]).PreferredHeight * 1.5f);
						break;

					case QuestionUnitTypes.Image:
						PreviewZone.Controls.Add (new PictureBox ());
						((PictureBox)PreviewZone.Controls[i]).BackgroundImage = qu.UnitImage;
						((PictureBox)PreviewZone.Controls[i]).BackgroundImageLayout = ImageLayout.Center;
						PreviewZone.Controls[i].Height = qu.UnitImage.Height;
						break;
					}

				// Расположение
				PreviewZone.Controls[i].Left = 10;
				if (i == 0)
					{
					PreviewZone.Controls[i].Top = 30;
					}
				else
					{
					PreviewZone.Controls[i].Top = PreviewZone.Controls[i - 1].Top + PreviewZone.Controls[i - 1].Height;
					}
				PreviewZone.Controls[i].Width = PreviewZone.Width - 20;
				}
			}
		}
	}
