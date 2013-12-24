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
                        var response = client.ExecuteCommand("connect localhost:8734 konstantin" + Guid.NewGuid());
                        response = client.ExecuteCommand("md temp");                        
                        response = client.ExecuteCommand("md temp2");
                        response = client.ExecuteCommand("md temp3");
                        response = client.ExecuteCommand(@"md temp\temp4");
                        response = client.ExecuteCommand(@"md temp\temp4\app");
                        response = client.ExecuteCommand(@"md temp\temp4\app2");
                        response = client.ExecuteCommand(@"md temp\temp4\app3");
                        response = client.ExecuteCommand(@"mf temp\temp4\app3\1.txt");
                        response = client.ExecuteCommand(@"lock temp\temp4\app3\1.txt");
                        response = client.ExecuteCommand("cd temp");
                        response = client.ExecuteCommand(@"deltree c:\temp\temp4");
                        response = client.ExecuteCommand(@"md temp5");                       
                        response = client.ExecuteCommand(@"mf temp5\1.txt");
                        response = client.ExecuteCommand(@"mf temp5\2.txt");
                        response = client.ExecuteCommand(@"mf 1.txt");
                        response = client.ExecuteCommand(@"mf 2.txt");
                        response = client.ExecuteCommand(@"del 1.txt");
                        response = client.ExecuteCommand(@"unlock temp4\app3\1.txt");
                        response = client.ExecuteCommand(@"copy c: c:\temp2");
                        response = client.ExecuteCommand(@"move temp4 temp5");
                        response = client.ExecuteCommand(@"mf c:\temp3\4.txt");
                        response = client.ExecuteCommand(@"mf c:\temp3\5.txt");
                        response = client.ExecuteCommand(@"md c:\temp3\temp6");
                        response = client.ExecuteCommand(@"deltree c:\temp3");
                        response = client.ExecuteCommand(@"move c:\temp\temp5\1.txt c:");
                        response = client.ExecuteCommand("quit");
                    }
                });
            }
            Task.WaitAll(clients);

            Console.WriteLine("Enter commands:");
            
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
