using Microsoft.EntityFrameworkCore;
using LibraryAPI.Models;

namespace LibraryAPI.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Book configuration
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(13);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasIndex(e => e.ISBN).IsUnique();

                entity.HasOne(e => e.Author)
                      .WithMany(a => a.Books)
                      .HasForeignKey(e => e.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Author configuration
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Biography).HasMaxLength(2000);
                entity.Property(e => e.Nationality).HasMaxLength(100);
            });

            // Member configuration
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MembershipNumber).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.MembershipNumber).IsUnique();
            });

            // Loan configuration
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LateFee).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Book)
                      .WithMany(b => b.Loans)
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Member)
                      .WithMany(m => m.Loans)
                      .HasForeignKey(e => e.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Authors
            modelBuilder.Entity<Author>().HasData(
                new Author
                {
                    Id = 1,
                    FirstName = "J.K.",
                    LastName = "Rowling",
                    Biography = "British author, best known for the Harry Potter fantasy series.",
                    DateOfBirth = new DateTime(1965, 7, 31),
                    Nationality = "British",
                    CreatedAt = DateTime.UtcNow
                },
                new Author
                {
                    Id = 2,
                    FirstName = "George R.R.",
                    LastName = "Martin",
                    Biography = "American novelist and short story writer in the fantasy and science fiction genres.",
                    DateOfBirth = new DateTime(1948, 9, 20),
                    Nationality = "American",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Books
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    ISBN = "9780439708180",
                    Title = "Harry Potter and the Sorcerer's Stone",
                    Description = "The first book in the Harry Potter series.",
                    PublishedDate = new DateTime(1997, 6, 26),
                    AvailableCopies = 5,
                    TotalCopies = 5,
                    AuthorId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 2,
                    ISBN = "9780553103540",
                    Title = "A Game of Thrones",
                    Description = "The first book in A Song of Ice and Fire series.",
                    PublishedDate = new DateTime(1996, 8, 1),
                    AvailableCopies = 3,
                    TotalCopies = 3,
                    AuthorId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
