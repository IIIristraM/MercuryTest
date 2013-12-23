using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerService;
using Domain;
using Microsoft.QualityTools.Testing.Fakes;
using System.ServiceModel;

namespace UnitTests
{
    [TestClass]
    public class FileManagerServiceTest
    {
        IFileSystemService _fileSystemService;
        IFileManagerService _fileManagerService;

        public FileManagerServiceTest()
        {
            _fileSystemService = new FileSystemService();
            _fileManagerService = new FileManagerService.FileManagerService(_fileSystemService);
        }

        [TestMethod]
        public void Connect()
        {
            var userName = "Konstantin";

            var result = _fileManagerService.Connect(userName);
            Assert.AreEqual(result, @"c:>");

            result = _fileManagerService.Connect(userName);
            Assert.AreEqual(result, "Error: User has been connected already");
        }

        [TestMethod]
        public void CreateDirectory()
        {
            var userName = "Konstantin";
            var path = @"c:\temp";
            _fileManagerService.Connect(userName);

            var result = _fileManagerService.CreateDirectory(path);
            Assert.AreEqual(result, @"Directory 'c:\temp' created");

            path = "temp";

            result = _fileManagerService.CreateDirectory(path);
            Assert.AreEqual(result, @"Error: Directory 'c:\temp' already exists");

            path = @"temp2\temp3";

            result = _fileManagerService.CreateDirectory(path);
            Assert.AreEqual(result, @"Error: Directory 'c:\temp2' doesn't exist");
        }

        [TestMethod]
        public void ChangeDirectory()
        {
            var userName = "Konstantin";
            var path = "temp";
            _fileManagerService.Connect(userName);
            var result = _fileManagerService.CreateDirectory(path);

            result = _fileManagerService.ChangeDirectory(path);
            Assert.AreEqual(result, @"c:\temp>");

            path = @"temp2\temp3";
            _fileManagerService.Connect(userName);

            result = _fileManagerService.ChangeDirectory(path);
            Assert.AreEqual(result, @"Error: Directory 'c:\temp\temp2\temp3' doesn't exist");
        }
    }
}
