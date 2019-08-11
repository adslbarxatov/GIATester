using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает отдельный компонент (ресурс) вопроса
	/// </summary>
	public class QuestionUnit
		{
		private uint id = uint.MaxValue;		// Уникальный идентификатор ресурса
		private string text = "";				// Текст
		private Image image = null;				// Изображение и пояснение к нему
		private QuestionUnitTypes unitType;		// Тип компонента вопроса
		private string resourcePath = "";		// Путь к базе ресурсов конкретного теста

		/// <summary>
		/// Конструктор. Загружает компонент вопроса из файла базы компонентов
		/// </summary>
		/// <param name="ID">Уникальный номер компонента</param>
		/// <param name="ResourcePath">Путь к списку ресурсов</param>
		public QuestionUnit (string ResourcePath, uint ID)
			{
			FileStream FS = null;
			StreamReader SR;

			bool isText = true;
			bool isImage = true;

			// Попытка открытия фрагмента
			resourcePath = ResourcePath;

			if (!FileEncryptor.Decrypt (resourcePath + "\\" + ID.ToString () + ".txt", resourcePath + "\\" + ID.ToString () + ".tmp"))
				{
				isText = false;
				}

			if ((isText == true) || !FileEncryptor.Decrypt (resourcePath + "\\" + ID.ToString () + ".png",
				resourcePath + "\\" + ID.ToString () + ".tmp"))
				{
				isImage = false;
				}

			// Открыт текстовый файл
			if (isText)
				{
				try
					{
					FS = new FileStream (resourcePath + "\\" + ID.ToString () + ".tmp", FileMode.Open);
					}
				catch
					{
					return;
					}

				unitType = QuestionUnitTypes.Text;
				SR = new StreamReader (FS, Encoding.GetEncoding (1251));
				text = SR.ReadToEnd ();
				id = ID;

				SR.Close ();
				FS.Close ();

				// Удаление дешифрованной копии
				try
					{
					File.Delete (resourcePath + "\\" + ID.ToString () + ".tmp");
					}
				catch
					{
					}
				return;
				}

			// Открыто изображение
			if (isImage)
				{
				try
					{
					FS = new FileStream (resourcePath + "\\" + ID.ToString () + ".tmp", FileMode.Open);
					image = Image.FromStream (FS);
					}
				catch
					{
					return;
					}

				// Устранение ссылочной зависимости загруженного изображения от файла
				Image image2 = (Image)image.Clone ();
				FS.Close ();
				image.Dispose ();
				image = image2;

				unitType = QuestionUnitTypes.Image;
				id = ID;

				// Удаление дешифрованной копии
				try
					{
					File.Delete (resourcePath + "\\" + ID.ToString () + ".tmp");
					}
				catch
					{
					}
				return;
				}
			}

		/// <summary>
		/// Конструктор. Создаёт компонент вопроса из нового файла
		/// </summary>
		/// <param name="ID">Уникальный номер нового компонента</param>
		/// <param name="Path">Путь к новому компоненту</param>
		/// <param name="ResourcePath">Путь к списку ресурсов, в который будет помещён компонент</param>
		public QuestionUnit (string ResourcePath, uint ID, string Path)
			{
			FileStream FS = null;
			StreamReader SR;
			bool isImage = true;
			resourcePath = ResourcePath;

			// Попытка открытия
			try
				{
				FS = new FileStream (Path, FileMode.Open);
				}
			catch
				{
				return;
				}

			// Попытка опознания изображения
			try
				{
				image = Image.FromStream (FS);
				}
			catch
				{
				isImage = false;
				}

			// Если получено изображение
			if (isImage)
				{
				Image image2 = (Image)image.Clone ();
				FS.Close ();
				image.Dispose ();
				image = image2;

				unitType = QuestionUnitTypes.Image;
				id = ID;

				return;
				}

			// Открыт текстовый файл
			if (Path.Contains (".txt"))
				{
				unitType = QuestionUnitTypes.Text;

				SR = new StreamReader (FS, Encoding.GetEncoding (1251));
				text = SR.ReadToEnd ();
				id = ID;

				SR.Close ();
				FS.Close ();
				}
			}

		/// <summary>
		/// Возвращает статус инициализации класса
		/// </summary>
		public bool IsInited
			{
			get
				{
				return ((text != "") || (image != null));
				}
			}

		/// <summary>
		/// Возвращает уникальный номер компонента вопроса
		/// </summary>
		public uint ID
			{
			get
				{
				return id;
				}
			}

		/// <summary>
		/// Возвращает текстовый фрагмент
		/// </summary>
		public string UnitText
			{
			get
				{
				return text;
				}
			}

		/// <summary>
		/// Возвращает картинку
		/// </summary>
		public Image UnitImage
			{
			get
				{
				return image;
				}
			}

		/// <summary>
		/// Возвращает тип компонента вопроса
		/// </summary>
		public QuestionUnitTypes UnitType
			{
			get
				{
				return unitType;
				}
			}

		/// <summary>
		/// Сохраняет ресурс согласно правилам работы программы
		/// </summary>
		/// <returns>Возвращает результата выполнения операции (успешно/с ошибкой)</returns>
		public bool SaveUnit ()
			{
			FileStream FS = null;
			StreamWriter SW;

			// Попытка создания файла
			try
				{
				FS = new FileStream (resourcePath + "\\" + id.ToString () + ".tmp", FileMode.Create);
				}
			catch
				{
				return false;
				}

			// Выбор варианта записи
			if (unitType == QuestionUnitTypes.Image)
				{
				try
					{
					Bitmap bmp = new Bitmap (image);
					bmp.Save (FS, ImageFormat.Png);
					}
				catch
					{
					return false;
					}

				FS.Close ();
				FileEncryptor.Encrypt (resourcePath + "\\" + id.ToString () + ".tmp", resourcePath + "\\" + id.ToString () + ".png");
				}
			else
				{
				SW = new StreamWriter (FS, Encoding.GetEncoding (1251));

				SW.Write (text);

				SW.Close ();
				FS.Close ();
				FileEncryptor.Encrypt (resourcePath + "\\" + id.ToString () + ".tmp", resourcePath + "\\" + id.ToString () + ".txt");
				}

			// Успешное завершение
			try
				{
				File.Delete (resourcePath + "\\" + ID.ToString () + ".tmp");
				}
			catch
				{
				}
			return true;
			}
		}
	}
