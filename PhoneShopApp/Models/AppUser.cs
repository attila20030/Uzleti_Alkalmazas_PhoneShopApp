using Microsoft.AspNetCore.Identity;

namespace PhoneShopApp.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = "";
}
