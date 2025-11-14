using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("RoleRequest")]
    public class RoleRequest
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(60)]
        public string Document { get; set; }

        public User User { get; set; }
    }
}
