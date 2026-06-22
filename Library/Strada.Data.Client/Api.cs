using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Strada.Data;

public static class Api
{
	#region Setup

	private static HttpClient _http;

	private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { IncludeFields = true };

	public static void Init(HttpClient http) => _http = http;

	#endregion

	#region Requests

	public static async Task<T> Get<T>(string route, object query = null)
	{
		var response = await _http.GetAsync(route + ToQuery(query));
		await EnsureSuccess(response);
		return await response.Content.ReadFromJsonAsync<T>(_json);
	}

	public static async Task<T> Post<T>(string route, object body, object query = null) =>
		await (await SendPost(route, body, query)).Content.ReadFromJsonAsync<T>(_json);

	public static async Task Post(string route, object body, object query = null) =>
		await SendPost(route, body, query);

	public static async Task<(MemoryStream stream, string fileName)> PostForFile(string route, object body, object query = null)
	{
		var response = await SendPost(route, body, query);

		var bytes = await response.Content.ReadAsByteArrayAsync();
		var fileName = (response.Content.Headers.ContentDisposition?.FileNameStar
			?? response.Content.Headers.ContentDisposition?.FileName)?.Trim('"');

		return (new MemoryStream(bytes), fileName);
	}

	public static async Task<T> Upload<T>(string route, Stream file, string fileName, object query = null)
	{
		using var content = new MultipartFormDataContent { { new StreamContent(file), "file", fileName } };
		var response = await _http.PostAsync(route + ToQuery(query), content);
		await EnsureSuccess(response);
		return await response.Content.ReadFromJsonAsync<T>(_json);
	}

	public static async Task<(MemoryStream stream, string contentType)> GetForFile(string route, object query = null)
	{
		var response = await _http.GetAsync(route + ToQuery(query));
		await EnsureSuccess(response);

		var bytes = await response.Content.ReadAsByteArrayAsync();
		return (new MemoryStream(bytes), response.Content.Headers.ContentType?.ToString());
	}

	#endregion

	#region Helpers

	private static async Task<HttpResponseMessage> SendPost(string route, object body, object query)
	{
		var response = await _http.PostAsJsonAsync(route + ToQuery(query), body, _json);
		await EnsureSuccess(response);
		return response;
	}

	private static async Task EnsureSuccess(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
			return;

		string message = null;
		try
		{
			var error = await response.Content.ReadFromJsonAsync<ApiError>(_json);
			message = error?.Message;
		}
		catch { /* non-JSON body (e.g. a raw 500 before the handler ran) — fall through */ }

		throw new Exception(string.IsNullOrWhiteSpace(message)
			? $"Request failed ({(int)response.StatusCode})."
			: message);
	}

	private static string ToQuery(object query)
	{
		if (query is null)
			return string.Empty;

		var pairs = query.GetType().GetProperties()
			.Select(p => (p.Name, Value: p.GetValue(query)))
			.Where(p => p.Value is not null)   // omit nulls so optional API params bind as null, not ""
			.Select(p => $"{p.Name}={Uri.EscapeDataString(FormatValue(p.Value))}");

		var queryString = string.Join("&", pairs);

		return queryString.Length == 0 ? string.Empty : "?" + queryString;
	}

	private static string FormatValue(object value) => value switch
	{
		DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
		DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
		TimeOnly t => t.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
		IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
		_ => value.ToString()
	};

	private class ApiError
	{
		public string Message { get; set; }
	}

	#endregion
}
