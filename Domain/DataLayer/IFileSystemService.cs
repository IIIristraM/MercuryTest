using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Domain
{
    public interface IFileSystemService
    {
        bool AddDirectory(Directory directory);
        bool AddFile(File file);
        bool AddUser(User user);
        IEnumerable<Directory> GetDirectories(Expression<Func<Directory, bool>> where);
        IEnumerable<File> GetFiles(Expression<Func<File, bool>> where);
        IEnumerable<User> GetUsers(Expression<Func<User, bool>> where);
        bool RemoveDirectory(Directory directory);
        bool RemoveFile(File file);
        bool RemoveUser(User user);
    }
}
