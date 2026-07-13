namespace WF.MES.Shared.Constants;

public static class WfMessageCodes
{
    public const string AuthInvalidCredentials = "auth.invalid_credentials";
    public const string AuthUserDisabled = "auth.user_disabled";
    public const string AuthUserNotFound = "auth.user_not_found";
    public const string AuthRefreshInvalid = "auth.refresh_invalid";
    public const string AuthPasswordSameAsOld = "auth.password_same_as_old";
    public const string AuthPasswordTooShort = "auth.password_too_short";
    public const string SessionReplaced = "session.replaced_by_other_device";
    public const string AuthFactoryRequired = "auth.factory_required";
    public const string AuthFactoryForbidden = "auth.factory_forbidden";
    public const string FactoryNotFound = "factory.not_found";
    public const string FactoryCodeExists = "factory.code_exists";
    public const string ValidationFailed = "validation.failed";
    public const string CommonInternalError = "common.internal_error";
    public const string CommonNotFound = "common.not_found";
}
