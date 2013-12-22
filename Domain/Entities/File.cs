using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class File
    {
        public virtual string Title { get; set; }
        public virtual string FullPath { get; set; }
        public virtual Directory Directory { get; set; }
        public virtual IEnumerable<User> LockingUsers { get; private set; }
    }
}
