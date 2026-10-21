namespace ApiDatos.Models;

public class Venta  
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public ApplicationUser Usuario { get; set; } = null!;
     public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}