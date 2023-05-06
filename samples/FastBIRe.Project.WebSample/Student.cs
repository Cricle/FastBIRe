using System.ComponentModel.DataAnnotations.Schema;

namespace FastBIRe.Project.WebSample
{
    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(nameof(Class))]
        public virtual int ClassId { get; set; }

        public virtual Class Class { get; set; }
    }
}