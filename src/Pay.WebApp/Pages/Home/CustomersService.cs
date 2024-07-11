using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using Newtonsoft.Json;

using Pay.WebApp.Configs;

namespace Pay.WebApp.Pages.Home
{
    public class CustomersService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<VerificationApiConfiguration> _apiConfig;

        public CustomersService(
            IHttpClientFactory httpClientFactory,
            IOptions<VerificationApiConfiguration> apiConfig,
            IHttpContextAccessor httpContextAccessor // todo: use this instead of hardcode; check other places
        )
        {
            _httpClientFactory = httpClientFactory;
            _apiConfig = apiConfig;
        }

        public async Task<CustomerModel> GetCustomerById(string customerId)
        {
            CustomerModel customer = null;

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_apiConfig.Value.Url);

            var context = new HttpContextAccessor().HttpContext;
            var accessToken = await context.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync($"/api/customers/{customerId}"); // todo: check all httpclients and move literals to appsettings

            if (response.StatusCode == HttpStatusCode.OK) // todo: if not OK - add logs; check other places
            {
                var result = await response.Content.ReadAsStringAsync();
                customer = JsonConvert.DeserializeObject<CustomerModel>(result);
            }
            return customer;
        }
    }
}