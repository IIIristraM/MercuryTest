﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.18408
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Client.FileManagerServiceProxy {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="FileManagerServiceProxy.IFileManagerService", CallbackContract=typeof(Client.FileManagerServiceProxy.IFileManagerServiceCallback))]
    public interface IFileManagerService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Connect", ReplyAction="http://tempuri.org/IFileManagerService/ConnectResponse")]
        string Connect(string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Connect", ReplyAction="http://tempuri.org/IFileManagerService/ConnectResponse")]
        System.Threading.Tasks.Task<string> ConnectAsync(string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Quit", ReplyAction="http://tempuri.org/IFileManagerService/QuitResponse")]
        string Quit();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Quit", ReplyAction="http://tempuri.org/IFileManagerService/QuitResponse")]
        System.Threading.Tasks.Task<string> QuitAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/CreateDirectory", ReplyAction="http://tempuri.org/IFileManagerService/CreateDirectoryResponse")]
        string CreateDirectory(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/CreateDirectory", ReplyAction="http://tempuri.org/IFileManagerService/CreateDirectoryResponse")]
        System.Threading.Tasks.Task<string> CreateDirectoryAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/ChangeDirectory", ReplyAction="http://tempuri.org/IFileManagerService/ChangeDirectoryResponse")]
        string ChangeDirectory(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/ChangeDirectory", ReplyAction="http://tempuri.org/IFileManagerService/ChangeDirectoryResponse")]
        System.Threading.Tasks.Task<string> ChangeDirectoryAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/DeleteDirectory", ReplyAction="http://tempuri.org/IFileManagerService/DeleteDirectoryResponse")]
        string DeleteDirectory(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/DeleteDirectory", ReplyAction="http://tempuri.org/IFileManagerService/DeleteDirectoryResponse")]
        System.Threading.Tasks.Task<string> DeleteDirectoryAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/DeleteTree", ReplyAction="http://tempuri.org/IFileManagerService/DeleteTreeResponse")]
        string DeleteTree(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/DeleteTree", ReplyAction="http://tempuri.org/IFileManagerService/DeleteTreeResponse")]
        System.Threading.Tasks.Task<string> DeleteTreeAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/CreateFile", ReplyAction="http://tempuri.org/IFileManagerService/CreateFileResponse")]
        string CreateFile(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/CreateFile", ReplyAction="http://tempuri.org/IFileManagerService/CreateFileResponse")]
        System.Threading.Tasks.Task<string> CreateFileAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/DeleteFile", ReplyAction="http://tempuri.org/IFileManagerService/DeleteFileResponse")]
        string DeleteFile(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/DeleteFile", ReplyAction="http://tempuri.org/IFileManagerService/DeleteFileResponse")]
        System.Threading.Tasks.Task<string> DeleteFileAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Lock", ReplyAction="http://tempuri.org/IFileManagerService/LockResponse")]
        string Lock(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Lock", ReplyAction="http://tempuri.org/IFileManagerService/LockResponse")]
        System.Threading.Tasks.Task<string> LockAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Unlock", ReplyAction="http://tempuri.org/IFileManagerService/UnlockResponse")]
        string Unlock(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Unlock", ReplyAction="http://tempuri.org/IFileManagerService/UnlockResponse")]
        System.Threading.Tasks.Task<string> UnlockAsync(string path);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Copy", ReplyAction="http://tempuri.org/IFileManagerService/CopyResponse")]
        string Copy(string sourcePath, string destinationPath);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Copy", ReplyAction="http://tempuri.org/IFileManagerService/CopyResponse")]
        System.Threading.Tasks.Task<string> CopyAsync(string sourcePath, string destinationPath);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Move", ReplyAction="http://tempuri.org/IFileManagerService/MoveResponse")]
        string Move(string sourcePath, string destinationPath);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Move", ReplyAction="http://tempuri.org/IFileManagerService/MoveResponse")]
        System.Threading.Tasks.Task<string> MoveAsync(string sourcePath, string destinationPath);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Print", ReplyAction="http://tempuri.org/IFileManagerService/PrintResponse")]
        string Print();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFileManagerService/Print", ReplyAction="http://tempuri.org/IFileManagerService/PrintResponse")]
        System.Threading.Tasks.Task<string> PrintAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IFileManagerServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IFileManagerService/PrintNotification")]
        void PrintNotification(string notification);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IFileManagerServiceChannel : Client.FileManagerServiceProxy.IFileManagerService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class FileManagerServiceClient : System.ServiceModel.DuplexClientBase<Client.FileManagerServiceProxy.IFileManagerService>, Client.FileManagerServiceProxy.IFileManagerService {
        
        public FileManagerServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public FileManagerServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public FileManagerServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public FileManagerServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public FileManagerServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public string Connect(string userName) {
            return base.Channel.Connect(userName);
        }
        
        public System.Threading.Tasks.Task<string> ConnectAsync(string userName) {
            return base.Channel.ConnectAsync(userName);
        }
        
        public string Quit() {
            return base.Channel.Quit();
        }
        
        public System.Threading.Tasks.Task<string> QuitAsync() {
            return base.Channel.QuitAsync();
        }
        
        public string CreateDirectory(string path) {
            return base.Channel.CreateDirectory(path);
        }
        
        public System.Threading.Tasks.Task<string> CreateDirectoryAsync(string path) {
            return base.Channel.CreateDirectoryAsync(path);
        }
        
        public string ChangeDirectory(string path) {
            return base.Channel.ChangeDirectory(path);
        }
        
        public System.Threading.Tasks.Task<string> ChangeDirectoryAsync(string path) {
            return base.Channel.ChangeDirectoryAsync(path);
        }
        
        public string DeleteDirectory(string path) {
            return base.Channel.DeleteDirectory(path);
        }
        
        public System.Threading.Tasks.Task<string> DeleteDirectoryAsync(string path) {
            return base.Channel.DeleteDirectoryAsync(path);
        }
        
        public string DeleteTree(string path) {
            return base.Channel.DeleteTree(path);
        }
        
        public System.Threading.Tasks.Task<string> DeleteTreeAsync(string path) {
            return base.Channel.DeleteTreeAsync(path);
        }
        
        public string CreateFile(string path) {
            return base.Channel.CreateFile(path);
        }
        
        public System.Threading.Tasks.Task<string> CreateFileAsync(string path) {
            return base.Channel.CreateFileAsync(path);
        }
        
        public string DeleteFile(string path) {
            return base.Channel.DeleteFile(path);
        }
        
        public System.Threading.Tasks.Task<string> DeleteFileAsync(string path) {
            return base.Channel.DeleteFileAsync(path);
        }
        
        public string Lock(string path) {
            return base.Channel.Lock(path);
        }
        
        public System.Threading.Tasks.Task<string> LockAsync(string path) {
            return base.Channel.LockAsync(path);
        }
        
        public string Unlock(string path) {
            return base.Channel.Unlock(path);
        }
        
        public System.Threading.Tasks.Task<string> UnlockAsync(string path) {
            return base.Channel.UnlockAsync(path);
        }
        
        public string Copy(string sourcePath, string destinationPath) {
            return base.Channel.Copy(sourcePath, destinationPath);
        }
        
        public System.Threading.Tasks.Task<string> CopyAsync(string sourcePath, string destinationPath) {
            return base.Channel.CopyAsync(sourcePath, destinationPath);
        }
        
        public string Move(string sourcePath, string destinationPath) {
            return base.Channel.Move(sourcePath, destinationPath);
        }
        
        public System.Threading.Tasks.Task<string> MoveAsync(string sourcePath, string destinationPath) {
            return base.Channel.MoveAsync(sourcePath, destinationPath);
        }
        
        public string Print() {
            return base.Channel.Print();
        }
        
        public System.Threading.Tasks.Task<string> PrintAsync() {
            return base.Channel.PrintAsync();
        }
    }
}
