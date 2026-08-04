namespace CryptoPal.Core;

/// <summary>Outcome of a cryptocurrency service operation.</summary>
/// <typeparam name="T">View model returned on success.</typeparam>
public sealed record ServiceResult<T>(bool IsSuccess, T? Value, ServiceErrorCode? ErrorCode, string? ErrorMessage)
{
    /// <summary>Creates a successful result.</summary>
    public static ServiceResult<T> Success(T value) => new(true, value, null, null);

    /// <summary>Creates a failed result.</summary>
    public static ServiceResult<T> Failure(ServiceErrorCode errorCode, string errorMessage) =>
        new(false, default, errorCode, errorMessage);
}
