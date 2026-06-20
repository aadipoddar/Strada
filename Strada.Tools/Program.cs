using OfficeOpenXml;

using Strada.Data.DataAccess;

SqlDataAccess.SetupConfiguration();

FileInfo fileInfo = new(@"C:\Others\document.xlsx");

ExcelPackage.License.SetNonCommercialPersonal("AadiSoft");

using var package = new ExcelPackage(fileInfo);

await package.LoadAsync(fileInfo);

var worksheet1 = package.Workbook.Worksheets[0];
// var worksheet2 = package.Workbook.Worksheets[1];


Console.WriteLine("Finished importing Items.");
Console.ReadLine();

#region Unused

/*

await ImportVehicles(worksheet1);

await ImportDrivers(worksheet1);

await ImportCards(worksheet1);

await ImportVehicleOMC(worksheet1);

await ImportVehicleDocuments(worksheet1);

static async Task ImportVehicles(ExcelWorksheet worksheet1)
{
	int row = 2;

	while (worksheet1.Cells[row, 1].Value != null)
	{
		var code = worksheet1.Cells[row, 1].Value.ToString();
		var shortCode = worksheet1.Cells[row, 2].Value.ToString();
		var chasis = worksheet1.Cells[row, 3].Value.ToString();
		var engine = worksheet1.Cells[row, 4].Value.ToString();
		var purchaseDateStr = worksheet1.Cells[row, 5].Value.ToString();
		var purchaseDate = DateTime.Parse(purchaseDateStr);
		var vehicleTypeId = worksheet1.Cells[row, 6].Value.ToString();
		var companyId = worksheet1.Cells[row, 7].Value.ToString();

		if (string.IsNullOrWhiteSpace(code) ||
			string.IsNullOrWhiteSpace(shortCode) ||
			string.IsNullOrWhiteSpace(chasis) ||
			string.IsNullOrWhiteSpace(engine) ||
			string.IsNullOrWhiteSpace(vehicleTypeId) ||
			string.IsNullOrWhiteSpace(companyId))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		code = code.Trim().RemoveSpace();
		shortCode = shortCode.Trim().RemoveSpace();
		engine = engine.Trim().RemoveSpace();
		chasis = chasis.Trim().RemoveSpace();

		Console.WriteLine("Inserting New Vehicle: " + code);
		await VehicleData.InsertVehicle(new()
		{
			Id = 0,
			Code = code,
			ShortCode = shortCode,
			ChasisCode = chasis,
			EngineCode = engine,
			OpeningKM = 0,
			Remarks = null,
			PurchaseDate = purchaseDate,
			VehicleTypeId = int.Parse(vehicleTypeId),
			CompanyId = int.Parse(companyId),
			Status = true
		});

		row++;
	}
}

static async Task ImportDrivers(ExcelWorksheet worksheet1)
 
static async Task ImportCards(ExcelWorksheet worksheet1)

static async Task ImportVehicleOMC(ExcelWorksheet worksheet1)
{
	int row = 1;

	var omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
	var vehicles = await CommonData.LoadTableData<VehicleModel>(FleetNames.Vehicle);

	while (worksheet1.Cells[row, 1].Value != null)
	{
		var name = worksheet1.Cells[row, 1].Value.ToString();
		var omc = worksheet1.Cells[row, 2].Value.ToString();

		if (string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(omc))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		var vehicle = vehicles.FirstOrDefault(x => x.Code.Equals(name, StringComparison.OrdinalIgnoreCase));
		var omcId = omcs.FirstOrDefault(x => x.Name.Equals(omc, StringComparison.OrdinalIgnoreCase))?.Id;

		vehicle.OMCId = omcId;
		Console.WriteLine("Inserting New Driver: " + name);
		await VehicleData.SaveTransaction(vehicle);

		row++;
	}
}

static async Task ImportVehicleDocuments(ExcelWorksheet worksheet1)
{
	int row = 1;

	var documentTypes = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);
	var vehicles = await CommonData.LoadTableData<VehicleModel>(FleetNames.Vehicle);

	while (worksheet1.Cells[row, 1].Value != null)
	{
		var name = worksheet1.Cells[row, 1].Value.ToString();
		var np = worksheet1.Cells[row, 2].Value?.ToString();
		var tax = worksheet1.Cells[row, 3].Value?.ToString();
		var insurance = worksheet1.Cells[row, 4].Value?.ToString();
		var fSala = worksheet1.Cells[row, 5].Value?.ToString();
		var rc = worksheet1.Cells[row, 6].Value?.ToString();
		var fitness = worksheet1.Cells[row, 7].Value?.ToString();

		name = name.RemoveSpace().Trim().Replace("/", "").Replace("-", "");

		if (string.IsNullOrWhiteSpace(name))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		var vehicle = vehicles.FirstOrDefault(x => x.Code.Contains(name, StringComparison.OrdinalIgnoreCase));
		var npDate = DateTime.TryParse(np, out var nDate) ? nDate : (DateTime?)null;
		var taxDate = DateTime.TryParse(tax, out var tDate) ? tDate : (DateTime?)null;
		var insuranceDate = DateTime.TryParse(insurance, out var iDate) ? iDate : (DateTime?)null;
		var rcDate = DateTime.TryParse(rc, out var rDate) ? rDate : (DateTime?)null;
		var fSalaDate = DateTime.TryParse(fSala, out var fsDate) ? fsDate : (DateTime?)null;
		var fitnessDate = DateTime.TryParse(fitness, out var fDate) ? fDate : (DateTime?)null;

		if (vehicle == null)
		{
			Console.WriteLine("Vehicle Not Found for Row = " + name);
			row++;
			continue;
		}

		Console.WriteLine("Inserting New Doucment: " + name);

		if (npDate.HasValue)
			await VehicleDocumentData.SaveTransaction(new()
			{
				TransactionNo = "NP-" + vehicle.Code + row.ToString(),
				VehicleId = vehicle.Id,
				VehicleDocumentTypeId = 4,
				RenewalDate = npDate.Value,
				TransactionDateTime = DateTime.Now.AddYears(-2),
				CreatedAt = DateTime.Now,
				CreatedBy = 1,
				CreatedFromPlatform = "Import Script",
			});

		if (taxDate.HasValue)
			await VehicleDocumentData.SaveTransaction(new()
			{
				TransactionNo = "TAX-" + vehicle.Code + row.ToString(),
				VehicleId = vehicle.Id,
				VehicleDocumentTypeId = 5,
				RenewalDate = taxDate.Value,
				TransactionDateTime = DateTime.Now.AddYears(-2),
				CreatedAt = DateTime.Now,
				CreatedBy = 1,
				CreatedFromPlatform = "Import Script",
			});

		if (insuranceDate.HasValue)
			await VehicleDocumentData.SaveTransaction(new()
			{
				TransactionNo = "INSURANCE-" + vehicle.Code + row.ToString(),
				VehicleId = vehicle.Id,
				VehicleDocumentTypeId = 6,
				RenewalDate = insuranceDate.Value,
				TransactionDateTime = DateTime.Now.AddYears(-2),
				CreatedAt = DateTime.Now,
				CreatedBy = 1,
				CreatedFromPlatform = "Import Script",
			});

		if (fSalaDate.HasValue)
			await VehicleDocumentData.SaveTransaction(new()
			{
				TransactionNo = "5SALA-" + vehicle.Code + row.ToString(),
				VehicleId = vehicle.Id,
				VehicleDocumentTypeId = 8,
				RenewalDate = fSalaDate.Value,
				TransactionDateTime = DateTime.Now.AddYears(-2),
				CreatedAt = DateTime.Now,
				CreatedBy = 1,
				CreatedFromPlatform = "Import Script",
			});

		if (rcDate.HasValue)
			await VehicleDocumentData.SaveTransaction(new()
			{
				TransactionNo = "RC-" + vehicle.Code + row.ToString(),
				VehicleId = vehicle.Id,
				VehicleDocumentTypeId = 9,
				RenewalDate = rcDate.Value,
				TransactionDateTime = DateTime.Now.AddYears(-10),
				CreatedAt = DateTime.Now,
				CreatedBy = 1,
				CreatedFromPlatform = "Import Script",
			});

		if (fitnessDate.HasValue)
			await VehicleDocumentData.SaveTransaction(new()
			{
				TransactionNo = "FITNESS-" + vehicle.Code + row.ToString(),
				VehicleId = vehicle.Id,
				VehicleDocumentTypeId = 7,
				RenewalDate = fitnessDate.Value,
				TransactionDateTime = DateTime.Now.AddYears(-2),
				CreatedAt = DateTime.Now,
				CreatedBy = 1,
				CreatedFromPlatform = "Import Script",
			});

		row++;
	}
}

 */
#endregion