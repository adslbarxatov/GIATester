using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GIATesterLib;

namespace GIATester
	{
	/// <summary>
	/// Форма позволяет редактировать тесты
	/// </summary>
	public partial class TestRedactor:Form
		{
		private Test test = null;
		private int position = 0, question = 0;		// Последние выбранные позиции в полях позиции и вопроса
		private bool updating = false;
		private ConfigAccessor ca = null;

		/// <summary>
		/// Конструктор. Создаёт новый тест
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		public TestRedactor (ConfigAccessor CA)
			{
			InitializeComponent ();

			test = new Test ("Тест 1", "", 15);
			ca = CA;

			InitializeForm ();
			}

		/// <summary>
		/// Конструктор. Редактирует ранее созданный тест
		/// </summary>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="TestName">Имя теста для редактирования</param>
		public TestRedactor (ConfigAccessor CA, string TestName)
			{
			InitializeComponent ();
			ca = CA;

			// Загрузка теста
			TestAccessor ta = new TestAccessor (ca);
			test = ta.LoadTest (TestName);
			if (!ta.Success)
				MessageBox.Show ("Ошибка загрузки теста: " + ta.ErrorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			InitializeForm ();
			}

		// Загрузка формы до её отображения
		private void InitializeForm ()
			{
			// Заполнение неизменяющихся списков
			QuesType.Items.Add ("Вопрос закрытого типа");
			QuesType.Items.Add ("Вопрос открытого типа");

			// Настройка информационных полей
			TestName.Text = test.Name;
			TestDescr.Text = test.Description;
			TestTime.Value = test.TestTime;

			StateUpdate ();

			this.ShowDialog ();
			}

		// Обновление состояния контролов
		private void StateUpdate ()
			{
			// Исключение рекурсии при изменении состояния полей
			if (updating)
				return;
			else
				updating = true;

			// Настройка поля списка полей теста
			PosNumber.Items.Clear ();
			for (uint i = 0; i < test.PositionsCount; i++)
				PosNumber.Items.Add ((i + 1).ToString ());

			if (test.PositionsCount != 0)
				{
				while (position >= test.PositionsCount)
					position--;

				PosNumber.Text = PosNumber.Items[position].ToString ();

				// Настройка поля списка вариантов вопроса
				QuesNumber.Items.Clear ();
				for (uint i = 0; i < test.GetQuestionsAtPosition (position).Count; i++)
					QuesNumber.Items.Add ((i + 1).ToString ());

				if (test.GetQuestionsAtPosition (position).Count != 0)
					{
					while (question >= test.GetQuestionsAtPosition (position).Count)
						question--;

					QuesNumber.Text = QuesNumber.Items[question].ToString ();

					// Настройка поля списка компонентов вопроса
					QuesType.SelectedIndex = (int)test.GetQuestionsAtPosition (position)[question].Type;

					ResList.Text = "";
					List<uint> units = test.GetQuestionsAtPosition (position)[question].GetUnitsList ();
					for (int i = 0; i < units.Count; i++)
						ResList.Text += units[i].ToString () + " ";
					ResList.Text = ResList.Text.Trim ().Replace (" ", ";");

					QuesAnswer.Text = test.GetQuestionsAtPosition (position)[question].Answer;

					// Загрузка зоны preview
					QuestionDraughter.Draw (QuesPreview, test, position, question, ca);
					}
				}

			// Обновление по признаку числа позиций
			switch (test.PositionsCount)
				{
				case 0:
					PosDelete.Enabled = false;
					PosUp.Enabled = false;
					PosDown.Enabled = false;

					QuesNumber.Enabled = false;
					QuesAdd.Enabled = false;
					QuesDelete.Enabled = false;

					QuesType.Enabled = false;
					ResList.Enabled = false;
					ResAdd.Enabled = false;
					ResClear.Enabled = false;

					updating = false;
					return;

				case 1:
					PosDelete.Enabled = false;
					PosUp.Enabled = false;
					PosDown.Enabled = false;

					QuesNumber.Enabled = true;
					QuesAdd.Enabled = true;
					break;

				default:
					PosDelete.Enabled = true;
					PosUp.Enabled = true;
					PosDown.Enabled = true;

					QuesNumber.Enabled = true;
					QuesAdd.Enabled = true;
					break;
				}

			// Обновление по признаку числа вариантов вопроса
			switch (test.GetQuestionsAtPosition (PosNumber.SelectedIndex).Count)
				{
				case 0:
					QuesDelete.Enabled = false;

					QuesType.Enabled = false;
					ResList.Enabled = false;
					ResAdd.Enabled = false;
					ResClear.Enabled = false;

					updating = false;
					return;

				case 1:
					QuesDelete.Enabled = false;

					QuesType.Enabled = true;
					ResList.Enabled = true;
					ResAdd.Enabled = true;
					ResClear.Enabled = true;
					break;

				default:
					QuesDelete.Enabled = true;

					QuesType.Enabled = true;
					ResList.Enabled = true;
					ResAdd.Enabled = true;
					ResClear.Enabled = true;
					break;
				}

			// Завершение
			updating = false;
			}

		// Изменение выбранной позиции
		private void PosNumber_SelectedIndexChanged (object sender, EventArgs e)
			{
			position = PosNumber.SelectedIndex;

			StateUpdate ();
			}

		// Добавление позиции в тест
		private void PosAdd_Click (object sender, EventArgs e)
			{
			position = PosNumber.Items.Count;
			test.AddQuestion ((int)test.PositionsCount, new Question (0, ca.TestsPath + "\\" + test.Name, QuestionTypes.ClosedAnswer, ""));
			PosNumber.Items.Add (PosNumber.Items.Count);

			StateUpdate ();
			}

		// Удаление позиции из текста
		private void PosDelete_Click (object sender, EventArgs e)
			{
			if (MessageBox.Show ("Вы действительно хотите удалить выбранный вопрос из теста? Все записи о вариантах этого вопроса будут также удалены",
				"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{

				test.DeletePosition (PosNumber.SelectedIndex);
				PosNumber.Items.RemoveAt (PosNumber.SelectedIndex);

				StateUpdate ();
				}
			}

		// Подъём вопроса на позицию вверх
		private void PosUp_Click (object sender, EventArgs e)
			{
			if (position != 0)
				{
				List<Question> a = test.GetQuestionsAtPosition (position);
				List<Question> b = test.GetQuestionsAtPosition (position - 1);
				test.SetQuestionsAtPosition (b, position);
				test.SetQuestionsAtPosition (a, position - 1);
				position--;

				StateUpdate ();
				}
			}

		// Спуск вопроса на позицию вниз
		private void PosDown_Click (object sender, EventArgs e)
			{
			if (position != test.PositionsCount - 1)
				{
				List<Question> a = test.GetQuestionsAtPosition (position);
				List<Question> b = test.GetQuestionsAtPosition (position + 1);
				test.SetQuestionsAtPosition (b, position);
				test.SetQuestionsAtPosition (a, position + 1);
				position++;

				StateUpdate ();
				}
			}

		// Изменение выбранного вопроса
		private void QuesNumber_SelectedIndexChanged (object sender, EventArgs e)
			{
			question = QuesNumber.SelectedIndex;

			StateUpdate ();
			}

		// Добавление вопроса в позицию
		private void QuesAdd_Click (object sender, EventArgs e)
			{
			question = QuesNumber.Items.Count;
			test.AddQuestion (PosNumber.SelectedIndex, new Question ((uint)question, ca.TestsPath + "\\" + test.Name, QuestionTypes.ClosedAnswer, ""));
			QuesNumber.Items.Add (QuesNumber.Items.Count);

			StateUpdate ();
			}

		// Удаление вопроса из позиции
		private void QuesDelete_Click (object sender, EventArgs e)
			{
			if (MessageBox.Show ("Вы действительно хотите удалить выбранный вариант вопроса из теста?",
				"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{

				test.DeleteQuestion (PosNumber.SelectedIndex, QuesNumber.SelectedIndex);
				QuesNumber.Items.RemoveAt (QuesNumber.SelectedIndex);

				StateUpdate ();
				}
			}

		// Изменение типа вопроса
		private void QuesType_SelectedIndexChanged (object sender, EventArgs e)
			{
			List<Question> lq = test.GetQuestionsAtPosition (PosNumber.SelectedIndex);
			lq[QuesNumber.SelectedIndex].Type = (QuestionTypes)QuesType.SelectedIndex;
			test.SetQuestionsAtPosition (lq, PosNumber.SelectedIndex);

			// StateUpdate ();	// Необязательно
			}

		// Добавление компонента (ресурса) к вопросу
		private void ResAdd_Click (object sender, EventArgs e)
			{
			ResourceManager rm = new ResourceManager (test.Name, ca);

			List<Question> lq = test.GetQuestionsAtPosition (PosNumber.SelectedIndex);
			lq[QuesNumber.SelectedIndex].Add (rm.SelectedResource);
			test.SetQuestionsAtPosition (lq, PosNumber.SelectedIndex);

			StateUpdate ();
			}

		// Очистка списка компонентов вопроса
		private void ResClear_Click (object sender, EventArgs e)
			{
			List<Question> lq = test.GetQuestionsAtPosition (PosNumber.SelectedIndex);
			lq[QuesNumber.SelectedIndex].Clear ();
			test.SetQuestionsAtPosition (lq, PosNumber.SelectedIndex);

			StateUpdate ();
			}

		// Проверка на непустоту поля названия
		private void TestName_TextChanged (object sender, EventArgs e)
			{
			if (TestName.Text == "")
				TestName.Text = "";
			}

		// Сохранение теста
		private void SaveTest_Click (object sender, EventArgs e)
			{
			// Проверка
			if (TestName.Text == "")
				{
				MessageBox.Show ("Поле названия теста не может быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if (test.PositionsCount == 0)
				{
				MessageBox.Show ("В тесте нет ни одной позиции", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			test.Name = TestName.Text;
			test.Description = TestDescr.Text;
			test.TestTime = (uint)TestTime.Value;

			// Подтверждение
			if (MessageBox.Show ("Сохранить внесённые изменения?", "Вопрос", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
				return;

			// Проверка на существование файла
			TestAccessor ta = new TestAccessor (ca);
			if (ta.Exists (test.Name))
				if (MessageBox.Show ("Тест с именем «" + test.Name + "» уже существует. Переписать его с учётом внесённых изменений?",
					"Вопрос", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
					return;

			// Запись
			ta.SaveTest (test);
			if (!ta.Success)
				{
				MessageBox.Show ("Ошибка сохранения: " + ta.ErrorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Успешно
			this.Close ();
			}

		// Отмена внесённых изменений
		private void AbortTest_Click (object sender, EventArgs e)
			{
			// Подтверждение
			if (MessageBox.Show ("Вы действительно хотите отменить все внесённые изменения? Все изменения будут утеряны",
				"Вопрос", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
				return;

			// Верно
			this.Close ();
			}

		// Изменение ответа на вопрос
		private void QuesAnswer_TextChanged (object sender, EventArgs e)
			{
			List<Question> lq = test.GetQuestionsAtPosition (PosNumber.SelectedIndex);

			if ((lq[QuesNumber.SelectedIndex].Type == QuestionTypes.ClosedAnswer) &&
				(QuesAnswer.Text != "А") && (QuesAnswer.Text != "Б") && (QuesAnswer.Text != "В") &&
				(QuesAnswer.Text != "Г") && (QuesAnswer.Text != ""))
				{
				MessageBox.Show ("Ответом на вопрос закрытого типа может быть только одна из букв А, Б, В или Г",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				QuesAnswer.Text = "";
				return;
				}

			lq[QuesNumber.SelectedIndex].Answer = QuesAnswer.Text;
			test.SetQuestionsAtPosition (lq, PosNumber.SelectedIndex);

			// StateUpdate ();	// Необязательно
			}
		}
	}
