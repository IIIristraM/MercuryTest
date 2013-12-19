using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Domain
{
    public class FileSystemService : IFileSystemService
    {
        #region Users

        public async Task<IEnumerable<User>> GetUsers(Expression<Func<User, bool>> where, Expression<Func<User, object>>[] includs)
        {
            using (FileSystemDb db = new FileSystemDb())
            {
                var query = db.Users.AsQueryable();
                if (includs != null)
                {
                    foreach (var include in includs)
                    {
                        query = query.Include(include);
                    }
                }
                if (where != null)
                {
                    query = query.Where(where);
                }
                return await query.ToListAsync();
            }
        }

        public async Task<User> GetUser(int id)
        {
            using (FileSystemDb db = new FileSystemDb())
            {
                return await db.Users.FindAsync(id);
            }
        }

        public async Task RemoveUser(User user)
        {
            if (user == null) throw new ArgumentNullException("User is NULL");
            using (FileSystemDb db = new FileSystemDb())
            {
                db.Set<User>().Attach(user);

                db.Users.Remove(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task AddUser(User user)
        {
            if (user == null) throw new ArgumentNullException("User is NULL");
            using (FileSystemDb db = new FileSystemDb())
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }

        #endregion

        #region Files

        public async Task<IEnumerable<File>> GetFiles(Expression<Func<File, bool>> where, Expression<Func<File, object>>[] includs)
        {
            using (FileSystemDb db = new FileSystemDb())
            {
                var query = db.Files.AsQueryable();
                if (includs != null)
                {
                    foreach (var include in includs)
                    {
                        query = query.Include(include);
                    }
                }
                if (where != null)
                {
                    query = query.Where(where);
                }
                return await query.ToListAsync();
            }
        }

        public async Task<File> GetFile(int id)
        {
            using (FileSystemDb db = new FileSystemDb())
            {
                return await db.Files.FindAsync(id);
            }
        }

        public async Task RemoveFile(File file)
        {
            if (file == null) throw new ArgumentNullException("File is NULL");
            using (FileSystemDb db = new FileSystemDb())
            {
                db.Set<File>().Attach(file);

                db.Files.Remove(file);
                await db.SaveChangesAsync();
            }
        }

        public async Task AddFile(File file)
        {
            if (file == null) throw new ArgumentNullException("File is NULL");
            using (FileSystemDb db = new FileSystemDb())
            {
                db.Files.Add(file);
                await db.SaveChangesAsync();
            }
        }

        #endregion

        #region Directories

        public async Task<IEnumerable<Directory>> GetDirectories(Expression<Func<Directory, bool>> where, Expression<Func<Directory, object>>[] includs)
        {
            using (FileSystemDb db = new FileSystemDb())
            {
                var query = db.Directories.AsQueryable();
                if (includs != null)
                {
                    foreach (var include in includs)
                    {
                        query = query.Include(include);
                    }
                }
                if (where != null)
                {
                    query = query.Where(where);
                }
                return await query.ToListAsync();
            }
        }

        public async Task<Directory> GetDirectory(int id)
        {
            using (FileSystemDb db = new FileSystemDb())
            {
                return await db.Directories.FindAsync(id);
            }
        }

        public async Task RemoveDirectory(Directory directory)
        {
            if (directory == null) throw new ArgumentNullException("Directory is NULL");
            using (FileSystemDb db = new FileSystemDb())
            {
                db.Set<Directory>().Attach(directory);

                db.Directories.Remove(directory);
                await db.SaveChangesAsync();
            }
        }

        public async Task AddDirectory(Directory directory)
        {
            if (directory == null) throw new ArgumentNullException("Directory is NULL");
            using (FileSystemDb db = new FileSystemDb())
            {
                db.Directories.Add(directory);
                await db.SaveChangesAsync();
            }
        }

        #endregion
    }
}
