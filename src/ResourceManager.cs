using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GIATesterLib;

namespace GIATester
	{
	/// <summary>
	/// Форма позволяет редактировать список ресурсов теста
	/// </summary>
	public partial class ResourceManager:Form
		{
		private string testName = "";
		private ConfigAccessor ca = null;
		private uint newName = 1;

		/// <summary>
		/// Возвращает номер выбранного ресурса
		/// </summary>
		public uint SelectedResource
			{
			get
				{
				return selectedRes;
				}
			}
		private uint selectedRes = 0;

		/// <summary>
		/// Конструктор. Запускает редактирование списка ресурсов
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="TestName">Название теста для загрузки ресурсов</param>
		public ResourceManager (string TestName, ConfigAccessor CA)
			{
			InitializeComponent ();

			testName = TestName;
			ca = CA;

			this.ShowDialog ();
			}

		// Запуск формы
		private void ResourceManager_Load (object sender, EventArgs e)
			{
			StateUpdate ();

			OpenRes.FileName = "";
			OpenRes.Filter = "Все поддерживаемые форматы|*.bmp;*.gif;*.jpe;*.jpeg;*.jpg;*.jfif;*.png;*.txt|" +
				"Текстовые файлы (*.txt)|*.txt|" +
				"Изображения (*.bmp, *.gif, *.jpe, *.jpeg, *.jpg, *.jfif, *.png)|*.bmp;*.gif;*.jpe;*.jpeg;*.jpg;*.jfif;*.png";
			OpenRes.Title = "Выберите файл для добавления в качестве компонента";
			}

		// Обновление состояния контролов
		private void StateUpdate ()
			{
			// Загрузка списка ресурсов
			List<string> files = new List<string> ();

			if (!Directory.Exists (ca.TestsPath + "\\" + testName))
				Directory.CreateDirectory (ca.TestsPath + "\\" + testName);

			files.AddRange (Directory.GetFiles (ca.TestsPath + "\\" + testName, "*.txt"));
			files.AddRange (Directory.GetFiles (ca.TestsPath + "\\" + testName, "*.png"));

			ResNumber.Items.Clear ();
			for (int i = 0; i < files.Count; i++)
				{
				string[] strs = files[i].Split (new char[] { '.', '\\' }, StringSplitOptions.RemoveEmptyEntries);
				uint p;
				if (uint.TryParse (strs[strs.Length - 2], out p))	// Защита от подмены ресурсов
					{
					ResNumber.Items.Add (strs[strs.Length - 2]);

					if (uint.Parse (strs[strs.Length - 2]) >= newName)	// Определение имени для нового ресурса
						newName = uint.Parse (strs[strs.Length - 2]) + 1;
					}
				}

			// Дополнительная настройка
			if (files.Count != 0)
				{
				ResNumber.Text = ResNumber.Items[0].ToString ();
				ResDelete.Enabled = true;
				ResSelect.Enabled = true;
				}
			else
				{
				ResDelete.Enabled = false;
				ResSelect.Enabled = false;
				}
			}

		// Добавление ресурса
		private void ResAdd_Click (object sender, EventArgs e)
			{
			// Попытка добавления ресурса
			while (OpenRes.FileName == "")
				{
				OpenRes.ShowDialog ();
				if (OpenRes.FileName == "")
					return;

				// Определение ресурса
				QuestionUnit qu = new QuestionUnit (ca.TestsPath + "\\" + testName, newName, OpenRes.FileName);
				if (!qu.IsInited)
					{
					MessageBox.Show ("Выбранный файл не поддерживается, является пустым или повреждён", "Ошибка", MessageBoxButtons.OK,
						 MessageBoxIcon.Exclamation);
					OpenRes.FileName = "";
					}

				if (!qu.SaveUnit ())
					{
					MessageBox.Show ("Ошибка сохранения ресурса. Возможно, у вас недостаточно прав для выполнения операции",
						"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					OpenRes.FileName = "";
					}

				// Выбор нового имени
				newName++;
				}

			// Выбор в случае успеха
			OpenRes.FileName = "";
			StateUpdate ();
			ResNumber.SelectedIndex = ResNumber.Items.IndexOf ((newName - 1).ToString ());
			}

		// Инменение выбранного ресурса
		private void ResNumber_SelectedIndexChanged (object sender, EventArgs e)
			{
			QuestionUnit qu = new QuestionUnit (ca.TestsPath + "\\" + testName, uint.Parse (ResNumber.Text));

			if (!qu.IsInited)
				{
				ResText.Visible = true;
				ResImage.Visible = false;
				ResText.Text = "<Ресурс №" + ResNumber.Text + " пуст, недоступен или повреждён>";
				}
			else
				{
				switch (qu.UnitType)
					{
					case QuestionUnitTypes.Text:
						ResText.Visible = true;
						ResImage.Visible = false;
						ResText.Text = qu.UnitText;
						break;

					case QuestionUnitTypes.Image:
						ResText.Visible = false;
						ResImage.Visible = true;
						ResImage.BackgroundImage = qu.UnitImage;
						break;
					}
				}
			}

		// Выбор ресурса
		private void ResSelect_Click (object sender, EventArgs e)
			{
			selectedRes = uint.Parse (ResNumber.Text);
			this.Close ();
			}

		// Отмена выбора
		private void ResAbort_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Удаление ресурса
		private void ResDelete_Click (object sender, EventArgs e)
			{
			if (MessageBox.Show ("Вы действительно хотите удалить ресурс №" + ResNumber.Text + "?", "Вопрос",
				 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{
				File.Delete (ca.TestsPath + "\\" + testName + "\\" + ResNumber.Text + ".txt");
				File.Delete (ca.TestsPath + "\\" + testName + "\\" + ResNumber.Text + ".png");
				StateUpdate ();
				}
			}
		}
	}
