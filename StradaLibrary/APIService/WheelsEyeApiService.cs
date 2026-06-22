using System.Globalization;
using System.Text.Json;

using StradaLibrary.DataAccess;

namespace StradaLibrary.APIService;

// Live vehicle locations from WheelsEye (currentLoc). Demo-grade: a single call
// that returns every truck's current position. No join to the vehicle master.
public static class WheelsEyeApiService
{
	private static readonly HttpClient _httpClient = new();
	private static readonly TimeZoneInfo _ist = ResolveIst();

	private const string BaseUrl = "https://api.wheelseye.com/currentLoc";

	public static async Task<List<WheelsEyeVehicleModel>> GetLiveVehicles()
	{
		var vehicles = new List<WheelsEyeVehicleModel>();
		var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		var pageNo = 0;
		var totalPages = 1;

		// currentLoc is paginated (size max 100). 84 trucks is one page today,
		// but loop so it keeps working as the fleet grows.
		while (pageNo < totalPages)
		{
			// isLocationReq=false skips reverse-geocoding (much faster); the map
			// plots from lat/long, so we don't need the street address.
			var url = $"{BaseUrl}?accessToken={Secrets.WheelsEyeAccessToken}&isLocationReq=false&pageNo={pageNo}&size=100";

			using var response = await _httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			await using var stream = await response.Content.ReadAsStreamAsync();
			using var json = await JsonDocument.ParseAsync(stream);

			// Success shape: { success, data: { totalPages, list: [...] } }.
			// Error shape: { success:false, message, errorCode } — no "data", so bail.
			if (!json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
				break;

			totalPages = Int(data, "totalPages") is var tp and > 0 ? tp : 1;

			if (data.TryGetProperty("list", out var list) && list.ValueKind == JsonValueKind.Array)
				foreach (var v in list.EnumerateArray())
				{
					var epoch = Long(v, "dttimeInEpoch");
					vehicles.Add(new()
					{
						VehicleNumber = Str(v, "vehicleNumber"),
						VehicleType = Str(v, "vehicleType"),
						Latitude = Dec(v, "latitude"),
						Longitude = Dec(v, "longitude"),
						Address = Str(v, "location"),
						Speed = (int)Math.Round(Dec(v, "speed")),
						IgnitionOn = Bool(v, "ignition"),
						Angle = Dec(v, "angle"),
						LastUpdate = ToIst(epoch),
						// No ping in 24h = a dead/offline device (some are years stale).
						IsStale = epoch <= 0 || epoch < nowUnix - 86400
					});
				}

			pageNo++;
		}

		return vehicles;
	}

	#region JSON helpers
	private static string Str(JsonElement e, string name) =>
		e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

	private static int Int(JsonElement e, string name) =>
		e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var n) ? n : 0;

	private static long Long(JsonElement e, string name) =>
		e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out var n) ? n : 0;

	private static decimal Dec(JsonElement e, string name)
	{
		if (e.TryGetProperty(name, out var p))
		{
			if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var n))
				return n;
			if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var s))
				return s;
		}
		return 0;
	}

	private static bool Bool(JsonElement e, string name) =>
		e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.True;

	private static DateTime ToIst(long epochSeconds) =>
		epochSeconds <= 0
			? default
			: TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(epochSeconds), _ist).DateTime;

	private static TimeZoneInfo ResolveIst()
	{
		foreach (var id in new[] { "India Standard Time", "Asia/Kolkata" })
			try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
			catch { /* try the next id */ }

		return TimeZoneInfo.Utc;
	}
	#endregion
}

public class WheelsEyeVehicleModel
{
	public string VehicleNumber { get; set; }
	public string VehicleType { get; set; }

	public decimal Latitude { get; set; }
	public decimal Longitude { get; set; }
	public string Address { get; set; }
	public bool HasValidPosition => Latitude != 0 && Longitude != 0;

	public int Speed { get; set; }
	public bool IgnitionOn { get; set; }
	public decimal Angle { get; set; }

	public DateTime LastUpdate { get; set; }
	public bool IsStale { get; set; }

	public string Status =>
		IsStale ? "Offline" : Speed > 0 ? "Moving" : IgnitionOn ? "Idle" : "Stopped";
}
