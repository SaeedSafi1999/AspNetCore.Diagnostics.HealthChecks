namespace HealthChecks.Hazelcast;

public class HazelcastHealthCheckOptions
{
    public string? Server { get; set; }
    public int Port { get; set; }
    public string? ClusterName { get; set; }
}
