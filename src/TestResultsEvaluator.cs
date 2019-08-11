using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GIATesterLib;

namespace GIATester
	{
	/// <summary>
	/// Форма отвечает за отображение и сохранение результатов тестирования
	/// </summary>
	public partial class TestResultsEvaluator:Form
		{
		private ConfigAccessor ca = null;
		private KeysAccessor ka = null;
		private Test test = null;

		/// <summary>
		/// Конструктор. Запускает форму
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="TestName">Название теста, к которому относятся результаты</param>
		public TestResultsEvaluator (ConfigAccessor CA, string TestName)
			{
			InitializeComponent ();

			// Инициализация
			ca = CA;

			TestAccessor ta = new TestAccessor (ca);
			test = ta.LoadTest (TestName);
			if (!ta.Success)
				{
				MessageBox.Show ("Ошибка загрузки теста: " + ta.ErrorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			ka = new KeysAccessor (ca);
			if (!ka.IsInited)
				{
				MessageBox.Show ("Ошибка загрузки набора ключей. Возможно, база данных программы недоступна для чтения/записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Настройка
			SaveResDialog.FileName = "";
			SaveResDialog.Filter = "Таблица в формате CSV (*.csv)|*.csv";
			SaveResDialog.Title = "Выберите файл для экспорта";

			// Формирование таблицы
			for (int i = 0; i < test.PositionsCount; i++)
				MainResultsView.Columns.Add ((i + 1).ToString (), (i + 1).ToString ());
			MainResultsView.Columns.Add ("Результат", "Результат");

			ResultsAccessor ra = new ResultsAccessor (ca, TestName, "");

			string[] uNames = ra.GetUsersNames ();
			for (int i = 0; i < uNames.Length; i++)
				{
				MainResultsView.Rows.Add ();
				MainResultsView.Rows[i].HeaderCell.Value = uNames[i];
				}

			// Расчёт
			for (int u = 0; u < MainResultsView.Rows.Count; u++)
				{
				float percentage = 0.0f;

				// Получение результатов тестируемого
				ra = new ResultsAccessor (ca, TestName, MainResultsView.Rows[u].HeaderCell.Value.ToString ());
				if (!ra.IsInited)
					{
					MainResultsView.Rows[u].Cells[MainResultsView.Columns.Count - 1].Value = "Файл результатов повреждён или недоступен";
					continue;
					}

				// Проверка ключа
				if (!ra.CheckKey (ka.GetKey (MainResultsView.Rows[u].HeaderCell.Value.ToString ())))
					{
					MainResultsView.Rows[u].Cells[MainResultsView.Columns.Count - 1].Value = "Файл результатов сфальсифицирован";
					continue;
					}

				// Проверка на соответствие имеющегося теста тому, который был использован при тестировании
				if (ra.QuestionsCount != test.PositionsCount)
					{
					MainResultsView.Rows[u].Cells[MainResultsView.Columns.Count - 1].Value = "Тест изменён и не соответствует ответам";
					continue;
					}

				// Загрузка результатов
				for (int q = 0; q < MainResultsView.Columns.Count - 1; q++)
					{
					MainResultsView.Rows[u].Cells[q].Value = ra.GetQuestionAnswer (q) + "/" +
						test.GetQuestionsAtPosition (q)[(int)ra.GetQuestionNumber (q)].Answer +
						" (" + ra.GetQuestionNumber (q).ToString () + ")";

					// Посимвольное сравнение ответов
					int c = 0;
					for (int i = 0; i < Math.Min (ra.GetQuestionAnswer (q).Length,
						test.GetQuestionsAtPosition (q)[(int)ra.GetQuestionNumber (q)].Answer.Length); i++)
						if (ra.GetQuestionAnswer (q)[i] == test.GetQuestionsAtPosition (q)[(int)ra.GetQuestionNumber (q)].Answer[i])
							c++;

					percentage += (float)c / (float)Math.Max (ra.GetQuestionAnswer (q).Length,
						test.GetQuestionsAtPosition (q)[(int)ra.GetQuestionNumber (q)].Answer.Length);
					}

				MainResultsView.Rows[u].Cells[MainResultsView.Columns.Count - 1].Value = 100.0f * percentage / 
					(float)(MainResultsView.Columns.Count - 1);
				MainResultsView.Rows[u].Cells[MainResultsView.Columns.Count - 1].Value += "%";
				}

			// Отображение
			this.ShowDialog ();
			}

		// Скрытие окна
		private void RClose_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Сохранение результатов в файл
		private void SaveToFile_Click (object sender, EventArgs e)
			{
			SaveResDialog.ShowDialog ();
			if (SaveResDialog.FileName == "")
				return;

			// Попытка открытия файла
			FileStream FS = null;
			try
				{
				FS = new FileStream (SaveResDialog.FileName, FileMode.Create);
				}
			catch
				{
				MessageBox.Show ("Не удалось создать указанный файл. Возможно, указанное расположение недоступно для записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}
			StreamWriter SW = new StreamWriter (FS, Encoding.GetEncoding (1251));

			// Запись заголовков
			for (int q = 0; q < MainResultsView.Columns.Count; q++)
				SW.Write (";" + MainResultsView.Columns[q].HeaderCell.Value.ToString ());
			SW.Write ("\n");

			// Запись результатов
			for (int u = 0; u < MainResultsView.Rows.Count; u++)
				{
				SW.Write (MainResultsView.Rows[u].HeaderCell.Value.ToString ());

				for (int q = 0; q < MainResultsView.Columns.Count; q++)
					if (MainResultsView.Rows[u].Cells[q].Value != null)
						SW.Write (";" + MainResultsView.Rows[u].Cells[q].Value.ToString ());
					else
						SW.Write (";");

				SW.Write ("\n");
				}

			// Завершение
			SW.Close ();
			FS.Close ();
			}
		}
	}
