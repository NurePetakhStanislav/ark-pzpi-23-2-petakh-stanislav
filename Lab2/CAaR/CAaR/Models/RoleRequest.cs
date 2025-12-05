using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("RoleRequest")]
    public class RoleRequest : IHasUser, IHasFile
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(60)]
        [Column("Document")]
        public string File { get; set; }

        public User User { get; set; }
    }
}
