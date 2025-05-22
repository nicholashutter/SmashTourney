namespace Entities;

public class User
{
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int AllTimeMatches { get; set; }
        public int AllTimeWins { get; set; }
        public int AllTimeLosses { get; set; }
        private string _passwordHash = "";
        private string _passwordSalt = "";

        public void SetPasswordHash(string hash) => _passwordHash = hash;
        public void SetPasswordSalt(string salt) => _passwordSalt = salt;

}
