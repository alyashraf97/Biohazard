using Microsoft.EntityFrameworkCore;
using Biohazard.Model;

namespace Biohazard.Data
{
    public class QMailDbContext : DbContext
    {
        public QMailDbContext(DbContextOptions<QMailDbContext> options) : base(options)
        {

        }

        public DbSet<QMail> QMails { get; set; }
        public DbSet<Response> Responses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=biohazard;Username=postgres;Password=postgres");
        }

        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the QMail entity
            
            modelBuilder.Entity<QMail>(entity =>
            {
                // Specify the table name
                entity.ToTable("quarantined_mail");

                // Specify the primary key
                entity.HasKey(e => e.ID);

                // Specify the column names and data types
                entity.Property(e => e.ID).HasColumnName("id").HasColumnType("varchar(255)");
                entity.Property(e => e.Sender).HasColumnName("sender").HasColumnType("varchar(255)");
                entity.Property(e => e.Body).HasColumnName("body").HasColumnType("text");
                entity.Property(e => e.Date).HasColumnName("date").HasColumnType("timestamp");
                entity.Property(e => e.Subject).HasColumnName("subject").HasColumnType("varchar(255)");
                entity.Property(e => e.Header).HasColumnName("header").HasColumnType("text");
                entity.Property(e => e.Severity).HasColumnName("severity").HasColumnType("int");

                // Specify any other constraints or configurations
                // For example, you can make some columns required or unique
                entity.Property(e => e.Sender).IsRequired();
                entity.Property(e => e.Subject).IsRequired();
                entity.HasIndex(e => e.ID).IsUnique();
            
            });
        }*/
    }

}