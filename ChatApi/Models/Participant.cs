namespace ChatApi.Models
{
    public class Participant
    {
        public int Id { get; set; }

        public string ParticipentName { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public int Status { get; set; }

    }
}
