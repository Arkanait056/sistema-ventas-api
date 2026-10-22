using ApiDatos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiDatos.Controllers;

public record RegistrarDto(string Email, string Password, string NombreCompleto, string Rol);
public record LoginDto(string Email, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpGet("whoami")]
    [Authorize]
    public IActionResult WhoAmI()
    {
        var claims = User.Claims.Select(c => new { tipo = c.Type, valor = c.Value });
        return Ok(claims);
    }

    // Solo un Admin autenticado puede crear usuarios (Admin o Analista).
    // El primer Admin se crea al arrancar la app (ver Program.cs).
    [HttpPost("registrar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Registrar(RegistrarDto dto)
    {
        if (dto.Rol != "Admin" && dto.Rol != "Analista")
        {
            return BadRequest("Rol inválido. Debe ser 'Admin' o 'Analista'.");
        }

        var usuario = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            NombreCompleto = dto.NombreCompleto
        };

        var resultado = await _userManager.CreateAsync(usuario, dto.Password);

        if (!resultado.Succeeded)
        {
            return BadRequest(resultado.Errors);
        }

        await _userManager.AddToRoleAsync(usuario, dto.Rol);

        return Ok(new { Mensaje = "Usuario registrado exitosamente." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);

        if (usuario == null || !await _userManager.CheckPasswordAsync(usuario, dto.Password))
        {
            return Unauthorized("Credenciales inválidas.");
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var token = GenerarToken(usuario, roles);

        return Ok(new { Token = token });
    }

    private string GenerarToken(ApplicationUser usuario, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(ClaimTypes.Email, usuario.Email!),
            new Claim("NombreCompleto", usuario.NombreCompleto)
        };

        foreach (var rol in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}