using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyClient
{

    class MyContinuousClient
    {
        delegate void Command();

        TcpClient client = null;
        public void Run()
        {
            
            Command[] commands;

            while (true)
            {

                
                Console.WriteLine("Hi! Let's Chat~");
                Console.WriteLine("1. 서버연결");
                Console.WriteLine("2. 채팅하기");
                Console.WriteLine("===============");
                string key = Console.ReadLine();
                int order = 0;

                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case 1:
                        {
                            if (client != null)
                            {
                                Console.WriteLine("Already Connected");
                                Console.ReadKey();
                                continue;
                            }
                            break;
                        }
                        case 2:
                        {
                            if (client == null)
                            {
                                Console.WriteLine("먼저 서버와 연결해주세요");
                                Console.ReadKey();
                                continue;
                            }
                            break;
                        }
                    }
                    commands = new Command[] { Connect, SendMessage };
                    commands[order - 1]();
                }
                
            }
        }

        public void Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", 9999);
                Console.WriteLine("서버연결 성공");
                Console.ReadKey();
            }
            catch (SocketException)
            {
                Connect();
            }

        }

        public void SendMessage()
        {
            Console.WriteLine("메세지 입력");
            string message = Console.ReadLine();
            byte[] buf = new byte[message.Length];
            buf = Encoding.Default.GetBytes(message);

            client.GetStream().Write(buf, 0, buf.Length);
            Console.WriteLine("성공");
            Console.ReadKey();
        }
    }
}
