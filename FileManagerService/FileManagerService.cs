using Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FileManagerService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class FileManagerService : IFileManagerService
    {
        private static IFileSystemService _fileSystem;
        private static ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new ConcurrentDictionary<string, ConnectedUser>();

        private string _defaultDirectory = @"C:\";

        public FileManagerService(IFileSystemService fileSystem)
        {

            _fileSystem = fileSystem;
        }

        private ConnectedUser RegisterUser(string userName)
        {
            var user = new ConnectedUser()
            {
                User = new User() { Name = userName },
                CurrentDirectory = _defaultDirectory,
                CallbackChannel = OperationContext.Current.GetCallbackChannel<IClientNotification>()
            };
            return user;
        }

        public string Connect(string userName)
        {
            var user = RegisterUser(userName);
            user.CallbackChannel.PrintNotification(user.User.Name + " connected");
            return user.CurrentDirectory;
        }

        public string Quit()
        {
            throw new NotImplementedException();
        }

        public string CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string ChangeDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string DeleteTree(string path)
        {
            throw new NotImplementedException();
        }

        public string CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public string DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public string Lock(string path)
        {
            throw new NotImplementedException();
        }

        public string Unlock(string path)
        {
            throw new NotImplementedException();
        }

        public string Copy(string sourcePath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public string Move(string sourcePath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public string Print()
        {
            throw new NotImplementedException();
        }
    }
}
