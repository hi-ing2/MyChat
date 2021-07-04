using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyClient
{
    class ConsoleClient
    {
        TcpClient client = null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        private string name = null;

        //서버 구동
        public void Run()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();

            receiveMessageThread = new Thread(ReceiveMessage);

            
            while (true)
            {

                Console.WriteLine("Hi! Let's Chat~");
                Console.WriteLine("1. 서버연결");
                Console.WriteLine("2. 메시지 보내기");
                Console.WriteLine("3. 보낸 메시지 확인");
                Console.WriteLine("4. 받은 메시지 확인");
                Console.WriteLine("0. 종료");
                Console.WriteLine("===============");

                string key = Console.ReadLine();
                int order = 0;

                if(int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case StaticDefine.CONNECT:
                            {
                                if(client != null)
                                {
                                    Console.WriteLine("Already Connected...");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    Connect();
                                }
                                break;
                            }
                        case StaticDefine.SEND_MESSAGE:
                            {
                                if(client == null)
                                {
                                    Console.WriteLine("서버와 먼저 연결하고 보내세용~");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    SendMessage();
                                }
                                break;
                            }
                        case StaticDefine.SEND_MSG_VIEW:
                            {
                                SendMessageView();
                                break;
                            }
                        case StaticDefine.RECEIVE_MEG_VIEW:
                            {
                                ReceiveMessageView();
                                break;
                            }
                        case StaticDefine.EXIT:
                            { 
                                if(client != null)
                                {
                                    client.Close();
                                }
                                receiveMessageThread.Abort();
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("잘 못 입력 했어여~");
                                Console.ReadKey();
                                break;
                            }

                    }
                }
                else
                {
                    Console.WriteLine("잘 못 입력 했어여~");
                    Console.ReadKey();
                }
                Console.Clear();
                Thread.Sleep(100);
            }
        }

        private void SendMessage()
        {
            string getUserList = string.Format("{0}<GiveMeUserList>", name);
            byte[] getUserByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserByte, 0, getUserByte.Length);

            Console.WriteLine("수신자 입력");
            string receiver = Console.ReadLine();
            Console.WriteLine("메세지 입력");
            string message = Console.ReadLine();

            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("입력 오류 - 확인 바람");
                Console.ReadKey();
                return;
            }

            string parsedMessage = string.Format("{0}<{1}>", receiver, message);

            byte[] byteData = new byte[parsedMessage.Length];
            byteData = Encoding.Default.GetBytes(parsedMessage);

            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToView.Add(string.Format("[{0}] Receiver : {1}, Message : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), receiver, message));
            Console.WriteLine("전송 성공!");
            Console.ReadKey();
        }

        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach(var item in messageList)
            {
                string sender = "";
                string message = "";

                if (item.Contains('<'))
                {
                    string[] splitedMsg = item.Split('<');

                    sender = splitedMsg[0];
                    message = splitedMsg[1];

                    if(sender == "관리자")
                    {
                        string userList = "";
                        string[] splitedUser = message.Split('$');
                        foreach(var el in splitedUser)
                        {
                            if (string.IsNullOrEmpty(el))
                                continue;
                            userList += el + " ";
                        }
                        Console.WriteLine(string.Format("[현재 접속인원] {0}", userList));
                        messageList.Clear();
                        return;
                    }
                    Console.WriteLine(string.Format("[메시지 수신] {0}: {1}", sender, message));
                    receiveMessageListToView.Add(string.Format("[{0}] Sender : {1}, Message : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, message));

                }
            }
        }



        private void SendMessageView()
        {
            if (sendMessageListToView.Count == 0)
            {
                Console.WriteLine("보낸메세지 낫띵~");
                Console.ReadKey();
                return;
            }
            foreach (var item in sendMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        private void ReceiveMessageView()
        {
            if (receiveMessageListToView.Count == 0)
            {
                Console.WriteLine("받은메세지 낫띵~");
                Console.ReadKey();
                return;
            }
            foreach (var item in receiveMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        private void Connect()
        {
            Console.WriteLine("이름 입력");
            name = Console.ReadLine();

            string parsedName = "%^&" + name;
            if (parsedName == "%^&")
            {
                Console.WriteLine("올바른 이름을 입력하세여~");
                Console.ReadKey();
                return;
            }
            
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", 9999);
                byte[] byteData = new byte[1024];
                byteData = Encoding.Default.GetBytes(parsedName);
                client.GetStream().Write(byteData, 0, byteData.Length);
                //????
                receiveMessageThread.Start();
                Console.WriteLine("서버연결 성공");
                Console.ReadKey();
            }
            catch (SocketException)
            {
                Connect();
            }
        }               

        private void ReceiveMessage()
        {
            string receiveMessage = "";
            List<string> receiveMessageLIst = new List<string>();
            while (true)
            {
                byte[] receiveByte = new byte[1024];
                client.GetStream().Read(receiveByte, 0, receiveByte.Length);
                receiveMessage = Encoding.Default.GetString(receiveByte);

                string[] receiveMessageArray = receiveMessage.Split('>');
                foreach(var item in receiveMessageArray)
                {
                    if (!item.Contains('<'))
                        continue;
                    if (item.Contains("관리자<TEST"))
                        continue;
                    receiveMessageLIst.Add(item);
                }
                ParsingReceiveMessage(receiveMessageLIst);
                Thread.Sleep(500);
            }
        }
    }
}
