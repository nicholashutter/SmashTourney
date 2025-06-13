using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Entities;

public class ApplicationUser : IdentityUser
{
        public override string Id { get => base.Id; set => base.Id = value; }
        public override string? UserName { get => base.UserName; set => base.UserName = value; }
        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; } = DateTime.UtcNow;
        public int AllTimeMatches { get; set; } = 0;
        public int AllTimeWins { get; set; } = 0;
        public int AllTimeLosses { get; set; } = 0;
}
