using StradaLibrary.Data.Common;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Exports.Fleet.Vehicle;

public static class VehicleExpenseTypeExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleExpenseTypeModel> vehicleExpenseTypeData,
		ReportExportType exportType)
	{
		var enrichedData = vehicleExpenseTypeData.Select(vehicleExpenseType => new
		{
			vehicleExpenseType.Id,
			vehicleExpenseType.Name,
			vehicleExpenseType.Code,
			vehicleExpenseType.Remarks,
			Status = vehicleExpenseType.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleExpenseTypeModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleExpenseTypeModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleExpenseTypeModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleExpenseTypeModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleExpenseTypeModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleExpenseTypeModel.Id),
			nameof(VehicleExpenseTypeModel.Name),
			nameof(VehicleExpenseTypeModel.Code),
			nameof(VehicleExpenseTypeModel.Remarks),
			nameof(VehicleExpenseTypeModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Expense_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE EXPENSE TYPE MASTER",
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
				"VEHICLE EXPENSE TYPE MASTER",
				"Vehicle Expense Type Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}