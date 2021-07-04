using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace myListener
{
    class ClientData
    {
        public TcpClient tcpClient { get; set; }
        public byte[] readBuffer { get; set; }
        public StringBuilder currentMsg { get; set; }
        public string clientName { get; set; }
        public int clientNumber { get; set; }

        public ClientData(TcpClient client)
        {
            currentMsg = new StringBuilder();
            this.tcpClient = client;
            this.readBuffer = new byte[1024];

            string clientEndPoint = client.Client.LocalEndPoint.ToString();

            char[] splitDivision = { '.', ':' };
            string[] splitData = null;
            splitData = clientEndPoint.Split(splitDivision);

            this.clientNumber = int.Parse(splitData[3]);
            
            Console.WriteLine("{0}번 사용자 접속 성공!", clientNumber);
        }
    }
}
