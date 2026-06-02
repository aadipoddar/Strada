using Microsoft.AspNetCore.Components;

namespace Strada.Shared.Components.Button;

public partial class RecheckSaveButton
{
	[Parameter] public EventCallback OnClick { get; set; }
	[Parameter] public bool Disabled { get; set; } = false;
	[Parameter] public string Text { get; set; } = "Recheck & Save";
	[Parameter] public string Title { get; set; } = "Recheck & Save";
}
