using Microsoft.Extensions.Caching.Memory;

using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Fleet.VehicleDocument.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Components.Dashboard;

public partial class DashboardAttention
{
	private int _warningDays = 30;

	private List<VehicleDocumentRenewalOverviewModel> _dueDocuments = [];
	private List<TripOverviewModel> _pendingTrips = [];
	private List<TripOverviewModel> _lossTrips = [];
	private List<VehicleModel> _idleVehicles = [];
	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];

	private SfGrid<VehicleDocumentRenewalOverviewModel> _sfGrid;
	private SfGrid<TripOverviewModel> _tripsGrid;
	private SfGrid<TripOverviewModel> _lossGrid;
	private SfGrid<VehicleModel> _idleGrid;

	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "Renew", Id = "Renew", IconCss = "e-icons e-refresh", Target = ".e-content" }
	];

	private readonly List<ContextMenuItemModel> _tripContextMenuItems =
	[
		new() { Text = "Bill", Id = "Bill", IconCss = "e-icons e-description", Target = ".e-content" }
	];

	private readonly List<ContextMenuItemModel> _lossContextMenuItems =
	[
		new() { Text = "View Trip", Id = "ViewTrip", IconCss = "e-icons e-eye", Target = ".e-content" }
	];

	private readonly List<ContextMenuItemModel> _idleContextMenuItems =
	[
		new() { Text = "View Vehicle", Id = "ViewVehicle", IconCss = "e-icons e-eye", Target = ".e-content" }
	];

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await LoadData();
	}

	private async Task LoadData()
	{
		LoadCachedAttentions();
		await LoadNewAttentions();

		var expiry = TimeSpan.FromMinutes(30);
		MemoryCache.Set(StorageFileNames.DueDocumentsDataFileName, _dueDocuments, expiry);
		MemoryCache.Set(StorageFileNames.UnBilledTripsDataFileName, _pendingTrips, expiry);
		MemoryCache.Set(StorageFileNames.LossTripsDataFileName, _lossTrips, expiry);
		MemoryCache.Set(StorageFileNames.IdleVehiclesDataFileName, _idleVehicles, expiry);
	}

	private void LoadCachedAttentions()
	{
		_dueDocuments = MemoryCache.Get<List<VehicleDocumentRenewalOverviewModel>>(StorageFileNames.DueDocumentsDataFileName) ?? [];
		_pendingTrips = MemoryCache.Get<List<TripOverviewModel>>(StorageFileNames.UnBilledTripsDataFileName) ?? [];
		_lossTrips = MemoryCache.Get<List<TripOverviewModel>>(StorageFileNames.LossTripsDataFileName) ?? [];
		_idleVehicles = MemoryCache.Get<List<VehicleModel>>(StorageFileNames.IdleVehiclesDataFileName) ?? [];
		StateHasChanged();
	}

	private async Task LoadNewAttentions()
	{
		try
		{
			var warningSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.ReportWarningDays);
			_warningDays = int.TryParse(warningSetting?.Value, out var days) ? days : 30;
			var currentDateTime = await CommonData.LoadCurrentDateTime();
			var warningWindowStart = currentDateTime.AddDays(-_warningDays);

			// Only documents due within the warning window or already expired.
			_dueDocuments = await CommonData.LoadTableData<VehicleDocumentRenewalOverviewModel>(FleetNames.VehicleDocumentRenewalOverview);
			_dueDocuments = [.. _dueDocuments.Where(_ => _.DaysRemaining < _warningDays).OrderBy(_ => _.DaysRemaining)];

			// All unbilled trips.
			_pendingTrips = await TripData.LoadTripOverviewByBillIdDate();
			_pendingTrips = [.. _pendingTrips.Where(_ => _.Status && _.BillId is null).OrderByDescending(_ => _.PendingDays)];

			// Trips within the warning window — used for both losses and vehicle activity.
			var windowTrips = await CommonData.LoadTableDataByDate<TripOverviewModel>(FleetNames.TripOverview, warningWindowStart, currentDateTime);
			windowTrips = [.. windowTrips.Where(_ => _.Status)];

			// Loss-making trips within the warning window (biggest loss first).
			_lossTrips = [.. windowTrips.Where(_ => _.ProfitLoss < 0).OrderBy(_ => _.ProfitLoss)];

			// Active vehicles that ran no trip in the warning window.
			var activeVehicleIds = windowTrips.Select(_ => _.VehicleId).ToHashSet();
			var vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
			_idleVehicles = [.. vehicles.Where(_ => !activeVehicleIds.Contains(_.Id)).OrderBy(_ => _.Code)];
			_companies = await CommonData.LoadTableData<CompanyModel>(AccountNames.Company);
			_omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
		}
		catch { }
		finally { StateHasChanged(); }
	}

	private void OnGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleDocumentRenewalOverviewModel> args)
	{
		if (args.Item.Id == "Renew")
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocument);
	}

	private void OnTripsContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		if (args.Item.Id == "Bill")
			NavigationManager.NavigateTo(PageRouteNames.Bill);
	}

	private void OnLossContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		if (args.Item.Id == "ViewTrip")
			NavigationManager.NavigateTo(PageRouteNames.Trip);
	}

	private void OnIdleContextMenuItemClicked(ContextMenuClickEventArgs<VehicleModel> args)
	{
		if (args.Item.Id == "ViewVehicle")
			NavigationManager.NavigateTo(PageRouteNames.VehicleMaster);
	}
}
