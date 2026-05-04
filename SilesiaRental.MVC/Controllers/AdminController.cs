using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SilesiaRental.MVC.Controllers {
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller {
        // Needed for creating query
        private readonly IHttpClientFactory _clientFactory;

        // Getting an API
        private string _apiUrl;

        public AdminController(IHttpClientFactory clientFactory, IConfiguration configuration) {
            _clientFactory = clientFactory;
            _apiUrl = configuration["ApiSettings:BaseUrl"] + "/api";
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard() {
            return PartialView("_AdminDashboard");
        }

        [HttpGet]
        public async Task<IActionResult> ToolsList() {
            try {
                var client = CreateClient();

                var envelope = await client.GetFromJsonAsync<Envelope<List<Tool>>>($"{_apiUrl}/Tools");

                if (!envelope!.IsSuccess) {
                    return Content($"<div class='alert alert-danger m-4 glass-alert'>API-Error. Code: {envelope.Message} </div>");
                }

                var tools = envelope?.Payload ?? new List<Tool>();

                return PartialView("_AdminTools", tools);
            } catch (Exception e) {
                return Content($"<div class='alert alert-danger m-4 glass-alert'>Critical Error: {e.Message}</div>");
            }
        }

        public IActionResult UsersList() => Content("<div class='alert alert-info m-4'>User Management coming soon...</div>");
        public IActionResult RentalsList() => Content("<div class='alert alert-warning m-4'>Rentals logic coming soon...</div>");

        private HttpClient CreateClient() {
            var client = _clientFactory.CreateClient();
            var token = User.FindFirst("ApiToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
