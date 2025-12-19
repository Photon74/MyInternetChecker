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
public class PingIt
{
    /// <summary>Синхронно пингует хост</summary>
    /// <param name="nameOrAddress">Имя или адрес хоста</param>
    /// <returns>Результат пинга</returns>
    public static PingResult PingHost(string nameOrAddress)
    {
        var pingable = false;
        var roundtripTime = 0L;
        Ping pinger = null;

        try
        {
            pinger = new Ping();
            var reply = pinger.Send(nameOrAddress, 1000);
            pingable = reply.Status == IPStatus.Success;

            if (pingable)
            {
                roundtripTime = reply.RoundtripTime;
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

        return new PingResult(pingable, roundtripTime, nameOrAddress);
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

            // Создаем задачу пинга
            var pingTask = pinger.SendPingAsync(nameOrAddress, 1000);

            // Создаем задачу отмены
            var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);

            // Ждем, какая задача завершится первой
            var completedTask = await Task.WhenAny(pingTask, cancellationTask);

            if (completedTask == cancellationTask)
            {
                // Операция была отменена
                return -1;
            }

            // Пинг завершился
            var reply = await pingTask;
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
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