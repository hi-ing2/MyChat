using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace myListener
{
    
    class Myserver
    {
        public Myserver()
        {
            AsyncServerStart();
        }
        private void AsyncServerStart()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 9999));
            listener.Start();
            Console.WriteLine("Server is opened....");

            while (true)
            {
                TcpClient accpetClient = listener.AcceptTcpClient();

                ClientData clientData = new ClientData(accpetClient);
                //비동기 처리(하단 while문 진입 동시에 BeginRead 대기) // 쓰레드 처리
                clientData.tcpClient.GetStream().BeginRead(clientData.readBuffer, 0, clientData.readBuffer.Length, new AsyncCallback(DataReceived), clientData);
            }
            /*while (true}
            {
                Console.WriteLine("서버구동중..");
                Thread.Sleep(1000);
            }*/
        }

        private void DataReceived(IAsyncResult asyncResult)
        {
            // clientData는 asyncReuslt.AsyncState에 들어간다.
            ClientData callbackClient = asyncResult.AsyncState as ClientData;

            int bytesRead = callbackClient.tcpClient.GetStream().EndRead(asyncResult);

            string readString = Encoding.Default.GetString(callbackClient.readBuffer, 0, bytesRead);
            Console.WriteLine("{0}번 사용자 : {1}", callbackClient.clientNumber, readString);

            // 재귀함수를 통한 재호출
            callbackClient.tcpClient.GetStream().BeginRead(callbackClient.readBuffer, 0, callbackClient.readBuffer.Length, new AsyncCallback(DataReceived), callbackClient);
        }


    }
    
    
}
