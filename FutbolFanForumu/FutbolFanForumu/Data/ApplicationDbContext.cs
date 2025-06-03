using FutbolFanForumu.Models; // Yeni modellerin namespace'ini ekleyin
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FutbolFanForumu.Data
{
    public class ApplicationDbContext : IdentityDbContext // IdentityUser vs. için
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Yeni DbSet'lerimizi buraya ekliyoruz
        public DbSet<ForumCategory> ForumCategories { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<UpcomingMatch> UpcomingMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Bu satır Identity tablolarının doğru kurulması için ÇOK ÖNEMLİ!

            // Gerekirse burada Fluent API ile ek konfigürasyonlar yapılabilir.
            // Örneğin, cascade delete davranışlarını ayarlamak vs.
            // Şimdilik varsayılanlar yeterli olacaktır.

            // Örnek: ForumThread ve ForumPost arasındaki ilişki için cascade delete'i kısıtlamak isteyebiliriz
            // (bir başlık silindiğinde yorumlar da silinsin mi yoksa kısıtlansın mı?)
            // Varsayılan olarak cascade delete aktif olabilir. Projenin ilerleyen aşamalarında
            // bu tür detaylara karar verebiliriz.

            // IdentityUser ile olan ilişkiler için cascade delete'i önlemek (RESTRICT)
            // Bir kullanıcı silindiğinde başlıkları veya postları ne olacak? 
            // Genellikle bu tür durumlarda kullanıcıyı anonimleştirmek veya içeriği korumak tercih edilir.
            // EF Core varsayılan olarak cascade yapabilir.
            // Bunu engellemek için:

            builder.Entity<ForumThread>()
                .HasOne(ft => ft.User)
                .WithMany() // Eğer IdentityUser'da bir ICollection<ForumThread> yoksa WithMany() argümansız kullanılır
                .HasForeignKey(ft => ft.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse ve başlığı varsa silme işlemini engelle

            builder.Entity<ForumPost>()
                .HasOne(fp => fp.User)
                .WithMany() // Eğer IdentityUser'da bir ICollection<ForumPost> yoksa WithMany() argümansız kullanılır
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse ve yorumu varsa silme işlemini engelle
        }
    }
}