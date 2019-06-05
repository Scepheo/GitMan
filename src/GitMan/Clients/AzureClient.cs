using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitMan.Clients
{
    internal class AzureClient
    {
        private readonly AzureClientConfig _config;

        public AzureClient(AzureClientConfig config)
        {
            _config = config;
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();

            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(acceptHeader);

            var patBytes = Encoding.ASCII.GetBytes(":" + _config.PersonalAccessToken);
            var authParameter = Convert.ToBase64String(patBytes);
            var authHeader = new AuthenticationHeaderValue("Basic", authParameter);
            client.DefaultRequestHeaders.Authorization = authHeader;

            client.Timeout = TimeSpan.FromSeconds(30);

            return client;
        }

        private string MakeParameterString(KeyValuePair<string, string> parameter)
        {
            var safeKey = Uri.EscapeUriString(parameter.Key);
            var safeValue = Uri.EscapeUriString(parameter.Value);
            var parameterString = $"{safeKey}={safeValue}";
            return parameterString;
        }

        private string MakeQuery(Dictionary<string, string> queryParams)
        {
            var pairs = queryParams.Select(MakeParameterString);
            var query = string.Join('&', pairs);
            return query;
        }

        private Uri BuildUri(string path)
        {
            var emptyQueryParams = new Dictionary<string, string>();
            var uri = BuildUri(path, emptyQueryParams);
            return uri;
        }

        private Uri BuildUri(string path, Dictionary<string, string> queryParams)
        {
            var organization = Uri.EscapeUriString(_config.Organization);
            var project = Uri.EscapeUriString(_config.Project);
            var fullPath = $"{organization}/{project}/_apis/{path}";

            queryParams["api-version"] = "5.0";
            var query = MakeQuery(queryParams);

            var builder = new UriBuilder
            {
                Host = "dev.azure.com",
                Scheme = "https",
                Query = query,
                Path = fullPath
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
            return document;
        }

        public RemoteRepository[] GetRepositories()
        {
            var document = GetResponse("git/repositories");
            var repositories = document.RootElement.GetProperty("value");

            var count = repositories.GetArrayLength();
            var azureRepos = new RemoteRepository[count];
            var index = 0;
            
            foreach (var repository in repositories.EnumerateArray())
            {
                var name = repository.GetProperty("name").GetString();
                var cloneUrl = repository.GetProperty("remoteUrl").GetString();
                var azureRepo = new RemoteRepository(name, cloneUrl);
                azureRepos[index] = azureRepo;
                index++;
            }

            return azureRepos;
        }
    }
}
