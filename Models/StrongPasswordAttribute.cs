using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HRDashboard.Models
{
    public class StrongPasswordAttribute : ValidationAttribute, IClientModelValidator
    {
        public StrongPasswordAttribute()
        {
            ErrorMessage = "Password must meet the complexity requirements.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Password is required.");
            }

            var password = value.ToString()!;

            // You already have [StringLength] on the property, so we skip length check here
            // But you can add it if you want this attribute to be fully self-contained

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter (A-Z).");
            }

            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                return new ValidationResult("Password must contain at least one special character (e.g. !@#$%^&*).");
            }

            // You can add more rules here if needed, e.g.:
            // if (!Regex.IsMatch(password, @"[0-9]")) → require digit
            // if (!Regex.IsMatch(password, @"[a-z]")) → require lowercase

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-strongpassword", ErrorMessage);

            // Specific error messages for client-side (shown by jQuery Validation)
            MergeAttribute(context.Attributes, "data-val-strongpassword-uppercase", 
                "Password must contain at least one uppercase letter (A-Z).");
            MergeAttribute(context.Attributes, "data-val-strongpassword-special", 
                "Password must contain at least one special character (e.g. !@#$%^&*).");

            // Optional: add more data attributes if you add more rules
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                attributes[key] = value;
            }
            else
            {
                attributes.Add(key, value);
            }
        }
    }
}