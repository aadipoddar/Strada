using Strada.Shared.Services;

namespace Strada.Wasm.Services;

public class NotificationService : INotificationService
{
	public async Task ShowLocalNotification(int id, string title, string subTitle, string description) =>
		await Task.CompletedTask;
}
