using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Entities
{
    public class AuditLog
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        public int RecordId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public string? ChangeData { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}