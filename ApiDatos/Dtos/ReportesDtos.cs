namespace ApiDatos.Dtos;

public record IngresoPeriodoDto(string Periodo, decimal Ingresos, int CantidadVentas);
public record MasVendidoDto(int ProductoId, string Nombre, int UnidadesVendidas, decimal Ingresos, decimal Ganancia);
public record StockBajoDto(int ProductoId, string Nombre, string Sku, int StockActual, int StockMinimo);