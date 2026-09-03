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

using System.Net;
using Couchbase.OperationalInsightsClient.Query;
using Couchbase.OperationalInsightsClient.Results;
using Couchbase.Core.Json;

namespace Couchbase.OperationalInsightsClient.Internal.Results;

internal abstract class OperationalInsightsResultBase : IQueryResult
{
    protected readonly Stream ResponseStream;
    protected readonly IDeserializer Serializer;
    private readonly IDisposable? _ownedForCleanup;
    private bool _disposed;

    /// <summary>
    /// Creates a new OperationalInsightsResultBase.
    /// </summary>
    /// <param name="responseStream"><see cref="Stream"/> to read.</param>
    /// <param name="serializer">The <see cref="ISerializer"/> to use for converting the response to an object.</param>
    /// <param name="ownedForCleanup">Additional object to dispose when complete.</param>
    protected OperationalInsightsResultBase(Stream responseStream, IDeserializer serializer, IDisposable? ownedForCleanup = null)
    {
        ResponseStream = responseStream ?? throw new ArgumentNullException(nameof(responseStream));
        Serializer = serializer;
        _ownedForCleanup = ownedForCleanup;
    }

    public abstract IAsyncEnumerator<OperationalInsightsRow> GetAsyncEnumerator(
        CancellationToken cancellationToken = default);

    public abstract Task InitializeAsync(CancellationToken cancellationToken = default);

    public IAsyncEnumerable<OperationalInsightsRow> Rows { get; protected set; } = null!;
    public QueryMetaData MetaData { get; protected set; } = null!;

    public IReadOnlyList<QueryError> Errors { get; protected set; } = Array.Empty<QueryError>();

    public HttpStatusCode? StatusCode { get; set; }

    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ResponseStream?.Dispose();
        _ownedForCleanup?.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (ResponseStream != null)
        {
            await ResponseStream.DisposeAsync().ConfigureAwait(false);
        }
        _ownedForCleanup?.Dispose();
    }
}
