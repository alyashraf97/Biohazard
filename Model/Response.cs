
using System.ComponentModel.DataAnnotations;

namespace Biohazard.Model
{
    public class Response
    {
        [Key] 
        public int Id { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public Guid EventId { get; set; }
        public string MessageId { get; set; }
        public QMail QMail { get; set; }
    }
}