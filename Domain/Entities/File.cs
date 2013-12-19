using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class File
    {
        private ICollection<User> _lockingUsers;

        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string FullPath { get; set; }
        public virtual int DirectoryId { get; set; }
        public virtual Directory Directory { get; set; }
        public virtual ICollection<User> LockingUsers
        {
            get
            {
                return _lockingUsers ?? (_lockingUsers = new HashSet<User>());
            }
        }
    }
}
