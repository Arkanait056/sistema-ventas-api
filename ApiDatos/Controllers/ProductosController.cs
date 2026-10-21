using ApiDatos.Data;
using ApiDatos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ApiDatos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/productos - El admin y Analista pueden ver 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        return await _context.Productos.ToListAsync();
    }

    // GET: api/productos/5 - Admin y Analista pueden ver
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
        {
            return NotFound();
        }

        return producto;
    }

    // POST: api/productos - Solo el admin 
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Producto>> PostProducto(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    // PUT: api/productos/5 - Solo el admin
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutProducto(int id, Producto producto)
    {
        if (id != producto.Id)
        {
            return BadRequest();
        }

        _context.Entry(producto).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductoExiste(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/productos/5 Solo el admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductoExiste(int id)
    {
        return _context.Productos.Any(e => e.Id == id);
    }
}