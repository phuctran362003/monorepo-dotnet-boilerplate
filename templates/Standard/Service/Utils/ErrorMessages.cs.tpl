namespace {{SolutionName}}.Service.Utils;

public static class ErrorMessages
{
    #region Account

    public const string AccountNotFound = "Không tìm thấy tài khoản với email này.";

    public const string AccountEmailAlreadyRegistered = "Email này đã được đăng ký.";

    #endregion

    #region Category

    public const string CategoryNotFound = "Không tìm thấy danh mục.";

    #endregion

    #region Product

    public const string ProductNotFound = "Không tìm thấy sản phẩm.";

    public const string ProductNameRequired = "Tên sản phẩm là bắt buộc.";

    #endregion

    #region File

    public const string FileEmpty = "File trống.";

    public const string FileTooLarge = "File vượt quá 10 MB.";

    #endregion
}
