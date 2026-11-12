namespace RustSharp.Core.Diagnostics;

/// <summary>
/// Однонаправленный поток диагностических сообщений одного уровня.
/// Является наблюдаемым: подписчики получают уведомления по мере поступления сообщений.
/// </summary>
public sealed class DiagnosticStream
{
    private readonly List<CompilerDiagnostic> _messages = new();
    private readonly List<Action<CompilerDiagnostic>> _subscribers = new();

    /// <summary>Уровень важности этого потока.</summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>Имя потока для отображения (INFO / WARN / ERR).</summary>
    public string Name { get; }

    public DiagnosticStream(DiagnosticSeverity severity)
    {
        Severity = severity;
        Name = severity switch
        {
            DiagnosticSeverity.Info    => "INFO",
            DiagnosticSeverity.Warning => "WARN",
            DiagnosticSeverity.Error   => "ERR",
            _                          => severity.ToString().ToUpperInvariant()
        };
    }

    /// <summary>Все сообщения в этом потоке (только для чтения).</summary>
    public IReadOnlyList<CompilerDiagnostic> Messages => _messages;

    /// <summary>Количество сообщений в потоке.</summary>
    public int Count => _messages.Count;

    /// <summary>Возвращает <c>true</c>, если поток содержит хотя бы одно сообщение.</summary>
    public bool HasMessages => _messages.Count > 0;

    /// <summary>Записывает сообщение в поток и уведомляет подписчиков.</summary>
    public void Write(CompilerDiagnostic diagnostic)
    {
        _messages.Add(diagnostic);
        foreach (var subscriber in _subscribers)
        {
            subscriber(diagnostic);
        }
    }

    /// <summary>
    /// Подписывается на новые сообщения потока.
    /// Возвращает токен отписки: вызовите <c>token()</c>, чтобы отписаться.
    /// </summary>
    public Action Subscribe(Action<CompilerDiagnostic> handler)
    {
        _subscribers.Add(handler);
        return () => _subscribers.Remove(handler);
    }

    /// <summary>Очищает накопленные сообщения (подписчики остаются).</summary>
    public void Clear() => _messages.Clear();
}
