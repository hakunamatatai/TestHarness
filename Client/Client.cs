/////////////////////////////////////////////////////////////////////////
//  Client.cs - sends TestRequests, displays results                   //
//  Language:     C#, VS 2015                                          //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis  //
//  Author:       Yuchang Chen                                         //
/////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Almost no functionality now.  Will be expanded to make 
 * Queries into Repository for Logs and Libraries.
 * 
 * Required Files:
 * - Client.cs, ITest.cs, Logger.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 20 Nov 2016
 * - added communication sender and receiver
 * ver 1.0 : 20 Oct 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace TestHarness
{
  public class Client : IClient
  {
    //build endpoint
    public static Comm<Client> comm { get; set; } = new Comm<Client>();
    public string endPoint { get; } = Comm<Client>.makeEndPoint("http://localhost", 8081);
    public string remoteEndPoint_repo = Comm<Client>.makeEndPoint("http://localhost", 8082);
    public string remoteEndPoint_TH = Comm<Client>.makeEndPoint("http://localhost", 8080);
    private Thread rcvThread_THtoCl = null;
    //private Thread rcvThread_RepotoCl = null;
    //------------------
    public SWTools.BlockingQueue<Message> inQ_ { get; set; }
    private ITestHarness th_ = null;
    private IRepository repo_ = null;

    public Client(ITestHarness th)
    {
      Console.Write("\n  Creating instance of Client");
      th_ = th;
      //comm.sndr.CreateSendChannel(endPoint);
      comm.rcvr.CreateRecvChannel(endPoint);
      rcvThread_THtoCl = comm.rcvr.start(rcvThreadProc_THtoCl);
      //rcvThread_RepotoCl= comm.rcvr.start(rcvThreadProc_RepotoCl);
    }

    public Client()
    {
        Console.Write("\n  Creating instance of Client");
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
                msg.time = DateTime.Now;
                Console.WriteLine("\n----------------------------Req 7-----------------------------");
                Console.Write("\n  {0} received message:   ------>Req 6", comm.name);
                //msg.showMsg();
                msg.showMsgInfo();
                makeQuery("test1");
                //makeQuery(client, "test1");
                //inQ_.enQ(msg);
                if (msg.body == "quit")
                { break; }
          }
    }

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
 
      Console.Write("\n  Client received results message:");
      Console.Write("\n  " + results.ToString());
      Console.WriteLine();
    }


    public void makeQuery(string queryText)
    {
        //Console.Write("\n  Results of client query for \"" + queryText + "\"");
        
        Message msg = makeMessage("Yuchang Chen", endPoint, remoteEndPoint_repo);
        SendfromCltoRepo(msg,queryText);

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
        Client.comm.sndr.PostMessage(msg);
        
        }
    static public void SendfromCltoRepo(Message msg,string query)
    {
        //Client client = CreateClient();
        //Message msg1 = client.makeMessage("Fawcett", client.endPoint, client.endPoint);
        //client.comm.sndr.PostMessage(msg1);

        //string remoteEndPoint = Comm<Client>.makeEndPoint("http://localhost", 8082);
        msg.body = query;
        //msg = msg.copy();
        //msg.to = remoteEndPoint;
        Client.comm.sndr.PostMessage(msg);
    }

        //#if (TEST_CLIENT)
        static void Main(string[] args)
        {
            Client client = new Client();
                          
            ////-----------------------------send message 1-------------------------------
            Message msg1 = client.makeMessage("YuchangChen_test", client.endPoint, client.remoteEndPoint_TH);
            msg1.body = MessageTest.makeTestRequest();
            SendfromCltoTH(msg1);
                          
        }
        //#endif
    }
}
