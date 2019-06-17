using GitMan.Config;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace GitMan.Providers
{
    internal class GitHubProvider : RemoteProvider
    {
        private readonly string _username;
        private readonly string _personalAccessToken;

        public GitHubProvider(GitHubProviderSettings settings)
            : base(
                  $"GitHub - {settings.Username}",
                  settings.DefaultConfig)
        {
            _username = settings.Username;
            _personalAccessToken = settings.PersonalAccessToken;
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();

            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Accept.Add(acceptHeader);

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var userAgentHeader = new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString());
            client.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);

            var patBytes = Encoding.ASCII.GetBytes(_username + ":" + _personalAccessToken);
            var authParameter = Convert.ToBase64String(patBytes);
            var authHeader = new AuthenticationHeaderValue("Basic", authParameter);
            client.DefaultRequestHeaders.Authorization = authHeader;

            client.Timeout = TimeSpan.FromSeconds(30);

            return client;
        }

        private Uri BuildUri(string path)
        {
            var builder = new UriBuilder
            {
                Host = "api.github.com",
                Scheme = "https",
                Path = path
            };

            var uriString = builder.ToString();
            var uri = new Uri(uriString);
            return uri;
        }

        private JsonDocument GetResponse(string path)
        {
            var uri = BuildUri(path);

            using var client = GetClient();
            using var response = client.GetAsync(uri).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var document = JsonDocument.Parse(json);

            if (response.IsSuccessStatusCode)
            {
                return document;
            }
            else
            {
                var statusCode = response.StatusCode;
                var message = document.RootElement.TryGetProperty("message", out var messageProperty)
                    ? messageProperty.GetString()
                    : "An unknown error occurred";
                var exception = new RemoteProviderException(statusCode, message);
                throw exception;
            }
        }

        public override RemoteRepository[] GetRepositories()
        {
            var document = GetResponse("user/repos");
            var repositories = document.RootElement;

            var count = repositories.GetArrayLength();
            var GitHubRepos = new RemoteRepository[count];
            var index = 0;
            
            foreach (var repository in repositories.EnumerateArray())
            {
                var name = repository.GetProperty("name").GetString();
                var fullName = repository.GetProperty("full_name").GetString();
                var cloneUrl = repository.GetProperty("clone_url").GetString();
                var GitHubRepo = new RemoteRepository(name, fullName, cloneUrl);
                GitHubRepos[index] = GitHubRepo;
                index++;
            }

            return GitHubRepos;
        }
    }
}
