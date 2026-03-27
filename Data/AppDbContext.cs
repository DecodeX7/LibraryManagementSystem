using LibraryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookRequest> BookRequests { get; set; }
        public DbSet<IssuedBook> IssuedBooks { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Unique BookCode
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.BookCode)
                .IsUnique();

            // ── BookRequests ──────────────────────────────────────────
            modelBuilder.Entity<BookRequest>()
                .HasOne(r => r.Student)
                .WithMany(u => u.BookRequests)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookRequest>()
                .HasOne(r => r.Book)
                .WithMany(b => b.BookRequests)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookRequest>()
                .HasOne(r => r.ActionByUser)
                .WithMany()
                .HasForeignKey(r => r.ActionBy)
                .OnDelete(DeleteBehavior.NoAction);

            // ── IssuedBooks ───────────────────────────────────────────
            modelBuilder.Entity<IssuedBook>()
                .HasOne(i => i.Student)
                .WithMany(u => u.IssuedBooks)
                .HasForeignKey(i => i.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IssuedBook>()
                .HasOne(i => i.Book)
                .WithMany(b => b.IssuedBooks)
                .HasForeignKey(i => i.BookId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IssuedBook>()
                .HasOne(i => i.IssuedByUser)
                .WithMany()
                .HasForeignKey(i => i.IssuedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Fines ─────────────────────────────────────────────────
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.IssuedBook)
                .WithOne(i => i.Fine)
                .HasForeignKey<Fine>(f => f.IssueId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Fine>()
                .HasOne(f => f.Student)
                .WithMany()
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Books → AddedBy ───────────────────────────────────────
            modelBuilder.Entity<Book>()
                .HasOne(b => b.AddedByUser)
                .WithMany()
                .HasForeignKey(b => b.AddedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // ── EmailLogs ─────────────────────────────────────────────
            modelBuilder.Entity<EmailLog>()
                .HasOne(e => e.IssuedBook)
                .WithMany()
                .HasForeignKey(e => e.IssueId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}