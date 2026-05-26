using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Strada.Shared.Components.Button;

public partial class FioriTile
{
	/// <summary>
	/// The tile title shown at the top.
	/// </summary>
	[Parameter] public string Title { get; set; } = string.Empty;

	/// <summary>
	/// Optional secondary line shown in muted text under the title.
	/// </summary>
	[Parameter] public string Subtitle { get; set; } = string.Empty;

	/// <summary>
	/// The SVG icon content shown at the bottom-left of the tile.
	/// </summary>
	[Parameter] public RenderFragment? IconContent { get; set; }

	/// <summary>
	/// The click event callback.
	/// </summary>
	[Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
}
