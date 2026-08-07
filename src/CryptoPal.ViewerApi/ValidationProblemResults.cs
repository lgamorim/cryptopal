namespace CryptoPal.ViewerApi;

internal static class ValidationProblemResults
{
    internal static IResult BadRequest(string detail) =>
        TypedResults.Problem(
            detail: detail,
            title: "BadRequest",
            statusCode: StatusCodes.Status400BadRequest);
}
