using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AccountController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient();
            // Note: Using port 7269 from your Part 2 Swagger. Change if your API uses a different port.
            var apiUrl = "https://localhost:7269/api/auth/login";

            var json = JsonSerializer.Serialize(new { model.Email, model.Password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var tokenData = JsonDocument.Parse(responseString);
                var token = tokenData.RootElement.GetProperty("token").GetString();

                // Store the token in a session so we can use it later
                HttpContext.Session.SetString("JWToken", token ?? "");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }
    }
}