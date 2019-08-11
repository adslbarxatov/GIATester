namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает отдельный ответ тестируемого с указанием номера вопроса
	/// в позиции теста, которому он соответствует
	/// </summary>
	public class ResultUnit
		{
		private uint qNumber = 0;	// Номер вопроса
		private string answer = "";	// Ответ

		/// <summary>
		/// Конструктор. Создаёт пустой ответ и привязывает его к конкретному вопросу в позиции
		/// </summary>
		/// <param name="QuestionNumber">Номер вопроса в позиции теста</param>
		public ResultUnit (uint QuestionNumber)
			{
			qNumber = QuestionNumber;
			}

		/// <summary>
		/// Возвращает номер вопроса в позиции, которому соответствует ответ
		/// </summary>
		public uint QuestionNumber
			{
			get
				{
				return qNumber;
				}
			}

		/// <summary>
		/// Возвращает или задаёт текст ответа
		/// </summary>
		public string Answer
			{
			get
				{
				return answer;
				}
			set
				{
				answer = value;
				}
			}
		}
	}
