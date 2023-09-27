namespace FastBIRe
{
    public class ScriptingRequest
    {
        public ScriptingRequest()
        {
            Scripts = CreateScripts();
            lstScripts = Scripts as List<string>;
        }
        private readonly List<string>? lstScripts;

        public virtual IList<string> Scripts { get; }

        protected virtual IList<string> CreateScripts()
        {
            return new List<string>();
        }

        public void AddScripts(IEnumerable<string> scripts)
        {
            if (lstScripts != null)
            {
                lstScripts.AddRange(scripts);
            }
            else
            {
                foreach (var item in scripts)
                {
                    Scripts.Add(item);
                }
            }
        }
    }
}
