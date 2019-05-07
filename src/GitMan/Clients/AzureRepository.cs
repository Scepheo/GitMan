namespace GitMan.Clients
{
    public class AzureRepository
    {
        public string Name { get; }
        public string RemoteUrl { get; }

        public AzureRepository(string name, string remoteUrl)
        {
            Name = name;
            RemoteUrl = remoteUrl;
        }
    }
}
