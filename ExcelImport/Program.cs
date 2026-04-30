using OfficeOpenXml;
using StradaLibrary.Data.Common;
using StradaLibrary.Data.Fleet.OMC;
using StradaLibrary.Data.Fleet.Vehicle;
using StradaLibrary.Data.Fleet.Route;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Vehicle;

Secrets.SetupConfiguration();

FileInfo fileInfo = new(@"C:\Others\vehicle.xlsx");

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

 */
#endregion
