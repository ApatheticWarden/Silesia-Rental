using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using SilesiaRental.MVC.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SilesiaRental.MVC.Controllers
{
    public class HomeController : Controller {
        private readonly IHttpClientFactory _clientFactory;

        private string _apiUrl;

        public HomeController(IHttpClientFactory clientFactory, IConfiguration configuration) {
            _clientFactory = clientFactory;
            _apiUrl = configuration["ApiSettings:BaseUrl"] + "/api";
        }

        public IActionResult Index() {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRentalsPartial() {
            var client = CreateClient();
            var res = await client.GetAsync($"{_apiUrl}/Tools");

            if (res.IsSuccessStatusCode == false) return PartialView("_ErrorPage");

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var envelope = JsonSerializer.Deserialize<Envelope<List<Tool>>>(json, options);

            if (envelope == null || !envelope.IsSuccess) {
                return PartialView("_ErrorPage", envelope?.Message);
            }

            return PartialView("_RentList", envelope.Payload);
        }

        // "Returns" view
        [HttpGet]
        public async Task<IActionResult> GetReturnsPartial() {
            var client = CreateClient();
            var res = await client.GetAsync($"{_apiUrl}/Rentals/active-rentals");

            if (res.IsSuccessStatusCode == false) return PartialView("_ErrorPage");

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // We take the ENVELOPE which containing DATA (PAYLOAD)
            var envelope = JsonSerializer.Deserialize<Envelope<List<Rental>>>(json, options);

            if (envelope == null || !envelope.IsSuccess) {
                return PartialView("_ErrorPage", envelope?.Message ?? "API Error");
            }

            return PartialView("_ReturnList", envelope.Payload);
        }

        // "Bills" view
        [HttpGet]
        public async Task<IActionResult> GetHistoryPartial() {
            var client = CreateClient();
            var res = await client.GetAsync($"{_apiUrl}/Rentals/closed-rentals");
            var rentals = new List<Rental>();

            if (res.IsSuccessStatusCode == false) return PartialView("_ErrorPage");

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // We take the ENVELOPE which containing DATA (PAYLOAD)
            var envelope = JsonSerializer.Deserialize<Envelope<List<Rental>>>(json, options); ;

            if (envelope == null || !envelope.IsSuccess) {
                return PartialView("_ErrorPage", envelope?.Message ?? "API Error");
            }

            return PartialView("_BillsList", envelope.Payload);
        }

        [HttpPost]
        public async Task<IActionResult> Rent(int toolId, string version)
        {
            var customer = User.Identity.Name;
            var jsonReq = new StringContent(JsonSerializer.Serialize(new { 
                ToolId = toolId,
                Version = version }
            ), 
                Encoding.UTF8, "application/json");

            var client = CreateClient();
            var res = await client.PostAsync($"{_apiUrl}/Rentals/rent", jsonReq);

            if (res.IsSuccessStatusCode) TempData["Message"] = "Rented!";
            else {
                var errorBody = await res.Content.ReadAsStringAsync();
                var statusCode = res.StatusCode;

                TempData["Error"] = $"Error {statusCode}: {errorBody}";
            }

            return RedirectToAction("Index"); // Refresh
        }

        [HttpPost]
        public async Task<IActionResult> ReturnTool(int rentalId)
        {
            var client = CreateClient();
            var res = await client.PostAsync($"{_apiUrl}/Rentals/return/{rentalId}", null);

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var envelope = JsonSerializer.Deserialize<Envelope<RentalCheckDTO>>(json, options);

            if (envelope != null && envelope.IsSuccess) {
                var check = envelope.Payload;

                TempData["Message"] = $"Success! {check.ToolName} returned. " +
                                      $"To pay: {check.TotalPrice}zł for {check.Days} d.";
            } else {
                TempData["Error"] = envelope?.Message ?? "Error during return";
            }

            return RedirectToAction("Index");
        }

        private HttpClient CreateClient()
        {
            var client = _clientFactory.CreateClient();
            var token = User.FindFirst("ApiToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}