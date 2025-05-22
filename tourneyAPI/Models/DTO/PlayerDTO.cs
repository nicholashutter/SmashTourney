namespace DTO;

public class PlayerDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = default!;
        public int CurrentScore { get; set; }
        public int CurrentRound { get; set; }
        public string CurrentOpponent { get; set; } = default!;
        public string CurrentCharacter { get; set; } = default!;
        public DateTime CurrentDate { get; set; }
        public Guid CurrentGameSessionID { get; set; }
        public bool HasVoted { get; set; }
        public Guid? RoundVote { get; set; }

    }