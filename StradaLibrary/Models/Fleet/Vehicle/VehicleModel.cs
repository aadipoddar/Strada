namespace StradaLibrary.Models.Fleet.Vehicle;

public class VehicleModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ShortCode { get; set; }
    public string ChasisCode { get; set; }
    public string EngineCode { get; set; }
    public int VehicleTypeId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal? OpeningHour { get; set; }
    public decimal? OpeningKM { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class VehicleTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}
