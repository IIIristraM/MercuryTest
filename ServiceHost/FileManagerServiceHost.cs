using Domain;
using FileManagerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceHost
{
    public class FileManagerServiceHost : System.ServiceModel.ServiceHost
    {
        private IFileSystemService _fileSystem;

        public FileManagerServiceHost(Type serviceType, IFileSystemService fileSystem)
            : base(serviceType)
        {
            _fileSystem = fileSystem;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            Description.Behaviors.Add(new FileManagerServiceBehavior(_fileSystem));
            base.OnOpen(timeout);
        }
    }
}
