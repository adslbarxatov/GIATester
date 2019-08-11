namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает ключ доступа тестируемого ученика
	/// </summary>
	public class UserKey
		// Видимо, функции изменения имени пользователя или его ключа бессмысленны
		{
		private string userName = "";
		private string userKey = "";
		private bool isActive = true;

		/// <summary>
		/// Возвращает имя пользователя, которому сопоставлен ключ
		/// </summary>
		public string UserName
			{
			get
				{
				return userName;
				}
			}

		/// <summary>
		/// Возвращает значение ключа
		/// </summary>
		public string UserKeyValue
			{
			get
				{
				return userKey;
				}
			}

		/// <summary>
		/// Конструктор. Создаёт объект-ключ
		/// </summary>
		/// <param name="Key">Значение ключа</param>
		/// <param name="Name">Имя пользователя, которому сопоставлен ключ</param>
		/// <param name="Status">Статус ключа (активен/неактивен)</param>
		public UserKey (string Name, string Key, bool Status)
			{
			userKey = Key;
			userName = Name;
			isActive = Status;
			}

		/// <summary>
		/// Деактивирует ключ
		/// </summary>
		public void DeactivateKey ()
			{
			isActive = false;
			}

		/// <summary>
		/// Возвращает статус ключа
		/// </summary>
		public bool IsActive
			{
			get
				{
				return isActive;
				}
			}
		}
	}
