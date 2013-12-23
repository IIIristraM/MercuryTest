using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class FileSystemServiceTest
    {
        IFileSystemService service = new FileSystemService();

        public FileSystemServiceTest()
        {
            AddUser();
            AddDirectory();
            AddFile();
        }

        [TestMethod]
        public void AddUser()
        {
            var count = service.GetUsers(null).Count();
            var user = new User()
            {
                Name = "Konstantin_" + Guid.NewGuid()
            };

            var success = service.AddUser(ref user);
            var newCount = service.GetUsers(null).Count();
            Assert.IsTrue(success);
            Assert.AreEqual(newCount, count + 1);

            //add null
            success = true;
            try
            {
                user = null;
                service.AddUser(ref user);
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void RemoveUser()
        {
            var count = service.GetUsers(null).Count();
            var user = service.GetUsers(null).First();

            var success = service.RemoveUser(user);
            var newCount = service.GetUsers(null).Count();
            Assert.IsTrue(success);
            Assert.AreEqual(newCount, count - 1);

            //remove null
            success = true;
            try
            {
                service.RemoveUser(null);
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void AddFile()
        {
            var count = service.GetFiles(null).Count();
            var file = new File()
            {
                FullPath = @"c:\temp_" + Guid.NewGuid() + ".txt",
                Directory = service.GetDirectories(null).First()
            };

            var success = service.AddFile(ref file);
            var newCount = service.GetFiles(null).Count();
            Assert.IsTrue(success);
            Assert.AreEqual(newCount, count + 1);

            //add null
            success = true;
            try
            {
                file = null;
                service.AddFile(ref file);
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void RemoveFile()
        {
            var count = service.GetFiles(null).Count();
            var file = service.GetFiles(null).First();

            var success = service.RemoveFile(file);
            var newCount = service.GetFiles(null).Count();
            Assert.IsTrue(success);
            Assert.AreEqual(newCount, count - 1);

            //remove null
            success = true;
            try
            {
                service.RemoveFile(null);
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void AddDirectory()
        {
            var count = service.GetDirectories(null).Count();
            var directory = new Directory()
            {
                FullPath = @"c:\data_" + Guid.NewGuid()
            };

            var success = service.AddDirectory(ref directory);
            var newCount = service.GetDirectories(null).Count();
            Assert.IsTrue(success);
            Assert.AreEqual(newCount, count + 1);

            //add null
            success = true;
            try
            {
                directory = null;
                service.AddDirectory(ref directory);
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void RemoveDirectory()
        {
            var count = service.GetDirectories(null).Count();
            var directory = service.GetDirectories(null).First();

            var success = service.RemoveDirectory(directory);
            var newCount = service.GetDirectories(null).Count();
            Assert.IsTrue(success);
            Assert.AreEqual(newCount, count - 1);

            //remove null
            success = true;
            try
            {
                service.RemoveDirectory(null);
            }
            catch
            {
                success = false;
            }
            Assert.IsFalse(success);
        }
    }
}
