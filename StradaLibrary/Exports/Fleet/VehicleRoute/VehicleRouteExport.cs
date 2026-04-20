using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Exports.Fleet.VehicleRoute;

public static class VehicleRouteExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleRouteModel> vehicleRouteData,
		ReportExportType exportType)
	{
		var routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation);

		var enrichedData = vehicleRouteData.Select(vehicleRoute => new
		{
			vehicleRoute.Id,
			FromLocation = routeLocations.FirstOrDefault(rl => rl.Id == vehicleRoute.FromLocationId)?.Name ?? "N/A",
			ToLocation = routeLocations.FirstOrDefault(rl => rl.Id == vehicleRoute.ToLocationId)?.Name ?? "N/A",
			vehicleRoute.Code,
			vehicleRoute.Remarks,
			Status = vehicleRoute.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleRouteModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			["FromLocation"] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IsRequired = true },
			["ToLocation"] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleRouteModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleRouteModel.Id),
			"FromLocation",
			"ToLocation",
			nameof(VehicleRouteModel.Code),
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