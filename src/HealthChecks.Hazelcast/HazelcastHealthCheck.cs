using Microsoft.Extensions.Diagnostics.HealthChecks;
using Hazelcast;

namespace HealthChecks.Hazelcast;

public class HazelcastHealthCheck : IHealthCheck
{
    private readonly HazelcastHealthCheckOptions _options;

    public HazelcastHealthCheck(HazelcastHealthCheckOptions options)
    {
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var options = new HazelcastOptionsBuilder()
            .With(args =>
            {
                args.ClusterName = _options.ClusterName;
                args.Networking.Addresses.Add($"{_options.Server}:{_options.Port}");
            })
            .Build();

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

        var map = await client.GetMapAsync<string, string>("healthcheck-map").ConfigureAwait(false);
        await map.SetAsync("healthcheck-key", "healthcheck-value").ConfigureAwait(false);
        var value = await map.GetAsync("healthcheck-key").ConfigureAwait(false);

        await map.DeleteAsync("healthcheck-key").ConfigureAwait(false);

        if (value == "healthcheck-value")
        {
            return HealthCheckResult.Healthy($"cluster {_options.ClusterName} is up");
        }
        else
        {
            return HealthCheckResult.Unhealthy($"Hazelcast cluster '{_options.ClusterName}' health check failed.");
        }

    }
}
