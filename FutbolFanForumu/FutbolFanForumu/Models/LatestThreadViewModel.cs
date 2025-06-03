// Models/LatestThreadViewModel.cs
namespace FutbolFanForumu.Models
{
    public class LatestThreadViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? AuthorUserName { get; set; } // Nullable olabilir
        public string? CategoryName { get; set; }   // Nullable olabilir
    }
}