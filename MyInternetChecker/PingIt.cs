using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MyInternetChecker;

/// <summary>Результат операции ping</summary>
public class PingResult
{
    /// <summary>Флаг успешного ответа</summary>
    public bool IsSuccess { get; }

    /// <summary>Время в миллисекундах для roundtrip</summary>
    public long RoundtripTime { get; }

    /// <summary>Имя или адрес хоста</summary>
    public string HostName { get; }

    /// <summary>Инициализирует новый экземпляр результата ping</summary>
    /// <param name="isSuccess">Флаг успешного ответа</param>
    /// <param name="roundtripTime">Время roundtrip в миллисекундах</param>
    /// <param name="hostName">Имя или адрес хоста</param>
    public PingResult(bool isSuccess, long roundtripTime, string hostName = null)
    {
        IsSuccess = isSuccess;
        RoundtripTime = roundtripTime;
        HostName = hostName;
    }

    /// <summary>Получает текстовое описание качества соединения</summary>
    /// <returns>Краткое текстовое описание качества</returns>
    public string GetQualityDescription()
    {
        return !IsSuccess
            ? "Недоступен"
            : RoundtripTime switch
            {
                long r when r < 50 => "Отлично",
                long r when r < 100 => "Хорошо",
                long r when r < 200 => "Нормально",
                long r when r < 500 => "Медленно",
                _ => "Очень медленно"
            };
    }

    /// <summary>Строковое представление результата ping</summary>
    /// <returns>Строка с информацией о результате</returns>
    public override string ToString()
    {
        return IsSuccess
            ? $"{RoundtripTime} мс ({GetQualityDescription()})"
            : "Недоступен";
    }
}

/// <summary>Утилита для выполнения ping к хостам</summary>
public static class PingIt
{
    /// <summary>Синхронно пингует хост</summary>
    /// <param name="nameOrAddress">Имя или адрес хоста</param>
    /// <returns>Результат пинга</returns>
    public static PingResult PingHost(string nameOrAddress)
    {
        Ping pinger = null;

        try
        {
            pinger = new Ping();

            for (var i = 0; i < 3; i++)
            {
                var reply = pinger.Send(nameOrAddress, 1000);
                if (reply.Status == IPStatus.Success)
                    return new PingResult(true, reply.RoundtripTime, nameOrAddress);
            }
        }
        catch (PingException)
        {
            // Игнорируем исключения пинга
        }
        finally
        {
            pinger?.Dispose();
        }

        return new PingResult(false, 0, nameOrAddress);
    }

    /// <summary>Асинхронно пингует хост с поддержкой отмены</summary>
    /// <param name="nameOrAddress">Имя или адрес хоста</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Время roundtrip в миллисекундах или -1 при ошибке/отмене</returns>
    public static async Task<long> PingHostAsync(string nameOrAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            using Ping pinger = new();

            // Пытаемся выполнить пинг до 3 раз
            for (var attempt = 0; attempt < 3; attempt++)
            {
                // Проверяем отмену перед каждой попыткой
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Выполняем пинг, передавая токен отмены напрямую
                    var reply = await pinger.SendPingAsync(
                        nameOrAddress,
                        TimeSpan.FromMilliseconds(1000),
                        cancellationToken: cancellationToken);

                    if (reply.Status == IPStatus.Success)
                        return reply.RoundtripTime;
                    // если неуспешно — повторим попытку
                }
                catch (PingException)
                {
                    // При исключении пинга пробуем ещё раз
                }
            }

            return -1;
        }
        catch (OperationCanceledException)
        {
            return -1;
        }
        catch (PingException)
        {
            return -1;
        }
    }
}