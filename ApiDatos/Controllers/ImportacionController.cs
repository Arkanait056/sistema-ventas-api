using System.Globalization;
using System.Text.Json;
using ApiDatos.Data;
using ApiDatos.Dtos;
using ApiDatos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiDatos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ImportacionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ImportacionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/importacion/productos  (archivo .csv o .json)
    [HttpPost("productos")]
    public async Task<ActionResult<ImportResultDto>> ImportarProductos(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debes enviar un archivo .csv o .json.");

        var errores = new List<string>();
        var filas = new List<ProductoImportDto>();
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        using var stream = archivo.OpenReadStream();

        if (extension == ".json")
        {
            try
            {
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                filas = await JsonSerializer.DeserializeAsync<List<ProductoImportDto>>(stream, opciones) ?? new();
            }
            catch (JsonException ex)
            {
                return BadRequest($"JSON inválido: {ex.Message}");
            }
        }
        else if (extension == ".csv")
        {
            using var reader = new StreamReader(stream);
            var numeroLinea = 0;
            string? linea;

            while ((linea = await reader.ReadLineAsync()) != null)
            {
                numeroLinea++;
                if (numeroLinea == 1 || string.IsNullOrWhiteSpace(linea)) continue; // encabezado y líneas vacías

                var c = linea.Split(',');
                if (c.Length != 6)
                {
                    errores.Add($"Línea {numeroLinea}: se esperaban 6 columnas.");
                    continue;
                }

                if (!decimal.TryParse(c[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var costo) ||
                    !decimal.TryParse(c[3], NumberStyles.Number, CultureInfo.InvariantCulture, out var venta) ||
                    !int.TryParse(c[4], out var stock) ||
                    !int.TryParse(c[5], out var minimo))
                {
                    errores.Add($"Línea {numeroLinea}: valores numéricos inválidos.");
                    continue;
                }

                filas.Add(new ProductoImportDto(c[0].Trim(), c[1].Trim(), costo, venta, stock, minimo));
            }
        }
        else
        {
            return BadRequest("Formato no soportado. Usa .csv o .json.");
        }

        var skus = filas.Select(f => f.Sku).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        var existentes = await _context.Productos
            .Where(p => skus.Contains(p.Sku))
            .ToDictionaryAsync(p => p.Sku, StringComparer.OrdinalIgnoreCase);

        int creados = 0, actualizados = 0;
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < filas.Count; i++)
        {
            var f = filas[i];

            if (string.IsNullOrWhiteSpace(f.Nombre) || string.IsNullOrWhiteSpace(f.Sku))
            {
                errores.Add($"Registro {i + 1}: nombre y SKU son obligatorios.");
                continue;
            }

            if (f.PrecioCosto < 0 || f.PrecioVenta < 0 || f.StockActual < 0 || f.StockMinimo < 0)
            {
                errores.Add($"Registro {i + 1} ({f.Sku}): no se permiten valores negativos.");
                continue;
            }

            if (!vistos.Add(f.Sku))
            {
                errores.Add($"Registro {i + 1}: el SKU '{f.Sku}' está repetido en el archivo.");
                continue;
            }

            if (existentes.TryGetValue(f.Sku, out var producto))
            {
                producto.Nombre = f.Nombre;
                producto.PrecioCosto = f.PrecioCosto;
                producto.PrecioVenta = f.PrecioVenta;
                producto.StockActual = f.StockActual;
                producto.StockMinimo = f.StockMinimo;
                actualizados++;
            }
            else
            {
                _context.Productos.Add(new Producto
                {
                    Nombre = f.Nombre,
                    Sku = f.Sku,
                    PrecioCosto = f.PrecioCosto,
                    PrecioVenta = f.PrecioVenta,
                    StockActual = f.StockActual,
                    StockMinimo = f.StockMinimo
                });
                creados++;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new ImportResultDto(creados, actualizados, errores));
    }
}