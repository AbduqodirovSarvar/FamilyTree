using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Behaviours
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class PhoneNumberValidationAttribute : ValidationAttribute
    {
        public PhoneNumberValidationAttribute()
        {
            ErrorMessage = "Invalid phone number.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || value is not string phoneNumber)
            {
                return false;
            }

            const string phonePattern = @"^\+\d{1,3}(\d{10,12})?$";
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, phonePattern);
        }
    }
}
