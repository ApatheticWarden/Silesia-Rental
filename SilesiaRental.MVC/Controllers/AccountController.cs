using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SilesiaRental.MVC.Controllers
{
    public class AccountController : Controller
    {
        // Needed for creating query
        private readonly IHttpClientFactory _clientFactory;

        // Getting an API
        private string _apiUrl;

        public AccountController(IHttpClientFactory clientFactory, IConfiguration configuration) {
            _clientFactory = clientFactory;
            _apiUrl = configuration["ApiSettings:BaseUrl"] + "/api/Users";
        }

        [HttpGet]
        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(string username, string password) {
            var regModel = new { Username = username, Password = password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(regModel), Encoding.UTF8, "application/json");

            var client = CreateClient();
            var res = await client.PostAsync($"{_apiUrl}/register", jsonContent);
            if (res.IsSuccessStatusCode) return RedirectToAction("Login");
            else {
                // ЧИТАЕМ ОШИБКУ
                var errorBody = await res.Content.ReadAsStringAsync();

                // Записываем её, чтобы увидеть на экране
                TempData["Error"] = $"Ошибка {res.StatusCode}: {errorBody}";

                // Возвращаем на страницу регистрации
                return RedirectToAction("Register");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string pass) {
            // Packing query
            var loginModel = new { Username = username, Password = pass };
            var jsonContent = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, 
                "application/json");
            // ^^^^
            // This makes from this shit -> object {Username = "something", Password = "somepass"}
            // "application/json" - for API
            // Making a "phone"
            var client = CreateClient();
            // Calling and waiting for response
            var res = await client.PostAsync($"{_apiUrl}/login", jsonContent);
            // Checking
            if (res.IsSuccessStatusCode)
            {
                var resString = await res.Content.ReadAsStringAsync();

                var token = JsonDocument.Parse(resString).RootElement.GetProperty("token").GetString();

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
                var roleValue = roleClaim?.Value ?? "User";

                // Like "passport" - we claim identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("ApiToken", token),
                    new Claim(ClaimTypes.Role, roleValue)
                };

                // Here. CookieAuth - "name of document"
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                // Who owns this passport? 
                var principal = new ClaimsPrincipal(identity);
                // Giving this person a "pass"
                await HttpContext.SignInAsync("CookieAuth", principal);
                // redirect
                return RedirectToAction("Index", "Home");
            }
            else{
                var errorJson = await res.Content.ReadAsStringAsync();
                string errorMessage = "Incorrect password";

                try {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var env = JsonSerializer.Deserialize<Envelope<object>>(errorJson, options);
                    if (env.Message != null && !string.IsNullOrEmpty(env.Message)) {
                        errorMessage = env.Message;
                    }
                } catch (Exception e) { 
                
                }
                ViewBag.Error = errorMessage;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile() {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username)) {
                return Content("<div class='alert alert-danger'>You have to be logged in</div>");
            }

            var client = CreateClient();

            var res = await client.GetAsync($"{_apiUrl}/profile/{username}");

            if (res.IsSuccessStatusCode) {
                var json = await res.Content.ReadAsStringAsync();
                var mdl = JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return PartialView("_Profile", mdl);
            } else {
                return Content($"<div class='alert alert-danger'>API Error: {res.StatusCode}</div>");
            }
        }

        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            return View();
        }

        // It returns View as it Named. Meaning: func Register with return View() will return View("Register")
        // Just synt sugar
        public async Task<IActionResult> Register() {
            return View();
        }
        private HttpClient CreateClient() {
            var client = _clientFactory.CreateClient();
            var token = User.FindFirst("ApiToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
