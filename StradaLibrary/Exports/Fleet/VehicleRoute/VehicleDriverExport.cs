using StradaLibrary.Data.Common;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Exports.Fleet.VehicleRoute;

public static class VehicleDriverExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleDriverModel> vehicleDriverData,
		ReportExportType exportType)
	{
		var enrichedData = vehicleDriverData.Select(vehicleDriver => new
		{
			vehicleDriver.Id,
			vehicleDriver.Name,
			vehicleDriver.Mobile,
			vehicleDriver.Code,
			vehicleDriver.Remarks,
			Status = vehicleDriver.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleDriverModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleDriverModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleDriverModel.Mobile)] = new() { DisplayName = "Mobile", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleDriverModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleDriverModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleDriverModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleDriverModel.Id),
			nameof(VehicleDriverModel.Name),
			nameof(VehicleDriverModel.Mobile),
			nameof(VehicleDriverModel.Code),
			nameof(VehicleDriverModel.Remarks),
			nameof(VehicleDriverModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Driver_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE DRIVER MASTER",
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
				"VEHICLE DRIVER",
				"Vehicle Driver Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}