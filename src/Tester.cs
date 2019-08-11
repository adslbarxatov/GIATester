using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GIATesterLib;

namespace GIATester
	{
	/// <summary>
	/// Форма выполняет тестирование
	/// </summary>
	public partial class Tester:Form
		{
		private List<Question> generatedQList = new List<Question> ();	// Список сгенерированных вопросов
		private ResultsAccessor ra = null;								// Объект-аксессор результатов прохождения теста
		private ConfigAccessor ca = null;								// Объект-аксессор конфигурации программы
		private Test test = null;										// Тест для загрузки
		private int curPosition = 0;									// Текущая позиция в тесте

		/// <summary>
		/// Конструктор. Запускает прохождение теста
		/// </summary>
		/// <param name="BaseTest">Тест, используемый для формирования бланка ответов и списка вопросов</param>
		/// <param name="CA">Объект-аксессор конфигурации программы</param>
		/// <param name="KA">Объект-аксессор набора ключей</param>
		/// <param name="UserKey">Ключ тестируемого</param>
		/// <param name="UserName">Имя тестируемого</param>
		public Tester (ConfigAccessor CA, KeysAccessor KA, Test BaseTest, string UserName, string UserKey)
			{
			InitializeComponent ();

			test = BaseTest;

			// Генерация списка вопросов
			for (int i = 0; i < test.PositionsCount; i++)
				generatedQList.Add (test.GetRandomQuestion (i));

			// Инициализация
			ca = CA;
			ra = new ResultsAccessor (ca, test.Name, generatedQList, UserName, UserKey);
			if (!ra.IsInited)
				{
				MessageBox.Show ("Ошибка загрузки теста. Обратитесь к администратору системы",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Настройка
			this.Text = "ГИА тестер - " + test.Name + " - " + UserName;
			MinLabel.Text = BaseTest.TestTime.ToString ();

			StateUpdate ();

			// Запуск
			KA.DeactivateKey (UserName);	// Деактивация ключа перед началом теста
			DefTimer.Enabled = true;
			this.ShowDialog ();
			}

		// Обновление состояния контролов
		private void StateUpdate ()
			{
			// Загрузка вопроса
			QuestionDraughter.Draw (ViewZone, test, curPosition, (int)generatedQList[curPosition].Number, ca);

			// Выбор варианта приёма ответа
			switch (generatedQList[curPosition].Type)
				{
				case QuestionTypes.ClosedAnswer:
					AnswerA.Visible = true;
					AnswerB.Visible = true;
					AnswerV.Visible = true;
					AnswerG.Visible = true;

					AnswerT.Visible = false;

					switch (ra.GetQuestionAnswer (curPosition))
						{
						case "А":
							AnswerA.Checked = true;
							break;

						case "Б":
							AnswerB.Checked = true;
							break;

						case "В":
							AnswerV.Checked = true;
							break;

						case "Г":
							AnswerG.Checked = true;
							break;

						default:
							AnswerA.Checked = true;
							AnswerA.Checked = false;
							break;
						}
					break;

				case QuestionTypes.OpenAnswer:
					AnswerA.Visible = false;
					AnswerB.Visible = false;
					AnswerV.Visible = false;
					AnswerG.Visible = false;

					AnswerT.Visible = true;
					AnswerT.Text = ra.GetQuestionAnswer (curPosition);
					break;
				}
			}

		// Сохранение текущего ответа
		private void SaveCurrentAnswer ()
			{
			// Выбор варианта приёма ответа
			switch (generatedQList[curPosition].Type)
				{
				case QuestionTypes.ClosedAnswer:
					if (AnswerA.Checked)
						ra.SetQuestionAnswer (curPosition, "А");

					if (AnswerB.Checked)
						ra.SetQuestionAnswer (curPosition, "Б");

					if (AnswerV.Checked)
						ra.SetQuestionAnswer (curPosition, "В");

					if (AnswerG.Checked)
						ra.SetQuestionAnswer (curPosition, "Г");

					break;

				case QuestionTypes.OpenAnswer:
					ra.SetQuestionAnswer (curPosition, AnswerT.Text);
					break;
				}
			}

		// Следующий вопрос
		private void QNext_Click (object sender, System.EventArgs e)
			{
			// Сохранение текущего ответа
			SaveCurrentAnswer ();

			// Смена позиции
			curPosition++;
			if (curPosition == generatedQList.Count)
				curPosition = 0;

			// Обновление состояния
			StateUpdate ();
			}

		// Предыдущий вопрос
		private void QPrevious_Click (object sender, System.EventArgs e)
			{
			// Сохранение текущего ответа
			SaveCurrentAnswer ();

			// Смена позиции
			curPosition--;
			if (curPosition == -1)
				curPosition = generatedQList.Count - 1;

			// Обновление состояния
			StateUpdate ();
			}

		// Счётчик времени
		private void DefTimer_Tick (object sender, System.EventArgs e)
			{
			// Перевод секунд
			int s = int.Parse (SecLabel.Text);
			int m = int.Parse (MinLabel.Text);
			s--;

			// Перевод минут
			if (s == -1)
				{
				s = 59;
				m--;
				}

			// Проверка на завершение тестирования
			if (m == -1)
				{
				DefTimer.Enabled = false;
				MessageBox.Show ("Время тестирования истекло", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
				EndTest ();
				return;	// На всякий случай
				}

			// Перекрашивание лейблов
			MinLabel.ForeColor = DblPLabel.ForeColor = SecLabel.ForeColor =
				Color.FromArgb ((int)(255.0f * (1.0f - (float)m / (float)test.TestTime)), (int)(255.0f * (float)m / (float)test.TestTime), 0);

			// Обычный случай
			MinLabel.Text = m.ToString ();
			SecLabel.Text = s.ToString ("D2");
			}

		// Завершение тестирования
		private void EndTest ()
			{
			// Сохранение текущего ответа
			SaveCurrentAnswer ();

			if (!ra.SaveResults ())
				{
				MessageBox.Show ("Ошибка сохранения результатов тестирования. Обратитесь к администратору системы", "Ошибка",
					 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				// Переход в режим ожидания
				QNext.Enabled = false;
				QPrevious.Enabled = false;
				AnswerA.Enabled = false;
				AnswerB.Enabled = false;
				AnswerV.Enabled = false;
				AnswerG.Enabled = false;
				AnswerT.Enabled = false;

				return;
				}

			this.Close ();
			}

		// Досрочное завершение тестирования
		private void CompleteTest_Click (object sender, System.EventArgs e)
			{
			// Подтверждение
			DefTimer.Enabled = false;
			if (MessageBox.Show ("Вы действительно хотите завершить тестирование прямо сейчас? " +
				"Настоятельно рекомендуется перед завершением тестирования проверить все ответы, поскольку повторное прохождение " +
				"теста будет невозможно",
				"Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
				{
				DefTimer.Enabled = true;
				return;
				}

			EndTest ();
			}
		}
	}
