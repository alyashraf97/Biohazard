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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			// Configure the QMail entity

			modelBuilder.Entity<QMail>(entity =>
			{
				// Specify the table name
				entity.ToTable("quarantined_mail");

				// Specify the primary key
				entity.HasKey(e => e.UniqueId);

				// Specify the column names and data types
				entity.Property(e => e.UniqueId).HasColumnName("id").HasColumnType("varchar(255)").IsRequired();
				entity.Property(e => e.Sender).HasColumnName("sender").HasColumnType("varchar(255)").IsRequired();
				entity.Property(e => e.Body).HasColumnName("body").HasColumnType("text").IsRequired();
				entity.Property(e => e.Date).HasColumnName("date").HasColumnType("timestamp").IsRequired();
				entity.Property(e => e.Subject).HasColumnName("subject").HasColumnType("varchar(255)").IsRequired();
				entity.Property(e => e.Header).HasColumnName("header").HasColumnType("text").IsRequired();
				entity.Property(e => e.Severity).HasColumnName("severity").HasColumnType("int").IsRequired();
				entity.Property(e => e.CurrentState).HasColumnName("current_state").HasColumnType("varchar(255)").IsRequired();

				// Configure the relationship with the Response entity
				entity.HasOne(e => e.Response)
					.WithOne(e => e.QMail)
					.HasForeignKey<Response>(e => e.MessageId)
					.IsRequired()
					.OnDelete(DeleteBehavior.Cascade);

				// Specify any other constraints or configurations
				// For example, you can make some columns required or unique
				entity.HasIndex(e => e.UniqueId).IsUnique();
			});

			modelBuilder.Entity<Response>(entity =>
			{
				// Specify the table name
				entity.ToTable("responses");

				// Specify the primary key
				entity.HasKey(e => e.Id);

				// Specify the column names and data types
				entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int").IsRequired();
				entity.Property(e => e.Type).HasColumnName("type").HasColumnType("varchar(255)").IsRequired();
				entity.Property(e => e.Time).HasColumnName("time").HasColumnType("timestamp").IsRequired();
				entity.Property(e => e.EventId).HasColumnName("event_id").HasColumnType("uuid").IsRequired();
				entity.Property(e => e.MessageId).HasColumnName("message_id").HasColumnType("text").IsRequired();

				// Specify any other constraints or configurations
				// For example, you can make some columns required or unique
				entity.HasIndex(e => e.Id).IsUnique();
			});
		}
    }

}