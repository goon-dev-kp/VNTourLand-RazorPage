using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.Settings;
using Microsoft.Extensions.Options;

namespace BLL.Services.Implement
{
    public class PayPalService : IPayPalService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PayPalSettings _settings;

        public PayPalService(IHttpClientFactory httpClientFactory, IOptions<PayPalSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }
        private string BaseUrl => _settings.UseSandbox ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

        public async Task<string> GetAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientID}:{_settings.ClientSecret}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

            var response = await client.PostAsync($"{BaseUrl}/v1/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            return result.GetProperty("access_token").GetString();
        }

        public async Task<string> CreateOrderAndGetRedirectUrlAsync(decimal amount, Guid bookingId)

        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var order = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
            {
                new {
                    custom_id = bookingId.ToString(),  // 👈 GẮN BookingId vào đây
                    amount = new {
                        currency_code = "USD",
                        value = amount.ToString("0.00")
                    }
                }
            },
                application_context = new
                {
                    return_url = _settings.ReturnUrl,
                    cancel_url = _settings.CancelUrl
                }
            };


            var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/v2/checkout/orders", content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            foreach (var link in result.GetProperty("links").EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    return link.GetProperty("href").GetString();
                }
            }

            throw new Exception("NOT FOUND link approve");
        }

        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 👇 Gán Content rỗng nhưng có Content-Type rõ ràng
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/v2/checkout/orders/{orderId}/capture", content);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Capture SUCCESS: " + json);
                return true;
            }

            Console.WriteLine("❌ Capture FALSE: " + json);
            return false;
        }


    }
}
