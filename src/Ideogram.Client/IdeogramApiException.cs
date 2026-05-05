using System.Net;

namespace Ideogram.Client;

public sealed class IdeogramApiException : HttpRequestException
{
    public HttpStatusCode StatusCodeValue { get; }

    public string? ResponseBody { get; }

    public string? RequestPath { get; }

    public string? RequestId { get; }

    public IdeogramApiException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        string? requestPath,
        string? requestId,
        Exception? innerException = null)
        : base(message, innerException, statusCode)
    {
        StatusCodeValue = statusCode;
        ResponseBody = responseBody;
        RequestPath = requestPath;
        RequestId = requestId;
    }
}
