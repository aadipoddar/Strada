namespace Strada.Shared.Pages.Fleet.Trip.Reports;

internal sealed record YesNoFilterOption(int Id, string Name);

internal static class TripFilterOptions
{
	public const int All = 0;
	public const int Yes = 1;
	public const int No = 2;

	public static readonly List<YesNoFilterOption> YesNo =
	[
		new(All, "All"),
		new(Yes, "Yes"),
		new(No, "No"),
	];
}
