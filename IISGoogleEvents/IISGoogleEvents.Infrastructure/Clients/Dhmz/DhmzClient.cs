using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using IISGoogleEvents.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Infrastructure.Clients.Dhmz;

public class DhmzClient(HttpClient httpClient, IOptions<DhmzOptions> config, DhmzFeedCache cache)
{
    private static readonly XmlSerializer Serializer = new(typeof(DhmzWeatherXml));

    public Task<DhmzWeatherXml> FetchWeatherDataAsync(CancellationToken cancellationToken) =>
        cache.GetOrFetchAsync(
            FetchFromUpstreamAsync,
            TimeSpan.FromMinutes(config.Value.CacheMinutes),
            cancellationToken);

    private async Task<DhmzWeatherXml> FetchFromUpstreamAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            config.Value.XmlPath,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return (DhmzWeatherXml?)Serializer.Deserialize(stream)
            ?? throw new InvalidOperationException("DHMZ feed je deserijaliziran u null.");
    }

    public static SocketsHttpHandler CreateIPv4OnlyHandler() => new()
    {
        ConnectCallback = async (context, cancellationToken) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, AddressFamily.InterNetwork, cancellationToken);

            if (addresses.Length == 0)
                throw new SocketException((int)SocketError.HostNotFound);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            try
            {
                await socket.ConnectAsync(addresses[0], context.DnsEndPoint.Port, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };
}
