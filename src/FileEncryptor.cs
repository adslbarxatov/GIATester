using System.IO;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает шифратор файлов, используемый программой
	/// </summary>
	public static class FileEncryptor
		{
		private static FileStream FSI, FSO;

		/// <summary>
		/// Метод шифрует файл
		/// </summary>
		/// <param name="InputFile">Путь к файлу, который необходимо зашифровать</param>
		/// <param name="OutputFile">Путь к зашифрованному файлу</param>
		/// <returns>Возвращает результат выполнения операции</returns>
		public static bool Encrypt (string InputFile, string OutputFile)
			{
			// Открытие файлов
			try
				{
				FSI = new FileStream (InputFile, FileMode.Open);
				FSO = new FileStream (OutputFile, FileMode.Create);
				}
			catch
				{
				return false;
				}

			// Шифровка
			for (int i = 0; i < FSI.Length; i++)
				{
				int b = FSI.ReadByte ();
				b = ((b >> (i % 8)) | (b << (8 - (i % 8)))) & 0xFF;
				FSO.WriteByte ((byte)b);
				}

			FSI.Close ();
			FSO.Close ();
			return true;
			}

		/// <summary>
		/// Метод дешифрует файл
		/// </summary>
		/// <param name="InputFile">Путь к файлу, который необходимо дешифровать</param>
		/// <param name="OutputFile">Путь к дешифрованному файлу</param>
		/// <returns>Возвращает результат выполнения операции</returns>
		public static bool Decrypt (string InputFile, string OutputFile)
			{
			try
				{
				FSI = new FileStream (InputFile, FileMode.Open);
				FSO = new FileStream (OutputFile, FileMode.Create);
				}
			catch
				{
				return false;
				}

			// Дешифровка
			for (int i = 0; i < FSI.Length; i++)
				{
				int b = FSI.ReadByte ();
				b = ((b << (i % 8)) | (b >> (8 - (i % 8)))) & 0xFF;
				FSO.WriteByte ((byte)b);
				}

			// Завершение
			FSI.Close ();
			FSO.Close ();
			return true;
			}
		}
	}
