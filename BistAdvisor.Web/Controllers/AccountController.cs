using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl)
    {
        var correctUsername = _configuration["AdminUsername"] ?? "admin";
        var correctPassword = _configuration["AdminPassword"];

        if (string.IsNullOrEmpty(correctPassword) || username != correctUsername || password != correctPassword)
        {
            ViewData["Error"] = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, correctUsername) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("BistAdvisorAuth", principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("BistAdvisorAuth");
        return RedirectToAction("Index", "Dashboard");
    }
}