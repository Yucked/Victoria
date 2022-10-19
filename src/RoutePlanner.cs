using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Victoria.Node;
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
        public static async Task<RouteStatus> GetStatusAsync(NodeConfiguration nodeConfiguration) {
            if (nodeConfiguration == null) {
                throw new ArgumentNullException(nameof(nodeConfiguration));
            }

            var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"{nodeConfiguration.HttpEndpoint}/routeplanner/status") {
                    Headers = {
                        {"Authorization", nodeConfiguration.Authorization}
                    }
                };

            var routeStatus = await VictoriaExtensions.ReadAsJsonAsync<RouteStatus>(requestMessage);
            if (routeStatus is not null) {
                return routeStatus;
            }

            var routeResponse = await VictoriaExtensions.ReadAsJsonAsync<RouteResponse>(requestMessage);
            throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
        }

        /// <summary>
        ///     Unmark a failed address.
        /// </summary>
        /// <param name="nodeConfiguration"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws if Lavalink throws an exception.</exception>
        public static async Task FreeAddressAsync(NodeConfiguration nodeConfiguration, string address) {
            if (nodeConfiguration == null) {
                throw new ArgumentNullException(nameof(nodeConfiguration));
            }

            if (string.IsNullOrWhiteSpace(address)) {
                throw new ArgumentNullException(nameof(address));
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{nodeConfiguration.HttpEndpoint}/routeplanner/free/address") {
                Headers = {
                    {"Authorization", nodeConfiguration.Authorization}
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
        public static async Task FreeAllAsync(NodeConfiguration nodeConfiguration) {
            if (nodeConfiguration == null) {
                throw new ArgumentNullException(nameof(nodeConfiguration));
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{nodeConfiguration.HttpEndpoint}/routeplanner/free/all") {
                Headers = {
                    {"Authorization", nodeConfiguration.Authorization}
                }
            };

            var routeResponse = await VictoriaExtensions.ReadAsJsonAsync<RouteResponse>(requestMessage);
            throw new Exception($"{routeResponse.Error} - {routeResponse.Message}");
        }
    }
}