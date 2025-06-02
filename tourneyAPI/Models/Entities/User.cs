using System.ComponentModel.DataAnnotations;

namespace Entities;

public class User
{
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Username { get; set; } = "";
        [Required]
        public string Email { get; set; } = "";
        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; } = DateTime.UtcNow;
        public int AllTimeMatches { get; set; } = 0;
        public int AllTimeWins { get; set; } = 0;
        public int AllTimeLosses { get; set; } = 0;

        [Required]
        private string _passwordHash = "";

        public string getPasswordHash()
        {
                return _passwordHash;
        }
        public void setPasswordHash(string newPasswordHash)
        {
                _passwordHash = newPasswordHash;
        }
        private string _passwordSalt { get; set; } = "";

        public string getPasswordSalt()
        {
                return _passwordSalt;
        }
        public void setPasswordSalt(string newPasswordSalt)
        {
                _passwordSalt = newPasswordSalt;
        }


}
