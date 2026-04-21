using StradaLibrary.Data.Common;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Exports.Fleet.VehicleRoute;

public static class VehicleRouteExpenseTypeExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleRouteExpenseTypeModel> vehicleRouteExpenseTypeData,
		ReportExportType exportType)
	{
		var enrichedData = vehicleRouteExpenseTypeData.Select(vehicleRouteExpenseType => new
		{
			vehicleRouteExpenseType.Id,
			vehicleRouteExpenseType.Name,
			vehicleRouteExpenseType.Code,
			vehicleRouteExpenseType.Remarks,
			Status = vehicleRouteExpenseType.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleRouteExpenseTypeModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRouteExpenseTypeModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteExpenseTypeModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleRouteExpenseTypeModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleRouteExpenseTypeModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleRouteExpenseTypeModel.Id),
			nameof(VehicleRouteExpenseTypeModel.Name),
			nameof(VehicleRouteExpenseTypeModel.Code),
			nameof(VehicleRouteExpenseTypeModel.Remarks),
			nameof(VehicleRouteExpenseTypeModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Route_Expense_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE ROUTE EXPENSE TYPE MASTER",
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
				"VEHICLE ROUTE EXPENSE TYPE",
				"Vehicle Route Expense Type Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}