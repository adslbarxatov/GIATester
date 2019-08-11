using System.Windows.Forms;
using GIATesterLib;

namespace GIATester
	{
	/// <summary>
	/// Форма позволяет редактировать набор ключей тестируемых
	/// </summary>
	public partial class KeysRedactor:Form
		{
		private ConfigAccessor ca = null;	// Объект-аксессор конфигурации программы
		private KeysAccessor ka = null;		// Объект-аксессор набора ключей программы

		/// <summary>
		/// Конструктор. Запускает редактирование
		/// </summary>
		public KeysRedactor ()
			{
			InitializeComponent ();

			// Инициализация
			ca = new ConfigAccessor ();
			if (!ca.IsInited)
				{
				MessageBox.Show ("Ошибка загрузки конфигурации программы. Возможно, база данных программы недоступна для чтения/записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			ka = new KeysAccessor (ca);
			if (!ka.IsInited)
				{
				MessageBox.Show ("Ошибка загрузки набора ключей. Возможно, база данных программы недоступна для чтения/записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			NamesListUpdate ();

			// Настройка
			SaveKeysDialog.FileName = "";
			SaveKeysDialog.Filter = "Таблица в формате CSV (*.csv)|*.csv";
			SaveKeysDialog.Title = "Выберите файл для экспорта";

			this.ShowDialog ();
			}

		// Обновление списка имён тестируемых
		private void NamesListUpdate ()
			{
			NamesList.Items.Clear ();
			NamesList.Items.AddRange (ka.UserNames (false));
			}

		// Выход
		private void KExit_Click (object sender, System.EventArgs e)
			{
			this.Close ();
			}

		// Добавление ключа
		private void AddKey_Click (object sender, System.EventArgs e)
			{
			// Проверка
			if (UserName.Text == "")
				{
				MessageBox.Show ("Введите имя тестируемого", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Подтверждение
			if (MessageBox.Show ("Добавить ключ для тестируемого «" + UserName.Text + "»? Отменить добавление можно только путём сброса списка ключей",
				"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			if (!ka.InsertKey (UserName.Text))
				MessageBox.Show ("Ошибка сохранения набора ключей. Возможно, база данных программы недоступна для чтения/записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			UserName.Text = "";
			NamesListUpdate ();
			}

		// Очистка списка ключей
		private void ClearKeysList_Click (object sender, System.EventArgs e)
			{
			// Подтверждение
			if (MessageBox.Show ("Вы уверены, что хотите очистить список ключей? Действие нельзя будет отменить" +
				"\nПри этом действующие ключи станут недействительными и останутся таковыми даже при добавлении того же списка тестируемых",
				"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
				return;

			if (MessageBox.Show ("Внимание! Оценка результатов тестирования, созданных с этим набором ключей, станет невозможной. Продолжить?",
				"Вопрос", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				return;

			if (!ka.ClearKeyTable ())
				MessageBox.Show ("Ошибка очистки набора ключей. Возможно, база данных программы недоступна для чтения/записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			NamesListUpdate ();
			}

		// Экспорт ключей в файл
		private void ExportKeys_Click (object sender, System.EventArgs e)
			{
			// Предупреждение
			MessageBox.Show ("Внимание! С момента экспорта ключи, созданные программой, становятся потенциально уязвимыми для кражи " +
			"и использования для фальсификации результатов тестирований. " +
			"Настоятельно рекомендуется в связи с этим объяснить тестируемым важность хранения выданных им ключей в тайне до окончания срока " +
			"их действия во избежание возможных негативных последствий",
				"Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			// Отображение окна выбора файла для сохранения ключей
			SaveKeysDialog.ShowDialog ();
			if (SaveKeysDialog.FileName == "")
				return;

			if (!ka.ExportKeys (SaveKeysDialog.FileName))
				MessageBox.Show ("Ошибка сохранения таблицы ключей. Возможно, указанное место недоступно для записи",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			SaveKeysDialog.FileName = "";
			}
		}
	}
