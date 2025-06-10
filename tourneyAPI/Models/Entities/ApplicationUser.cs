using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Entities;

public class ApplicationUser : IdentityUser<Guid>
{
        public override Guid Id { get => base.Id; set => base.Id = value; }
        public override string? UserName { get => base.UserName; set => base.UserName = value; }
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
