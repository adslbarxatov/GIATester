using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс обеспечивает доступ к файлу ключей тестируемых
	/// Генерация и хранение ключей выполняется внутри экземпляра
	/// Доступ ограничен одним ключом в единицу времени
	/// Извлечение всех ключей выполняется только посредством экспорта в файл
	/// </summary>
	public class KeysAccessor
		{
		// Константы
		private const string KeyFile = "GIATester.key";			// Имя файла ключей

		// Переменные
		private List<UserKey> keysTable = new List<UserKey> ();	// Таблица ключей
		private ConfigAccessor ca;								// Объект-аксессор конфигурации программы

		/// <summary>
		/// Длина ключа, используемая программой
		/// </summary>
		public const uint KeySize = 15;							// Длина ключа


		/// <summary>
		/// Конструктор. Проверяет наличие файла ключей и, при необходимости, создаёт новый
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		public KeysAccessor (ConfigAccessor CA)
			{
			FileStream FS = null;
			StreamReader SR = null;

			// Попытка открытия файла
			ca = CA;
			if (!FileEncryptor.Decrypt (ca.TestsPath + "\\" + KeyFile, ca.TestsPath + "\\" + KeyFile + ".tmp"))
			// Файл отсутствует или недоступен
				{
				try
					{
					// Попытка создания
					FS = new FileStream (ca.TestsPath + "\\" + KeyFile, FileMode.Create);
					FS.Close ();
					isInited = true;

					// Чтение не требуется
					}
				catch
					{
					// Полный отказ
					}

				return;
				}

			// Файл найден. Обработка
			try
				{
				FS = new FileStream (ca.TestsPath + "\\" + KeyFile + ".tmp", FileMode.Open);
				SR = new StreamReader (FS, Encoding.GetEncoding (1251));

				string name, key, status;
				while (((name = SR.ReadLine ()) != null) && ((key = SR.ReadLine ()) != null) && ((status = SR.ReadLine ()) != null))
					{
					keysTable.Add (new UserKey (name, key, bool.Parse (status)));
					}

				SR.Close ();
				}
			catch
				{
				// Чтение будет прекращено при обнаружении ошибки
				}

			// Чтение завершено
			FS.Close ();
			try
				{
				File.Delete (ca.TestsPath + "\\" + KeyFile + ".tmp");
				}
			catch
				{
				}

			isInited = true;
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
		private bool isInited = false;

		/// <summary>
		/// Возвращает список тестируемых, для которых в таблице имеются ключи
		/// </summary>
		/// <param name="ActiveOnly">Указывает, какие имена возвращать (только с активными ключами/все)</param>
		/// <returns>Список имён тестируемых</returns>
		public string[] UserNames (bool ActiveOnly)
			{
			List<string> names = new List<string> ();

			for (int i = 0; i < keysTable.Count; i++)
				{
				if (ActiveOnly && keysTable[i].IsActive || !ActiveOnly)
					{
					names.Add (keysTable[i].UserName + (ActiveOnly ? "" : (" (" + (keysTable[i].IsActive ? "" : "не") + "активен)")));
					}
				}

			return names.ToArray ();
			}

		/// <summary>
		/// Возвращает ключ по указанному имени пользователя
		/// </summary>
		/// <param name="UserName">Имя пользователя, ключ которого требуется найти</param>
		/// <returns>Возвращает значение ключа или null, если ключ не найден</returns>
		public string GetKey (string UserName)
			{
			// Поиск ключа
			for (int i = 0; i < keysTable.Count; i++)
				{
				if (keysTable[i].UserName == UserName)
					{
					return keysTable[i].UserKeyValue;
					}
				}

			// Если ключ не найден
			return null;
			}

		/// <summary>
		/// Сбрасывает таблицу ключей
		/// </summary>
		/// <returns>Возвращает результат выполнения операции (успешно/с ошибкой)</returns>
		public bool ClearKeyTable ()
			{
			keysTable.Clear ();

			return SaveKeyTable ();
			}

		// Сохраняет таблицу ключей в файл
		private bool SaveKeyTable ()
			{
			FileStream FS = null;
			StreamWriter SW = null;

			// Открытие для записи
			try
				{
				FS = new FileStream (ca.TestsPath + "\\" + KeyFile + ".tmp", FileMode.Create);
				}
			catch
				{
				return false;
				}
			SW = new StreamWriter (FS, Encoding.GetEncoding (1251));

			for (int i = 0; i < keysTable.Count; i++)
				{
				SW.WriteLine (keysTable[i].UserName);
				SW.WriteLine (keysTable[i].UserKeyValue);
				SW.WriteLine (keysTable[i].IsActive.ToString ());
				}

			SW.Close ();
			FS.Close ();

			// Шифрование файла
			bool res = FileEncryptor.Encrypt (ca.TestsPath + "\\" + KeyFile + ".tmp", ca.TestsPath + "\\" + KeyFile);
			try
				{
				File.Delete (ca.TestsPath + "\\" + KeyFile + ".tmp");
				}
			catch
				{
				}

			return res;
			}

		/// <summary>
		/// Генерирует и добавляет ключ в таблицу
		/// </summary>
		/// <param name="UserName">Имя тестируемого, которому требуется сопоставить новый ключ</param>
		/// <returns>Возвращает результат выполнения операции (успешно/с ошибкой)</returns>
		public bool InsertKey (string UserName)
			{
			string key = "";

			Random rnd = new Random ();
			byte[] bytes = new byte[KeySize];

			for (uint i = 0; i < KeySize; i++)
				{
				if (i % 10 < 5)
					{
					bytes[i] = (byte)rnd.Next (0x41, 0x5B);
					}
				else
					{
					bytes[i] = (byte)rnd.Next (0x30, 0x3A);
					}
				}

			// Сборка ключа и добавление в таблицу
			key = Encoding.GetEncoding (1251).GetString (bytes);
			keysTable.Add (new UserKey (UserName, key, true));

			// Сохранение таблицы
			return SaveKeyTable ();
			}

		/// <summary>
		/// Экспортирует ключи и имена тестируемых в файл в формате CSV
		/// </summary>
		/// <param name="Path">Путь для сохранения</param>
		/// <returns>Возвращает результат выполнения операции (успешно/с ошибкой)</returns>
		public bool ExportKeys (string Path)
			{
			FileStream FSI = null, FSO = null;
			StreamReader SR = null;
			StreamWriter SW = null;

			// Открытие файлов
			if (!FileEncryptor.Decrypt (ca.TestsPath + "\\" + KeyFile, ca.TestsPath + "\\" + KeyFile + ".tmp"))
				{
				return false;
				}

			try
				{
				FSI = new FileStream (ca.TestsPath + "\\" + KeyFile + ".tmp", FileMode.Open);
				FSO = new FileStream (Path, FileMode.Create);
				}
			catch
				{
				File.Delete (ca.TestsPath + "\\" + KeyFile + ".tmp");
				return false;
				}

			SR = new StreamReader (FSI, Encoding.GetEncoding (1251));
			SW = new StreamWriter (FSO, Encoding.GetEncoding (1251));

			// Трансляция
			string name, key, status;
			while (((name = SR.ReadLine ()) != null) && ((key = SR.ReadLine ()) != null) && ((status = SR.ReadLine ()) != null))
				{
				SW.WriteLine (name + ";" + key);
				}

			// Завершение
			SW.Close ();
			SR.Close ();
			FSI.Close ();
			FSO.Close ();

			try
				{
				File.Delete (ca.TestsPath + "\\" + KeyFile + ".tmp");
				}
			catch
				{
				}

			return true;
			}

		/// <summary>
		/// Удаляет ключ из таблицы, делая его неактивным
		/// </summary>
		/// <param name="UserName">Имя тестируемого, ключ которого деактивируется</param>
		/// <returns>Возвращает результат выполнения операции (успешно/с ошибкой)</returns>
		public bool DeactivateKey (string UserName)
			{
			// Поиск ключа
			int i;
			for (i = 0; i < keysTable.Count; i++)
				{
				if (keysTable[i].UserName == UserName)
					{
					keysTable[i].DeactivateKey ();
					break;
					}
				}

			// Если ключ не найден
			if (i == keysTable.Count)
				{
				return false;
				}

			// Сохранение таблицы
			return SaveKeyTable ();
			}
		}
	}
