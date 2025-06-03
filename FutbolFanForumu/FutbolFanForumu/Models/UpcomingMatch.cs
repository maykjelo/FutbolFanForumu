using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutbolFanForumu.Models
{
    public class UpcomingMatch
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Home team name cannot be empty.")]
        [StringLength(100)]
        [Display(Name = "Ev Sahibi Takım")]
        public string HomeTeamName { get; set; } = null!;

        [Required(ErrorMessage = "Away team name cannot be empty.")]
        [StringLength(100)]
        [Display(Name = "Deplasman Takım")]
        public string AwayTeamName { get; set; } = null!;

        [Required(ErrorMessage = "Match date and time cannot be empty.")]
        [Display(Name = "Maç Tarihi ve Saati")]
        public DateTime MatchDateTime { get; set; }

        [StringLength(100)]
        [Display(Name = "Turnuva/Lig Adı")]
        public string? CompetitionName { get; set; }

        [Display(Name = "İlişkili Forum Başlığı")]
        public int? RelatedForumThreadId { get; set; }
        [ForeignKey("RelatedForumThreadId")]
        public virtual ForumThread? RelatedForumThread { get; set; }
    }
}