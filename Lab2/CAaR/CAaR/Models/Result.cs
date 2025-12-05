using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("Result")]
    public class Result : IHasUser
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Range(0, 100)]
        public decimal? Hemoglobin { get; set; }

        [Range(0, 100)]
        public decimal? Erythrocytes { get; set; }

        [Range(0, 100)]
        public decimal? Leukocytes { get; set; }

        [Range(0, 1000)]
        public decimal? Platelets { get; set; }

        [Range(0, 100)]
        public decimal? Hematocrit { get; set; }

        [Range(0, 1000)]
        public decimal? Glucose { get; set; }

        [Range(0, 1000)]
        public decimal? Cholesterol { get; set; }

        [Required]
        [RegularExpression("[MF]")]
        public char Sex { get; set; }

        [Required]
        [Range(0, 120)]
        public byte Age { get; set; }

        [Required]
        public DateTime ExecutionDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public User User { get; set; }
    }
}
