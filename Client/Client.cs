using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class FileManagerClient : IDisposable
    {
        private FileManagerServiceProxy.FileManagerServiceClient _serviceProxy;
        public FileManagerClient()
        {
            _serviceProxy = new FileManagerServiceProxy.FileManagerServiceClient(new System.ServiceModel.InstanceContext(new Notificator()));
            _serviceProxy.Open();
        }

        public string ExecuteCommand(string command)
        {
            return _serviceProxy.ExecuteCommand(command);
        }

        public void Dispose()
        {
            _serviceProxy.Close();
        }
}
}
