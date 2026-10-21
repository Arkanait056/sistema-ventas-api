namespace ApiDatos.Dtos;

public record DetalleVentaInputDto(int ProductoId, int Cantidad);
public record VentaInputDto(List<DetalleVentaInputDto> Detalles);

public record DetalleVentaOutputDto(int ProductoId, string NombreProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
public record VentaOutputDto(int Id, DateTime Fecha, string UsuarioId, decimal Total, List<DetalleVentaOutputDto> Detalles);