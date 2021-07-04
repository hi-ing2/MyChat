using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace myListener
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static object lockObj = new object();

        Task connectCheckThread = null;

        public MainWindow()
        {

            InitializeComponent();
            MainServer mainServer = new MainServer();
            mainServer.ConsoleView();
            ClientManager.messageParsingAction += mainServer.MessageParsing;
            ClientManager.ChangeListViewAction += mainServer.ChangeListView;


            ChattingLogListView.ItemsSource = chattingLogList;
            UserListView.ItemsSource = userList;
            AccessLogListView.ItemsSource = AccessLogList;
            connectCheckThread = new Task(mainServer.ConnectCheckLoop);
            connectCheckThread.Start();
        }
    }
}
