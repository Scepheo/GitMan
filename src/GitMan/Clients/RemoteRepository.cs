namespace GitMan.Clients
{
    internal class RemoteRepository
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string CloneUrl { get; set; }

        public RemoteRepository(string name, string cloneUrl)
            : this(name, name, cloneUrl)
        { }

        public RemoteRepository(string name, string displayName, string cloneUrl)
        {
            Name = name;
            DisplayName = displayName;
            CloneUrl = cloneUrl;
        }
    }
}
