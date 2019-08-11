using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает загрузчик теста
	/// </summary>
	public class TestAccessor
		{
		// Переменные
		private ConfigAccessor ca = null;	// Объект-аксессор конфигурации программы
		private char[] qspl = { '|' };
		private char[] rspl = { ';' };		// Сплиттеры вопросов и компонентов вопросов

		/// <summary>
		/// Расширение имени файла теста
		/// </summary>
		public const string TestFNExt = ".tst";

		/// <summary>
		/// Расширение имени файла ответов
		/// </summary>
		public const string TestAnsFNExt = ".ans";

		/// <summary>
		/// Возвращает статус загрузки/сохранения теста
		/// </summary>
		public bool Success
			{
			get
				{
				return success;
				}
			}
		private bool success = true;

		/// <summary>
		/// Возвращает сообщение о статусе инициализации
		/// </summary>
		public string ErrorMsg
			{
			get
				{
				return errorMsg;
				}
			}
		private string errorMsg = "OK";

		/// <summary>
		/// Конструктор. Создаёт объект-загрузчик
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		public TestAccessor (ConfigAccessor CA)
			{
			ca = CA;
			}

		/// <summary>
		/// Загружает тест из файла и формирует соответствующий объект
		/// </summary>
		/// <param name="Name">Имя теста для загрузки</param>
		/// <returns>Возвращает объект-тест</returns>
		public Test LoadTest (string Name)
			{
			FileStream FST = null, FSA = null;
			StreamReader SRT = null, SRA = null;
			Test test;
			bool ansAvailable = true;

			// Дешифровка файлов
			if (!FileEncryptor.Decrypt (ca.TestsPath + "\\" + Name + TestFNExt, ca.TestsPath + "\\" + Name + ".tmp"))
				{
				errorMsg = "файл теста с указанным именем отсутствует или база тестов недоступна для чтения/записи";
				success = false;
				return null;
				}

			if (!FileEncryptor.Decrypt (ca.AnswersPath + "\\" + Name + TestAnsFNExt, ca.AnswersPath + "\\" + Name + ".tmp"))
				{
				errorMsg = "файл ответов к данному тесту недоступен";
				ansAvailable = false;
				}

			// Открытие файла
			try
				{
				FST = new FileStream (ca.TestsPath + "\\" + Name + ".tmp", FileMode.Open);
				if (ansAvailable)
					{
					FSA = new FileStream (ca.AnswersPath + "\\" + Name + ".tmp", FileMode.Open);
					}
				}
			catch
				{
				errorMsg = "база тестов недоступна для чтения/записи";
				success = false;

				// Обязательное удаление промежуточного расшифрованного файла
				if (FST != null)
					{
					FST.Close ();
					File.Delete (ca.TestsPath + "\\" + Name + ".tmp");
					}

				return null;
				}

			SRT = new StreamReader (FST, Encoding.GetEncoding (1251));
			if (ansAvailable)
				{
				SRA = new StreamReader (FSA, Encoding.GetEncoding (1251));
				}

			// Чтение
			try
				{
				test = new Test (Name, SRT.ReadLine ().Replace (qspl[0].ToString (), "\r\n"), uint.Parse (SRT.ReadLine ()));

				string str;
				int p = 0;

				// Чтение позиций теста
				while ((str = SRT.ReadLine ()) != null)
					{
					string[] questions = str.Split (qspl);
					string[] answers = { };

					if (ansAvailable)
						{
						string str1 = SRA.ReadLine ();
						answers = str1.Split (qspl);
						}

					// Чтение вопросов одной позиции
					for (int i = 0; i < questions.Length; i++)
						{
						string[] qData = questions[i].Split (rspl);

						Question q = new Question ((uint)i, ca.TestsPath + "\\" + Name, (QuestionTypes)int.Parse (qData[0]),
							(answers.Length == 0) ? "" : answers[i]);

						// Чтение компонентов одного вопроса
						for (int j = 1; j < qData.Length; j++)
							{
							if (!q.Add (uint.Parse (qData[j])))
								{
								errorMsg = "ресурс с номером " + qData[j] + " не найден или недоступен";
								success = false;
								}
							}

						// Добавление вопроса
						test.AddQuestion (p, q);
						}

					p++;
					}
				}
			catch
				{
				errorMsg = "структура файла теста «" + Name + TestFNExt + "» нарушена. Тест не может быть загружен";
				success = false;

				test = null;
				}

			// Завершение чтения
			SRT.Close ();
			FST.Close ();
			try
				{
				File.Delete (ca.TestsPath + "\\" + Name + ".tmp");
				}
			catch
				{
				}

			if (ansAvailable)
				{
				SRA.Close ();
				FSA.Close ();
				try
					{
					File.Delete (ca.AnswersPath + "\\" + Name + ".tmp");
					}
				catch
					{
					}
				}

			return test;
			}

		/// <summary>
		/// Сохраняет тест в файл
		/// </summary>
		/// <param name="TestToSave">Тест для сохранения</param>
		public void SaveTest (Test TestToSave)
			{
			FileStream FST = null, FSA = null;
			StreamWriter SWT = null, SWA = null;

			// Открытие файла
			try
				{
				FST = new FileStream (ca.TestsPath + "\\" + TestToSave.Name + ".tmp", FileMode.Create);
				FSA = new FileStream (ca.AnswersPath + "\\" + TestToSave.Name + ".tmp", FileMode.Create);
				}
			catch
				{
				errorMsg = "база тестов недоступна для чтения/записи";
				success = false;

				if (FST != null)
					{
					FST.Close ();
					FSA.Close ();
					File.Delete (ca.TestsPath + "\\" + TestToSave.Name + ".tmp");
					File.Delete (ca.AnswersPath + "\\" + TestToSave.Name + ".tmp");
					}

				return;
				}
			SWT = new StreamWriter (FST, Encoding.GetEncoding (1251));
			SWA = new StreamWriter (FSA, Encoding.GetEncoding (1251));

			// Запись
			SWT.WriteLine (TestToSave.Description.Replace ("\r\n", qspl[0].ToString ()));
			SWT.WriteLine (TestToSave.TestTime.ToString ());

			// Запись позиции
			for (int p = 0; p < TestToSave.PositionsCount; p++)
				{
				List<Question> lq = TestToSave.GetQuestionsAtPosition (p);
				string str = "", str1 = "";

				// Запись вопроса в позиции
				for (int q = 0; q < lq.Count; q++)
					{
					List<uint> lu = lq[q].GetUnitsList ();

					// Тип вопроса
					str += ((uint)lq[q].Type).ToString ();	// Тип вопроса
					// Номера вопросов обновляются при каждой загрузке теста. Поэтому они не сохраняются

					// Компоненты вопроса
					for (int u = 0; u < lu.Count; u++)
						{
						str += rspl[0].ToString () + lu[u].ToString ();
						}

					str += qspl[0].ToString ();

					// Ответ на вопрос
					str1 += lq[q].Answer.Replace (qspl[0], ',').Replace (rspl[0], ',') + qspl[0];
					}

				SWT.WriteLine (str.Substring (0, str.Length - 1));
				SWA.WriteLine (str1.Substring (0, str1.Length - 1));
				}

			SWT.Close ();
			FST.Close ();
			SWA.Close ();
			FSA.Close ();

			// Шифрование
			if (!FileEncryptor.Encrypt (ca.TestsPath + "\\" + TestToSave.Name + ".tmp", ca.TestsPath + "\\" + TestToSave.Name + TestFNExt) ||
				!FileEncryptor.Encrypt (ca.AnswersPath + "\\" + TestToSave.Name + ".tmp", ca.AnswersPath + "\\" + TestToSave.Name + TestAnsFNExt))
				{
				errorMsg = "база тестов недоступна для чтения/записи";
				success = false;
				}

			try
				{
				File.Delete (ca.TestsPath + "\\" + TestToSave.Name + ".tmp");
				File.Delete (ca.AnswersPath + "\\" + TestToSave.Name + ".tmp");
				}
			catch
				{
				}

			// Успешно
			}

		/// <summary>
		/// Проверяет существование теста с данным именем
		/// </summary>
		/// <param name="Name">Имя теста для проверки</param>
		/// <returns>Возвращает результат проверки</returns>
		public bool Exists (string Name)
			{
			return File.Exists (ca.TestsPath + "\\" + Name + TestFNExt);
			}

		/// <summary>
		/// Возвращает список названий доступных тестов
		/// </summary>
		/// <returns>Список названий</returns>
		public string[] GetTestsNames ()
			{
			char[] spl = { '\\', '.' };

			string[] tests = { };
			try
				{
				tests = Directory.GetFiles (ca.TestsPath, "*" + TestFNExt);
				}
			catch
				{
				return tests;
				}

			List<string> names = new List<string> ();
			for (int i = 0; i < tests.Length; i++)
				{
				string[] strs = tests[i].Split (spl);
				names.Add (strs[strs.Length - 2]);
				}

			return names.ToArray ();
			}
		}
	}
