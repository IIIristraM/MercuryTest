using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
    public class FileManagerClient: IDisposable
    {
        private FileManagerServiceProxy.FileManagerServiceClient _serviceProxy;
        public FileManagerClient()
        {
            _serviceProxy = new FileManagerServiceProxy.FileManagerServiceClient(new InstanceContext(new Notificator()));
            _serviceProxy.Open();
        }

        public string ExecuteCommand(string command)
        {
            command = command.ToLower();
            Regex exp = new Regex(@"(\S+)(\s*)(\S*)");
            var match = exp.Match(command);

            string com = match.Groups[1].Captures[0].Value;
            string param = match.Groups[3].Captures[0].Value;

            string response;
            switch (com)
            {
                case "connect":
                    response = Connect(param);
                    break;
                case "quit":
                    response = Quit();
                    break;
                case "md":
                    response = CreateDirectory(param);
                    break;
                case "cd":
                    response = ChangeDirectory(param);
                    break;
                default:
                    response = "unknown command";
                    break;
            }
            return response;
        }

        public string Connect(string userName)
        {
            return _serviceProxy.Connect(userName);
        }

        public string Quit()
        {
            return _serviceProxy.Quit();
        }

        public string CreateDirectory(string path)
        {
            return _serviceProxy.CreateDirectory(path);
        }

        public string ChangeDirectory(string path)
        {
            return _serviceProxy.ChangeDirectory(path);
        }

        public void Dispose()
        {
            _serviceProxy.Close();
        }
    }
}
