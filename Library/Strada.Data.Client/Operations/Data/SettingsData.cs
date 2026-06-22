using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Data;

public static class SettingsData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(SettingsData));

	public static async Task<SettingsModel> LoadSettingsByKey(string Key) =>
		await Api.Get<SettingsModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadSettingsByKey)), new { Key });

	public static async Task<int> UpdateSettings(SettingsModel settingsModel) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(UpdateSettings)), settingsModel);

	public static async Task<int> ResetSettings() =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ResetSettings)), new { });
}
