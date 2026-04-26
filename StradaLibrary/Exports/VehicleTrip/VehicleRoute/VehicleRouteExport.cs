using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.VehicleTrip.VehicleRoute;

namespace StradaLibrary.Exports.VehicleTrip.VehicleRoute;

public static class VehicleRouteExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleRouteModel> vehicleRouteData,
		ReportExportType exportType)
	{
		var routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(VehicleTripNames.VehicleRouteLocation);

		var enrichedData = vehicleRouteData.Select(vehicleRoute => new
		{
			vehicleRoute.Id,
			FromLocation = routeLocations.FirstOrDefault(rl => rl.Id == vehicleRoute.FromLocationId)?.Name ?? "N/A",
			ToLocation = routeLocations.FirstOrDefault(rl => rl.Id == vehicleRoute.ToLocationId)?.Name ?? "N/A",
			vehicleRoute.Code,
			vehicleRoute.EstimatedHours,
			vehicleRoute.EstimatedDistance,
			vehicleRoute.EstimatedFuelConsumption,
			vehicleRoute.EstimatedCost,
			vehicleRoute.Remarks,
			Status = vehicleRoute.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleRouteModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			["FromLocation"] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IsRequired = true },
			["ToLocation"] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteModel.EstimatedHours)] = new() { DisplayName = "Estimated Hours", Alignment = CellAlignment.Right, Format = "#,##0", IncludeInTotal = false },
			[nameof(VehicleRouteModel.EstimatedDistance)] = new() { DisplayName = "Estimated Distance", Alignment = CellAlignment.Right, Format = "#,##0", IncludeInTotal = false },
			[nameof(VehicleRouteModel.EstimatedFuelConsumption)] = new() { DisplayName = "Estimated Fuel", Alignment = CellAlignment.Right, Format = "#,##0", IncludeInTotal = false },
			[nameof(VehicleRouteModel.EstimatedCost)] = new() { DisplayName = "Estimated Cost", Alignment = CellAlignment.Right, Format = "#,##0.00", IncludeInTotal = false },
			[nameof(VehicleRouteModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleRouteModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleRouteModel.Id),
			"FromLocation",
			"ToLocation",
			nameof(VehicleRouteModel.Code),
			nameof(VehicleRouteModel.EstimatedHours),
			nameof(VehicleRouteModel.EstimatedDistance),
			nameof(VehicleRouteModel.EstimatedFuelConsumption),
			nameof(VehicleRouteModel.EstimatedCost),
			nameof(VehicleRouteModel.Remarks),
			nameof(VehicleRouteModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Route_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE ROUTE MASTER",
				null,
				null,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: false
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				enrichedData,
				"VEHICLE ROUTE",
				"Vehicle Route Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}