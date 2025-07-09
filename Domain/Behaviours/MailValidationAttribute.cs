using System.ComponentModel.DataAnnotations;

namespace Domain.Behaviours
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class MailValidationAttribute : ValidationAttribute
    {
        public MailValidationAttribute()
        {
            ErrorMessage = "Invalid mail address!";
        }
        public override bool IsValid(object? value)
        {
            if (value == null || value is not string email)
            {
                return false;
            }

            const string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern);
        }
    }
}
