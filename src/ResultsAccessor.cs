using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает объект-аксессор для файлов ответов тестируемых
	/// </summary>
	public class ResultsAccessor
		{
		private List<ResultUnit> answers = new List<ResultUnit> ();		// Список ответов
		private ConfigAccessor ca = null;								// Объект-аксессор конфигурации программы
		private string testName = "";									// Название теста
		private string userName = "", userKey = "";						// Имя пользователя и его ключ
		private bool isInited = true;									// Статус инициализации класса

		/// <summary>
		/// Константа содержит расширение имени файла результатов
		/// </summary>
		public const string ResultFNExt = ".res";

		/// <summary>
		/// Конструктор. Создаёт файл результатов и подготавливает все необходимые переменные
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="TestName">Название теста</param>
		/// <param name="UserName">Имя тестируемого</param>
		/// <param name="GeneratedQList">Список вопросов, сгенерированный тестером</param>
		/// <param name="UserKey">Ключ тестируемого</param>
		public ResultsAccessor (ConfigAccessor CA, string TestName, List<Question> GeneratedQList, string UserName, string UserKey)
			{
			FileStream FS = null;

			// Сохранение переменных
			ca = CA;
			userName = UserName;
			userKey = UserKey;
			testName = TestName;

			// Организация доступа. Создание файла
			if (!Directory.Exists (ca.ResultsPath + "\\" + testName))
				{
				try
					{
					Directory.CreateDirectory (ca.ResultsPath + "\\" + testName);
					}
				catch
					{
					isInited = false;
					return;
					}
				}

			try
				{
				FS = new FileStream (ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp", FileMode.Create);
				}
			catch
				{
				isInited = false;
				return;
				}
			FS.Close ();

			// Создание массива ответов
			for (int i = 0; i < GeneratedQList.Count; i++)
				{
				answers.Add (new ResultUnit (GeneratedQList[i].Number));
				}

			// Инициализация завершена
			}

		/// <summary>
		/// Конструктор. Загружает имеющийся файл результатов
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="TestName">Название теста</param>
		/// <param name="UserName">Имя тестируемого</param>
		public ResultsAccessor (ConfigAccessor CA, string TestName, string UserName)
			{
			FileStream FS = null;
			StreamReader SR = null;

			// Сохранение переменных
			ca = CA;
			userName = UserName;
			testName = TestName;

			// Попытка открытия файла
			if (!FileEncryptor.Decrypt (ca.ResultsPath + "\\" + testName + "\\" + userName + ResultFNExt,
				ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp"))
				{
				isInited = false;
				return;
				}

			try
				{
				FS = new FileStream (ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp", FileMode.Open);
				}
			catch
				{
				return;
				}
			SR = new StreamReader (FS, Encoding.GetEncoding (1251));

			// Чтение файла
			try
				{
				userKey = SR.ReadLine ();

				string number, answer;
				while (((number = SR.ReadLine ()) != null) && ((answer = SR.ReadLine ()) != null))
					{
					answers.Add (new ResultUnit (uint.Parse (number)));
					answers[answers.Count - 1].Answer = answer;
					}
				}
			catch
				{
				isInited = false;
				}

			// Завершение
			SR.Close ();
			FS.Close ();
			try
				{
				File.Delete (ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp");
				}
			catch
				{
				}
			}

		/// <summary>
		/// Проверяет совпадение оригинального ключа пользователя с тем, 
		/// который был получен при загрузке теста
		/// </summary>
		/// <param name="UserKey">Ключ тестируемого</param>
		/// <returns>Возвращает результат проверки</returns>
		public bool CheckKey (string UserKey)
			{
			return (UserKey == userKey);
			}

		/// <summary>
		/// Устанавливает ответ для вопроса в определённой позиции
		/// </summary>
		/// <param name="Answer">Ответ на вопрос</param>
		/// <param name="PositionInTest">Позиция в тесте</param>
		/// <returns>Возвращает результат выполнения операции (успешно/с ошибкой)</returns>
		public bool SetQuestionAnswer (int PositionInTest, string Answer)
			{
			if ((PositionInTest < 0) || (PositionInTest >= answers.Count))
				{
				return false;
				}

			answers[PositionInTest].Answer = Answer;
			return true;
			}

		/// <summary>
		/// Возвращает ответ для вопроса в определённой позиции
		/// </summary>
		/// <param name="PositionInTest">Позиция в тесте</param>
		/// <returns>Возвращает текущий ответ</returns>
		public string GetQuestionAnswer (int PositionInTest)
			{
			if ((PositionInTest < 0) || (PositionInTest >= answers.Count))
				{
				return null;
				}

			return answers[PositionInTest].Answer;
			}

		/// <summary>
		/// Возвращает номер вопроса в определённой позиции, на который был дан ответ
		/// </summary>
		/// <param name="PositionInTest">Позиция в тесте</param>
		/// <returns>Возвращает номер вопроса в позиции</returns>
		public uint GetQuestionNumber (int PositionInTest)
			{
			if ((PositionInTest < 0) || (PositionInTest >= answers.Count))
				{
				return uint.MaxValue;
				}

			return answers[PositionInTest].QuestionNumber;
			}

		/// <summary>
		/// Сохраняет результаты
		/// </summary>
		/// <returns>Возвращает результат выполнения операции (успешно/с ошибкой)</returns>
		public bool SaveResults ()
			{
			FileStream FS = null;
			StreamWriter SW = null;

			// Попытка открытия файла
			try
				{
				FS = new FileStream (ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp", FileMode.Create);
				}
			catch
				{
				return false;
				}
			SW = new StreamWriter (FS, Encoding.GetEncoding (1251));

			// Запись файла
			SW.WriteLine (userKey);

			for (int i = 0; i < answers.Count; i++)
				{
				SW.WriteLine (answers[i].QuestionNumber.ToString ());
				SW.WriteLine (answers[i].Answer);
				}

			// Завершение
			SW.Close ();
			FS.Close ();

			// Шифрование
			if (!FileEncryptor.Encrypt (ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp",
				ca.ResultsPath + "\\" + testName + "\\" + userName + ResultFNExt))
				{
				return false;
				}

			try
				{
				File.Delete (ca.ResultsPath + "\\" + testName + "\\" + userName + ".tmp");
				}
			catch
				{
				}

			return true;
			}

		/// <summary>
		/// Возвращает результат инициализации класса
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}

		/// <summary>
		/// Возвращает список имён тестируемых, для которых доступны результаты тестирования
		/// </summary>
		/// <returns>Список имён</returns>
		public string[] GetUsersNames ()
			{
			char[] spl = { '\\', '.' };

			string[] results = { };
			try
				{
				results = Directory.GetFiles (ca.ResultsPath + "\\" + testName, "*" + ResultFNExt);
				}
			catch
				{
				return results;
				}

			List<string> names = new List<string> ();
			for (int i = 0; i < results.Length; i++)
				{
				string[] strs = results[i].Split (spl);
				names.Add (strs[strs.Length - 2]);
				}

			return names.ToArray ();
			}

		/// <summary>
		/// Возвращает количество вопросов теста, на который были даны ответы
		/// </summary>
		public int QuestionsCount
			{
			get
				{
				return answers.Count;
				}
			}
		}
	}
