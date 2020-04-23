using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Victoria.Converters;
using Victoria.Responses.Rest;

namespace Victoria {
	/// <summary>
	///     Interacting with Lavalink's RoutePlanner API.
	/// </summary>
	public readonly struct RoutePlanner {
		private readonly HttpClient _httpClient;

		private static readonly JsonSerializerOptions SerializerOptions
			= new JsonSerializerOptions {
				Converters = {
					new RouteResponseConverter()
				}
			};

		internal RoutePlanner(HttpClient httpClient) {
			_httpClient = httpClient;
		}

		/// <summary>
		///     Returns the current status of route planner.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public async Task<RouteStatus> GetStatusAsync() {
			using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/routeplanner/status");
			using var responseMessage = await _httpClient.SendAsync(requestMessage)
			   .ConfigureAwait(false);

			var responseContent = await responseMessage.Content.ReadAsByteArrayAsync()
			   .ConfigureAwait(false);

			if (responseContent.TryDeserialize<RouteStatus>(out var routeStatus, SerializerOptions)) {
				return routeStatus;
			}

			var routeResponse = JsonSerializer.Deserialize<RouteResponse>(responseContent);
			throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
		}

		/// <summary>
		///     Unmark a failed address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		/// <exception cref="Exception">Throws if Lavalink throws an exception.</exception>
		public async Task FreeAddressAsync(string address) {
			var payload = JsonSerializer.SerializeToUtf8Bytes(new {
				address
			});

			var responseMessage = await _httpClient
			   .PostAsync("/routeplanner/free/address", new ByteArrayContent(payload))
			   .ConfigureAwait(false);

			if (responseMessage.IsSuccessStatusCode) {
				return;
			}

			var responseContent = await responseMessage.Content.ReadAsByteArrayAsync()
			   .ConfigureAwait(false);

			var routeResponse = JsonSerializer.Deserialize<RouteResponse>(responseContent);
			throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
		}

		/// <summary>
		///     Unmark all failed address.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception">Throws if Lavalink throws an exception.</exception>
		public async Task FreeAllAsync() {
			using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/routeplanner/free/all");
			using var responseMessage = await _httpClient.SendAsync(requestMessage)
			   .ConfigureAwait(false);

			if (responseMessage.StatusCode == HttpStatusCode.NoContent) {
				return;
			}

			var responseContent = await responseMessage.Content.ReadAsByteArrayAsync()
			   .ConfigureAwait(false);

			var routeResponse = JsonSerializer.Deserialize<RouteResponse>(responseContent);
			throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
		}
	}
}