using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Victoria.Responses.Route;

namespace Victoria {
    /// <summary>
    ///     Interacting with Lavalink's RoutePlanner API.
    /// </summary>
    public sealed class RoutePlanner {
        /// <summary>
        ///     Returns the current status of route planner.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task<RouteStatus> GetStatusAsync(LavaConfig lavaConfig) {
            if (lavaConfig == null) {
                throw new ArgumentNullException(nameof(lavaConfig));
            }

            var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"{lavaConfig.HttpEndpoint}/routeplanner/status") {
                    Headers = {
                        {"Authorization", lavaConfig.Authorization}
                    }
                };

            var routeStatus = await VictoriaExtensions.ReadAsJsonAsync<RouteStatus>(requestMessage);
            if (!routeStatus.Equals(default)) {
                return routeStatus;
            }

            var routeResponse = await VictoriaExtensions.ReadAsJsonAsync<RouteResponse>(requestMessage);
            throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
        }

        /// <summary>
        ///     Unmark a failed address.
        /// </summary>
        /// <param name="lavaConfig"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws if Lavalink throws an exception.</exception>
        public static async Task FreeAddressAsync(LavaConfig lavaConfig, string address) {
            if (lavaConfig == null) {
                throw new ArgumentNullException(nameof(lavaConfig));
            }

            if (string.IsNullOrWhiteSpace(address)) {
                throw new ArgumentNullException(nameof(address));
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{lavaConfig.HttpEndpoint}/routeplanner/free/address") {
                Headers = {
                    {"Authorization", lavaConfig.Authorization}
                },
                Content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(new {
                    address
                }))
            };

            var routeResponse = await VictoriaExtensions.ReadAsJsonAsync<RouteResponse>(requestMessage);
            throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
        }

        /// <summary>
        ///     Unmark all failed address.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Throws if Lavalink throws an exception.</exception>
        public static async Task FreeAllAsync(LavaConfig lavaConfig) {
            if (lavaConfig == null) {
                throw new ArgumentNullException(nameof(lavaConfig));
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{lavaConfig.HttpEndpoint}/routeplanner/free/all") {
                Headers = {
                    {"Authorization", lavaConfig.Authorization}
                }
            };

            var routeResponse = await VictoriaExtensions.ReadAsJsonAsync<RouteResponse>(requestMessage);
            throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
        }
    }
}