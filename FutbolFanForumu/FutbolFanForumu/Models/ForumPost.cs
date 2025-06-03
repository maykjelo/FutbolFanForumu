using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutbolFanForumu.Models
{
    public class ForumPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment content cannot be empty.")]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = null!;

        [Display(Name = "Gönderilme Tarihi")]
        public DateTime PostedDate { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        [Required]
        public int ForumThreadId { get; set; }
        [ForeignKey("ForumThreadId")]
        public virtual ForumThread ForumThread { get; set; } = null!; 
    }
}