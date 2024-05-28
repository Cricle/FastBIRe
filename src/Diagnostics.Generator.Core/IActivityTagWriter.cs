using System.Diagnostics;

namespace Diagnostics.Generator.Core
{
    public interface IActivityTagWriter
    {
        void Write(Activity activity, object input);
    }
    public interface IActivityTagWriter<T>: IActivityTagWriter
    {
        void Write(Activity activity, T input);
    }
    public interface IActivityTagExporter
    {
        void Write(Activity activity);
    }

    public interface IActivityTagMerge
    {
        void Merge(ActivityTagsCollection tags, object input);

        ActivityTagsCollection Merge(object input);
    }
    public interface IActivityTagMerge<T>: IActivityTagMerge
    {
        void Merge(ActivityTagsCollection tags, T input);

        ActivityTagsCollection Merge(T input);
    }
}
