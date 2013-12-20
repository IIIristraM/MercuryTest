using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Domain
{
    public class FileSystemService : IFileSystemService
    {
        private static ConcurrentDictionary<string, User> Users = new ConcurrentDictionary<string, User>();
        private static ConcurrentDictionary<string, File> Files = new ConcurrentDictionary<string, File>();
        private static ConcurrentDictionary<string, Directory> Directories = new ConcurrentDictionary<string, Directory>();

        private class LocalUser : User
        {
        }

        private class LocalFile : File
        {
            private object _sync = new object();
            public override IEnumerable<User> LockingUsers
            {
                get
                {
                    return FileSystemService.Users.Select(u => u.Value).Where(u => u.LockedFiles.Any(f => f.Id == Id));
                }
            }

            public override Directory Directory
            {
                get
                {
                    lock (_sync)
                    {
                        return base.Directory;
                    }
                }
                set
                {
                    lock (_sync)
                    {
                        base.Directory = value;
                    }
                }
            }
        }

        private class LocalDirectory: Directory
        {
            private object _sync = new object();
            public override IEnumerable<File> Files
            {
                get
                {
                    return FileSystemService.Files.Select(f => f.Value).Where(f => f.Directory.Id == Id);
                }
            }

            public override IEnumerable<Directory> Subdirectories
            {
                get
                {
                    return FileSystemService.Directories.Select(d => d.Value).Where(d => d.Root.Id == Id);
                }
            }

            public override Directory Root
            {
                get
                {
                    lock (_sync)
                    {
                        return base.Root;
                    }
                }
                set
                {
                    lock (_sync)
                    {
                        base.Root = value;
                    }
                }
            }
        }

        public IEnumerable<User> GetUsers(Expression<Func<User, bool>> where)
        {
            var query = Users.Select(u => u.Value);
            if (where != null)
            {
                query = query.Where(where.Compile());
            }
            return query.ToList();
        }

        public bool RemoveUser(User user)
        {
            return Users.TryRemove(user.Id, out user);
        }

        public bool AddUser(User user)
        {
            var local = new LocalUser()
            {
                Id = Guid.NewGuid().ToString(),
                Name = user.Name
            };
            return Users.TryAdd(local.Id, local);
        }

        public IEnumerable<File> GetFiles(Expression<Func<File, bool>> where)
        {
            var query = Files.Select(u => u.Value);
            if (where != null)
            {
                query = query.Where(where.Compile());
            }
            return query.ToList();
        }

        public bool RemoveFile(File file)
        {
            return Files.TryRemove(file.Id, out file);
        }

        public bool AddFile(File file)
        {
            var local = new LocalFile()
            {
                Id = Guid.NewGuid().ToString(),
                Title = file.Title,
                FullPath = file.FullPath,
                Directory = file.Directory
            };
            return Files.TryAdd(local.Id, local);
        }

        public IEnumerable<Directory> GetDirectories(Expression<Func<Directory, bool>> where)
        {
            var query = Directories.Select(u => u.Value);
            if (where != null)
            {
                query = query.Where(where.Compile());
            }
            return query.ToList();
        }

        public bool RemoveDirectory(Directory directory)
        {
            return Directories.TryRemove(directory.Id, out directory);
        }

        public bool AddDirectory(Directory directory)
        {
            var local = new LocalDirectory()
            {
                Id = Guid.NewGuid().ToString(),
                Title = directory.Title,
                FullPath = directory.FullPath,
                Root = directory.Root
            };
            return Directories.TryAdd(local.Id, local);
        }
    }
}
