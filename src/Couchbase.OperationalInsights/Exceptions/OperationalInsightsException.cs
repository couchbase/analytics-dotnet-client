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

using System.Text;
using Couchbase.OperationalInsightsClient.Internal.Retry;

namespace Couchbase.OperationalInsightsClient.Exceptions;

/// <summary>
/// Base exception type for OperationalInsights.
/// </summary>
public class OperationalInsightsException : Exception
{
    internal ErrorContext? ErrorContext { get; set; }

    public OperationalInsightsException()
    {
    }




    public OperationalInsightsException(string? message) : base(message)
    {
    }

    public OperationalInsightsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    internal OperationalInsightsException(string? message, ErrorContext? errorContext) : base(message)
    {
        ErrorContext = errorContext;
    }

    internal OperationalInsightsException(string? message, Exception? innerException, ErrorContext? errorContext) : base(message, innerException)
    {
        ErrorContext = errorContext;
    }

    internal OperationalInsightsException(string? message, AggregateException? aggregateException, ErrorContext? errorContext) : base(message, aggregateException)
    {
        ErrorContext = errorContext;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Message);
        sb.Append(InnerException?.Message);
        sb.Append($" Context Info: {ErrorContext?.ToString()}");
        return sb.ToString();
    }
}
