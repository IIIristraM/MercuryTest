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
        private static object _syncDir = new object();
        private static object _syncFile = new object();
        private static object _syncUser = new object();

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
        private string _sourceNotExistsError = "Error: Source directory or file doesn't exist";
        private string _destinationNotExistsError = "Error: Destination directory doesn't exist";
        private string _destinationHasContent = "Error: Destination already has such directory or file";

        public FileManagerService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
            var root = new Directory()
            {
                Title = "c:",
                FullPath = "c:"
            };
            _fileSystem.AddDirectory(ref root);
        }

        #region Protected Methods

        protected virtual string GetSessionId()
        {
            return OperationContext.Current.SessionId;
        }
        protected virtual IClientNotification GetCallbackChanel()
        {
            return OperationContext.Current.GetCallbackChannel<IClientNotification>();
        }

        #endregion

        #region Private Methods

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
                CurrentDirectory = _defaultDirectory,
                SessionId = GetSessionId(),
                CallbackChannel = GetCallbackChanel()
            };
            var success = _connectedUsers.TryAdd(user.User.Name, user);
            if (success) _currentUser = user;
            return success;
        }
        private void Broadcast(string message)
        {
            lock (_syncUser)
            {
                foreach (var connectedUser in _connectedUsers.Select(u => u.Value).Where(u => u.User.Name != _currentUser.User.Name))
                {
                    var u = connectedUser;
                    try
                    {
                        u.CallbackChannel.PrintNotification(message);
                    }
                    catch(CommunicationObjectAbortedException e)
                    {
                        _connectedUsers.TryRemove(connectedUser.User.Name, out u);
                    }
                }
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
                fullPath = parentPath + ((!String.IsNullOrEmpty(dirTitle)) ? @"\" + dirTitle : String.Empty);
            }
            else
            {
                if (path.LastIndexOf(@"\") != -1)
                {
                    parentPath += path.Substring(0, path.LastIndexOf(@"\"));
                    dirTitle = path.Substring(path.LastIndexOf(@"\") + 1);
                    fullPath = parentPath + ((!String.IsNullOrEmpty(dirTitle)) ? @"\" + dirTitle : String.Empty);
                }
                else
                {
                    parentPath = String.Empty;
                    dirTitle = path;
                    fullPath = ((!String.IsNullOrEmpty(parentPath)) ? parentPath + @"\" : String.Empty) + dirTitle;
                }
            }

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
        private void CopyTree(Directory directory, Directory root)
        {
            var dirCopy = new Directory()
            {
                Title = directory.Title,
                Root = root,
                FullPath = root.FullPath + @"\" + directory.Title
            };

            foreach (var subdir in directory.Subdirectories)
            {
                CopyTree(subdir, dirCopy);
            }

            foreach (var file in directory.Files)
            {
                CopyFile(file, dirCopy);
            }

            _fileSystem.AddDirectory(ref dirCopy);
        }
        private void CopyFile(File file, Directory destination)
        {
            var fileCopy = new File()
            {
                Directory = destination,
                Title = file.Title,
                FullPath = destination.FullPath + @"\" + file.Title
            };
            _fileSystem.AddFile(ref fileCopy);
        }

        #endregion

        #region Public Methods

        public string ExecuteCommand(string command)
        {
            command = command.ToLower();
            var pattern = new Regex(@"(\S*)(\s*)(\S*)(\s*)(\S*)");
            var match = pattern.Match(command);

            var com = match.Groups[1].Captures[0].Value;
            var param = match.Groups[3].Captures[0].Value;
            var param2 = match.Groups[5].Captures[0].Value;

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
                case "copy":
                    response = Copy(param, param2);
                    break;
                case "move":
                    response = Move(param, param2);
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
                return _connectionError + "\r\n";
            }
            return _connectionError + "\r\n";
        }
        public string Quit()
        {
            if (_currentUser != null)
            {
                var sessionId = GetSessionId();
                ConnectedUser connectedUser = _connectedUsers.Select(u => u.Value).Where(u => u.SessionId == sessionId).FirstOrDefault();
                if (connectedUser != null)
                {
                    lock (_syncUser)
                    {
                        _connectedUsers.TryRemove(connectedUser.User.Name, out connectedUser);
                    }
                }
                return _disconnectMsg + "\r\n";
            }
            return _authError + "\r\n";
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
                    lock (_syncDir)
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
                            Broadcast(_currentUser.User.Name + " create directory '" + fullPath + "'\r\n");
                            return _currentUser.CurrentDirectory + ">";
                        }
                        return "Error: Directory '" + parentPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                return "Error: Directory '" + fullPath + "' already exists\r\n" + _currentUser.CurrentDirectory + ">";
            }
            return _authError + "\r\n";
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
                return "Error: Directory '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
            }
            return _authError + "\r\n";
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
                    if (dir.FullPath != _currentUser.CurrentDirectory)
                    {
                        lock (_syncDir)
                        {
                            if (dir.Subdirectories.Count() == 0)
                            {
                                lock (_syncFile)
                                {
                                    var files = dir.Files;
                                    if (!files.Where(f => f.LockingUsers.Any()).Any())
                                    {
                                        foreach (var file in files)
                                        {
                                            _fileSystem.RemoveFile(file);
                                        }
                                        _fileSystem.RemoveDirectory(dir);
                                        Broadcast(_currentUser.User.Name + " delete directory '" + fullPath + "'\r\n");
                                        return _currentUser.CurrentDirectory + ">";
                                    }
                                    return "Error: Directory '" + dir.FullPath + "' has locked files\r\n" + _currentUser.CurrentDirectory + ">";
                                }
                            }
                            return _existingSubdirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                        }
                    }
                    return _deleteCurrentDirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                }
                return "Error: Directory '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
            }
            return _authError + "\r\n";
        }
        public string DeleteTree(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[2];
                lock (_syncDir)
                {
                    var dirs = _fileSystem.GetDirectories(d => (d.FullPath == fullPath) || d.FullPath.StartsWith(fullPath + @"\"));
                    if (dirs.Count() > 0)
                    {
                        lock (_syncFile)
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
                                    Broadcast(_currentUser.User.Name + " deltree '" + fullPath + "'\r\n");
                                    return _currentUser.CurrentDirectory + ">";
                                }
                                return _deleteCurrentDirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                            }
                            return "Error: Directory '" + fullPath + "' has locked files\r\n" + _currentUser.CurrentDirectory + ">";
                        }
                    }
                    return "Error: Directory '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            return _authError + "\r\n";
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
                    lock (_syncDir)
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
                            Broadcast(_currentUser.User.Name + " create file '" + fullPath + "'\r\n");
                            return _currentUser.CurrentDirectory + ">";
                        }
                        return "Error: Directory '" + parentPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                return "Error: File '" + fullPath + "' already exists\r\n" + _currentUser.CurrentDirectory + ">";
            }
            return _authError + "\r\n";
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
                    lock (_syncFile)
                    {
                        if (!file.LockingUsers.Any())
                        {
                            _fileSystem.RemoveFile(file);
                            Broadcast(_currentUser.User.Name + " delete file '" + fullPath + "'\r\n");
                            return _currentUser.CurrentDirectory + ">";
                        }
                        return _fileLockedError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                }
                return "Error: File '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
            }
            return _authError + "\r\n";
        }
        public string Lock(string path)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(path);
                string fullPath = parser[2];
                lock (_syncFile)
                {
                    var file = _fileSystem.GetFiles(d => d.FullPath == fullPath).FirstOrDefault();
                    if (file != null)
                    {
                        if (!file.LockingUsers.Where(u => u.Name == _currentUser.User.Name).Any())
                        {
                            _currentUser.User.LockedFiles.Add(file);
                            Broadcast(_currentUser.User.Name + " lock file '" + fullPath + "'\r\n");
                            return _currentUser.CurrentDirectory + ">";
                        }
                        return _twiceLockError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                    return "Error: File '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
                }
            }
            return _authError + "\r\n";
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
                        lock (_syncFile)
                        {
                            _currentUser.User.LockedFiles.Remove(file);
                            Broadcast(_currentUser.User.Name + " unlock file '" + fullPath + "'\r\n");
                            return _currentUser.CurrentDirectory + ">";
                        }
                    }
                    return _unlockError + "\r\n" + _currentUser.CurrentDirectory + ">";
                }
                return "Error: File '" + fullPath + "' doesn't exist\r\n" + _currentUser.CurrentDirectory + ">";
            }
            return _authError + "\r\n";
        }
        public string Copy(string sourcePath, string destinationPath)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(destinationPath);
                string destinationFullPath = parser[2];

                lock (_syncDir)
                {
                    var destination = _fileSystem.GetDirectories(d => d.FullPath == destinationFullPath).FirstOrDefault();
                    if (destination == null)
                    {
                        return _destinationNotExistsError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }

                    parser = ParsePath(sourcePath);
                    string sourceFullPath = parser[2];
                    string sourceTitle = parser[1];
                    string sourceParentPath = parser[0];

                    var sourceDir = _fileSystem.GetDirectories(d => d.FullPath == sourceFullPath).FirstOrDefault();
                    if (sourceDir != null)
                    {
                        string path = String.Empty;
                        if (!String.IsNullOrEmpty(sourceParentPath))
                        {
                            path = sourceFullPath.Replace(sourceParentPath, destinationFullPath);
                        }
                        else
                        {
                            //когда пытаются скопировать рутовый каталог
                            path = destinationFullPath + @"\" + sourceTitle;
                        }
                        if (!_fileSystem.GetDirectories(f => f.FullPath == path).Any())
                        {
                            lock (_syncFile)
                            {
                                CopyTree(sourceDir, destination);
                                Broadcast(_currentUser.User.Name + " copy '" + sourceFullPath + "' to '" + destinationFullPath + "'\r\n");
                                return _currentUser.CurrentDirectory + ">";
                            }
                        }
                        return _destinationHasContent + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        lock (_syncFile)
                        {
                            var sourceFile = _fileSystem.GetFiles(f => f.FullPath == sourceFullPath).FirstOrDefault();
                            if (sourceFile != null)
                            {
                                var path = sourceFullPath.Replace(sourceParentPath, destinationFullPath);
                                if (!_fileSystem.GetFiles(f => f.FullPath == path).Any())
                                {
                                    CopyFile(sourceFile, destination);
                                    Broadcast(_currentUser.User.Name + " copy '" + sourceFullPath + "' to '" + destinationFullPath + "'\r\n");
                                    return _currentUser.CurrentDirectory + ">";
                                }
                                return _destinationHasContent + "\r\n" + _currentUser.CurrentDirectory + ">";
                            }
                            return _sourceNotExistsError + "\r\n" + _currentUser.CurrentDirectory + ">";
                        }
                    }
                }
            }
            return _authError + "\r\n";
        }
        public string Move(string sourcePath, string destinationPath)
        {
            if (_currentUser != null)
            {
                var parser = ParsePath(destinationPath);
                string destinationFullPath = parser[2];

                lock (_syncDir)
                {
                    var destination = _fileSystem.GetDirectories(d => d.FullPath == destinationFullPath).FirstOrDefault();
                    if (destination == null)
                    {
                        return _destinationNotExistsError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }

                    parser = ParsePath(sourcePath);
                    string sourceFullPath = parser[2];
                    string sourceTitle = parser[1];
                    string sourceParentPath = parser[0];

                    var sourceTree = _fileSystem.GetDirectories(d => (d.FullPath == sourceFullPath) || (d.FullPath.StartsWith(sourceFullPath + @"\")));
                    if (sourceTree.Any())
                    {
                        string path = String.Empty;
                        if (!String.IsNullOrEmpty(sourceParentPath))
                        {
                            path = sourceFullPath.Replace(sourceParentPath, destinationFullPath);
                        }
                        else
                        {
                            //когда пытаются скопировать рутовый каталог
                            path = destinationFullPath + @"\" + sourceTitle;
                        }
                        if (!_fileSystem.GetDirectories(f => f.FullPath == path).Any())
                        {
                            lock (_syncFile)
                            {
                                var files = _fileSystem.GetFiles(f => f.FullPath.StartsWith(sourceFullPath + @"\"));
                                if (!files.Any(f => f.LockingUsers.Any()))
                                {
                                    foreach (var file in files)
                                    {
                                        var f = file;
                                        _fileSystem.RemoveFile(f);
                                        f.FullPath = f.FullPath.Replace(sourceParentPath, destinationFullPath);
                                        _fileSystem.AddFile(ref f);
                                    }
                                    foreach (var dir in sourceTree)
                                    {
                                        var d = dir;
                                        _fileSystem.RemoveDirectory(d);
                                        dir.FullPath = dir.FullPath.Replace(sourceParentPath, destinationFullPath);
                                        if (dir.FullPath == destinationFullPath + @"\" + sourceTitle) dir.Root = destination;
                                        _fileSystem.AddDirectory(ref d);
                                    }
                                    Broadcast(_currentUser.User.Name + " move '" + sourceFullPath + "' to '" + destinationFullPath + "'\r\n");
                                    return _currentUser.CurrentDirectory + ">";
                                }
                                return "Error: Directory '" + sourceFullPath + "' has locked files\r\n" + _currentUser.CurrentDirectory + ">";
                            }
                        }
                        return _destinationHasContent + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        lock (_syncFile)
                        {
                            var sourceFile = _fileSystem.GetFiles(f => f.FullPath == sourceFullPath).FirstOrDefault();
                            if (sourceFile != null)
                            {
                                 var path = sourceFullPath.Replace(sourceParentPath, destinationFullPath);
                                 if (!_fileSystem.GetFiles(f => f.FullPath == path).Any())
                                 {
                                     if (!sourceFile.LockingUsers.Any())
                                     {
                                         _fileSystem.RemoveFile(sourceFile);
                                         sourceFile.FullPath = path;
                                         sourceFile.Directory = destination;
                                         _fileSystem.AddFile(ref sourceFile);
                                         Broadcast(_currentUser.User.Name + " move '" + sourceFullPath + "' to '" + destinationFullPath + "'\r\n");
                                         return _currentUser.CurrentDirectory + ">";
                                     }
                                     return _fileLockedError + "\r\n" + _currentUser.CurrentDirectory + ">";
                                 }
                                return _destinationHasContent + "\r\n" + _currentUser.CurrentDirectory + ">";
                            }
                            return _sourceNotExistsError + "\r\n" + _currentUser.CurrentDirectory + ">";
                        }
                    }
                }
            }
            return _authError + "\r\n";
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
            return _authError + "\r\n";
        }

        #endregion
    }
}
