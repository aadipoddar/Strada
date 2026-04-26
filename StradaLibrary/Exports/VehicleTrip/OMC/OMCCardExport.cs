using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.VehicleTrip.OMC;

namespace StradaLibrary.Exports.VehicleTrip.OMC;

public static class OMCCardExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<OMCCardModel> omcCardData,
		ReportExportType exportType)
	{
		var omcs = await CommonData.LoadTableData<OMCModel>(VehicleTripNames.OMC);

		var enrichedData = omcCardData.Select(omc => new
		{
			omc.Id,
			omc.CardNumber,
			omc.Code,
			OMC = omcs.FirstOrDefault(o => o.Id == omc.OMCId)?.Name ?? "N/A",
			omc.OpeningBalance,
			omc.Remarks,
			Status = omc.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(OMCCardModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardModel.CardNumber)] = new() { DisplayName = "Card Number", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(OMCCardModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			["OMC"] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left },
			[nameof(OMCCardModel.OpeningBalance)] = new() { DisplayName = "Opening Balance", Alignment = CellAlignment.Right, Format = "#,##0.00" },
			[nameof(OMCCardModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(OMCCardModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(OMCCardModel.Id),
			nameof(OMCCardModel.CardNumber),
			nameof(OMCCardModel.Code),
			"OMC",
			nameof(OMCCardModel.OpeningBalance),
			nameof(OMCCardModel.Remarks),
			nameof(OMCCardModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"OMC_Card_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"OMC CARD MASTER",
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
				"OMC Cards",
				"OMC Card Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}