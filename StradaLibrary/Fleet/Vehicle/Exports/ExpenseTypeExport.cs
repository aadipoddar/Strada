using StradaLibrary.Common;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Utils.ExportUtils;

namespace StradaLibrary.Fleet.Vehicle.Exports;

public static class ExpenseTypeExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<ExpenseTypeModel> expenseTypeData,
		ReportExportType exportType)
	{
		var enrichedData = expenseTypeData.Select(expenseType => new
		{
			expenseType.Id,
			expenseType.Name,
			expenseType.Code,
			expenseType.Remarks,
			Status = expenseType.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(ExpenseTypeModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseTypeModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(ExpenseTypeModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(ExpenseTypeModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(ExpenseTypeModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(ExpenseTypeModel.Id),
			nameof(ExpenseTypeModel.Name),
			nameof(ExpenseTypeModel.Code),
			nameof(ExpenseTypeModel.Remarks),
			nameof(ExpenseTypeModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Expense_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"EXPENSE TYPE MASTER",
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
				"EXPENSE TYPE MASTER",
				"Expense Type Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}