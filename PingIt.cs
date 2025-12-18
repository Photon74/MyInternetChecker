using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MyInternetChecker
{
    public class PingResult
    {
        public bool IsSuccess { get; }

        public long RoundtripTime { get; }

        public string HostName { get; }

        public PingResult(bool isSuccess, long roundtripTime, string hostName = null)
        {
            IsSuccess = isSuccess;
            RoundtripTime = roundtripTime;
            HostName = hostName;
        }

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

        public override string ToString()
        {
            return IsSuccess
                ? $"{RoundtripTime} мс ({GetQualityDescription()})"
                : "Недоступен";
        }
    }

    public class PingIt
    {
        public static PingResult PingHost(string nameOrAddress)
        {
            bool pingable = false;
            long roundtripTime = 0;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress, 1000);
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

        public static async Task<long> PingHostAsync(string nameOrAddress, CancellationToken cancellationToken = default)
        {
            try
            {
                using Ping pinger = new();
                cancellationToken.ThrowIfCancellationRequested();

                PingReply reply = await pinger.SendPingAsync(nameOrAddress, 1000);
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
}