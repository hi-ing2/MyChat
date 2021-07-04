using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace myListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("서버콘솔창. \n\n\n");

            //Myserver myserver = new Myserver();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
  
}
