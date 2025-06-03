using System.ComponentModel.DataAnnotations;

namespace FutbolFanForumu.Models
{
    public class ForumCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name cannot be empty.")]
        [StringLength(100, ErrorMessage = "Category name can be at most 100 characters.")]
        [Display(Name = "Kategori Adı")] 
        public string Name { get; set; } = null!;

        [StringLength(250, ErrorMessage = "Description can be at most 250 characters.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "İkon Sınıfı")]
        [StringLength(50)]
        public string? IconClass { get; set; }

        public virtual ICollection<ForumThread>? Threads { get; set; }
    }
}