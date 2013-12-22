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
            Task[] clients = new Task[100];
            for (var i = 0; i < clients.Length; i++)
            {
                clients[i] = Task.Run(() =>
                {
                    using (var client = new FileManagerClient())
                    {
                        var response = client.Connect("konstantin" + Guid.NewGuid());
                        Console.WriteLine(response);

                        response = client.CreateDirectory("temp");
                        Console.WriteLine(response);

                        response = client.ChangeDirectory("temp");
                        Console.WriteLine(response);

                        response = client.Quit();
                        Console.WriteLine(response);
                    }
                });
            }
            Task.WaitAll(clients);
            
            using (var client = new FileManagerClient())
            {
                while (true)
                {
                    var request = Console.ReadLine();
                    var response = client.ExecuteCommand(request);
                    Console.WriteLine(response);
                }
            }
        }
    }
}
