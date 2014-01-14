using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    //реализация callback интерфейса
    public class Notificator: FileManagerServiceProxy.IFileManagerServiceCallback
    {
        public void PrintNotification(string notification)
        {
            Console.WriteLine(notification);
        }
    }
}
