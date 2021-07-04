using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace myListener
{
    class ClientManager
    {
        public static ConcurrentDictionary<int, ClientData> clientDic = new ConcurrentDictionary<int, ClientData>();
        public static event Action<string, string> messageParsingAction = null;
        public static event Action<string, int> ChangeListViewAction = null;

        public void AddClient(TcpClient newClient)
        {
            ClientData currentClient = new ClientData(newClient);

            try
            {
                //비동기 처리(BeginRead 대기) // 쓰레드 처리
                currentClient.tcpClient.GetStream().BeginRead(currentClient.readBuffer, 0, currentClient.readBuffer.Length, new AsyncCallback(DataReceived), currentClient);
                clientDic.TryAdd(currentClient.clientNumber, currentClient);
            }
            catch(Exception e)
            {

            }
        }
        private void DataReceived(IAsyncResult asyncResult)
        {
            // currentclien는 asyncReuslt.AsyncState에 들어간다.
            ClientData callbackClient = asyncResult.AsyncState as ClientData;
            try 
            {
                int byteLength = callbackClient.tcpClient.GetStream().EndRead(asyncResult);

                string readString = Encoding.Default.GetString(callbackClient.readBuffer, 0, byteLength);

                // 재귀함수를 통한 재호출
                callbackClient.tcpClient.GetStream().BeginRead(callbackClient.readBuffer, 0, callbackClient.readBuffer.Length, new AsyncCallback(DataReceived), callbackClient);

                if (string.IsNullOrEmpty(callbackClient.clientName))
                {
                    // event가 null이 아닐때만(콜백메소드가 event 참조할때만) 진입해서, 이벤트 발생 시킬 수 있도록 함. (에러 방지)
                    if(ChangeListViewAction != null)
                    {
                        if (CheckID(readString))
                        {
                            string userName = readString.Substring(3);
                            callbackClient.clientName = userName;
                            ChangeListViewAction.Invoke(callbackClient.clientName, StaticDefine_Form.ADD_USER_LIST);
                            string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm;ss"), callbackClient.clientName);
                            //이벤트 발생 시점
                            ChangeListViewAction.Invoke(accessLog, StaticDefine_Form.ADD_ACCESS_LIST);
                            return;
                        }

                    }
                }
                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(callbackClient.clientName, readString, null, null);
                }
            }

             
            catch (Exception e)
            {

            }
        }
        // 클라이언트는 최초 접속시 "%^&이름" 을 보내도록 구현되어있습니다.
        // '%^&' 기호가 왔다면 서버는 해당클라이언트에게 이름을 부여합니다.

        private bool CheckID(string ID)
        {
            if (ID.Contains("%^&"))
            {
                return true;
            }
            return false;
        }
    }
}