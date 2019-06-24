using GitMan.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitMan.Providers
{
    internal class AzureProvider : RemoteProvider
    {
        private readonly string _organization;
        private readonly string _project;
        private readonly string _personalAccessToken;

        public AzureProvider(AzureProviderSettings settings)
            : base(
                  $"{settings.Organization} - {settings.Project}",
                  settings.DefaultConfig)
        {
            _organization = settings.Organization
                 ?? throw new ArgumentNullException("Azure provider organization cannot be null");
            _project = settings.Project
                 ?? throw new ArgumentNullException("Azure provider project cannot be null");
            _personalAccessToken = settings.PersonalAccessToken
                 ?? throw new ArgumentNullException("Azure provider personal access token cannot be null");
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();

            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(acceptHeader);

            var patBytes = Encoding.ASCII.GetBytes(":" + _personalAccessToken);
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
            var organization = Uri.EscapeUriString(_organization);
            var project = Uri.EscapeUriString(_project);
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
