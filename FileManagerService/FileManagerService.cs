using Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManagerService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FileManagerService : IFileManagerService
    {
        private static IFileSystemService _fileSystem;
        private static ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new ConcurrentDictionary<string, ConnectedUser>();

        private ConnectedUser _currentUser;

        private string _defaultDirectory = "c:";
        private string _connectionError = "Error: User has been connected already";
        private string _disconnectMsg = "Disconnect comlete";
        private string _authError = "Error: You need to login";
        private string _unknownCommandError = "Error: Unknown command";
        private string _deleteCurrentDirError = "Error: Current directory can't be removed";
        private string _existingSubdirError = "Error: Directory has subdirectories";
        private string _twiceLockError = "Error: File can be locked only once";
        private string _fileLockedError = "Error: File is locked";
        private string _unlockError = "Error: You haven't lock this file";

        #region Private Methods

        public FileManagerService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
            var root = new Directory() { 
                Title = "c:",
                FullPath = "c:"
            };
            _fileSystem.AddDirectory(ref root);
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
                _fileSystem.AddUser(ref localUser);
            }
            var user = new ConnectedUser()
            {
                User = localUser,
                CurrentDirectory = _defaultDirectory/*,
                SessionId = OperationContext.Current.SessionId,
                CallbackChannel = OperationContext.Current.GetCallbackChannel<IClientNotification>()*/
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
            string fullPath = String.Empty;

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
            fullPath = parentPath + ((!String.IsNullOrEmpty(dirTitle)) ? @"\" + dirTitle : String.Empty);

            var result = new string[3];
            result[0] = parentPath;
            result[1] = dirTitle;
            result[2] = fullPath;
            return result;
        }
        private void PrintDirectory(Directory dir, StringBuilder tree, int level)
        {
            var prefix = GeneratePrefix(level - 1);
            tree.Append(prefix).Append(dir.Title + "\r\n");

            var subdirs = dir.Subdirectories;
            var files = dir.Files;
            var contentCount = subdirs.Count() + files.Count();

            var n = 0;
            foreach (var subdir in subdirs)
            {
                PrintDirectory(subdir, tree, level + 1);
                var postfix = GeneratePostfix(level);
                if ((subdirs.Count() > 0) && (n != contentCount - 1)) tree.Append(postfix + "\r\n");
                n++;
            }

            foreach (var file in files)
            {
                PrintFile(file, tree, level);
            }
        }
        private void PrintFile(File file, StringBuilder tree, int level)
        {
            var prefix = GeneratePrefix(level);
            var lockers = file.LockingUsers;
            var lockInfo = new StringBuilder();
            if (lockers.Any())
            {
                lockInfo.Append(" [LOCKED by ");
                foreach (var user in lockers)
                {
                    lockInfo.Append(user.Name + ", ");
                }
                lockInfo.Remove(lockInfo.Length - 2, 2).Append("]");
            }
            tree.Append(prefix).Append(file.Title).Append(lockInfo + "\r\n");
        }
        private StringBuilder GeneratePrefix(int level)
        {
            var prefix = new StringBuilder();
            if (level >= 0)
            {
                for (var i = 0; i < level; i++)
                {
                    prefix.Append("|  ");
                }
                prefix.Append("|__");
            }
            return prefix;
        }
        private StringBuilder GeneratePostfix(int level)
        {
            var postfix = new StringBuilder();
            for (var i = 0; i <= level; i++)
            {
                postfix.Append("|  ");
            }
            return postfix;
        }

        #endregion

        public string ExecuteCommand(string command)
        {
            command = command.ToLower();
            var pattern = new Regex(@"(\S*)(\s*)(\S*)");
            var match = pattern.Match(command);

            var com = match.Groups[1].Captures[0].Value;
            var param = match.Groups[3].Captures[0].Value;

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
                case "rd":
                    response = DeleteDirectory(param);
                    break;
                case "deltree":
                    response = DeleteTree(param);
                    break;
                case "print":
                    response = Print();
                    break;
                case "mf":
                    response = CreateFile(param);
                    break;
                case "del":
                    response = DeleteFile(param);
                    break;
                case "lock":
                    response = Lock(param);
                    break;
                case "unlock":
                    response = Unlock(param);
                    break;
                default:
                    response = _unknownCommandError;
                    break;
            }
            return response;
        }
        public string Connect(string userName)
        {
            if (!_connectedUsers.ContainsKey(userName))
            {
                if (RegisterUser(userName))
                {
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
                ConnectedUser connectedUser = _connectedUsers.Select(u => u.Value).Where(u => u.SessionId == sessionId).FirstOrDefault();
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
                string fullPath = parser[2];

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
                        _fileSystem.AddDirectory(ref directory);
                        return _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        return "Error: Directory '" + parentPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: Directory '" + fullPath + "' already exists\r\n" + _currentUser.CurrentDirectory + ">";
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
                string fullPath = parser[2];

                if (_fileSystem.GetDirectories(d => d.FullPath == fullPath).Any())
                {
                    _currentUser.CurrentDirectory = fullPath;
                    return fullPath + ">";
                }
                else
                {
                    return "Error: Directory '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
        }
        public string DeleteDirectory(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[2];
                var dir = _fileSystem.GetDirectories(d => d.FullPath == fullPath).FirstOrDefault();
                if (dir != null)
                {
                    if (dir.Subdirectories.Count() == 0)
                    {
                        if (dir.FullPath != _currentUser.CurrentDirectory)
                        {
                            var files = dir.Files;
                            if (!files.Where(f => f.LockingUsers.Any()).Any())
                            {
                                foreach (var file in files)
                                {
                                    _fileSystem.RemoveFile(file);
                                }
                                _fileSystem.RemoveDirectory(dir);
                                return _currentUser.CurrentDirectory + ">";
                            }
                            else
                            {
                                return "Error: Directory '" + dir.FullPath + "' has locked files\r\n" + _currentUser.CurrentDirectory + ">";
                            }
                        }
                        else
                        {
                            return _deleteCurrentDirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                        }
                    }
                    else
                    {
                        return _existingSubdirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: Directory '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
        }
        public string DeleteTree(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[2];
                var dirs = _fileSystem.GetDirectories(d => (d.FullPath == fullPath) || d.FullPath.Contains(fullPath + @"\"));
                if (dirs.Count() > 0)
                {
                    if (!dirs.Where(d => d.Files.Any(f => f.LockingUsers.Any())).Any())
                    {
                        if (!dirs.Where(d => d.FullPath == _currentUser.CurrentDirectory).Any())
                        {
                            foreach (var dir in dirs)
                            {
                                _fileSystem.RemoveDirectory(dir);
                                foreach (var file in dir.Files)
                                {
                                    _fileSystem.RemoveFile(file);
                                }
                            }
                            return _currentUser.CurrentDirectory + ">";
                        }
                        else
                        {
                            return _deleteCurrentDirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                        }
                    }
                    else
                    {
                        return "Error: Directory '" + fullPath + "' has locked files\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: Directory '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
        }
        public string CreateFile(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string parentPath = parser[0];
                string fileTitle = parser[1];
                string fullPath = parser[2];

                if (!_fileSystem.GetFiles(d => d.FullPath == fullPath).Any())
                {
                    var parentDir = _fileSystem.GetDirectories(d => d.FullPath == parentPath).FirstOrDefault();
                    if (parentDir != null)
                    {
                        var file = new File()
                        {
                            Title = fileTitle,
                            Directory = parentDir,
                            FullPath = fullPath
                        };
                        _fileSystem.AddFile(ref file);
                        return _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        return "Error: Directory '" + parentPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: File '" + fullPath + "' already exists\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
        }
        public string DeleteFile(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[2];
                var file = _fileSystem.GetFiles(d => d.FullPath == fullPath).FirstOrDefault();
                if (file != null)
                {
                    if (!file.LockingUsers.Any())
                    {
                        _fileSystem.RemoveFile(file);
                        return _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        return _fileLockedError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: File '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
        }
        public string Lock(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[2];
                var file = _fileSystem.GetFiles(d => d.FullPath == fullPath).FirstOrDefault();
                if (file != null)
                {
                    if (!file.LockingUsers.Where(u => u.Name == _currentUser.User.Name).Any())
                    {
                        _currentUser.User.LockedFiles.Add(file);
                        return _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        return _twiceLockError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: File '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
        }
        public string Unlock(string path)
        {
            if (_currentUser != null)
            {
                 var parser = ParsePath(path);
                string fullPath = parser[2];
                var file = _fileSystem.GetFiles(d => d.FullPath == fullPath).FirstOrDefault();
                if (file != null)
                {
                    if (file.LockingUsers.Where(u => u.Name == _currentUser.User.Name).Any())
                    {
                        _currentUser.User.LockedFiles.Remove(file);
                        return _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        return _unlockError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                else
                {
                    return "Error: File '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            else
            {
                return _authError;
            }
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
            if (_currentUser != null)
            {
                StringBuilder tree = new StringBuilder();
                var roots = _fileSystem.GetDirectories(d => d.Root == null);
                foreach (var root in roots)
                {
                    PrintDirectory(root, tree, 0);
                    tree.Append("\r\n");
                }
                return tree.Append(_currentUser.CurrentDirectory + ">").ToString();
            }
            else
            {
                return _authError;
            }
        }
    }
}
