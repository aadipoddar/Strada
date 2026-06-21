using System.Net.Http.Json;

namespace Strada.Data;

public static class Api
{
	private static HttpClient _http;

	public static void Init(HttpClient http) => _http = http;

	public static async Task<T> Get<T>(string route, object query = null)
	{
		var response = await _http.GetAsync(route + ToQuery(query));
		await EnsureSuccess(response);
		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task<T> Post<T>(string route, object body, object query = null)
	{
		var response = await _http.PostAsJsonAsync(route + ToQuery(query), body);
		await EnsureSuccess(response);
		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task Post(string route, object body, object query = null)
	{
		var response = await _http.PostAsJsonAsync(route + ToQuery(query), body);
		await EnsureSuccess(response);
	}

	private static async Task EnsureSuccess(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
			return;

		string message = null;
		try
		{
			var error = await response.Content.ReadFromJsonAsync<ApiError>();
			message = error?.Message;
		}
		catch { /* non-JSON body (e.g. a raw 500 before the handler ran) — fall through */ }

		throw new Exception(string.IsNullOrWhiteSpace(message)
			? $"Request failed ({(int)response.StatusCode})."
			: message);
	}

	private class ApiError
	{
		public string Message { get; set; }
	}

	private static string ToQuery(object query)
	{
		if (query is null)
			return string.Empty;

		var pairs = query.GetType().GetProperties()
			.Select(p => $"{p.Name}={Uri.EscapeDataString(p.GetValue(query)?.ToString() ?? string.Empty)}");

		return "?" + string.Join("&", pairs);
	}
}
