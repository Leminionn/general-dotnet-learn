using FirstAPIProject.Domain.Common.Enums;

namespace FirstAPIProject.Domain.Entities
{
    public class User : LifecycleEntity
    {
        public string?UserName { get; private set; }

        public string Email { get; private set; } = null!;

        public string? PhoneNumber { get; private set; }

        public string? AvatarUrl { get; private set; }

        public DateTime? LastLoginAt { get; private set; }

        public string PasswordHash { get; private set; } = null!;

        public UserRole Role { get; private set; }

        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        private User()
        {
        }

        private User(string? userName, string email, string? phoneNumber, string passwordHash, UserRole role)
        {
            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            Role = role;
        }

        public static User Create(string email, string passwordHash, string? userName = null, string? phoneNumber = null, UserRole role = UserRole.User)
        {
            return new User(userName, email, phoneNumber, passwordHash, role);
        }

        public void ChangePassword(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public void ChangeEmail(string email)
        {
            Email = email;
        }

        public void ChangeUserName(string userName)
        {
            UserName = userName;
        }

        public void ChangePhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void UpdateProfile(string? userName, string? phoneNumber)
        {
            UserName = userName;
            PhoneNumber = phoneNumber;
        }

        public void UpdateAvatar(string? avatarUrl)
        {
            AvatarUrl = avatarUrl;
        }

        public void UpdateRole(UserRole role)
        {
            Role = role;
        }

        public void RecordLogin(DateTime? loginTime = null)
        {
            LastLoginAt = loginTime ?? DateTime.UtcNow;
        }
    }
}
