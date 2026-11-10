public class ApiResult<T>
{
    public bool IsSuccess { get; set; }

    public bool IsFailure => !IsSuccess;

    public ResponseDataContent<T>? Value { get; set; }

    public ErrorContent? Error { get; set; }

    public static ApiResult<T> Success(T data, string code = "200", string message = "Operation successful.") => new()
    {
        IsSuccess = true,
        Value = new ResponseDataContent<T> { Code = code, Message = message, Data = data },
        Error = null
    };

    public static ApiResult<T> Failure(string code = "400", string message = "Operation failed.", T? value = default) => new()
    {
        IsSuccess = false,
        Value = value is null ? null : new ResponseDataContent<T> { Code = code, Message = message, Data = value },
        Error = new ErrorContent { Code = code, Message = message }
    };
}

public class ApiResult
{
    public bool IsSuccess { get; set; }

    public bool IsFailure => !IsSuccess;

    public ResponseContent? Value { get; set; }

    public ErrorContent? Error { get; set; }

    public static ApiResult Success(string code = "200", string message = "Operation successful.") => new()
    {
        IsSuccess = true,
        Value = new ResponseContent { Code = code, Message = message },
        Error = null
    };

    public static ApiResult Failure(string code = "400", string message = "Operation failed.") => new()
    {
        IsSuccess = false,
        Value = null,
        Error = new ErrorContent { Code = code, Message = message }
    };
}

public class ResponseDataContent<T>
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }
}

public class ResponseContent
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}

public class ErrorContent
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
