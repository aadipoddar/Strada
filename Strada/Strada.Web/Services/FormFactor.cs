using Strada.Shared.Services;

namespace Strada.Web.Services;

public class FormFactor : IFormFactor
{
	public string GetFormFactor() =>
		"Web";

	public string GetPlatform() =>
		Environment.OSVersion.ToString();
}
