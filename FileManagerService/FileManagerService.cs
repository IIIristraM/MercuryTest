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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FileManagerService : IFileManagerService
    {
        private static IFileSystemService _fileSystem;
        private static ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new ConcurrentDictionary<string, ConnectedUser>();

        private ConnectedUser _currentUser;

        private string _defaultDirectory = @"c:";
        private string _connectionError = "Error: User has been connected already";
        private string _disconnectMsg = "Disconnect comlete";
        private string _authError = "You need to login";

        public FileManagerService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
            _fileSystem.AddDirectory(new Directory() { 
                Title = "c:",
                FullPath = "c:"
            });
        }

        private bool RegisterUser(string userName)
        {
            var localUser = _fileSystem.GetUsers(u => u.Name == userName).FirstOrDefault();
            if (localUser == null)
            {
                localUser = new User()
                {
                    Name = userName
                };
                _fileSystem.AddUser(localUser);
            }
            var user = new ConnectedUser()
            {
                User = localUser,
                CurrentDirectory = _defaultDirectory,
                SessionId = OperationContext.Current.SessionId,
                CallbackChannel = OperationContext.Current.GetCallbackChannel<IClientNotification>()
            };
            var success = _connectedUsers.TryAdd(user.User.Name, user);
            if (success) _currentUser = user;
            return success;
        }

        private void Broadcast(string message)
        {
            foreach (var connectedUser in _connectedUsers.Select(u => u.Value).Where(u => u.User.Name != _currentUser.User.Name))
            {
                connectedUser.CallbackChannel.PrintNotification(message);
            }
        }

        private string[] ParsePath(string path)
        {
            string parentPath = String.Empty;
            string dirTitle = String.Empty;

            if (path.IndexOf(":") == -1)
            {
                parentPath = _currentUser.CurrentDirectory;
                if (path.LastIndexOf(@"\") != -1)
                {
                    parentPath += @"\" + path.Substring(0, path.LastIndexOf(@"\"));
                    dirTitle = path.Substring(path.LastIndexOf(@"\") + 1);
                }
                else
                {
                    dirTitle = path;
                }
            }
            else
            {
                if (path.LastIndexOf(@"\") != -1)
                {
                    parentPath += path.Substring(0, path.LastIndexOf(@"\"));
                    dirTitle = path.Substring(path.LastIndexOf(@"\") + 1);
                }
                else
                {
                    parentPath = path;
                }
            }

            var result = new string[2];
            result[0] = parentPath;
            result[1] = dirTitle;
            return result;
        }

        public string Connect(string userName)
        {
            if (!_connectedUsers.ContainsKey(userName))
            {
                if (RegisterUser(userName))
                {
                    Broadcast(userName + " connected");
                    return _currentUser.CurrentDirectory + ">";
                }
                else
                {
                    return _connectionError;
                }
            }
            else
            {
                return _connectionError;
            }
        }

        public string Quit()
        {
            if (_currentUser != null)
            {
                var sessionId = OperationContext.Current.SessionId;
                ConnectedUser connectedUser = _connectedUsers.Select(u=>u.Value).Where(u=>u.SessionId == sessionId).FirstOrDefault();
                if (connectedUser != null)
                {
                    _connectedUsers.TryRemove(connectedUser.User.Name, out connectedUser);
                }
                return _disconnectMsg;
            }
            else
            {
                return _authError;
            }
        }

        public string CreateDirectory(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string parentPath = parser[0];
                string dirTitle = parser[1];
                string fullPath = parentPath + ((!String.IsNullOrEmpty(dirTitle)) ?  @"\" + dirTitle : String.Empty);

                if (!_fileSystem.GetDirectories(d => d.FullPath == fullPath).Any())
                {
                    var parentDir = _fileSystem.GetDirectories(d => d.FullPath == parentPath).FirstOrDefault();
                    if (parentDir != null)
                    {
                        var directory = new Directory()
                        {
                            Title = dirTitle,
                            Root = parentDir,
                            FullPath = fullPath
                        };
                        _fileSystem.AddDirectory(directory);
                        return "Directory '" + fullPath + "' created";
                    }
                    else
                    {
                        return "Error: Directory '" + parentPath + "' doesn't exist";
                    }
                }
                else
                {
                    return "Error: Directory '" + fullPath + "' already exists";
                }
            }
            else
            {
                return _authError;
            }
        }

        public string ChangeDirectory(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[0] + ((!String.IsNullOrEmpty(parser[1])) ? @"\" + parser[1] : String.Empty);

                if (_fileSystem.GetDirectories(d => d.FullPath == fullPath).Any())
                {
                    _currentUser.CurrentDirectory = fullPath;
                    return fullPath + ">";
                }
                else
                {
                    return "Error: Directory '" + fullPath + "' doesn't exist";
                }
            }
            else
            {
                return _authError;
            }
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
