using GIATesterLib;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GIATester
	{
	/// <summary>
	/// Главная форма программы
	/// </summary>
	public partial class MainForm2:Form
		{
		private ConfigAccessor ca = null;	// Объект-аксессор конфигурации программы
		private KeysAccessor ka = null;		// Объект-аксессор набора ключей
		private Test test = null;			// Загружаемый тест

		/// <summary>
		/// Конструктор. Создаёт форму
		/// </summary>
		public MainForm2 ()
			{
			InitializeComponent ();
			}

		// Запуск формы
		private void MainForm2_Load (object sender, System.EventArgs e)
			{
			// Поиск или создание файла конфигурации модуля
			FileStream FS = null;
			StreamReader SR = null;
			StreamWriter SW = null;

tryAgain:
			try
				// Попытка нормальной загрузки конфигурации
				{
				FS = new FileStream (Application.StartupPath + "\\GIATesterPath.cfg", FileMode.Open);	// Исключение по отсутствующему файлу
				SR = new StreamReader (FS, Encoding.GetEncoding (1251));

				string path = SR.ReadLine ();

				SR.Close ();
				FS.Close ();

				// Попытка инициализации конфигурации из указанного файла
				if (path + "\\" + ConfigAccessor.CfgFile != Application.StartupPath + "\\" + ConfigAccessor.CfgFile)	// Бывает и так
					File.Copy (path + "\\" + ConfigAccessor.CfgFile, Application.StartupPath + "\\" + ConfigAccessor.CfgFile, true);	// Исключение по отсутствующему источнику

				ca = new ConfigAccessor ();
				if (!ca.IsInited)
					{
					int a = 0;
					a = 1 / a;	// Принудительный вызов исключения
					}
				}
			catch
				{
				while ((ca == null) || !ca.IsInited)
					{
					// Проверка на готовность к поиску
					if (MessageBox.Show ("Файл конфигурации программы, без которого работа программы невозможна, не найден или повреждён\nУказать его новое расположение вручную?",
						"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
						{
						this.Close ();
						return;
						}

					// Выбор директории
					FindCfgFolder.ShowDialog ();
					if (FindCfgFolder.SelectedPath == "")
						{
						continue;
						}

					// Попытка записи информации о выборе
					try
						{
						FS = new FileStream (Application.StartupPath + "\\GIATesterPath.cfg", FileMode.Create);
						}
					catch
						{
						MessageBox.Show ("Критическая ошибка в работе программы. Обратитесь к администратору системы", "Ошибка",
							 MessageBoxButtons.OK, MessageBoxIcon.Error);
						this.Close ();
						return;
						}

					SW = new StreamWriter (FS, Encoding.GetEncoding (1251));

					SW.WriteLine (FindCfgFolder.SelectedPath);

					SW.Close ();
					FS.Close ();
					FindCfgFolder.SelectedPath = "";

					goto tryAgain;
					}
				}

			// Инициализация аксессора ключей
			ka = new KeysAccessor (ca);
			if (!ka.IsInited)
				{
				MessageBox.Show ("Ошибка загрузки набора ключей. Обратитесь к администратору системы",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Загрузка
			UserName.Items.AddRange (ka.UserNames (true));

			KeyValue.MaxLength = (int)KeysAccessor.KeySize;
			KeyLabel.Text = "Ключ (" + KeysAccessor.KeySize.ToString () + " символов):";
			}

		// Выход из программы
		private void UExit_Click (object sender, System.EventArgs e)
			{
			this.Close ();
			}

		// Вход с учётными данными
		private void UEnter_Click (object sender, System.EventArgs e)
			{
			// Проверка
			if (ka.GetKey (UserName.Text) != KeyValue.Text)
				{
				MessageBox.Show ("Введённый ключ не соответствует указанному пользователю", "Ошибка",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Вход
			UserName.Enabled = false;
			KeyValue.Enabled = false;
			KeyValue.PasswordChar = '\xF';
			UEnter.Enabled = false;
			UExit.Enabled = false;

			TestsList.Enabled = true;
			ChangeUser.Enabled = true;

			// Загрузка списка тестов
			TestAccessor ta = new TestAccessor (ca);
			TestsList.Items.Clear ();
			TestsList.Items.AddRange (ta.GetTestsNames ());
			}

		// Смена пользователя
		private void ChangeUser_Click (object sender, System.EventArgs e)
			{
			UserName.Enabled = true;
			KeyValue.Enabled = true;
			KeyValue.PasswordChar = '\x0';
			KeyValue.Text = "";
			UEnter.Enabled = true;
			UExit.Enabled = true;

			TestsList.Enabled = false;
			StartTest.Enabled = false;
			ChangeUser.Enabled = false;
			DescrLabel.Text = "";

			// Обновление списка ключей
			UserName.Items.Clear ();
			UserName.Items.AddRange (ka.UserNames (true));
			}

		// Выбор теста
		private void TestsList_SelectedIndexChanged (object sender, System.EventArgs e)
			{
			TestAccessor ta = new TestAccessor (ca);

			test = ta.LoadTest (TestsList.SelectedItem.ToString ());
			if (!ta.Success)
				{
				DescrLabel.Text = "(Тест недоступен)";
				StartTest.Enabled = false;
				}
			else
				{
				DescrLabel.Text = test.Description + "\r\n\r\nВремя тестирования: " + test.TestTime + " минут";
				StartTest.Enabled = true;
				}
			}

		// Запуск тестирования
		private void StartTest_Click (object sender, System.EventArgs e)
			{
			Tester t = new Tester (ca, ka, test, UserName.Text, KeyValue.Text);

			// Смена пользователя по завершении
			ChangeUser_Click (sender, e);
			}
		}
	}
