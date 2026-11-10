namespace MDRN_HATS.Application.Exceptions;

public sealed class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
