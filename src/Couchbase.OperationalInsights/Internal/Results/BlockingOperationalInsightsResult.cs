#region License
/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2025 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/
#endregion

using System.Runtime.CompilerServices;
using Couchbase.OperationalInsightsClient.Options;
using Couchbase.OperationalInsightsClient.Query;
using Couchbase.OperationalInsightsClient.Results;
using Couchbase.OperationalInsightsClient.Utils;
using Couchbase.Core.Json;

namespace Couchbase.OperationalInsightsClient.Internal.Results;

/// <summary>
/// A "blocking" result class for OperationalInsights queries.
/// </summary>
/// <remarks>For large result sets use the <see cref="StreamingOperationalInsightsResult"/> class by setting <see cref="QueryOptions.AsStreaming"/> to true, which is the default.</remarks>
internal class BlockingOperationalInsightsResult : OperationalInsightsResultBase
{
    private IEnumerable<OperationalInsightsRow>? _rows;
    private int _enumerated; // 0 = not started, 1 = started (atomic via Interlocked)

    public BlockingOperationalInsightsResult(Stream responseStream, IDeserializer serializer, IDisposable? ownedForCleanup = null)
        : base(responseStream, serializer, ownedForCleanup)
    {
    }

    public override IAsyncEnumerator<OperationalInsightsRow> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
        => EnumerateRows(cancellationToken).GetAsyncEnumerator(cancellationToken);

    private async IAsyncEnumerable<OperationalInsightsRow> EnumerateRows(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_rows == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BlockingOperationalInsightsResult)} has not been initialized, call InitializeAsync first");
        }

        if (Interlocked.CompareExchange(ref _enumerated, 1, 0) != 0)
        {
            throw new InvalidOperationException(
                "Query results can only be enumerated once. The result stream has already been consumed.");
        }

        foreach (var row in _rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return row;
        }
    }

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var reader = Serializer.CreateJsonStreamReader(ResponseStream, cancellationToken);

        if (!await reader.InitializeAsync(cancellationToken).ConfigureAwait(false))
        {
            ThrowHelper.ThrowArgumentNullException("No data received.");
        }

        MetaData = new QueryMetaData();

        var rows = new List<OperationalInsightsRow>();

        while (true)
        {
            var path = await reader.ReadToNextAttributeAsync(cancellationToken).ConfigureAwait(false);
            if (path == null)
            {
                break;
            }

            switch (path)
            {
                case "requestID" when reader.ValueType == typeof(string):
                    MetaData.RequestId = reader.Value?.ToString();
                    break;
                case "metrics":
                    MetaData.Metrics = await reader.ReadObjectAsync<QueryMetrics>(cancellationToken).ConfigureAwait(false);
                    break;
                case "results":
                    {
                        await foreach (var token in reader.ReadTokensAsync(cancellationToken).ConfigureAwait(false))
                        {
                            rows.Add(new OperationalInsightsRow(token));
                        }
                        break;
                    }
                case "errors":
                    var errors = await reader.ReadObjectAsync<QueryError[]>(cancellationToken).ConfigureAwait(false);
                    Errors = errors ?? Array.Empty<QueryError>();
                    break;
            }
        }

        _rows = rows;
        Rows = EnumerateRows(cancellationToken);
    }
}
