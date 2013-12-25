using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

//интерфейс WCF сервиса
namespace FileManagerService
{
    [ServiceContract(CallbackContract = typeof(IClientNotification))]
    public interface IFileManagerService
    {
        //выполнение абстрактной команды
        //единственная доступная клиенту операция, позволяет не переделывать клиент при изменении набора команд сервера
        [OperationContract]
        string ExecuteCommand(string command);
        
        /// <summary>
        /// Авторизоваться на сервере
        /// </summary>
        /// <param name="userName">Логин</param>
        /// <returns></returns>
        string Connect(string userName);
        
        /// <summary>
        /// Завершить сеанс
        /// </summary>
        /// <returns></returns>
        string Quit();
        
        /// <summary>
        /// Создание каталога (md)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к каталогу</param>
        /// <returns></returns>
        string CreateDirectory(string path);
        
        /// <summary>
        /// Смена текущего каталога (cd)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к каталогу</param>
        /// <returns></returns>
        string ChangeDirectory(string path);
        
        /// <summary>
        /// Удаление каталога (rd)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к каталогу</param>
        /// <returns></returns>
        string DeleteDirectory(string path);
      
        /// <summary>
        /// Удаление дерева каталогов
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к корню дерева</param>
        /// <returns></returns>
        string DeleteTree(string path);
      
        /// <summary>
        /// Создание файла (mf)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к файлу</param>
        /// <returns></returns>
        string CreateFile(string path);
       
        /// <summary>
        /// Удаление файла (del)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к файлу</param>
        /// <returns></returns>
        string DeleteFile(string path);
     
        /// <summary>
        /// Блокирование файла (lock)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к файлу</param>
        /// <returns></returns>
        string Lock(string path);
       
        /// <summary>
        ///  Разблокирование файла (unlock)
        /// </summary>
        /// <param name="path">Абсолютный или относительный (к текущему каталогу) путь к файлу</param>
        /// <returns></returns>
        string Unlock(string path);

        /// <summary>
        /// Копирование файла или каталога со всем содержимым (copy)
        /// </summary>
        /// <param name="sourcePath">Абсолютный или относительный (к текущему каталогу) путь к объекту копирования (файл или каталог)</param>
        /// <param name="destinationPath">Абсолютный или относительный (к текущему каталогу) путь к конечному каталогу</param>
        /// <returns></returns>
        string Copy(string sourcePath, string destinationPath);
       
        /// <summary>
        /// Перемещение файла или каталога со всем содержимым (move)
        /// </summary>
        /// <param name="sourcePath">Абсолютный или относительный (к текущему каталогу) путь к объекту перемещения (файл или каталог)</param>
        /// <param name="destinationPath">Абсолютный или относительный (к текущему каталогу) путь к конечному каталогу</param>
        /// <returns></returns>
        string Move(string sourcePath, string destinationPath);
        
        /// <summary>
        /// Печать структуры файловой системы
        /// </summary>
        /// <returns></returns>
        string Print();
    }

    //интерфейс обратного вызова клиента
    public interface IClientNotification
    {
        /// <summary>
        /// Печать сообщения сервера
        /// </summary>
        /// <param name="notification">Сообщение</param>
        [OperationContract(IsOneWay = true)]
        void PrintNotification(string notification);
    }
}
