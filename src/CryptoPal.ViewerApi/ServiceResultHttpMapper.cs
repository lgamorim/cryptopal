using CryptoPal.Core;

namespace CryptoPal.ViewerApi;
/// <summary>Maps <see cref="ServiceResult{T}"/> values to HTTP results.</summary>
internal static class ServiceResultHttpMapper
{
    /// <summary>Returns <c>200 OK</c> on success or a ProblemDetails response on failure.</summary>
    public static IResult ToHttpResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        return TypedResults.Problem(
            detail: result.ErrorMessage,
            title: result.ErrorCode?.ToString(),
            statusCode: MapStatusCode(result.ErrorCode));
    }

    private static int MapStatusCode(ServiceErrorCode? errorCode) => errorCode switch
    {
        ServiceErrorCode.NotFound => StatusCodes.Status404NotFound,
        ServiceErrorCode.RateLimited => StatusCodes.Status429TooManyRequests,
        ServiceErrorCode.ResponseMappingFailed => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status502BadGateway
    };
}
