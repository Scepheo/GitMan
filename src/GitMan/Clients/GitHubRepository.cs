namespace GitMan.Clients
{
    public class GitHubRepository
    {
        public string Name { get; }
        public string FullName { get; }
        public string RemoteUrl { get; }

        public GitHubRepository(string name, string fullName, string remoteUrl)
        {
            Name = name;
            FullName = fullName;
            RemoteUrl = remoteUrl;
        }
    }
}
