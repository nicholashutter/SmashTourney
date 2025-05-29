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
        private string _passwordHash { get; set; } = "";
        [Required]
        private string _passwordSalt { get; set; } = "";

}
