using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
    public class FileManagerClient : IDisposable
    {
        private bool _disposed = false;
        private FileManagerServiceProxy.FileManagerServiceClient _serviceProxy;

        private string _authError = "You need connect\r\n";
        private string _addressError = "Wrong address or service unavailable\r\n";
        private string _reconnectError = "You need quit before reconnect\r\n";

        private string Connect(string address, string userName)
        {
            try
            {
                if (_serviceProxy == null)
                {
                    _serviceProxy = new FileManagerServiceProxy.FileManagerServiceClient(
                        new System.ServiceModel.InstanceContext(new Notificator()),
                        new NetTcpContextBinding(),
                        new EndpointAddress("net.tcp://" + address + "/FileManagerService/"));
                    _serviceProxy.Open();
                }
                else
                {
                    return _reconnectError;
                }
            }
            catch
            {
                _serviceProxy = null;
                return _addressError;
            }
            return _serviceProxy.ExecuteCommand("connect " + userName);
        }

        private string Quit()
        {
            if(_serviceProxy != null)
            {
                var result = _serviceProxy.ExecuteCommand("quit");
                _serviceProxy.Close();

                _serviceProxy = null;
                _disposed = true;

                return result;
            }
            return _authError;
        }

        public string ExecuteCommand(string command)
        {
            command = command.ToLower();
            command = command.Trim();
            var pattern = new Regex(@"(\S*)(\s*)(\S*)(\s*)(\S*)");
            var match = pattern.Match(command);

            var com = match.Groups[1].Captures[0].Value;
            var param = match.Groups[3].Captures[0].Value;
            var param2 = match.Groups[5].Captures[0].Value;

            switch (com)
            {
                case "connect":
                    return Connect(param, param2);
                case "quit":
                    return Quit();
                default:
                    if (_serviceProxy != null)
                    {
                        return _serviceProxy.ExecuteCommand(command);
                    }
                    return _authError;
            }           
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Quit();
            }
        }
}
}
