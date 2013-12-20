using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var callback = new Notificator();
            FileManagerServiceProxy.FileManagerServiceClient serviceProxy = new FileManagerServiceProxy.FileManagerServiceClient(new InstanceContext(callback));

            serviceProxy.Open();
            var answer = serviceProxy.Connect("Konstantin");

            Console.WriteLine(answer);
            Console.ReadLine();
        }
    }
}
