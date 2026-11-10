namespace {{SolutionName}}.Service.Utils;

public static class ExceptionUtils
{
    public static int ExtractStatusCode(Exception ex)
    {
        if (ex.Data.Contains("StatusCode") && int.TryParse(ex.Data["StatusCode"]?.ToString(), out var code))
        {
            return code;
        }

        return 500;
    }

    public static ApiResult<T> CreateErrorResponse<T>(Exception ex) =>
        ApiResult<T>.Failure(ExtractStatusCode(ex).ToString(), ex.Message ?? "Lỗi không xác định.");
}
