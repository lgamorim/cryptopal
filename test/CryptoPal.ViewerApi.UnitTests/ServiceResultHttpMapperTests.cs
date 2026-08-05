using CryptoPal.Core;
using CryptoPal.Core.CurrentPrice;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CryptoPal.ViewerApi.UnitTests;

public class ServiceResultHttpMapperTests
{
    [Fact]
    public void Should_ReturnOk_When_ResultIsSuccess()
    {
        var view = new CurrentPriceView([]);

        var result = ServiceResultHttpMapper.ToHttpResult(ServiceResult<CurrentPriceView>.Success(view));

        var okResult = result.Should().BeOfType<Ok<CurrentPriceView>>().Subject;
        okResult.Value.Should().BeSameAs(view);
    }

    [Theory]
    [InlineData(ServiceErrorCode.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ServiceErrorCode.RateLimited, StatusCodes.Status429TooManyRequests)]
    [InlineData(ServiceErrorCode.UpstreamUnavailable, StatusCodes.Status502BadGateway)]
    [InlineData(ServiceErrorCode.ResponseMappingFailed, StatusCodes.Status500InternalServerError)]
    [InlineData(ServiceErrorCode.RequestTimedOut, StatusCodes.Status504GatewayTimeout)]
    public void Should_ReturnProblemDetailsWithMappedStatus_When_ResultFails(ServiceErrorCode errorCode, int expectedStatusCode)
    {
        const string detail = "Something went wrong.";

        var result = ServiceResultHttpMapper.ToHttpResult(
            ServiceResult<CurrentPriceView>.Failure(errorCode, detail));

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(expectedStatusCode);
        problemResult.ProblemDetails.Title.Should().Be(errorCode.ToString());
        problemResult.ProblemDetails.Detail.Should().Be(detail);
    }
}
