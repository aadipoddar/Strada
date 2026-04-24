using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.FinancialAccounting;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Fleet.VehicleTrip;

namespace StradaLibrary.Exports.Fleet.VehicleTrip;

public static class VehicleTripReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<VehicleTripOverviewModel> tripData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		VehicleRouteOverviewModel route = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripOverviewModel.TransactionNo),
				nameof(VehicleTripOverviewModel.CompanyName),
				nameof(VehicleTripOverviewModel.TransactionDateTime),
				nameof(VehicleTripOverviewModel.FinancialYear),
				nameof(VehicleTripOverviewModel.ChallanNo),
				nameof(VehicleTripOverviewModel.OMCName),
				nameof(VehicleTripOverviewModel.VehicleCode),
				nameof(VehicleTripOverviewModel.FromLocation),
				nameof(VehicleTripOverviewModel.ToLocation),
				nameof(VehicleTripOverviewModel.RouteDisplay),
				nameof(VehicleTripOverviewModel.DriverName),
				nameof(VehicleTripOverviewModel.DriverMobile),
				nameof(VehicleTripOverviewModel.DriverDisplay),
				nameof(VehicleTripOverviewModel.Quantity),
				nameof(VehicleTripOverviewModel.EstimatedDistance),
				nameof(VehicleTripOverviewModel.EstimatedHours),
				nameof(VehicleTripOverviewModel.EstimatedFuelConsumption),
				nameof(VehicleTripOverviewModel.EstimatedCost),
				nameof(VehicleTripOverviewModel.TotalExpense),
				nameof(VehicleTripOverviewModel.Remarks),
				nameof(VehicleTripOverviewModel.CreatedByName),
				nameof(VehicleTripOverviewModel.CreatedAt),
				nameof(VehicleTripOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripOverviewModel.LastModifiedAt),
				nameof(VehicleTripOverviewModel.LastModifiedFromPlatform),
				nameof(VehicleTripOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.VehicleCode));

			if (route is not null)
			{
				columnOrder.Remove(nameof(VehicleTripOverviewModel.FromLocation));
				columnOrder.Remove(nameof(VehicleTripOverviewModel.ToLocation));
				columnOrder.Remove(nameof(VehicleTripOverviewModel.RouteDisplay));
			}

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripOverviewModel.TransactionNo),
				nameof(VehicleTripOverviewModel.CompanyName),
				nameof(VehicleTripOverviewModel.TransactionDateTime),
				nameof(VehicleTripOverviewModel.ChallanNo),
				nameof(VehicleTripOverviewModel.OMCName),
				nameof(VehicleTripOverviewModel.VehicleCode),
				nameof(VehicleTripOverviewModel.RouteDisplay),
				nameof(VehicleTripOverviewModel.DriverDisplay),
				nameof(VehicleTripOverviewModel.Quantity),
				nameof(VehicleTripOverviewModel.TotalExpense),
				nameof(VehicleTripOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.RouteDisplay));

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.Status));
		}

		string fileName = $"VEHICLE_TRIP_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				tripData,
				"VEHICLE TRIP REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				tripData,
				"VEHICLE TRIP REPORT",
				"Vehicle Trip Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
