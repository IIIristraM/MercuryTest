using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Directory
    {
        private ICollection<File> _files;
        private ICollection<Directory> _subdirectories;

        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string FullPath { get; set; }
        public virtual int? RootId { get; set; }
        public virtual Directory Root { get; set; }
        public virtual ICollection<File> Files
        {
            get
            {
                return _files ?? (_files = new HashSet<File>());
            }
        }
        public virtual ICollection<Directory> Subdirectories
        {
            get
            {
                return _subdirectories ?? (_subdirectories = new HashSet<Directory>());
            }
        }
    }
}
