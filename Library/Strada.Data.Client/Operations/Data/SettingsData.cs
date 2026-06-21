using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Data;

public static class SettingsData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(SettingsData));

	public static Task<SettingsModel> LoadSettingsByKey(string Key) =>
		Api.Get<SettingsModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadSettingsByKey)), new { Key });

	public static Task<int> UpdateSettings(SettingsModel settingsModel) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(UpdateSettings)), settingsModel);

	public static Task<int> ResetSettings() =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ResetSettings)), new { });
}
