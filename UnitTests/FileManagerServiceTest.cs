using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerService;
using Domain;

namespace UnitTests
{
    [TestClass]
    public class FileManagerServiceTest
    {
        IFileSystemService _fileSystem;
        IFileManagerService _service;

        public FileManagerServiceTest()
        {
            _fileSystem = new FileSystemService();
            _service = new FileManagerService.FileManagerService(_fileSystem);
        }

        [TestMethod]
        public void Connect()
        {
            var userName = "konstantin" + Guid.NewGuid();
            var response = _service.Connect(userName);
            Assert.AreEqual(response, "c:>");

            response = _service.Connect(userName);
            Assert.AreEqual(response, "Error: User has been connected already");
        }

        /*[TestMethod]
        public void Quit()
        {
            var response = _service.Quit();
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            response = _service.Connect(userName);
            response = _service.Quit();
            Assert.AreEqual(response, "Disconnect comlete");
        }*/

        [TestMethod]
        public void CreateDirectory()
        {
            var path = "temp" + Guid.NewGuid();
            var response = _service.CreateDirectory(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);

            response = _service.CreateDirectory(path + @"\temp2");
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' doesn't exist\r\nc:>");

            response = _service.CreateDirectory(path);
            Assert.AreEqual(response, "c:>");

            response = _service.CreateDirectory(path);
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' already exists\r\nc:>");
        }

        [TestMethod]
        public void ChangeDirectory()
        {
            var path = "temp" + Guid.NewGuid();
            var response = _service.ChangeDirectory(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);
            response = _service.ChangeDirectory(path);
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' doesn't exist\r\nc:>");

            response = _service.CreateDirectory(path);
            response = _service.ChangeDirectory(path);
            Assert.AreEqual(response, @"c:\" + path + ">");
        }

        [TestMethod]
        public void DeleteDirectory()
        {
            var path = "temp" + Guid.NewGuid();
            var response = _service.DeleteDirectory(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);
            response = _service.DeleteDirectory(path);
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' doesn't exist\r\nc:>");

            _service.CreateDirectory(path);
            _service.ChangeDirectory(path);
             response = _service.DeleteDirectory("");
             Assert.AreEqual(response, "Error: Current directory can't be removed\r\nc:\\" + path + ">");

             response = _service.DeleteDirectory("c:");
             Assert.AreEqual(response, "Error: Directory has subdirectories\r\nc:\\" + path + ">");
            
            _service.ChangeDirectory("c:");
            response = _service.DeleteDirectory(path);
            Assert.AreEqual(response, "c:>");

            _service.CreateDirectory(path);
            _service.CreateFile(path + @"\1.txt");
            _service.Lock(path + @"\1.txt");
            response = _service.DeleteDirectory(path);
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' has locked files\r\nc:>");
        }

        [TestMethod]
        public void DeleteTree()
        {
            var path = "temp" + Guid.NewGuid();
            var response = _service.DeleteTree(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);
            response = _service.DeleteTree(path);
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' doesn't exist\r\nc:>");

            _service.CreateDirectory(path);
            _service.ChangeDirectory(path);
            response = _service.DeleteTree("");
            Assert.AreEqual(response, "Error: Current directory can't be removed\r\nc:\\" + path + ">");

            _service.CreateDirectory("temp2");
            _service.ChangeDirectory("c:");
            response = _service.DeleteTree(@"c:\" + path );
            Assert.AreEqual(response, "c:>");

            _service.CreateDirectory(path);
            _service.ChangeDirectory(path);
            _service.CreateDirectory("temp2");
            _service.CreateFile(@"temp2\1.txt");
            _service.Lock(@"temp2\1.txt");
            _service.ChangeDirectory("c:");
            response = _service.DeleteTree(@"c:\" + path);
            Assert.AreEqual(response, "Error: Directory 'c:\\" + path + "' has locked files\r\nc:>");
        }

        [TestMethod]
        public void CreateFile()
        {
            var path = "1.txt" + Guid.NewGuid();
            var response = _service.CreateFile(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);

            response = _service.CreateFile(@"temp\2.txt");
            Assert.AreEqual(response, "Error: Directory 'c:\\temp' doesn't exist\r\nc:>");

            response = _service.CreateFile(path);
            Assert.AreEqual(response, "c:>");

            response = _service.CreateFile(path);
            Assert.AreEqual(response, "Error: File 'c:\\" + path + "' already exists\r\nc:>");
        }

        [TestMethod]
        public void DeleteFile()
        {
            var path = "1.txt" + Guid.NewGuid();
            var response = _service.DeleteFile(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);
            response = _service.DeleteFile(path);
            Assert.AreEqual(response, "Error: File 'c:\\" + path + "' doesn't exist\r\nc:>");

            response = _service.CreateFile(path);
            response = _service.DeleteFile(path);
            Assert.AreEqual(response, "c:>");

            response = _service.CreateFile(path);
            response = _service.Lock(path);
            response = _service.DeleteFile(path);
            Assert.AreEqual(response, "Error: File is locked\r\nc:>");
        }

        [TestMethod]
        public void Lock()
        {
            var path = "1.txt" + Guid.NewGuid();
            var response = _service.Lock(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);
            response = _service.Lock(path);
            Assert.AreEqual(response, "Error: File 'c:\\" + path + "' doesn't exist\r\nc:>");

            response = _service.CreateFile(path);
            response = _service.Lock(path);
            Assert.AreEqual(response, "c:>");

            response = _service.Lock(path);
            Assert.AreEqual(response, "Error: File can be locked only once\r\nc:>");
        }

        [TestMethod]
        public void Unlock()
        {
            var path = "1.txt" + Guid.NewGuid();
            var response = _service.Unlock(path);
            Assert.AreEqual(response, "Error: You need to login");

            var userName = "konstantin" + Guid.NewGuid();
            _service.Connect(userName);
            response = _service.Unlock(path);
            Assert.AreEqual(response, "Error: File 'c:\\" + path + "' doesn't exist\r\nc:>");

            response = _service.CreateFile(path);
            response = _service.Unlock(path);
            Assert.AreEqual(response, "Error: You haven't lock this file\r\nc:>");

            response = _service.Lock(path);
            response = _service.Unlock(path);
            Assert.AreEqual(response, @"c:>");
        }

        [TestMethod]
        public void Copy()
        {
            var source = "temp";
            var destination = "temp2";
            var response = _service.Copy(source, destination);
            Assert.AreEqual(response, "Error: You need to login");
        }
    }
}
