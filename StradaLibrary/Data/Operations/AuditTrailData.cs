using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Operations;
using System.Text;
using System.Text.Json;

namespace StradaLibrary.Data.Operations;

public static class AuditTrailData
{
	private static async Task<int> InsertAuditTrail(AuditTrailModel auditTrail, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(OperationNames.InsertAuditTrail, auditTrail, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Audit Trail.");

	public static async Task<string> GetTransactionDifference(List<object> recordeValue)
	{
		string diffValue = string.Empty;

		if (recordeValue is not null)
			foreach (var item in recordeValue)
				if (item is not null)
					diffValue += JsonSerializer.Serialize(item) + Environment.NewLine;

		return diffValue;
	}

	public static async Task SaveAuditTrail(AuditTrailModel auditTrail, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, auditTrail.CreatedBy, sqlDataAccessTransaction);
		auditTrail.CreatedByName = user.Name;
		await InsertAuditTrail(auditTrail, sqlDataAccessTransaction);
	}
}
