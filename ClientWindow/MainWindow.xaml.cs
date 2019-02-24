////////////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs - sends TestRequests, displays results using WPF   //
//  Language:     C#, VS 2015                                             //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis     //
//  Author:       Yudai Pan                                               //
//  SUID:         258799833                                               //
////////////////////////////////////////////////////////////////////////////

//Maintenance History:
// * --------------------
// * ver 1.0 : 25 Nov 2016
// * - first release

using System;
using System.Collections.Generic;
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
using System.Threading;

namespace TestHarness
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Comm<MainWindow> comm { get; set; } = new Comm<MainWindow>();
        public string endPoint { get; } = Comm<MainWindow>.makeEndPoint("http://localhost", 8081);
        public string remoteEndPoint_repo = Comm<MainWindow>.makeEndPoint("http://localhost", 8082);
        public string remoteEndPoint_TH = Comm<MainWindow>.makeEndPoint("http://localhost", 8080);
        private Thread rcvThread_THtoCl = null;
        //private Thread rcvThread_RepotoCl = null;
        //------------------
        public SWTools.BlockingQueue<Message> inQ_ { get; set; }
        private ITestHarness th_ = null;
        private IRepository repo_ = null;
        static string result = "no result";

        public MainWindow(ITestHarness th)
        {
            
            th_ = th;
            //comm.sndr.CreateSendChannel(endPoint);
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread_THtoCl = comm.rcvr.start(rcvThreadProc_THtoCl);
            //rcvThread_RepotoCl= comm.rcvr.start(rcvThreadProc_RepotoCl);
        }

        public MainWindow()
        {
            InitializeComponent();
            //th_ = th;
            //comm.sndr.CreateSendChannel(endPoint);
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread_THtoCl = comm.rcvr.start(rcvThreadProc_THtoCl);
            //rcvThread_RepotoCl= comm.rcvr.start(rcvThreadProc_RepotoCl);
            
        }

        void rcvThreadProc_THtoCl()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                if (msg.body == "quit")
                    { break; }
                msg.time = DateTime.Now;
                result = msg.body;
                //listBox1.Items.Insert(0, msg.author);
                //Console.WriteLine("\n----------------------------Req 7-----------------------------");
                //Console.Write("\n  {0} received message:   ------>Req 6", comm.name);
                //msg.showMsg();
                //msg.showMsgInfo();
                makeQuery("test1");
                //makeQuery(client, "test1");
                //inQ_.enQ(msg);
                
            }
        }
        //public Client()
        //{
        //  comm.rcvr.CreateRecvChannel(endPoint);
        //  rcvThread = comm.rcvr.start(rcvThreadProc);
        //}
        public void setRepository(IRepository repo)
        {
            repo_ = repo;
        }

        public void sendTestRequest(Message testRequest)
        {
            th_.sendTestRequest(testRequest);
        }

        public void wait()
        {
            //rcvThread_RepotoCl.Join();
            rcvThread_THtoCl.Join();
        }

        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }

        public void sendResults(Message results)
        {
            //RLog.write("\n  Client received results message:");
            //RLog.write("\n  " + results.ToString());
            //RLog.putLine();
            //Console.Write("\n  Client received results message:");
            //Console.Write("\n  " + results.ToString());
            //Console.WriteLine();
        }
        //copy
        //public void makeQuery(string queryText)
        //{
        //  Console.Write("\n  Results of client query for \"" + queryText + "\"");
        //  if (repo_ == null)
        //    return;
        //  List<string> files = repo_.queryLogs(queryText);
        //  //rcvThread_RepotoCl = comm.rcvr.start(rcvThreadProc_RepotoCl);
        //  Console.Write("\n  first 10 reponses to query \"" + queryText + "\"");
        //  for (int i = 0; i < 10; ++i)
        //  {
        //    if (i == files.Count())
        //      break;
        //    Console.Write("\n  " + files[i]);
        //  }
        //}

        public void makeQuery(string queryText)
        {
            //Console.Write("\n  Results of client query for \"" + queryText + "\"");

            Message msg = makeMessage("Pan Yudai", endPoint, remoteEndPoint_repo);
            SendfromCltoRepo(msg, queryText);
            //List<string> files = repo_.queryLogs(queryText);
            ////rcvThread_RepotoCl = comm.rcvr.start(rcvThreadProc_RepotoCl);
            //Console.Write("\n  first 10 reponses to query \"" + queryText + "\"");
            //for (int i = 0; i < 10; ++i)
            //{
            //    if (i == files.Count())
            //        break;
            //    Console.Write("\n  " + files[i]);
            //}
            //if (repo_ == null)
            //    return;
        }
        //static public Client CreateClient()
        //{
        //    Client client = new Client();
        //    return client;
        //}
        static public void SendfromCltoTH(Message msg)
        {
            //Client client = CreateClient();
            //Message msg1 = client.makeMessage("Fawcett", client.endPoint, client.endPoint);
            //client.comm.sndr.PostMessage(msg1);

            //string remoteEndPoint = Comm<Client>.makeEndPoint("http://localhost", 8080);
            //msg.body = MessageTest.makeTestRequest();
            //msg = msg.copy();
            //msg.to = remoteEndPoint;
            MainWindow.comm.sndr.PostMessage(msg);

        }
        static public void SendfromCltoRepo(Message msg, string query)
        {
            //Client client = CreateClient();
            //Message msg1 = client.makeMessage("Fawcett", client.endPoint, client.endPoint);
            //client.comm.sndr.PostMessage(msg1);

            //string remoteEndPoint = Comm<Client>.makeEndPoint("http://localhost", 8082);
            msg.body = query;
            //msg = msg.copy();
            //msg.to = remoteEndPoint;
            MainWindow.comm.sndr.PostMessage(msg);
        }

        //public MainWindow()
        //{
        //    InitializeComponent();

        //    //if (demo) Demonstrate();
        //}
        
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow win=new MainWindow();
            Message msg1 = makeMessage("Pan Yudai_test", endPoint, remoteEndPoint_TH);
            msg1.body = MessageTest.makeTestRequest();
            MainWindow.comm.sndr.PostMessage(msg1);
            listBox.Items.Insert(0, msg1.ToString());
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Message msg2 = makeMessage("Pan Yudai_test", endPoint, remoteEndPoint_TH);
            //msg2.body = "quit";
            //MainWindow.comm.sndr.PostMessage(msg2);
            //Environment.Exit(0);

            System.Environment.Exit(System.Environment.ExitCode);
            //this.Dispose();
            this.Close();

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Insert(0, result);
        }
    }
}
