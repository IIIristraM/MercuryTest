using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Domain;

namespace FileManagerService
{
    //необходим для разделения конкретной реализации сервиса и конкретной реализации файловой системы 
    public class FileManagerServiceInstanceProvider : IInstanceProvider
    {
        private IFileSystemService _fileSystem;

        public FileManagerServiceInstanceProvider(IFileSystemService creator)
        {
            this._fileSystem = creator;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return new FileManagerService(_fileSystem);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.GetInstance(instanceContext, null);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}
