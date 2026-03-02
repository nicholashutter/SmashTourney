namespace Entities;

using Microsoft.AspNetCore.Identity;

// Stores authenticated user profile and aggregate match statistics.
public class ApplicationUser : IdentityUser
{

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; } = DateTime.UtcNow;
        public int AllTimeMatches { get; set; } = 0;
        public int AllTimeWins { get; set; } = 0;
        public int AllTimeLosses { get; set; } = 0;
}
