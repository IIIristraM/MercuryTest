using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IFileSystemService
    {
        Task<IEnumerable<User>> GetUsers(Expression<Func<User, bool>> where, Expression<Func<User, object>>[] includs);
        Task<User> GetUser(int id);
        Task RemoveUser(User user);
        Task AddUser(User user);

        Task<IEnumerable<File>> GetFiles(Expression<Func<File, bool>> where, Expression<Func<File, object>>[] includs);
        Task<File> GetFile(int id);
        Task RemoveFile(File file);
        Task AddFile(File file);

        Task<IEnumerable<Directory>> GetDirectories(Expression<Func<Directory, bool>> where, Expression<Func<Directory, object>>[] includs);
        Task<Directory> GetDirectory(int id);
        Task RemoveDirectory(Directory directory);
        Task AddDirectory(Directory directory);
    }
}
