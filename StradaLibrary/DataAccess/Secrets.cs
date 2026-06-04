using Microsoft.Extensions.Configuration;

using System.Reflection;

namespace StradaLibrary.DataAccess;

public static partial class Secrets
{
	public static string DatabaseName => "Strada";

	public static string AzureConnectionString = GetSecret(nameof(AzureConnectionString));
	public static string AzureTestingConnectionString = GetSecret(nameof(AzureTestingConnectionString));
	public static string LocalConnectionString = "Data Source=AADILAPIKIIT;Initial Catalog=Strada;Integrated Security=True;Connect Timeout=300;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

	public static string AzureBlobStorageAccountName => "stradastore";
	public static string AzureBlobStorageConnectionString = GetSecret(nameof(AzureBlobStorageConnectionString));
	public static string AzureBlobStorageAccountKey = GetSecret(nameof(AzureBlobStorageAccountKey));

	public static string SyncfusionLicense = GetSecret(nameof(SyncfusionLicense));

	public static string WheelsEyeAccessToken = GetSecret(nameof(WheelsEyeAccessToken));

	public static string Email => "softaadi@gmail.com";
	public static string EmailPassword = GetSecret(nameof(EmailPassword));

	public static string ToEmail = "ajay@ashokroadlines.com";
	public static string ToName => "Strada";

	public static string OnlineFullLogoPath => "https://raw.githubusercontent.com/aadipoddar/Strada/refs/heads/main/Strada/Strada.Web/wwwroot/images/logo_full.png";
	public static string AadiSoftWebsite => "https://aadisoft.vercel.app";
	public static string AppWebsite => "https://strada.azurewebsites.net";
	private static string GetSecret(string key) =>
		new ConfigurationBuilder()
			.AddUserSecrets(Assembly.GetExecutingAssembly())
			.AddEnvironmentVariables()
			.Build()
			.GetSection(key).Value;
}