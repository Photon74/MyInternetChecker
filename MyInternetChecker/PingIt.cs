using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MyInternetChecker;

/// <summary>Утилита для выполнения ping к хостам</summary>
public static class PingIt
{
    /// <summary>Синхронно пингует хост</summary>
    /// <param name="nameOrAddress">Имя или адрес хоста</param>
    /// <returns>Результат пинга</returns>
    public static PingResult PingHost(string nameOrAddress)
    {
        try
        {
            using var pinger = new Ping();

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