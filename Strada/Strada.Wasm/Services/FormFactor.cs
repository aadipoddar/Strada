using Strada.Shared.Services;

namespace Strada.Wasm.Services;

public class FormFactor : IFormFactor
{
	public string GetFormFactor() =>
		"Web";

	public string GetPlatform() =>
		Environment.OSVersion.ToString();
}
