namespace Diagnostics.Traces
{
    public interface IInputHandlerSync<T>
    {
       void Handle(T input);
    }
}
