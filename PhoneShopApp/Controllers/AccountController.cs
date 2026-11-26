using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhoneShopApp.Models;

namespace PhoneShopApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        var user = new AppUser { UserName = email, Email = email, FullName = fullName };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View();
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Phones");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/Phones");

        ModelState.AddModelError("", "Hibás e-mail-cím vagy jelszó.");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => Content("Hozzáférés megtagadva! Nincs jogosultságod ehhez a művelethez.");
}
