using System.Runtime.CompilerServices;

#if !SIGNING
[assembly: InternalsVisibleTo("Couchbase.OperationalInsights.UnitTests")]
[assembly: InternalsVisibleTo("Couchbase.OperationalInsightsClient.FunctionalTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Couchbase.OperationalInsights.Performer")]
#endif
