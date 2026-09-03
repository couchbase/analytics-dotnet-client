using Xunit;

namespace Couchbase.OperationalInsightsClient.FunctionalTests.Fixtures;

[CollectionDefinition(Name)]
public class JwtCollection : ICollectionFixture<JwtFixture>
{
    public const string Name = "JwtCollection";
}
