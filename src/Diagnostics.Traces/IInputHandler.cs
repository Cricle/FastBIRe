namespace Diagnostics.Traces
{
    public interface IInputHandler<T>
    {
        Task HandleAsync(T input, CancellationToken token);
    }
    public interface IInputHandlerSync<T>
    {
       void Handle(T input);
    }
}
