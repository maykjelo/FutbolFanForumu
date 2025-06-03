using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutbolFanForumu.Models
{
    public class ForumThread
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title cannot be empty.")]
        [StringLength(200, ErrorMessage = "Title can be at most 200 characters.")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Content cannot be empty.")]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = null!;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Son Aktivite Tarihi")]
        public DateTime LastPostDate { get; set; }

        [Display(Name = "Görüntülenme Sayısı")]
        public int ViewCount { get; set; } = 0;

        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Kategori")]
        public int ForumCategoryId { get; set; }
        [ForeignKey("ForumCategoryId")]
        public virtual ForumCategory ForumCategory { get; set; } = null!;

        public virtual ICollection<ForumPost>? Posts { get; set; }

        public ForumThread()
        {
            CreatedDate = DateTime.Now;
            LastPostDate = DateTime.Now;
        }
    }
}