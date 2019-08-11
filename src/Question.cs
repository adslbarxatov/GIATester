using System.Collections.Generic;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает отдельный вопрос (набор входящих в него ресурсов)
	/// </summary>
	public class Question
		{
		private uint numberInPosition;					// Идентификационный номер вопроса в позиции
		private string resourcePath;					// Название теста
		private QuestionTypes type;						// Тип вопроса
		private string answer;							// Ответ на вопрос
		private List<uint> units = new List<uint> ();	// Список компонентов данного вопроса

		/// <summary>
		/// Конструктор. Создаёт объект-вопрос
		/// </summary>
		/// <param name="NumberInPosition">Номер вопроса в группе вопросов одной позиции</param>
		/// <param name="ResourcePath">Путь к списку ресурсов</param>
		/// <param name="Type">Тип вопроса</param>
		/// <param name="Answer">Ответ на вопрос</param>
		public Question (uint NumberInPosition, string ResourcePath, QuestionTypes Type, string Answer)
			{
			numberInPosition = NumberInPosition;
			resourcePath = ResourcePath;
			type = Type;
			answer = Answer;
			}

		/// <summary>
		/// Добавляет компонент (ресурс) в вопрос
		/// </summary>
		/// <param name="ID">Уникальный номер компонента</param>
		/// <returns>Возвращает результат выполнения операции</returns>
		public bool Add (uint ID)
			{
			QuestionUnit qu = new QuestionUnit (resourcePath, ID);

			if (qu.IsInited)
				{
				units.Add (ID);
				return true;
				}
			else
				{
				return false;
				}
			}

		/// <summary>
		/// Очищает список компонентов вопроса
		/// </summary>
		public void Clear ()
			{
			units.Clear ();
			}

		/// <summary>
		/// Возвращает тип вопроса
		/// </summary>
		public QuestionTypes Type
			{
			get
				{
				return type;
				}
			set
				{
				type = value;
				}
			}

		/// <summary>
		/// Возвращает ответ на вопрос
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

		/// <summary>
		/// Возвращает номер вопроса в позиции
		/// </summary>
		public uint Number
			{
			get
				{
				return numberInPosition;
				}
			}

		/// <summary>
		/// Возвращает список компонентов (ресурсов) вопроса
		/// </summary>
		/// <returns>Возвращает набор компонентов</returns>
		public List<uint> GetUnitsList ()
			{
			return units;
			}
		}
	}
