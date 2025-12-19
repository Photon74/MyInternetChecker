namespace MyInternetChecker;

/// <summary>Результат операции ping</summary>
/// <param name="IsSuccess">Флаг успешного ответа</param>
/// <param name="RoundtripTime">Время roundtrip в миллисекундах</param>
/// <param name="HostName">Имя или адрес хоста</param>
public record PingResult(bool IsSuccess, long RoundtripTime, string HostName = null)
{
    /// <summary>Получает текстовое описание качества соединения</summary>
    /// <returns>Краткое текстовое описание качества</returns>
    public string GetQualityDescription()
        => !IsSuccess
            ? "Недоступен"
            : RoundtripTime switch
            {
                long r when r < 50 => "Отлично",
                long r when r < 100 => "Хорошо",
                long r when r < 200 => "Нормально",
                long r when r < 500 => "Медленно",
                _ => "Очень медленно"
            };

    /// <summary>Строковое представление результата ping</summary>
    /// <returns>Строка с информацией о результате</returns>
    public override string ToString() =>
        IsSuccess
            ? $"{RoundtripTime} мс ({GetQualityDescription()})"
            : "Недоступен";
}
