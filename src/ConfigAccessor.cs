using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс обеспечивает доступ к конфигурации программы
	/// </summary>
	public class ConfigAccessor
		{
		/// <summary>
		/// Содержит стандартное для всех модулей имя файла конфигурации
		/// </summary>
		public const string CfgFile = "GIATester.cfg";		// Константа с именем файла конфигурации
		private string CfgPath = Application.StartupPath + "\\" + CfgFile;
		private const string TempExt = ".tmp";

		/// <summary>
		/// Возвращает или устанавливает путь к базе тестов
		/// </summary>
		public string TestsPath
			{
			get
				{
				return testsPath;
				}
			set
				{
				testsPath = value;
				}
			}
		private string testsPath;

		/// <summary>
		/// Возвращает или устанавливает путь к базе ответов к тестам
		/// </summary>
		public string AnswersPath
			{
			get
				{
				return answersPath;
				}
			set
				{
				answersPath = value;
				}
			}
		private string answersPath;

		/// <summary>
		/// Возвращает или устанавливает путь к базе результатов
		/// </summary>
		public string ResultsPath
			{
			get
				{
				return resultsPath;
				}
			set
				{
				resultsPath = value;
				}
			}
		private string resultsPath;

		/// <summary>
		/// Возвращает или устанавливает пароль администратора системы
		/// </summary>
		public string AdminPassword
			{
			get
				{
				return adminPassword;
				}
			set
				{
				adminPassword = value;
				}
			}
		private string adminPassword;

		/// <summary>
		/// Возвращает стандартную кодовую страницу для текстовых файлов
		/// </summary>
		public static Encoding DefaultEncoding
			{
			get
				{
				return Encoding.GetEncoding (1251);
				}
			}

		// Метод создаёт директории, заданные в конфигурации, в случае их отсутствия
		private void CreateDirectories ()
			{
			try
				{
				if (!Directory.Exists (testsPath))
					{
					Directory.CreateDirectory (testsPath);
					}
				}
			catch
				{
				}

			try
				{
				if (!Directory.Exists (answersPath))
					{
					Directory.CreateDirectory (answersPath);
					}
				}
			catch
				{
				}

			try
				{
				if (!Directory.Exists (resultsPath))
					{
					Directory.CreateDirectory (resultsPath);
					}
				}
			catch
				{
				}
			}

		// Метод, загружающий файл конфигурации
		private bool LoadConfiguration ()
			{
			FileStream FSI = null;
			StreamReader SR = null;

			// Дешифровка файла
			if (!FileEncryptor.Decrypt (CfgPath, CfgPath + TempExt))
				{
				return false;
				}

			try
				{
				// Чтение
				FSI = new FileStream (CfgPath + TempExt, FileMode.Open);
				SR = new StreamReader (FSI, DefaultEncoding);

				testsPath = SR.ReadLine ();
				adminPassword = SR.ReadLine ();
				answersPath = SR.ReadLine ();
				resultsPath = SR.ReadLine ();

				// Завершение чтения
				SR.Close ();
				FSI.Close ();
				File.Delete (CfgPath + TempExt);
				}
			catch
				{
				return false;
				}

			// Проверка на существование
			CreateDirectories ();

			return true;
			}

		/// <summary>
		/// Метод, сохраняющий файл конфигурации
		/// </summary>
		/// <returns>Возвращает результат сохранения (успешно/с ошибкой)</returns>
		public bool SaveConfiguration ()
			{
			FileStream FSO = null;
			StreamWriter SW = null;

			// Открытие для записи
			try
				{
				FSO = new FileStream (CfgPath + TempExt, FileMode.Create);
				}
			catch
				{
				return false;
				}
			SW = new StreamWriter (FSO, DefaultEncoding);

			SW.WriteLine (testsPath);
			SW.WriteLine (adminPassword);
			SW.WriteLine (answersPath);
			SW.WriteLine (resultsPath);

			SW.Close ();
			FSO.Close ();

			// Шифрование файла
			if (!FileEncryptor.Encrypt (CfgPath + TempExt, CfgPath))
				{
				return false;
				}

			try
				{
				File.Delete (CfgPath + TempExt);
				}
			catch
				{
				}

			// Проверка на существование
			CreateDirectories ();

			return true;
			}

		/// <summary>
		/// Возвращает статус инициализации класса
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Конструктор. Автоматически загружает конфигурацию или создаёт образец с базовыми значениями
		/// </summary>
		public ConfigAccessor ()
			{
			if (!LoadConfiguration ())
				{
				testsPath = Application.StartupPath + "\\Tests";
				adminPassword = "123456";
				answersPath = Application.StartupPath + "\\Answers";
				resultsPath = Application.StartupPath + "\\Results";

				if (!SaveConfiguration ())
					{
					return;
					}
				}

			isInited = false;
			}
		}
	}
