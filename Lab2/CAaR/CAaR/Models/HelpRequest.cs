using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("HelpRequest")]
    public class HelpRequest
    {
        [Key]
        public int HelpID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Help { get; set; }

        [Required]
        public User User { get; set; }
    }
}
