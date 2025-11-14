using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("NameRequest")]
    public class NameRequest
    {
        [Key]
        public int NameID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NewName { get; set; }

        public User User { get; set; }
    }
}
