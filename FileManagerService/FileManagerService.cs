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
        //Файловая система
        private static IFileSystemService _fileSystem;
        //Список подключенных в данный момент клиентов
        private static ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new ConcurrentDictionary<string, ConnectedUser>();
        //Объекты для синхронизации работы клиентов
        private static object _syncDir = new object();
        private static object _syncFile = new object();
        private static object _syncUser = new object();
        //Текущий пользователь
        private ConnectedUser _currentUser;

        private string _defaultDirectory = "c:";
        private string _connectionError = "Error: User has been connected already";
        private string _disconnectMsg = "Disconnect comlete";
        private string _authError = "Error: You need to login";
        private string _unknownCommandError = "Error: Unknown command";
        private string _deleteCurrentDirError = "Error: Current directory can't be moved or deleted";
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
            //создаем корневой каталог
            //можно ничего не синхронизировать, т.к. в файловой системе создать два каталога с одинаковым FullPath невозможно
            var root = new Directory()
            {
                Title = "c:",
                FullPath = "c:"
            };
            _fileSystem.AddDirectory(ref root);
        }

        //Эти методы нужны для облегчения тестирования
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

        //неблокирующая регистрация пользователя при вызове connect
        private bool RegisterUser(string userName)
        {
            //если пользователя нет в файловой системе, создаем, если есть - извлекаем
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
            //пробуем добавить к пользователям онлайн
            //если одновременно будет происходмить добавление нескольких пользователей, то true вернется только одному
            var success = _connectedUsers.TryAdd(user.User.Name, user);
            if (success) _currentUser = user;
            return success;
        }

        //рассылка сообщения всем подключенным клиентам
        private void Broadcast(string message)
        {
            //лочим, чтобы во время выполнения клиент не мог отключится, не получив оповещения
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
                        //на случай, если клиент был закрыт без вызова quit, и канал остался висеть
                        _connectedUsers.TryRemove(connectedUser.User.Name, out u);
                    }
                }
            }
        }

        //парсинг пути, полученного от клиента
        private string[] ParsePath(string path)
        {
            //полный путь к родительскому каталогу
            string parentPath = String.Empty;
            //название файла и ли каталога, к которому указан путь
            string dirTitle = String.Empty;
            //полный путь к указанному файлу или каталогу
            string fullPath = String.Empty;

            //проверяем, указан относительный путь или абсолютный
            if (path.IndexOf(":") == -1)
            {
                parentPath = _currentUser.CurrentDirectory;
                //проверяем простой путь или составной
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
                //проверяем простой путь или составной
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

        //рекурсивное построение дерева каталогов
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

        //рекурсивное копирование каталога со всем его содержимым
        private void CopyTree(Directory directory, Directory root)
        {
            //удаляем ":", чтобы избежать неверной интерпритации относительного/абсолютного пути
            var title = directory.Title.Replace(":", "");
            var dirCopy = new Directory()
            {
                Title = title,
                Root = root,
                FullPath = root.FullPath + @"\" + title
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
        
        //парсит команду и вызывает соответствующий метод сервиса
        public string ExecuteCommand(string command)
        {
            command = command.ToLower();
            command = command.Trim();
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
                    response = _unknownCommandError + "\r\n" + ((_currentUser != null) ? _currentUser.CurrentDirectory + ">" : String.Empty);
                    break;
            }
            return response;
        }
        public string Connect(string userName)
        {
            //первая проверка не дает гарантии того, что пользователи, вызвавшие одновременно connect с одним логином, не пройдут,
            //но может сэкономить время выполненя, если один уже залогинен
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
                //проверяем конектился ли пользователь в рамках текущей сессии
                ConnectedUser connectedUser = _connectedUsers.Select(u => u.Value).Where(u => u.SessionId == sessionId).FirstOrDefault();
                if (connectedUser != null)
                {
                    //лочим, чтобы не отвалились запущенные на данный момент рассылки опповещений клиентам
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

                //можно не синхронизировать, два каталога с одним FullPath все равно не могут быть добавлены в ConcurrentDictionary
                if (!_fileSystem.GetDirectories(d => d.FullPath == fullPath).Any())
                {
                    //лочим, чтобы во время добавления каталога ни один каталог из вышележащей иерархии не был удален 
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
                    {   //лочим, чтобы во время удаления не могли быть добавлены подкаталоги
                        lock (_syncDir)
                        {
                            if (dir.Subdirectories.Count() == 0)
                            {
                                //лочим, чтобы во время удаления не могли быть заблокированы файлы
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
                //лочим, чтобы во время удаления в удаляемую иерархию не могли быть добавлены каталоги
                lock (_syncDir)
                {
                    var dirs = _fileSystem.GetDirectories(d => (d.FullPath == fullPath) || d.FullPath.StartsWith(fullPath + @"\"));
                    if (dirs.Count() > 0)
                    {
                        //лочим, чтобы во время удаления не могли быть заблокированы файлы
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
                    //лочим, чтобы во время создания не мог быть удален родительский каталог
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
                    //лочим, чтобы избежать блокировки файла во время удаления
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
                //лочим, чтобы избежать удаления файла во время блокировки
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
                        //лочим, чтобы вызванные операции удаления дождались разблокировки файла
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
                //лочим, чтобы избежать создания и удаления каталогов, а также созания файлов во время выполнения
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
                            path = destinationFullPath + @"\" + (sourceTitle = sourceTitle.Remove(sourceTitle.Length - 1)); //удаляем ":", чтобы избежать неверной интерпритации относительного/абсолютного пути
                        }
                        if (!_fileSystem.GetDirectories(f => f.FullPath == path).Any())
                        {
                            //лочим, чтобы избежать удаления файлов
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
                        //лочим, чтобы избежать удаления файлов
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
                //лочим, чтобы избежать создания и удаления каталогов, а также созания файлов во время выполнения
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
                        if (!sourceTree.Any(d => d.FullPath == _currentUser.CurrentDirectory))
                        {
                            var path =  sourceFullPath.Replace(sourceParentPath, destinationFullPath);
                            if (!_fileSystem.GetDirectories(f => f.FullPath == path).Any())
                            {
                                //лочим, чтобы избежать удаления и блокировки файлов
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
                        return _deleteCurrentDirError + "\r\n" + _currentUser.CurrentDirectory + ">";
                    }
                    else
                    {
                        //лочим, чтобы избежать удаления и блокировки файлов
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
