using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static Pay.WebApp.Commands;
using System.Text.Json;
using System.Text;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Options;
using System.Net;
using Pay.Shared.Extensions;
using Pay.WebApp.Configs;

namespace Pay.WebApp
{
    public class VerificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VerificationApiConfiguration _apiConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VerificationService(
            IOptions<VerificationApiConfiguration> apiConfig,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _apiConfig = apiConfig.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        // todo: split into smaller methods: one method for each API call
        public async Task SendVerificationDetails(VerificationModel model)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var customerId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // create the draft
            var detailsId = Guid.NewGuid().ToString();

            var draftCommand = new CreateDraftVerificationDetails {
                    VerificationDetailsId = detailsId,
                    CustomerId = customerId
            };
            var draftCommandJson = new StringContent(
                JsonSerializer.Serialize(draftCommand),
                Encoding.UTF8, 
                "application/json"
            );

            var response = await httpClient.PostAsync(_apiConfig.Url.Combine(_apiConfig.DraftRoute), draftCommandJson);

            // todo: refactor to use proper type of exception + exception middleware; add logs; check all other throws;
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");

            // add date of birth
            var dobCommand = new AddDateOfBirth {
                VerificationDetailsId = detailsId,
                DateOfBirth = model.DateOfBirth
            };
            var dobCommandJson = new StringContent(
                JsonSerializer.Serialize(dobCommand),
                Encoding.UTF8,
                "application/json"
            );
            response = await httpClient.PostAsync(_apiConfig.Url.Combine(_apiConfig.DobRoute), dobCommandJson);
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");

            // add the address
            var addressCommand = new AddAddress {
                VerificationDetailsId = detailsId,
                Address1 = model.Address1,
                Address2 = model.Address2,
                CityTown = model.CityTown,
                CountyState = model.CountyState,
                Code = model.Code,
                Country = model.Country
            };
            var addressCommandJson = new StringContent(
                JsonSerializer.Serialize(addressCommand),
                Encoding.UTF8,
                "application/json"
            );
            response = await httpClient.PostAsync(_apiConfig.Url.Combine(_apiConfig.AddressRoute), addressCommandJson);
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");

            // submit the details
            var submitCommand = new SubmitDetails {
                VerificationDetailsId = detailsId
            };
            var submitCommandJson = new StringContent(
                JsonSerializer.Serialize(submitCommand),
                Encoding.UTF8,
                "application/json"
            );
            response = await httpClient.PostAsync(_apiConfig.Url.Combine(_apiConfig.SubmitRoute), submitCommandJson);
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");
        }
    }
}