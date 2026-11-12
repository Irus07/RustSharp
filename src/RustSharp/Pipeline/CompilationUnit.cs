namespace RustSharp.Core.Pipeline;

/// <summary>
/// Данные, передаваемые между стадиями пайплайна компиляции.
/// Является иммутабельной «посылкой»: каждая стадия принимает один тип и возвращает другой.
/// </summary>
/// <typeparam name="T">Тип полезной нагрузки (источника, токенов, дерева разбора и т.д.).</typeparam>
public sealed class CompilationUnit<T>
{
    /// <summary>Полезная нагрузка, результат данной стадии.</summary>
    public T Payload { get; }

    /// <summary>Метаданные сессии компиляции (имя файла, параметры и пр.).</summary>
    public CompilationContext Context { get; }

    public CompilationUnit(T payload, CompilationContext context)
    {
        Payload = payload;
        Context = context;
    }

    /// <summary>Создаёт новый <see cref="CompilationUnit{TNext}"/> с другой нагрузкой, сохраняя контекст.</summary>
    public CompilationUnit<TNext> WithPayload<TNext>(TNext next) =>
        new(next, Context);
}
