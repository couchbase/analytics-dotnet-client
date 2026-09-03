using Xunit;

namespace Couchbase.OperationalInsightsClient.FunctionalTests.Fixtures;

[CollectionDefinition(Name)]
public class CertificateCollection : ICollectionFixture<CertificateFixture>
{
    public const string Name = "Certificate";
}
