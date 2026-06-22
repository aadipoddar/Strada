namespace StradaLibrary.DataAccess;

public static partial class Secrets
{
	public static readonly string DatabaseName = "Strada";

	public static readonly string AzureConnectionString;
	public static readonly string AzureTestingConnectionString;
	public static readonly string LocalConnectionString = "Data Source=AADILAPIKIIT;Initial Catalog=Strada;Integrated Security=True;Connect Timeout=300;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

	public static readonly string AzureBlobStorageAccountName = "stradastore";
	public static readonly string AzureBlobStorageConnectionString;
	public static readonly string AzureBlobStorageAccountKey;

	public static readonly string SyncfusionLicense;

	public static readonly string WheelsEyeAccessToken;

	public static readonly string Email = "softaadi@gmail.com";
	public static readonly string EmailPassword;

	public static readonly string ToName = "Strada";

	public static readonly string OnlineFullLogoPath = "https://raw.githubusercontent.com/aadipoddar/Strada/refs/heads/main/Strada/Strada.Web/wwwroot/images/logo_full.png";
	public static readonly string AadiSoftWebsite = "https://aadisoft.vercel.app";
	public static readonly string AppWebsite = "https://strada.azurewebsites.net";
}