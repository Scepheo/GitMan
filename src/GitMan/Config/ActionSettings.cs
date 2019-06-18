namespace GitMan.Config
{
    internal class ActionSettings
    {
        public string Name { get; set; }
        public string Program { get; set; }
        public string[] Args { get; set; }
        public string SearchFilter { get; set; }
        public bool? Shell { get; set; }
    }
}
