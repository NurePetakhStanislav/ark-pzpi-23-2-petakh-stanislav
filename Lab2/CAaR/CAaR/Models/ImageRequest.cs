using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("ImageRequest")]
    public class ImageRequest : IHasUser, IHasFile
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Image")]
        public string File { get; set; }

        public User User { get; set; }
    }
}
