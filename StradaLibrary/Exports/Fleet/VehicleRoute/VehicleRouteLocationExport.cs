using StradaLibrary.Data.Common;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Exports.Fleet.VehicleRoute;

public static class VehicleRouteLocationExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleRouteLocationModel> routeLocationData,
		ReportExportType exportType)
	{
		var enrichedData = routeLocationData.Select(routeLocation => new
		{
			routeLocation.Id,
			routeLocation.Name,
			routeLocation.Code,
			routeLocation.Remarks,
			Status = routeLocation.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleRouteLocationModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRouteLocationModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteLocationModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteLocationModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleRouteLocationModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleRouteLocationModel.Id),
			nameof(VehicleRouteLocationModel.Name),
			nameof(VehicleRouteLocationModel.Code),
			nameof(VehicleRouteLocationModel.Remarks),
			nameof(VehicleRouteLocationModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Route_Location_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"ROUTE LOCATION MASTER",
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
				"ROUTE LOCATION",
				"Route Location Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
