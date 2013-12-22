using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Directory
    {
        public virtual string Title { get; set; }
        public virtual string FullPath { get; set; }
        public virtual Directory Root { get; set; }
        public virtual IEnumerable<File> Files { get; private set; }
        public virtual IEnumerable<Directory> Subdirectories { get; private set; }
    }
}
