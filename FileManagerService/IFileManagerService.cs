using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FileManagerService
{
    [ServiceContract(CallbackContract = typeof(IClientNotification))]
    public interface IFileManagerService
    {
        [OperationContract]
        string Connect(string userName);

        [OperationContract]
        string Quit();

        [OperationContract]
        string CreateDirectory(string path);

        [OperationContract]
        string ChangeDirectory(string path);

        [OperationContract]
        string DeleteDirectory(string path);

        [OperationContract]
        string DeleteTree(string path);

        [OperationContract]
        string CreateFile(string path);

        [OperationContract]
        string DeleteFile(string path);

        [OperationContract]
        string Lock(string path);

        [OperationContract]
        string Unlock(string path);

        [OperationContract]
        string Copy(string sourcePath, string destinationPath);

        [OperationContract]
        string Move(string sourcePath, string destinationPath);

        [OperationContract]
        string Print();
    }

    public interface IClientNotification
    {
        [OperationContract(IsOneWay = true)]
        void PrintNotification(string notification);
    }
}
