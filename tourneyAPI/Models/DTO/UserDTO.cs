namespace DTO;

public class UserDTO
{
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int AllTimeMatches { get; set; }
        public int AllTimeWins { get; set; }
        public int AllTimeLosses { get; set; }

}
