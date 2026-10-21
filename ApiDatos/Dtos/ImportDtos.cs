namespace ApiDatos.Dtos;

public record ProductoImportDto(string Nombre, string Sku, decimal PrecioCosto, decimal PrecioVenta, int StockActual, int StockMinimo);
public record ImportResultDto(int Creados, int Actualizados, List<string> Errores);