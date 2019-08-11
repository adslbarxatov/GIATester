using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace GIATester
	{
	static class Program
		{
		/// <summary>
		/// Главная точка входа для приложения
		/// </summary>
		[STAThread]
		static void Main ()
			{
			// Проверка запуска единственной копии
			bool result;
			Mutex instance = new Mutex (true, ProgramDescription.AssemblyTitle, out result);
			if (!result)
				{
				MessageBox.Show ("Программа " + ProgramDescription.AssemblyTitle + " уже запущена",
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Проверка наличия компонентов программы
			for (int i = 0; i < ProgramDescription.CriticalComponents.Length; i++)
				{
				if (!File.Exists (Application.StartupPath + "\\" + ProgramDescription.CriticalComponents[i]))
					{
					MessageBox.Show ("Отсутствует необходимый компонент программы: " + ProgramDescription.CriticalComponents[i] +
						". Переустановите программу, после чего повторите попытку", ProgramDescription.AssemblyTitle, MessageBoxButtons.OK,
						 MessageBoxIcon.Error);
					return;
					}
				}

			// Запуск
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new MainForm ());
			}
		}
	}
