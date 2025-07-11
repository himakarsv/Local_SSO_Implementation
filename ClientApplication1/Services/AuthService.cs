using System.Text;
using System.Text.Json;
using ClientApplication1.ViewModels;

namespace ClientApplication1.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _authUrl;


        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _authUrl = _configuration["AuthenticationServer:BaseUrl"] ?? "https://localhost:7125";
        }


        public async Task<HttpResponseMessage> LoginUserAsync(LoginViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authUrl}/api/Auth/login";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }
        
        public async Task<HttpResponseMessage> RegisterUserAsync(RegisterViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authUrl}/api/Auth/register";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> ValidateSSOTokenAsync(ValidateSSOTokenModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authUrl}/api/Auth/ValidateSSOToken";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }
    }
}
