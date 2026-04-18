using StradaLibrary.DataAccess;

Secrets.SetupConfiguration();

//FileInfo fileInfo = new(@"C:\Others\order.xlsx");

//ExcelPackage.License.SetNonCommercialPersonal("AadiSoft");

//using var package = new ExcelPackage(fileInfo);

//await package.LoadAsync(fileInfo);

//var worksheet1 = package.Workbook.Worksheets[0];
//var worksheet2 = package.Workbook.Worksheets[1];

Console.WriteLine("Finished importing Items.");
Console.ReadLine();