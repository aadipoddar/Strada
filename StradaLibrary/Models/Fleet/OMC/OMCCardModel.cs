namespace StradaLibrary.Models.Fleet.OMC;

public class OMCCardModel
{
	public int Id { get; set; }
	public string CardNumber { get; set; }
	public string Code { get; set; }
	public int OMCId { get; set; }
	public decimal OpeningBalance { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}
