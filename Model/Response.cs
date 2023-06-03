namespace Biohazard.Model
{
    public class Response
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public Guid EventId { get; set; }
        public QMail Mail { get; set; }
    }
}