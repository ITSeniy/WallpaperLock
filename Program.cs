using System;
using System.Windows.Forms;

namespace WallpaperLockDemo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Показываем предупреждение при запуске
            var startupWarning = MessageBox.Show(
                "ОБРАЗОВАТЕЛЬНАЯ ПРОГРАММА\n\n" +
                "Эта программа создана для демонстрации поведения потенциально нежелательного ПО " +
                "в образовательных целях.\n\n" +
                "Программа будет:\n" +
                "• Спрашивать разрешение перед каждым действием\n" +
                "• Объяснять что именно она делает\n" +
                "• Предоставлять возможность отмены всех изменений\n\n" +
                "Продолжить?",
                "Образовательная демонстрация",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (startupWarning == DialogResult.Yes)
            {
                Application.Run(new Form1());
            }
        }
    }
}