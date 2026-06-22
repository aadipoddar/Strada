using Strada.Library.Common;
using Strada.Library.Fleet.OMC.Models;
using Strada.Library.Utils.ExportUtils;

namespace Strada.Library.Fleet.OMC.Exports;

public static class OMCExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<OMCModel> omcData,
		ReportExportType exportType)
	{
		var enrichedData = omcData.Select(omc => new
		{
			omc.Id,
			omc.Name,
			omc.Code,
			omc.Remarks,
			Status = omc.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(OMCModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(OMCModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(OMCModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(OMCModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(OMCModel.Id),
			nameof(OMCModel.Name),
			nameof(OMCModel.Code),
			nameof(OMCModel.Remarks),
			nameof(OMCModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"OMC_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"OMC MASTER",
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
				"OMC",
				"OMC Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}