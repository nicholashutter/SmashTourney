namespace DTO;
public class GameDTO
    {
        public List<PlayerDTO> CurrentPlayers { get; set; } = new List<PlayerDTO>();
        public DateTime CurrentDate { get; set; }
        public int CurrentRound { get; set; }
        public Guid SessionID { get; set; }
    }