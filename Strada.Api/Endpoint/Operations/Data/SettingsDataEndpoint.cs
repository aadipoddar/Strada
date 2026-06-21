using Strada.Data.Operations.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Operations.Data;

public class SettingsDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(SettingsDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(SettingsData.LoadSettingsByKey),
			(string Key) => SettingsData.LoadSettingsByKey(Key));

		group.MapPost(nameof(SettingsData.UpdateSettings), SettingsData.UpdateSettings);
		group.MapPost(nameof(SettingsData.ResetSettings), SettingsData.ResetSettings);
	}
}
