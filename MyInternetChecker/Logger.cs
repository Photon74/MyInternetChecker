using System;
using System.IO;

namespace MyInternetChecker
{
    /// <summary>
    /// Простой логгер для записи ошибок в файл
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();

        static Logger()
        {
            // Получаем путь к папке с настройками (такой же, как в SettingsLoader)
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "MyInternetChecker");

            // Создаем путь к файлу логов (в той же папке)
            LogFilePath = Path.Combine(appFolder, "error.log");
        }

        /// <summary>
        /// Записывает ошибку в лог-файл
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public static void LogError(string message)
        {
            try
            {
                lock (LockObject) // Блокировка для многопоточности
                {
                    // Создаем папку, если ее нет
                    Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));

                    // Формируем строку для записи
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}\n";

                    // Записываем в файл (дописываем в конец)
                    File.AppendAllText(LogFilePath, logEntry);
                }
            }
            catch (Exception ex)
            {
                // Если не удалось записать в лог, выводим в консоль (или игнорируем)
                Console.WriteLine($"Не удалось записать в лог: {ex.Message}");
            }
        }

        /// <summary>
        /// Записывает ошибку в лог-файл с информацией об исключении
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <param name="context">Контекст, где произошла ошибка (необязательно)</param>
        public static void LogError(Exception exception, string context = null)
        {
            string message = exception.Message;

            // Добавляем контекст, если он указан
            if (!string.IsNullOrEmpty(context))
            {
                message = $"{context}: {message}";
            }

            // Добавляем стек вызовов для более полной информации
            message += $"\nStackTrace: {exception.StackTrace}";

            LogError(message);
        }
    }
}