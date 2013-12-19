using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class FileSystemDb : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Directory> Directories { get; set; }

        public FileSystemDb()
            : base("DefaultConnection")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<FileSystemDb>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<File>().HasKey(f => f.Id);
            modelBuilder.Entity<Directory>().HasKey(d => d.Id);

            modelBuilder.Entity<User>().HasMany(u => u.LockedFiles)
                                       .WithMany(f => f.LockingUsers)
                                       .Map(t => t.MapLeftKey("UserId")
                                                  .MapRightKey("FileId")
                                                  .ToTable("Locks"));

            modelBuilder.Entity<File>().HasRequired(f => f.Directory)
                                       .WithMany(d => d.Files)
                                       .HasForeignKey(f => f.DirectoryId)
                                       .WillCascadeOnDelete(true);

            modelBuilder.Entity<Directory>().HasOptional(d => d.Root)
                                            .WithMany(d => d.Subdirectories)
                                            .HasForeignKey(d => d.RootId)
                                            .WillCascadeOnDelete(false);
        }
    }
}
