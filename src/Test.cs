using System;
using System.Collections.Generic;

namespace GIATesterLib
	{
	/// <summary>
	/// Класс описывает отдельный тест
	/// </summary>
	public class Test
		{
		private List<List<Question>> qlist = new List<List<Question>> ();	// Список вопросов
		private string name, description;									// Описание теста
		private uint time;													// Длительность тестирования
		private Random rnd = new Random ();									// ГПСЧ

		/// <summary>
		/// Конструктор. Создаёт объект-список вопросов
		/// </summary>
		/// <param name="Description">Описание теста</param>
		/// <param name="Name">Название теста</param>
		/// <param name="TestTime">Время тестирования</param>
		public Test (string Name, string Description, uint TestTime)
			{
			time = TestTime;
			name = Name;
			description = Description;
			}

		/// <summary>
		/// Возвращает или задаёт название теста
		/// </summary>
		public string Name
			{
			get
				{
				return name;
				}
			set
				{
				name = value;
				}
			}

		/// <summary>
		/// Возвращает или задаёт описание теста
		/// </summary>
		public string Description
			{
			get
				{
				return description;
				}
			set
				{
				description = value;
				}
			}

		/// <summary>
		/// Возвращает или задаёт время прохождения теста (в минутах)
		/// </summary>
		public uint TestTime
			{
			get
				{
				return time;
				}
			set
				{
				time = value;
				}
			}

		/// <summary>
		/// Добавляет вопрос в указанную позицию списка
		/// </summary>
		/// <param name="PositionInList">Позиция вопроса в списке</param>
		/// <param name="SomeQuestion">Объект-вопрос для добавления</param>
		/// <returns>Возвращает статус добавления (выполнено/не выполнено)</returns>
		public bool AddQuestion (int PositionInList, Question SomeQuestion)
			// Список вопросов поддерживает случайный выбор из группы вопросов
			// в рамках одного пункта анкеты
			{
			if ((PositionInList < 0) || (PositionInList > qlist.Count))
				{
				return false;
				}

			if (PositionInList == qlist.Count)
				{
				qlist.Add (new List<Question> ());
				}
			qlist[PositionInList].Add (SomeQuestion);

			return true;
			}

		/// <summary>
		/// Удаляет указанную позицию из списка
		/// </summary>
		/// <param name="PositionInList">Позиция в списке</param>
		/// <returns>Возвращает статус удаления (выполнено/не выполнено)</returns>
		public bool DeletePosition (int PositionInList)
			{
			if ((PositionInList < 0) || (PositionInList > qlist.Count))
				{
				return false;
				}

			qlist.RemoveAt (PositionInList);

			return true;
			}

		/// <summary>
		/// Очищает указанную позицию в списке
		/// !Метод не используется. Реализация не выполнена!
		/// </summary>
		/// <param name="PositionInList">Позиция в списке</param>
		/// <param name="QuestionInPosition">Номер вопроса в позиции</param>
		/// <returns>Возвращает статус очистки (выполнена/не выполнена)</returns>
		public bool DeleteQuestion (int PositionInList, int QuestionInPosition)
			{
			if ((PositionInList < 0) || (PositionInList > qlist.Count))
				{
				return false;
				}

			if ((QuestionInPosition < 0) || (QuestionInPosition > qlist[PositionInList].Count))
				{
				return false;
				}

			qlist[PositionInList].RemoveAt (QuestionInPosition);
			return true;
			}

		/// <summary>
		/// Возвращает случайный вопрос из указанной позиции
		/// </summary>
		/// <param name="PositionInList">Позиция в тесте</param>
		/// <returns>Возвращает случайный объект-вопрос из указанной позиции теста</returns>
		public Question GetRandomQuestion (int PositionInList)
			{
			if ((PositionInList < 0) || (PositionInList >= qlist.Count))
				{
				return null;
				}

			return qlist[PositionInList][rnd.Next (qlist[PositionInList].Count)];
			}

		/// <summary>
		/// Возвращает число позиций в списке вопросов теста
		/// </summary>
		public uint PositionsCount
			{
			get
				{
				return (uint)qlist.Count;
				}
			}

		/// <summary>
		/// Возвращает список вопросов в данной позиции
		/// </summary>
		/// <param name="Position">Позиция в тесте, для которой требуется получить список вопросов</param>
		/// <returns>Список вопросов</returns>
		public List<Question> GetQuestionsAtPosition (int Position)
			{
			if ((Position < 0) || (Position >= qlist.Count))
				{
				return new List<Question> ();
				}

			return qlist[Position];
			}

		/// <summary>
		/// Устанавливает список вопросов для данной позиции
		/// </summary>
		/// <param name="Position">Позиция в тесте, в которой требуется установить список вопросов</param>
		/// <param name="QList">Устанавливаемый список</param>
		public void SetQuestionsAtPosition (List<Question> QList, int Position)
			{
			if ((Position < 0) || (Position >= qlist.Count))
				{
				return;
				}

			qlist[Position] = QList;
			}
		}
	}
