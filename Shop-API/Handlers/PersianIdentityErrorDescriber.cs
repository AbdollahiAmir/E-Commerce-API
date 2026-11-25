using Microsoft.AspNetCore.Identity;

namespace Shop_API.Handlers
{
    public class PersianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
        {
            return new IdentityError { Code = nameof(DefaultError), Description = "خطای ناشناخته‌ای رخ داد." };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "اطلاعات هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید." };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError { Code = nameof(PasswordMismatch), Description = "رمز عبور اشتباه است." };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError { Code = nameof(InvalidToken), Description = "توکن نامعتبر است." };
        }

        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            return new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = "کد بازیابی نامعتبر است." };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "این ورود قبلاً به کاربر دیگری وصل شده است." };
        }

        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError { Code = nameof(InvalidUserName), Description = "شماره موبایل معتبر نیست." };
        }

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError { Code = nameof(InvalidEmail), Description = "ایمیل معتبر نیست." };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError { Code = nameof(DuplicateUserName), Description = "این شماره موبایل قبلاً ثبت شده است." };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError { Code = nameof(DuplicateEmail), Description = "این ایمیل قبلاً ثبت شده است." };
        }

        public override IdentityError InvalidRoleName(string? role)
        {
            return new IdentityError { Code = nameof(InvalidRoleName), Description = "نام نقش معتبر نیست." };
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError { Code = nameof(DuplicateRoleName), Description = "نقشی با این نام قبلاً ثبت شده است." };
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "این کاربر از قبل رمز عبور دارد." };
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "قفل‌شدن حساب برای این کاربر فعال نیست." };
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError { Code = nameof(UserAlreadyInRole), Description = "کاربر از قبل این نقش را دارد." };
        }

        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError { Code = nameof(UserNotInRole), Description = "کاربر این نقش را ندارد." };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError { Code = nameof(PasswordTooShort), Description = $"رمز عبور باید حداقل {length} کاراکتر باشد." };
        }

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        {
            return new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"رمز عبور باید حداقل {uniqueChars} کاراکتر متفاوت داشته باشد." };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "رمز عبور باید حداقل یک کاراکتر خاص داشته باشد." };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "رمز عبور باید حداقل یک عدد داشته باشد." };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد." };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد." };
        }
    }
}