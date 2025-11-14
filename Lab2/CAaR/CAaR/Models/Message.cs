using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAaR.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public int MessageID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Description { get; set; }

        [Required]
        public DateTime MessageDate { get; set; }


        public User User { get; set; }
    }
}
