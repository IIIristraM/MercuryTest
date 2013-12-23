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
                        var response = client.ExecuteCommand("connect konstantin" + Guid.NewGuid());
                        Console.WriteLine(response);

                        response = client.ExecuteCommand("md temp");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand("md temp2");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand("md temp3");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"md temp\temp4");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"md temp\temp4\app");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"md temp\temp4\app2");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"md temp\temp4\app3");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"mf temp\temp4\app3\1.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"lock temp\temp4\app3\1.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand("cd temp");
                        Console.WriteLine(response);
                        
                        response = client.ExecuteCommand(@"deltree c:\temp\temp4");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"md temp5");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"mf temp5\1.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"mf temp5\2.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"deltree temp5");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"mf 1.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"mf 2.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"del 1.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand(@"unlock temp4\app3\1.txt");
                        Console.WriteLine(response);

                        response = client.ExecuteCommand("quit");
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
                    Console.Write("\r\n" + response);
                }
            }
        }
    }
}
