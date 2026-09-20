using System;

namespace FirstAPIProject.Domain.Entities
{
    public class EmailWhitelist : LifecycleEntity
    {
        public string Pattern { get; private set; } = null!;

        public string? Description { get; private set; }

        private EmailWhitelist()
        {
        }

        private EmailWhitelist(string pattern, string? description)
        {
            Pattern = pattern.Trim().ToLowerInvariant();
            Description = description;
        }

        public static EmailWhitelist Create(string pattern, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Whitelist pattern cannot be empty.", nameof(pattern));
            }

            return new EmailWhitelist(pattern, description);
        }

        public void Update(string pattern, string? description)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Whitelist pattern cannot be empty.", nameof(pattern));
            }

            Pattern = pattern.Trim().ToLowerInvariant();
            Description = description;
        }

        public bool Matches(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            // Pattern can be exact email: "user@domain.com"
            if (Pattern.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Pattern can be domain with @: "@uit.edu.vn"
            if (Pattern.StartsWith('@'))
            {
                return normalizedEmail.EndsWith(Pattern, StringComparison.OrdinalIgnoreCase);
            }

            // Pattern can be wildcard: "*@uit.edu.vn"
            if (Pattern.StartsWith("*@"))
            {
                var domain = Pattern[1..]; // "@uit.edu.vn"
                return normalizedEmail.EndsWith(domain, StringComparison.OrdinalIgnoreCase);
            }

            // Pattern can be bare domain: "uit.edu.vn"
            if (normalizedEmail.EndsWith("@" + Pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
