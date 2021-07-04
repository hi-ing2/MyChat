using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace myListener
{
    class MainServer
    {
        ClientManager _clientManager = null;
        ConcurrentBag<string> chattingLog = null;
        ConcurrentBag<string> AccessLog = null;
        private ObservableCollection<string> chattingLogList = new ObservableCollection<string>(); 
        private ObservableCollection<string> userList = new ObservableCollection<string>();
        private ObservableCollection<string> AccessLogList = new ObservableCollection<string>();


        Thread connectCheckTread = null;


        public MainServer()
        {
            // 생성자에서는 클라이언트매니저의 객체를 생성, 
            // 채팅로그와 접근로그를 담을 컬렉션 생성 
            // 서버 스레드 및 하트비트 스레드 시작

            _clientManager = new ClientManager();
            chattingLog = new ConcurrentBag<string>();
            AccessLog = new ConcurrentBag<string>();
            Task serverStart = Task.Run(() =>
            {
                ServerRun();
            });

            //connectCheckTread = new Thread(ConnectCheckLoop);
            connectCheckTread.Start();
        }

        public void ChangeListView(string Message, int key)
        {
            switch (key)
            {
                case StaticDefine_Form.ADD_ACCESS_LIST:
                    {
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                          //  () => {
                                AccessLogList.Add(Message); 
                            //}));
                        break;
                    }
                case StaticDefine_Form.ADD_CHATTING_LIST:
                    {
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                          //  () => { 
                                chattingLogList.Add(Message); 
                            //}));
                        break;
                    }
                case StaticDefine_Form.ADD_USER_LIST:
                    {
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                        //() => { 
                            userList.Add(Message);
                        //}));
                        break;
                    }
                case StaticDefine_Form.REMOVE_USER_LIST:
                    {
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                            //() => { 
                                userList.Remove(Message); 
                            //}));
                        break;
                    }
                default:
                    break;
            }
        }

        public void ConnectCheckLoop()
        {
            while (true)
            {
                foreach (var item in ClientManager.clientDic)
                {
                    try
                    {
                        string sendStringData = "관리자<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.Default.GetBytes(sendStringData);
                        item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                    }
                    catch (Exception e)
                    {
                        RemoveClient(item.Value);
                    }
                }
                Thread.Sleep(1000);
            }
        }
        // 클라이언트의 접속종료가 감지됐을때 static 예약어로 저장된 clientDic에서 
        // 해당클라이언트를 제거하고, 로그를 남깁니다.

        private void RemoveClient(ClientData targetClient)
        {
            //ClientData reuslt = null;
            ClientManager.clientDic.TryRemove(targetClient.clientNumber, out ClientData result);
            string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
            AccessLog.Add(leaveLog);
            ChangeListView(leaveLog, StaticDefine_Form.ADD_ACCESS_LIST);
            ChangeListView(result.clientName, StaticDefine_Form.REMOVE_USER_LIST);

        }


        private void ServerRun()
        {
            TcpListener listener = new TcpListener(new System.Net.IPEndPoint(IPAddress.Any, 9999));
            listener.Start();

            while (true)
            {
                //public Task<TcpClient> AcceptTcpClientAsync();
                //Task라는 형식이며, 형식 매개변수는 TcpClient 형식 사용하는 TcpListener 클래스의 AcceptTcpClientAsync() 함수??
                //즉 Task<TcpClient>의 형태로 반환
                //Task<TcpClient>.Result는 TcpClient 형식임.
                Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();

                acceptTask.Wait();

                TcpClient newClient = acceptTask.Result;

                _clientManager.AddClient(newClient);
            }
        }

        public void ConsoleView()
        {
            while (true)
            {
                Console.WriteLine("========서버========");
                Console.WriteLine("1. 현재접속인원확인");
                Console.WriteLine("2. 접속기록확인");
                Console.WriteLine("3. 채팅로그확인");
                Console.WriteLine("0. 종료");
                Console.WriteLine("====================");

                string key = Console.ReadLine();
                int order = 0;

                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case StaticDefine.SHOW_CURRENT_CLIENT:
                            {
                                ShowCurentClient();
                                break;
                            }
                        case StaticDefine.SHOW_ACCESS_LOG:
                            {
                                ShowAccessLog();
                                break;
                            }
                        case StaticDefine.SHOW_CHATTING_LOG:
                            {
                                ShowChattingLog();
                                break;
                            }
                        case StaticDefine.EXIT:
                            {
                                connectCheckTread.Abort();
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("다시 입력하세요!");
                                Console.ReadKey();
                                break;
                            }
                    }
                }
                else
                {
                    Console.WriteLine("다시 입력하세요!(2)");
                    Console.ReadKey();
                }
                Console.Clear();
                Thread.Sleep(100);
            }
        }

        private void ShowChattingLog()
        {

            if (chattingLog.Count == 0)
            {
                Console.WriteLine("채팅기록 없어여~");
                Console.ReadKey();
                return;
            }

            foreach (var item in chattingLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();

        }

        private void ShowAccessLog()
        {

            if (AccessLog.Count == 0)
            {
                Console.WriteLine("접속기록 없어여~");
                Console.ReadKey();
                return;
            }

            foreach (var item in AccessLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
            
        }

        private void ShowCurentClient()
        {
            if (ClientManager.clientDic.Count == 0)
            {
                Console.WriteLine("아무도 없어여~");
                Console.ReadKey();
                return;
            }

            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine(item.Value.clientName);
            }
            Console.ReadKey();
        }

        // 클라이언트에게 메시지를 보내는 첫번째 과정입니다.
        // ex) 클라이언트<메시지1, 클라이언트<메시지2, ...
        public void MessageParsing(string sender, string message)
        {
            lock (MainWindow.lockObj)
            {
                //정제된 메시지 리스트
                List<string> msgList = new List<string>();
                // '>' 기준으로 split한 메시지 어레이 
                string[] msgArray = message.Split('>');
                // Array -> List 로 전환하기 위함.
                foreach (var item in msgArray)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    msgList.Add(item);
                }
                SendMsgToClient(msgList, sender);
            }
        }

        // 클라이언트에게 메시지를 보내는 두번째 과정입니다.
        // ex) 클라이언트, 메시지1
        private void SendMsgToClient(List<string> msgList, string sender)
        {
            string LogMessage = "";
            string parsedMessage = "";
            string receiver = "";

            int senderNumber = -1;
            int receiverNumber = -1;

            foreach (var item in msgList)
            {
                string[] splitedMsg = item.Split('<');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);

                if (parsedMessage.Contains("<GroupChattingStart>"))
                {
                    string[] groupSplit = receiver.Split('#');

                    foreach(var el in groupSplit)
                    {
                        if (string.IsNullOrEmpty(el))
                            continue;
                        string groupLogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), groupSplit[0], el, splitedMsg[1]);
                        ChangeListView(groupLogMessage, StaticDefine_Form.ADD_CHATTING_LIST);

                        receiverNumber = GetClientNumber(el);

                        parsedMessage = string.Format("{0}<GroupChattingStart>", receiver);
                        byte[] sendGroupByteData = Encoding.Default.GetBytes(parsedMessage);
                        ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendGroupByteData, 0, sendGroupByteData.Length);
                    }
                    return;
                }

                if (receiver.Contains('#'))
                {
                    string[] groupSplit = receiver.Split('#');
                    foreach(var el in groupSplit)
                    {
                        if (string.IsNullOrEmpty(el))
                            continue;
                        string groupLogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), groupSplit[0], el, splitedMsg[1]);
                        ChangeListView(groupLogMessage, StaticDefine_Form.ADD_CHATTING_LIST);

                        receiverNumber = GetClientNumber(el);

                        parsedMessage = string.Format("{0}<{1}>", receiver, splitedMsg[1]);
                        byte[] sendGroupByteData = Encoding.Default.GetBytes(parsedMessage);
                        ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendGroupByteData, 0, sendGroupByteData.Length);
                    }
                    return;
                }
                senderNumber = GetClientNumber(sender);
                receiverNumber = GetClientNumber(receiver);

                if(senderNumber == -1 || receiverNumber == -1)
                {
                    return;
                }


                if (parsedMessage.Contains("<GiveMeUserList>"))
                {
                    string userListStringData = "관리자<";
                    foreach (var el in ClientManager.clientDic)
                    {
                        userListStringData += string.Format("${0}", el.Value.clientName);
                    }
                    userListStringData += ">";
                    byte[] userListByteData = new byte[userListStringData.Length];
                    userListByteData = Encoding.Default.GetBytes(userListStringData);
                    ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(userListByteData, 0, userListByteData.Length);
                    return;
                }

                LogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, splitedMsg[1]);
                ClientEvent(LogMessage, StaticDefine.ADD_CHATTING_LOG);

                byte[] sendByteData = Encoding.Default.GetBytes(parsedMessage);

                if (parsedMessage.Contains("<ChattingStart>"))
                {
                    parsedMessage = string.Format("{0}<ChattingStart>", receiver);
                    sendByteData = Encoding.Default.GetBytes(parsedMessage);
                    ClientManager.clientDic[senderNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);

                    parsedMessage = string.Format("{0}<ChattingStart>", sender);
                    sendByteData = Encoding.Default.GetBytes(parsedMessage);
                    ClientManager.clientDic[senderNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);

                    return;
                }

                if (parsedMessage.Contains(""))
                    ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);

            }
        }

        private int GetClientNumber(string targetClientName)
        {
            foreach (var item in ClientManager.clientDic)
            {
                if(item.Value.clientName == targetClientName)
                {
                    return item.Value.clientNumber;
                }
            }
            return -1;
        }

        private void ClientEvent(string message, int key)
        {
            switch (key)
            {
                case StaticDefine.ADD_ACCESS_LOG:
                    {
                        AccessLog.Add(message);
                        break;
                    }
                case StaticDefine.ADD_CHATTING_LOG:
                    {
                        chattingLog.Add(message);
                        break;
                    }
            }
        }
    }
}