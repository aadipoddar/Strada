namespace StradaLibrary.DataAccess;

public static partial class Secrets
{
	public static string DatabaseName = "Strada";

	public static string AzureConnectionString;
	public static string AzureTestingConnectionString;
	public static string LocalConnectionString = "Data Source=AADILAPIKIIT;Initial Catalog=Strada;Integrated Security=True;Connect Timeout=300;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

	public static string AzureBlobStorageAccountName = "stradastore";
	public static string AzureBlobStorageConnectionString;
	public static string AzureBlobStorageAccountKey;

	public static string SyncfusionLicense;

	public static string WheelsEyeAccessToken;

	public static string Email = "softaadi@gmail.com";
	public static string EmailPassword;

	public static string ToName = "Strada";

	public static string OnlineFullLogoPath = "https://raw.githubusercontent.com/aadipoddar/Strada/refs/heads/main/Strada/Strada.Web/wwwroot/images/logo_full.png";
	public static string AadiSoftWebsite = "https://aadisoft.vercel.app";
	public static string AppWebsite = "https://strada.azurewebsites.net";
}