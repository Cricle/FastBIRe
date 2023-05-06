namespace FastBIRe.Project.WebSample
{
    public class Class
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}