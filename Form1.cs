using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WallpaperLockDemo
{
    public partial class Form1 : Form
    {
        // API для изменения обоев рабочего стола
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private string originalWallpaper;
        private string tempWallpaperPath;
        private bool wallpaperLocked = false;

        public Form1()
        {
            SetupUI();
            this.Text = "Демонстрация изменения обоев (Образовательная программа)";
            this.Size = new Size(600, 400);

            // Сохраняем текущие обои
            SaveCurrentWallpaper();

            // Создаем временный файл с демонстрационными обоями
            CreateDemoWallpaper();
        }

        private void SetupUI()
        {
            var mainLabel = new Label
            {
                Text = "Образовательная демонстрация поведения программ, изменяющих обои",
                Location = new Point(20, 20),
                Size = new Size(550, 60),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            var descLabel = new Label
            {
                Text = "Эта программа демонстрирует:\n" +
                       "1. Программное изменение обоев рабочего стола\n" +
                       "2. Блокировку возможности изменения обоев пользователем\n" +
                       "3. Использование RunAsInvoker для работы без админ-прав\n\n" +
                       "ВНИМАНИЕ: Каждое действие требует вашего разрешения!",
                Location = new Point(20, 80),
                Size = new Size(550, 120),
                Font = new Font("Arial", 10)
            };

            var changeWallpaperBtn = new Button
            {
                Text = "1. Изменить обои",
                Location = new Point(20, 220),
                Size = new Size(200, 40),
                BackColor = Color.LightBlue
            };
            changeWallpaperBtn.Click += ChangeWallpaperBtn_Click;

            var lockWallpaperBtn = new Button
            {
                Text = "2. Заблокировать изменение обоев",
                Location = new Point(240, 220),
                Size = new Size(200, 40),
                BackColor = Color.Orange
            };
            lockWallpaperBtn.Click += LockWallpaperBtn_Click;

            var restoreBtn = new Button
            {
                Text = "Восстановить всё",
                Location = new Point(20, 280),
                Size = new Size(200, 40),
                BackColor = Color.LightGreen
            };
            restoreBtn.Click += RestoreBtn_Click;

            var exitBtn = new Button
            {
                Text = "Выход",
                Location = new Point(240, 280),
                Size = new Size(200, 40),
                BackColor = Color.LightCoral
            };
            exitBtn.Click += (s, e) => Application.Exit();

            var statusLabel = new Label
            {
                Name = "statusLabel",
                Text = "Статус: Готов к работе",
                Location = new Point(20, 330),
                Size = new Size(400, 20),
                Font = new Font("Arial", 9, FontStyle.Italic)
            };

            this.Controls.AddRange(new Control[] {
                mainLabel, descLabel, changeWallpaperBtn,
                lockWallpaperBtn, restoreBtn, exitBtn, statusLabel
            });
        }

        private void SaveCurrentWallpaper()
        {
            try
            {
                // Получаем путь к текущим обоям из реестра
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    originalWallpaper = key?.GetValue("Wallpaper")?.ToString();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка сохранения текущих обоев: {ex.Message}");
            }
        }

        private void CreateDemoWallpaper()
        {
            try
            {
                // Создаем простое изображение для демонстрации
                tempWallpaperPath = Path.Combine(Path.GetTempPath(), "demo_wallpaper.bmp");

                using (var bitmap = new Bitmap(800, 600))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.DarkRed);
                    graphics.DrawString("ДЕМОНСТРАЦИОННЫЕ ОБОИ",
                        new Font("Arial", 24, FontStyle.Bold),
                        Brushes.White, new PointF(200, 250));
                    graphics.DrawString("Образовательная программа",
                        new Font("Arial", 16),
                        Brushes.Yellow, new PointF(250, 300));

                    bitmap.Save(tempWallpaperPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка создания демо-обоев: {ex.Message}");
            }
        }

        private void ChangeWallpaperBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Программа хочет изменить обои рабочего стола на демонстрационное изображение.\n\n" +
                "Действие: Вызов SystemParametersInfo(SPI_SETDESKWALLPAPER)\n" +
                "Цель: Установить новые обои рабочего стола\n\n" +
                "Разрешить выполнить это действие?",
                "Запрос разрешения - Изменение обоев",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Изменяем обои используя Windows API
                    int result_code = SystemParametersInfo(
                        SPI_SETDESKWALLPAPER,
                        0,
                        tempWallpaperPath,
                        SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                    if (result_code != 0)
                    {
                        UpdateStatus("✓ Обои успешно изменены");
                        MessageBox.Show("Обои рабочего стола изменены на демонстрационное изображение.",
                                      "Действие выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        UpdateStatus("✗ Ошибка изменения обоев");
                        MessageBox.Show("Не удалось изменить обои рабочего стола.",
                                      "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"✗ Исключение: {ex.Message}");
                }
            }
            else
            {
                UpdateStatus("Изменение обоев отменено пользователем");
            }
        }

        private void LockWallpaperBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Программа хочет заблокировать возможность изменения обоев рабочего стола.\n\n" +
                "Действие: Создание ключа реестра NoChangingWallPaper\n" +
                "Расположение: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\ActiveDesktop\n" +
                "Эффект: Пользователь не сможет изменить обои через настройки Windows\n\n" +
                "ВНИМАНИЕ: Это действие изменит системный реестр!\n\n" +
                "Разрешить выполнить это действие?",
                "Запрос разрешения - Блокировка изменения обоев",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Создаем ключ реестра для блокировки изменения обоев
                    var keyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies";
                    using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
                    {
                        using (var activeDesktopKey = key.CreateSubKey("ActiveDesktop"))
                        {
                            activeDesktopKey.SetValue("NoChangingWallPaper", 1, RegistryValueKind.DWord);
                        }
                    }

                    wallpaperLocked = true;
                    UpdateStatus("✓ Изменение обоев заблокировано");
                    MessageBox.Show(
                        "Изменение обоев заблокировано!\n\n" +
                        "Теперь пользователь не сможет изменить обои через:\n" +
                        "• Настройки Windows\n" +
                        "• Контекстное меню \"Установить как фон рабочего стола\"\n" +
                        "• Панель управления\n\n" +
                        "Для восстановления используйте кнопку \"Восстановить всё\"",
                        "Блокировка активирована",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UpdateStatus($"✗ Ошибка блокировки: {ex.Message}");
                    MessageBox.Show($"Не удалось заблокировать изменение обоев: {ex.Message}",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                UpdateStatus("Блокировка отменена пользователем");
            }
        }

        private void RestoreBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Программа хочет восстановить первоначальные настройки:\n\n" +
                "• Восстановить оригинальные обои рабочего стола\n" +
                "• Удалить блокировку изменения обоев из реестра\n" +
                "• Удалить временные файлы\n\n" +
                "Выполнить восстановление?",
                "Подтверждение восстановления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Восстанавливаем оригинальные обои
                    if (!string.IsNullOrEmpty(originalWallpaper) && File.Exists(originalWallpaper))
                    {
                        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, originalWallpaper,
                                           SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                    }

                    // Удаляем блокировку из реестра
                    try
                    {
                        var keyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop";
                        using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
                        {
                            if (key != null)
                            {
                                key.DeleteValue("NoChangingWallPaper", false);
                            }
                        }
                    }
                    catch
                    {
                        // Ключ может не существовать
                    }

                    // Удаляем временный файл
                    if (File.Exists(tempWallpaperPath))
                    {
                        File.Delete(tempWallpaperPath);
                    }

                    wallpaperLocked = false;
                    UpdateStatus("✓ Всё восстановлено");
                    MessageBox.Show("Все изменения успешно отменены!\n\n" +
                                  "• Обои восстановлены\n" +
                                  "• Блокировка снята\n" +
                                  "• Временные файлы удалены",
                                  "Восстановление завершено",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UpdateStatus($"✗ Ошибка восстановления: {ex.Message}");
                }
            }
        }

        private void UpdateStatus(string message)
        {
            var statusLabel = this.Controls.Find("statusLabel", false)[0] as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = $"Статус: {message}";
                statusLabel.Refresh();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Предупреждаем о необходимости восстановления
            if (wallpaperLocked)
            {
                var result = MessageBox.Show(
                    "ВНИМАНИЕ! Блокировка изменения обоев всё ещё активна.\n\n" +
                    "Рекомендуется сначала восстановить настройки кнопкой \"Восстановить всё\".\n\n" +
                    "Всё равно закрыть программу?",
                    "Предупреждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }
    }
}