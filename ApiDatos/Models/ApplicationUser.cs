using Microsoft.AspNetCore.Identity;

namespace ApiDatos.Models;

public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
}