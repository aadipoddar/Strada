using StradaLibrary.Models.Fleet.VehicleExpense;
using StradaLibrary.Models.Fleet.VehicleTrip;

namespace StradaLibrary.Models.Fleet.Vehicle;

public class VehicleRegisterModel
{
	public int VehicleId { get; set; }
	public string VehicleCode { get; set; }

	public int TotalTrips { get; set; }
	public int LoadedTrips { get; set; }
	public int EmptyTrips { get; set; }
	public decimal TotalQuantity { get; set; }

	public decimal TotalTripExpenses { get; set; }
	public List<VehicleRegisterExpensesModel> TripExpenses { get; set; } = [];

	public decimal TotalVehicleExpenses { get; set; }
	public List<VehicleRegisterExpensesModel> VehicleExpenses { get; set; } = [];

	public int TotalBills { get; set; }
	public decimal TotalGrossAmount { get; set; }
	public decimal TotalTDSAmount { get; set; }
	public decimal TotalPenaltyAmount { get; set; }
	public decimal TotalNetAmount { get; set; }

	public decimal TotalProfitLoss { get; set; }

	public List<VehicleTripOverviewModel> VehicleTripOverviews { get; set; } = [];
	public List<VehicleExpenseDetailsOverviewModel> VehicleExpenseDetailsOverviews { get; set; } = [];
}

public class VehicleRegisterExpensesModel
{
	public int ExpenseTypeId { get; set; }
	public string ExpenseType { get; set; }
	public decimal ExpenseAmount { get; set; }
}