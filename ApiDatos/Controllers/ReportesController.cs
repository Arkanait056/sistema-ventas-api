using ApiDatos.Data;
using ApiDatos.Dtos;
using ApiDatos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiDatos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Administrador y Analista pueden ver reportes
public class ReportesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/reportes/ingresos?desde=2026-01-01&hasta=2026-12-31&agrupar=mes
    [HttpGet("ingresos")]
    public async Task<ActionResult<IEnumerable<IngresoPeriodoDto>>> Ingresos(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string agrupar = "dia")
    {
        var query = _context.Ventas.AsQueryable();
        if (desde.HasValue) query = query.Where(v => v.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(v => v.Fecha < hasta.Value.Date.AddDays(1));

        if (agrupar == "mes")
        {
            var porMes = await query
                .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Ingresos = g.Sum(v => v.Total), Cantidad = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(porMes.Select(x => new IngresoPeriodoDto($"{x.Year}-{x.Month:D2}", x.Ingresos, x.Cantidad)));
        }

        var porDia = await query
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new { Dia = g.Key, Ingresos = g.Sum(v => v.Total), Cantidad = g.Count() })
            .OrderBy(x => x.Dia)
            .ToListAsync();

        return Ok(porDia.Select(x => new IngresoPeriodoDto(x.Dia.ToString("yyyy-MM-dd"), x.Ingresos, x.Cantidad)));
    }

        // GET: api/reportes/mas-vendidos?top=5
    [HttpGet("mas-vendidos")]
    public async Task<ActionResult<IEnumerable<MasVendidoDto>>> MasVendidos(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int top = 5)
    {
        var query = _context.Set<DetalleVenta>().AsQueryable();
        if (desde.HasValue) query = query.Where(d => d.Venta!.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(d => d.Venta!.Fecha < hasta.Value.Date.AddDays(1));

        var datos = await query
            .Select(d => new
            {
                d.ProductoId,
                Nombre = d.Producto!.Nombre,
                d.Cantidad,
                d.PrecioUnitario,
                PrecioCosto = d.Producto!.PrecioCosto
            })
            .GroupBy(x => new { x.ProductoId, x.Nombre })
            .Select(g => new
            {
                g.Key.ProductoId,
                g.Key.Nombre,
                Unidades = g.Sum(x => x.Cantidad),
                Ingresos = g.Sum(x => x.Cantidad * x.PrecioUnitario),
                Ganancia = g.Sum(x => x.Cantidad * (x.PrecioUnitario - x.PrecioCosto))
            })
            .OrderByDescending(x => x.Unidades)
            .Take(top)
            .ToListAsync();

        return Ok(datos.Select(x => new MasVendidoDto(
            x.ProductoId, x.Nombre, x.Unidades, x.Ingresos, x.Ganancia)));
    }
}