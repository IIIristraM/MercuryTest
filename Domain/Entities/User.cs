using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual IEnumerable<File> LockedFiles { get; set; }
    }
}
