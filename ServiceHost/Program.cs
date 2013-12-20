using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace ServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileManagerServiceHost host = new FileManagerServiceHost(typeof(FileManagerService.FileManagerService), new FileSystemService()))
            {
                host.Open();
                Console.WriteLine("Service ready...\nPress any key to stop the service");
                Console.ReadKey();
                host.Close();
            }
        }
    }
}
