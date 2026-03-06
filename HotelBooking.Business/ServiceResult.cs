namespace HotelBooking.Business;

public class ServiceResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    protected ServiceResult(bool isSuccess, string? errorMessage, string? errorCode)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static ServiceResult Success() => new(true, null, null);
    public static ServiceResult Failure(string message, string? code = null) => new(false, message, code);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    private ServiceResult(bool isSuccess, T? data, string? errorMessage, string? errorCode)
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
    }

    public static ServiceResult<T> Success(T data) => new(true, data, null, null);
    public new static ServiceResult<T> Failure(string message, string? code = null) => new(false, default, message, code);
}
