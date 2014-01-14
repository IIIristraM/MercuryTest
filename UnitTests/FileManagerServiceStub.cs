using Domain;
using FileManagerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    //избавляемся от вызовов OperationContext
    public class FileManagerServiceStub : FileManagerService.FileManagerService
    {
        public FileManagerServiceStub(IFileSystemService fileSystem)
            : base(fileSystem)
        { }

        protected override string GetSessionId()
        {
            return "505b986025-b6-45bu-45-456-b45b-wm45j";
        }

        protected override IClientNotification GetCallbackChanel()
        {
            return new CallbackStub();
        }
    }

    //заглушка на клиентский callback
    public class CallbackStub : IClientNotification
    {
        public void PrintNotification(string notification)
        {
        }
    }
}
