using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class AuthModel
    {
        public class LoginRequest
        {
            [Key]
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;

        }

        public class LoginResponse
        {
            [Key]
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresAtUtc { get; set; }
        }
    }
}
