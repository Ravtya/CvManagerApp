using CvManager.Web.Ui;
using Microsoft.AspNetCore.Identity;

namespace CvManager.Web.Security;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), userName);

    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), email);

    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), email ?? string.Empty);

    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), length);

    private static IdentityError Error(string code, params object[] args) =>
        new()
        {
            Code = code,
            Description = UiMessages.Text(code, args)
        };
}
