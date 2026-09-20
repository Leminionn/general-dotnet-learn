using System;

namespace FirstAPIProject.Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime? UsedAt { get; private set; }

        public User User { get; private set; } = null!;

        private PasswordResetToken()
        {
        }

        private PasswordResetToken(Guid userId, string tokenHash, DateTime expiresAt)
        {
            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            IsUsed = false;
        }

        public static PasswordResetToken Create(Guid userId, string tokenHash, TimeSpan validDuration)
        {
            return new PasswordResetToken(userId, tokenHash, DateTime.UtcNow.Add(validDuration));
        }

        public bool IsActive()
        {
            return !IsUsed && DateTime.UtcNow <= ExpiresAt;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
        }
    }
}
