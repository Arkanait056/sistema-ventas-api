using ApiDatos.Data;
using ApiDatos.Dtos;
using ApiDatos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiDatos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VentasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ventas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaOutputDto>>> GetVentas()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .Select(v => new VentaOutputDto(
                v.Id,
                v.Fecha,
                v.UsuarioId,
                v.Total,
                v.Detalles.Select(d => new DetalleVentaOutputDto(
                    d.ProductoId,
                    d.Producto!.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Cantidad * d.PrecioUnitario
                )).ToList()
            ))
            .ToListAsync();

        return Ok(ventas);
    }

    // POST: api/ventas
    [HttpPost]
    public async Task<ActionResult<VentaOutputDto>> PostVenta(VentaInputDto input)
    {
        if (input.Detalles == null || !input.Detalles.Any())
        {
            return BadRequest("La venta debe tener al menos un producto.");
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Usamos una transacción explícita: si algo falla, nada se guarda
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var venta = new Venta
            {
                Fecha = DateTime.UtcNow,
                UsuarioId = usuarioId,
                Total = 0
            };

            decimal totalCalculado = 0;

            foreach (var item in input.Detalles)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null)
                {
                    return BadRequest($"El producto con Id {item.ProductoId} no existe.");
                }

                if (item.Cantidad <= 0)
                {
                    return BadRequest($"La cantidad para '{producto.Nombre}' debe ser mayor a 0.");
                }

                // Regla de negocio: todo o nada si no hay stock suficiente
                if (producto.StockActual < item.Cantidad)
                {
                    return BadRequest($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.StockActual}, solicitado: {item.Cantidad}.");
                }

                var detalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.PrecioVenta // el backend decide el precio, no el cliente
                };

                producto.StockActual -= item.Cantidad;
                totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;

                venta.Detalles.Add(detalle);
            }

            venta.Total = totalCalculado;

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Recargamos con los nombres de producto para la respuesta
            await _context.Entry(venta).Collection(v => v.Detalles).Query()
                .Include(d => d.Producto)
                .LoadAsync();

            var output = new VentaOutputDto(
                venta.Id,
                venta.Fecha,
                venta.UsuarioId,
                venta.Total,
                venta.Detalles.Select(d => new DetalleVentaOutputDto(
                    d.ProductoId,
                    d.Producto!.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Cantidad * d.PrecioUnitario
                )).ToList()
            );

            return CreatedAtAction(nameof(GetVentas), output);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}