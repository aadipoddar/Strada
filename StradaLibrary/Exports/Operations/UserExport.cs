using StradaLibrary.Data.Common;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Exports.Operations;

public static class UserExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<UserModel> userData,
		ReportExportType exportType)
	{
		var enrichedData = userData.Select(user => new
		{
			user.Id,
			user.Name,
			user.Phone,
			user.Email,
			Accounts = user.Accounts ? "Yes" : "No",
			Fleet = user.Fleet ? "Yes" : "No",
			Reports = user.Reports ? "Yes" : "No",
			Admin = user.Admin ? "Yes" : "No",
			user.Remarks,
			user.FailedAttempts,
			user.CodeResends,
			user.LastCode,
			user.LastCodeDeviceId,
			user.LastCodeDateTime,
			Status = user.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(UserModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(UserModel.Phone)] = new() { DisplayName = "Phone", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(UserModel.Email)] = new() { DisplayName = "Email", Alignment = CellAlignment.Left },
			[nameof(UserModel.Accounts)] = new() { DisplayName = "Accounts", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Fleet)] = new() { DisplayName = "Fleet", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Reports)] = new() { DisplayName = "Reports", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Admin)] = new() { DisplayName = "Admin", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(UserModel.FailedAttempts)] = new() { DisplayName = "Failed Attempts", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.CodeResends)] = new() { DisplayName = "Code Resends", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.LastCode)] = new() { DisplayName = "Last Code", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.LastCodeDeviceId)] = new() { DisplayName = "Device ID", Alignment = CellAlignment.Left },
			[nameof(UserModel.LastCodeDateTime)] = new() { DisplayName = "Last Code Time", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(UserModel.Id),
			nameof(UserModel.Name),
			nameof(UserModel.Phone),
			nameof(UserModel.Email),
			nameof(UserModel.Accounts),
			nameof(UserModel.Fleet),
			nameof(UserModel.Reports),
			nameof(UserModel.Admin),
			nameof(UserModel.Remarks),
			nameof(UserModel.FailedAttempts),
			nameof(UserModel.CodeResends),
			nameof(UserModel.LastCode),
			nameof(UserModel.LastCodeDeviceId),
			nameof(UserModel.LastCodeDateTime),
			nameof(UserModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"User_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"USER MASTER",
				null,
				null,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: true
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				enrichedData,
				"USER MASTER",
				"User Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}