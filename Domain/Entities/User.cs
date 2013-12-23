using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        public virtual string Name { get; set; }
        public virtual IList<File> LockedFiles { get; private set; }
    }
}
