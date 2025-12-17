using System.Net.NetworkInformation;

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
            if (!IsSuccess) return "Недоступен";

            return RoundtripTime switch
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

        public static bool PingHostSimple(string nameOrAddress)
        {
            return PingHost(nameOrAddress).IsSuccess;
        }
    }
}