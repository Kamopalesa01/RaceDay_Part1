using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Entities
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public decimal? Pace { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "DNS";

        public DateTime RecordedDate { get; set; } = DateTime.Now;

        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }
    }
}