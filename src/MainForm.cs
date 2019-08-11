using System.IO;
using System.Windows.Forms;
using GIATesterLib;

namespace GIATester
	{
	/// <summary>
	/// Главная форма программы
	/// </summary>
	public partial class MainForm:Form
		{
		private ConfigAccessor ca = new ConfigAccessor ();

		/// <summary>
		/// Инициализация формы
		/// </summary>		
		public MainForm ()
			{
			InitializeComponent ();
			}

		// Запуск формы
		private void MainForm_Load (object sender, System.EventArgs e)
			{
			// Настройка контролов
			AdminPass.PasswordChar = '\xF';

			// Проверка загрузки конфигурации
			if (!ca.IsInited)
				{
				MessageBox.Show ("Ошибка загрузки конфигурации программы. Возможно, база данных программы недоступна для чтения/записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.Close ();
				}
			}

		// Вход в систему при подтверждении пароля
		private void AdminEnter_Click (object sender, System.EventArgs e)
			{
			if (AdminPass.Text == ca.AdminPassword)
				{
				// Переход в режим изменения пароля
				AdminEnter.Enabled = false;
				ChangePass.Enabled = true;
				AdminPass.PasswordChar = '\x0';

				// Включение всех функций
				SetAnswersPath.Enabled = true;
				SetTestPath.Enabled = true;
				SetResultPath.Enabled = true;

				TestsList.Enabled = true;
				EditTest.Enabled = true;
				EvaluateResults.Enabled = true;
				DeleteTest.Enabled = true;
				CreateTest.Enabled = true;
				EditKeys.Enabled = true;

				button1.Width = 10;

				// Обновление списка тестов
				TestsListUpdate ();
				}
			else
				{
				MessageBox.Show ("Введённый пароль неверен", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}
			}

		// Смена пароля
		private void ChangePass_Click (object sender, System.EventArgs e)
			{
			if (AdminPass.Text.Length < 6)
				{
				MessageBox.Show ("Длина пароля не может быть меньше 6 символов",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			ca.AdminPassword = AdminPass.Text;

			if (!ca.SaveConfiguration ())
				MessageBox.Show ("Не удалось изменить пароль. Возможно, потеряна связь с базой данных программы",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

		// Установка пути к директории тестов
		private void SetTestPath_Click (object sender, System.EventArgs e)
			{
			SelectDirectory.Description = "Выберите директорию, в которой будут храниться разрабатываемые тестовые задания";
			SelectDirectory.SelectedPath = ca.TestsPath;

			SelectDirectory.ShowDialog ();

			ca.TestsPath = SelectDirectory.SelectedPath;
			if (!ca.SaveConfiguration ())
				MessageBox.Show ("Не удалось изменить путь. Возможно, потеряна связь с базой данных программы",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			TestsListUpdate ();
			}

		// Нажатие Enter в поле пароля
		private void AdminPass_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				AdminEnter_Click (sender, new System.EventArgs ());
			}

		// Выход из программы
		private void FExit_Click (object sender, System.EventArgs e)
			{
			if (MessageBox.Show ("Завершить работу с программой?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
				this.Close ();
				}
			}

		// Установка пути к директории ответов
		private void SetAnswersPath_Click (object sender, System.EventArgs e)
			{
			SelectDirectory.Description = "Выберите директорию, в которой будут храниться эталоны ответов к тестовым заданиям";
			SelectDirectory.SelectedPath = ca.AnswersPath;

			SelectDirectory.ShowDialog ();

			ca.AnswersPath = SelectDirectory.SelectedPath;
			if (!ca.SaveConfiguration ())
				{
				MessageBox.Show ("Не удалось изменить путь. Возможно, потеряна связь с базой данных программы",
				   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

		// Установка пути к директории результатов
		private void SetResultPath_Click (object sender, System.EventArgs e)
			{
			SelectDirectory.Description = "Выберите директорию, в которой будут храниться ответы учащихся";
			SelectDirectory.SelectedPath = ca.ResultsPath;

			SelectDirectory.ShowDialog ();

			ca.ResultsPath = SelectDirectory.SelectedPath;
			if (!ca.SaveConfiguration ())
				{
				MessageBox.Show ("Не удалось изменить путь. Возможно, потеряна связь с базой данных программы",
				   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

		// Обновление списка тестов
		private void TestsListUpdate ()
			{
			TestAccessor ta = new TestAccessor (ca);
			TestsList.Items.Clear ();
			TestsList.Items.AddRange (ta.GetTestsNames ());

			if (TestsList.Items.Count != 0)
				{
				TestsList.SelectedIndex = 0;

				EditTest.Enabled = true;
				DeleteTest.Enabled = true;
				EvaluateResults.Enabled = true;
				}
			else
				{
				EditTest.Enabled = false;
				DeleteTest.Enabled = false;
				EvaluateResults.Enabled = false;
				}
			}

		// Редактирование теста
		private void EditTest_Click (object sender, System.EventArgs e)
			{
			TestRedactor tr = new TestRedactor (ca, TestsList.Items[TestsList.SelectedIndex].ToString ());

			TestsListUpdate ();
			}

		// Удаление теста
		private void DeleteTest_Click (object sender, System.EventArgs e)
			{
			if (MessageBox.Show ("Вы действительно хотите удалить выбранный тест? Все компоненты, эталоны и результаты теста будут потеряны!",
				"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				if (MessageBox.Show ("Подтвердите удаление",
					"Вопрос", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					{
					int oldPosition = TestsList.SelectedIndex;

					// Удаление
					File.Delete (ca.TestsPath + "\\" + TestsList.Items[TestsList.SelectedIndex].ToString () + TestAccessor.TestFNExt);
					File.Delete (ca.AnswersPath + "\\" + TestsList.Items[TestsList.SelectedIndex].ToString () + TestAccessor.TestAnsFNExt);

					try
						{
						Directory.Delete (ca.TestsPath + "\\" + TestsList.Items[TestsList.SelectedIndex].ToString (), true);
						}
					catch
						{
						}

					try
						{
						Directory.Delete (ca.ResultsPath + "\\" + TestsList.Items[TestsList.SelectedIndex].ToString (), true);
						}
					catch
						{
						}

					// Обновление списка
					TestsListUpdate ();
					while (oldPosition >= TestsList.Items.Count)
						{
						oldPosition--;
						}

					if (oldPosition >= 0)
						{
						TestsList.SelectedIndex = oldPosition;
						}
					}
			}

		// Создание нового теста
		private void CreateTest_Click (object sender, System.EventArgs e)
			{
			TestRedactor tr = new TestRedactor (ca);

			TestsListUpdate ();
			}

		// Оценка результатов
		private void EvaluateResults_Click (object sender, System.EventArgs e)
			{
			TestResultsEvaluator tre = new TestResultsEvaluator (ca, TestsList.Items[TestsList.SelectedIndex].ToString ());
			}

		// Редактирование списка ключей тестируемых
		private void EditKeys_Click (object sender, System.EventArgs e)
			{
			KeysRedactor kr = new KeysRedactor ();
			}
		}
	}
