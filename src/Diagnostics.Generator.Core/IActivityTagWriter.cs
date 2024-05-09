using System.Diagnostics;

namespace Diagnostics.Generator.Core
{
    public interface IActivityTagWriter
    {
        void Write(Activity activity, object inst);
    }
    public interface IActivityTagWriter<T>: IActivityTagWriter
    {
        void Write(Activity activity, T inst);
    }
    public interface IActivityTagExporter
    {
        void Write(Activity activity);
    }
}
