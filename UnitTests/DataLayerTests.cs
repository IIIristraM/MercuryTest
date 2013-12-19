using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class DataLayerTests
    {
        IFileSystemService service = new FileSystemService();

        [TestMethod]
        public void DbCreation()
        {
            FileSystemDb db = new FileSystemDb();
            db.Database.Initialize(false);
        }

        [TestMethod]
        public async Task AddUser()
        {
            await Task.Run(async () =>
            {
                //Add user
                User user = new User()
                    {
                        Name = "Konstantin " + Guid.NewGuid()
                    };
                var count = (await service.GetUsers(null, null)).Count();

                await service.AddUser(user);

                var newCount = (await service.GetUsers(null, null)).Count();
                Assert.AreEqual(newCount, count + 1);

                //Add Null
                var isException = false;
                try
                {
                    await service.AddUser(null);
                }
                catch
                {
                    isException = true;
                }
                if (!isException) Assert.Fail();
            });
        }

        [TestMethod]
        public async Task DeleteUser()
        {
            await Task.Run(async () =>
            {
                //Delete user
                var count = (await service.GetUsers(null, null)).Count();
                User user = (await service.GetUsers(null, null)).First();

                await service.RemoveUser(user);

                var newCount = (await service.GetUsers(null, null)).Count();
                Assert.AreEqual(newCount, count - 1);

                //Delete Null
                var isException = false;
                try
                {
                    await service.RemoveUser(null);
                }
                catch
                {
                    isException = true;
                }
                if (!isException) Assert.Fail();
            });
        }

        [TestMethod]
        public async Task AddDirectory()
        {
            await Task.Run(async () =>
            {
                //Add directory
                var path = "temp_" + Guid.NewGuid();
                Directory directory = new Directory()
                {
                    FullPath = @"C:\" + path
                };
                var count = (await service.GetDirectories(null, null)).Count();

                await service.AddDirectory(directory);

                var newCount = (await service.GetDirectories(null, null)).Count();
                Assert.AreEqual(newCount, count + 1);

                //Add Null
                var isException = false;
                try
                {
                    await service.AddDirectory(null);
                }
                catch
                {
                    isException = true;
                }
                if (!isException) Assert.Fail();
            });
        }

        [TestMethod]
        public async Task DeleteDirectory()
        {
            await Task.Run(async () =>
            {
                //Delete directory
                var count = (await service.GetDirectories(null, null)).Count();
                Directory directory = (await service.GetDirectories(null, null)).First();

                await service.RemoveDirectory(directory);

                var newCount = (await service.GetDirectories(null, null)).Count();
                Assert.AreEqual(newCount, count - 1);

                //Delete Null
                var isException = false;
                try
                {
                    await service.RemoveDirectory(null);
                }
                catch
                {
                    isException = true;
                }
                if (!isException) Assert.Fail();
            });
        }

        [TestMethod]
        public async Task AddFile()
        {
            await Task.Run(async () =>
            {
                //Add file
                Directory directory = (await service.GetDirectories(null, null)).First();
                var path = "text_" + Guid.NewGuid() + ".txt";
                File file = new File()
                {
                    FullPath = @"C:\" + path,
                    Directory = directory
                };
                var count = (await service.GetFiles(null, null)).Count();

                await service.AddFile(file);

                var newCount = (await service.GetFiles(null, null)).Count();
                Assert.AreEqual(newCount, count + 1);

                //Add Null
                var isException = false;
                try
                {
                    await service.AddFile(null);
                }
                catch
                {
                    isException = true;
                }
                if (!isException) Assert.Fail();
            });
        }

        [TestMethod]
        public async Task DeleteFile()
        {
            await Task.Run(async () =>
            {
                //Delete directory
                var count = (await service.GetFiles(null, null)).Count();
                File file = (await service.GetFiles(null, null)).First();

                await service.RemoveFile(file);

                var newCount = (await service.GetFiles(null, null)).Count();
                Assert.AreEqual(newCount, count - 1);

                //Delete Null
                var isException = false;
                try
                {
                    await service.RemoveFile(null);
                }
                catch
                {
                    isException = true;
                }
                if (!isException) Assert.Fail();
            });
        }
    }
}
