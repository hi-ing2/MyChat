using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("클라이언트콘솔창. \n\n\n");

            ConsoleClient consoleClient = new ConsoleClient();
            consoleClient.Run();
        }
    }
}
