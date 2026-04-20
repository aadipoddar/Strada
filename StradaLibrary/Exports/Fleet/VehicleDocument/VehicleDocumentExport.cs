using StradaLibrary.Data.Common;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleDocument;

namespace StradaLibrary.Exports.Fleet.VehicleDocument;

public static class VehicleDocumentExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportTransaction(
		IEnumerable<VehicleDocumentOverviewModel> vehicleDocumentData,
		ReportExportType exportType)
	{
		var enrichedData = vehicleDocumentData.Select(vehicleDocument => new
		{
			vehicleDocument.Id,
			vehicleDocument.TransactionNo,
			vehicleDocument.TransactionDateTime,
			vehicleDocument.FinancialYear,
			vehicleDocument.VehicleDocumentType,
			vehicleDocument.Vehicle,
			vehicleDocument.CurrentKM,
			vehicleDocument.Rate,
			vehicleDocument.RenewalDate,
			vehicleDocument.Remarks,
			vehicleDocument.DocumentUrl,
			Status = vehicleDocument.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleDocumentOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.TransactionNo)] = new() { DisplayName = "Transaction No", Alignment = CellAlignment.Left, IsRequired = true, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.TransactionDateTime)] = new() { DisplayName = "Transaction Date", Alignment = CellAlignment.Center, Format = "dd-MMM-yyyy", IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.VehicleDocumentType)] = new() { DisplayName = "Document Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.Vehicle)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Alignment = CellAlignment.Right, Format = "#,##0.00", IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.Rate)] = new() { DisplayName = "Rate", Alignment = CellAlignment.Right, Format = "#,##0.00", IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.RenewalDate)] = new() { DisplayName = "Renewal Date", Alignment = CellAlignment.Center, Format = "dd-MMM-yyyy", IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.DocumentUrl)] = new() { DisplayName = "Document URL", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleDocumentOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleDocumentOverviewModel.Id),
			nameof(VehicleDocumentOverviewModel.TransactionNo),
			nameof(VehicleDocumentOverviewModel.TransactionDateTime),
			nameof(VehicleDocumentOverviewModel.FinancialYear),
			nameof(VehicleDocumentOverviewModel.VehicleDocumentType),
			nameof(VehicleDocumentOverviewModel.Vehicle),
			nameof(VehicleDocumentOverviewModel.CurrentKM),
			nameof(VehicleDocumentOverviewModel.Rate),
			nameof(VehicleDocumentOverviewModel.RenewalDate),
			nameof(VehicleDocumentOverviewModel.Remarks),
			nameof(VehicleDocumentOverviewModel.DocumentUrl),
			nameof(VehicleDocumentOverviewModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Document_Transaction_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE DOCUMENT TRANSACTION",
				null,
				null,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: true
			);

			return (stream, fileName + ".pdf");
		}

		var excelStream = await ExcelReportExportUtil.ExportToExcel(
			enrichedData,
			"VEHICLE DOCUMENT TRANSACTION",
			"Vehicle Document Data",
			null,
			null,
			columnSettings,
			columnOrder
		);

		return (excelStream, fileName + ".xlsx");
	}
}