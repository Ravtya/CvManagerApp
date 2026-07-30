namespace CvManager.Application.Common.ErrorsCodes;

public static class CommonErrorCodes
{
    public const string NotFound = "NotFound";
    public const string DuplicateName = "DuplicateName";
    public const string ConcurrencyConflict = "ConcurrencyConflict";
    public const string NothingSelected = "NothingSelected";
    public const string InUse = "InUse";
    public const string ValueTooLong = "ValueTooLong";
    public const string FormInvalid = "FormInvalid";
}

public static class AttributeErrorCodes
{
    public const string DropdownAtLeastOneOption = "Attributes.DropdownAtLeastOneOption";
    public const string DeleteBuiltInNotAllowed = "Attributes.Delete.BuiltInNotAllowed";
    public const string DeleteInUseByPosition = "Attributes.Delete.InUseByPosition";
}

public static class PositionErrorCodes
{
    public const string RestrictedRequiresRule = "Positions.RestrictedRequiresRule";
    public const string RuleOperatorNotAllowed = "Positions.Rule.OperatorNotAllowed";
    public const string RuleUnsupportedType = "Positions.Rule.UnsupportedType";
    public const string RuleRequiresValue = "Positions.Rule.RequiresValue";
    public const string DeleteInUseByCv = "Positions.Delete.InUseByCv";
}

public static class CvErrorCodes
{
    public const string NotOwner = "Cv.NotOwner";
}

public static class DiscussionErrorCodes
{
    public const string ContentRequired = "Discussion.ContentRequired";
    public const string NotAllowed = "Discussion.NotAllowed";
}

public static class AdminErrorCodes
{
    public const string InvalidRole = "Admin.InvalidRole";
    public const string OperationFailed = "Admin.OperationFailed";
}

public static class IdentityErrorCodes
{
    public const string UserAlreadyInRole = "UserAlreadyInRole";
    public const string UserNotInRole = "UserNotInRole";
}

public static class AuthErrorCodes
{
    public const string AccountLockedOut = "Auth.AccountLockedOut";
    public const string IncorrectEmailOrPassword = "Auth.IncorrectEmailOrPassword";
    public const string EmailNotConfirmed = "Auth.EmailNotConfirmed";
    public const string InvalidConfirmationLink = "Auth.InvalidConfirmationLink";
    public const string ExternalSignInFailed = "Auth.ExternalSignInFailed";
    public const string ExternalEmailMissing = "Auth.ExternalEmailMissing";
    public const string ExternalAccountCreateFailed = "Auth.ExternalAccountCreateFailed";
    public const string ExternalAccountLinkFailed = "Auth.ExternalAccountLinkFailed";
}

public static class SalesforceErrorCodes
{
    public const string ExportFailed = "Salesforce.ExportFailed";
}
