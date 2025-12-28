using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MyInternetChecker.Chart
{
    public static class HistoryManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<(DateTime timestamp, long ping)>>
            _history = new();

        private static readonly TimeSpan _historyDuration = TimeSpan.FromMinutes(1);

        public static void AddPingResult(string host, long pingTime)
        {
            var now = DateTime.Now;
            var queue = _history.GetOrAdd(host, _ => new ConcurrentQueue<(DateTime, long)>());

            // Добавляем новую точку с временной меткой
            queue.Enqueue((now, pingTime));

            // Удаляем точки старше минуты
            while (queue.TryPeek(out var oldest) && (now - oldest.timestamp) > _historyDuration)
            {
                queue.TryDequeue(out _);
            }
        }

        public static double[] GetLatestHistoryForHost(string host)
        {
            var now = DateTime.Now;

            return _history.TryGetValue(host, out var queue)
                ? queue
                    .Where(x => (now - x.timestamp) <= _historyDuration) // Фильтруем по времени
                    .Select(x => (double)x.ping)
                    .ToArray()
                : [];
        }

        public static long GetLatestPing(string host)
        {
            return _history.TryGetValue(host, out var queue) && queue.TryPeek(out var latest)
                ? latest.ping
                : -1;
        }
    }
}