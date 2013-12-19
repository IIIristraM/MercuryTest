using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        private ICollection<File> _lockedFiles;

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ICollection<File> LockedFiles
        {
            get
            {
                return _lockedFiles ?? (_lockedFiles = new HashSet<File>());
            }
        }
    }
}
