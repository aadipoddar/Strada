using Microsoft.JSInterop;

using Strada.Library.APIService;
using Strada.Library.Operations.Models;

namespace Strada.Shared.Pages.Fleet.Tracking;

public partial class LiveFleetMapPage : IAsyncDisposable
{
	private const string MapId = "liveFleetMap";

	private UserModel _user;
	private bool _isLoading = true;
	private bool _isProcessing;
	private bool _mapPending;
	private string _error;

	private List<WheelsEyeVehicleModel> _vehicles = [];

	private int _movingCount;
	private int _idleCount;
	private int _stoppedCount;
	private int _offlineCount;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			try
			{
				_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
				await LoadData();
			}
			catch { NavigationManager.NavigateTo(OperationRouteNames.Dashboard); }
			return;
		}

		// The map div only exists once loading is done; draw after that render.
		if (_mapPending)
		{
			_mapPending = false;
			await RenderMap();
		}
	}

	private async Task LoadData()
	{
		_isProcessing = true;
		_error = null;
		StateHasChanged();

		try
		{
			_vehicles = await WheelsEyeApiService.GetLiveVehicles();
		}
		catch (Exception ex)
		{
			_vehicles = [];
			_error = ex.Message;
		}

		_movingCount = _vehicles.Count(v => v.Status == "Moving");
		_idleCount = _vehicles.Count(v => v.Status == "Idle");
		_stoppedCount = _vehicles.Count(v => v.Status == "Stopped");
		_offlineCount = _vehicles.Count(v => v.Status == "Offline");

		_isProcessing = false;
		_isLoading = false;
		_mapPending = true;
		StateHasChanged();
	}

	private async Task RenderMap()
	{
		var markers = _vehicles
			.Where(v => v.HasValidPosition)
			.Select(v => new
			{
				number = v.VehicleNumber,
				lat = v.Latitude,
				lng = v.Longitude,
				status = v.Status,
				speed = v.Speed,
				ignition = v.IgnitionOn,
				type = v.VehicleType,
				address = v.Address,
				updated = v.LastUpdate == default ? "" : v.LastUpdate.ToString("dd MMM, hh:mm tt")
			})
			.ToArray();

		await JSRuntime.InvokeVoidAsync("showFleet", MapId, markers);
	}

	private async Task Refresh()
	{
		if (_isProcessing)
			return;

		await LoadData();
	}

	public async ValueTask DisposeAsync()
	{
		try { await JSRuntime.InvokeVoidAsync("disposeFleet", MapId); }
		catch { /* circuit/WebView already gone — nothing to clean up */ }

		GC.SuppressFinalize(this);
	}
}
