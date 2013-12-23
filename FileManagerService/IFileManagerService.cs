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
        string ExecuteCommand(string command);
        
        string Connect(string userName);
        
        string Quit();
        
        string CreateDirectory(string path);
        
        string ChangeDirectory(string path);
        
        string DeleteDirectory(string path);
      
        string DeleteTree(string path);
      
        string CreateFile(string path);
       
        string DeleteFile(string path);
     
        string Lock(string path);
       
        string Unlock(string path);
        
        string Copy(string sourcePath, string destinationPath);
       
        string Move(string sourcePath, string destinationPath);
        
        string Print();
    }

    public interface IClientNotification
    {
        [OperationContract(IsOneWay = true)]
        void PrintNotification(string notification);
    }
}
