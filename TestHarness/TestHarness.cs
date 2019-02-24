/////////////////////////////////////////////////////////////////////////
//  TestHarness.cs - TestHarness Engine: creates child domains         //
//  ver 2.1                                                            //
//  Language:     C#, VS 2015                                          //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis  //
//  Author:       Yuchang Chen                                         //
/////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TestHarness package provides integration testing services.  It:
 * - receives structured test requests
 * - retrieves cited files from a repository
 * - executes tests on all code that implements an ITest interface,
 *   e.g., test drivers.
 * - reports pass or fail status for each test in a test request
 * - stores test logs in the repository
 * It contains classes:
 * - TestHarness that runs all tests in child AppDomains
 * - Callback to support sending messages from a child AppDomain to
 *   the TestHarness primary AppDomain.
 * - Test and RequestInfo to support transferring test information
 *   from TestHarness to child AppDomain
 * 
 * Required Files:
 * ---------------
 * - TestHarness.cs, BlockingQueue.cs
 * - ITest.cs
 * - LoadAndTest, Logger, Messages
 *
 * Maintanence History:
 * --------------------
 * ver 3.0 : 20 Nov 2016
 * - added Communication
 * - edited some functions
 * ver 2.1 : 15 Nov 2016
 * - added Thread Local Storage: TLS dictionary
 * ver 2.0 : 13 Nov 2016
 * - added creation of threads to run tests in ProcessMessages
 * - removed logger statements as they were confusing with multiple threads
 * - added locking in a few places
 * - added more error handling
 * - No longer save temp directory name in member data of TestHarness class.
 *   It's now captured in TestResults data structure.
 * ver 1.1 : 11 Nov 2016
 * - added ability for test harness to pass a load path to
 *   LoadAndTest instance in child AppDomain
 * ver 1.0 : 16 Oct 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Policy;    // defines evidence needed for AppDomain construction
using System.Runtime.Remoting;   // provides remote communication between AppDomains
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace TestHarness
{
    //public class THServer
    //{
    //    public Comm<THServer> comm { get; set; } = new Comm<THServer>();

    //    public string endPoint { get; } = Comm<THServer>.makeEndPoint("http://localhost", 8080);

    //    private Thread rcvThread = null;

    //    public THServer()
    //    {
    //        comm.rcvr.CreateRecvChannel(endPoint);
    //        rcvThread = comm.rcvr.start(rcvThreadProc);
    //    }

    //    static public THServer CreateTHServer()
    //    {
    //        THServer thserver = new THServer();
    //        return thserver;
    //    }

    //    public void wait()
    //    {
    //        rcvThread.Join();
    //    }
    //    public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
    //    {
    //        Message msg = new Message();
    //        msg.author = author;
    //        msg.from = fromEndPoint;
    //        msg.to = toEndPoint;
    //        return msg;
    //    }

    //    void rcvThreadProc()
    //    {
    //        while (true)
    //        {
    //            Message msg = comm.rcvr.GetMessage();
    //            msg.time = DateTime.Now;
    //            Console.Write("\n  {0} received message:", comm.name);
    //            //msg.showMsg();
    //            Console.Write("\n");
    //            msg.showMsgInfo();
    //            if (msg.body == "quit")
    //                break;
    //        }
    //    }
    //}
        ///////////////////////////////////////////////////////////////////
        // Callback class is used to receive messages from child AppDomain
        //
  public class Callback : MarshalByRefObject, ICallback
  {
    public void sendMessage(Message message)
    {
      Console.Write("\n  received msg from childDomain: \"" + message.body + "\"");
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Test and RequestInfo are used to pass test request information
  // to child AppDomain
  //
  [Serializable]
  class Test : ITestInfo
  {
    public string testName { get; set; }
    public List<string> files { get; set; } = new List<string>();
  }
  [Serializable]
  class RequestInfo : IRequestInfo
  {
    public string tempDirName { get; set; }
    public List<ITestInfo> requestInfo { get; set; } = new List<ITestInfo>();
  }
  ///////////////////////////////////////////////////////////////////
  // class TestHarness

  public class TestHarness : ITestHarness
  {
    public SWTools.BlockingQueue<Message> inQ_ { get; set; } = new SWTools.BlockingQueue<Message>();
    private ICallback cb_;
    private IRepository repo_;
    private IClient client_;
    //private string repoPath_ = "../../../Repository/RepositoryStorage/";
    private string repoPath_ = "../../../Repository/RepositoryStorage/";
    private string filePath_;
    object sync_ = new object();
    List<Thread> threads_ = new List<Thread>();     
        //Threads of dealing with the tests
    Dictionary<int, string> TLS = new Dictionary<int, string>();
        // Thread Local Storage: key is thread ID, value is filePath_

    //build endpoint
    public static Comm<TestHarness> comm { get; set; } = new Comm<TestHarness>();
    public string endPoint { get; } = Comm<TestHarness>.makeEndPoint("http://localhost", 8080);
    public string remoteEndPoint_cl = Comm<TestHarness>.makeEndPoint("http://localhost", 8081);
    public string remoteEndPoint_repo = Comm<TestHarness>.makeEndPoint("http://localhost", 8082);
    private Thread rcvThread = null;        //Thread of receiving msg
    //-----------------

        //public THServer()
        //{
        //    comm.rcvr.CreateRecvChannel(endPoint);
        //    rcvThread = comm.rcvr.start(rcvThreadProc);
        //}

        //static public TestHarness CreateTHServer()
        //{
        //    TestHarness thserver = new TestHarness(repo_);
        //    return thserver;
        //}

    public void wait()
    {
        rcvThread.Join();
    }
    public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
    {
        Message msg = new Message();
        msg.author = author;
        msg.from = fromEndPoint;
        msg.to = toEndPoint;
        return msg;
    }

    void rcvThreadProc()
    {
        while (true)
        {
            Message msg = comm.rcvr.GetMessage();
            msg.time = DateTime.Now;
            if (msg.body == "quit")
                {
                    SendfromTHtoCL(msg);
                    break;
                }
            Console.Write("\n  {0} received message: ------>Req 6", comm.name);
            //msg.showMsg();
            Console.Write("\n");
            Console.WriteLine("\n----------------------------Req 2-----------------------------");
            Console.WriteLine("\n     Creating Test Request as message with XML body");
            Console.WriteLine("----------------------------------------------------------------");
            msg.showMsgInfo();
            inQ_.enQ(msg);
            
        }
    }
    public TestHarness()
    {
        Console.Write("\n  creating instance of TestHarness");
        //repo_ = repo;
        repoPath_ = System.IO.Path.GetFullPath(repoPath_);
        cb_ = new Callback();

        comm.rcvr.CreateRecvChannel(endPoint);
        rcvThread = comm.rcvr.start(rcvThreadProc);
    }

    public TestHarness(IRepository repo)
    {
      Console.Write("\n  creating instance of TestHarness");
      repo_ = repo;
      repoPath_ = System.IO.Path.GetFullPath(repoPath_);
      cb_ = new Callback();
      
      comm.rcvr.CreateRecvChannel(endPoint);
      rcvThread = comm.rcvr.start(rcvThreadProc);    
    }
    //----< called by TestExecutive >--------------------------------

    public void setClient(IClient client)
    {
      client_ = client;
    }
    //----< called by clients >--------------------------------------

    public void sendTestRequest(Message testRequest)
    {
      //Console.Write("\n  TestHarness received a testRequest - Req #2");
      inQ_.enQ(testRequest);
    }
    //----< not used for Project #2 >--------------------------------

    public Message sendMessage(Message msg)
    {
      return msg;
    }
    //----< make path name from author and time >--------------------

    string makeKey(string author)
    {
      DateTime now = DateTime.Now;
      string nowDateStr = now.Date.ToString("d");
      string[] dateParts = nowDateStr.Split('/');
      string key = "";
      foreach (string part in dateParts)
        key += part.Trim() + '_';
      string nowTimeStr = now.TimeOfDay.ToString();
      string[] timeParts = nowTimeStr.Split(':');
      for (int i = 0; i < timeParts.Count() - 1; ++i)
        key += timeParts[i].Trim() + '_';
      key += timeParts[timeParts.Count() - 1];
      key = author + "_" + key + "_" + "ThreadID" + Thread.CurrentThread.ManagedThreadId;
      return key;
    }
    //----< retrieve test information from testRequest >-------------

    List<ITestInfo> extractTests(Message testRequest)
    {
      Console.Write("\n  parsing test request");
      List<ITestInfo> tests = new List<ITestInfo>();
      XDocument doc = XDocument.Parse(testRequest.body);
      foreach (XElement testElem in doc.Descendants("test"))
      {
        Test test = new Test();
        string testDriverName = testElem.Element("testDriver").Value;
        test.testName = testElem.Attribute("name").Value;
        test.files.Add(testDriverName);
        foreach (XElement lib in testElem.Elements("library"))
        {
          test.files.Add(lib.Value);
        }
        tests.Add(test);
      }
      return tests;
    }
    //----< retrieve test code from testRequest >--------------------

    List<string> extractCode(List<ITestInfo> testInfos)
    {
      Console.Write("\n  retrieving code files from testInfo data structure");
      List<string> codes = new List<string>();
      foreach (ITestInfo testInfo in testInfos)
        codes.AddRange(testInfo.files);
      return codes;
    }
    //----< create local directory and load from Repository >--------

    RequestInfo processRequestAndLoadFiles(Message testRequest)
    {
      string localDir_ = "";
      RequestInfo rqi = new RequestInfo();
      rqi.requestInfo = extractTests(testRequest);
      List<string> files = extractCode(rqi.requestInfo);

      localDir_ = makeKey(testRequest.author);            // name of temporary dir to hold test files
      rqi.tempDirName = localDir_;
      lock (sync_)
      {
        filePath_ = System.IO.Path.GetFullPath(localDir_);  // LoadAndTest will use this path
        TLS[Thread.CurrentThread.ManagedThreadId] = filePath_;
      }
      Console.Write("\n  creating local test directory \"" + localDir_ + "\"");
      System.IO.Directory.CreateDirectory(localDir_);

      Console.Write("\n  loading code from Repository");
      foreach (string file in files)
      {
        string name = System.IO.Path.GetFileName(file);
        string src = System.IO.Path.Combine(repoPath_, file);
        if (System.IO.File.Exists(src))
        {
          string dst = System.IO.Path.Combine(localDir_, name);
          try
          {
            System.IO.File.Copy(src, dst, true);
          }
          catch
          {
            /* do nothing because file was already copied and is being used */
          }
          Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": retrieved file \"" + name + "\"");
        }
        else
        {
          Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": could not retrieve file \"" + name + "\"");
        }
      }
      Console.WriteLine();
      return rqi;
    }
    //----< save results and logs in Repository >--------------------
  
    //----< run tests >----------------------------------------------
    /*
     * In Project #4 this function becomes the thread proc for
     * each child AppDomain thread.
     */
    ITestResults runTests(Message testRequest)
    {
      AppDomain ad = null;
      ILoadAndTest ldandtst = null;
      RequestInfo rqi = null;
      ITestResults tr = null;

      try
      {
        //lock (sync_)
        {
          rqi = processRequestAndLoadFiles(testRequest);
          ad = createChildAppDomain();
          ldandtst = installLoader(ad);
        }
        if (ldandtst != null)
        {
          tr = ldandtst.test(rqi);
        }
        // unloading ChildDomain, and so unloading the library

        //saveResultsAndLogs(tr);

        //lock (sync_)  // this lock scope is no longer needed due to use of "thread local storage Dictionary - TLS"
        {
          Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": unloading: \"" + ad.FriendlyName + "\"\n");
          AppDomain.Unload(ad);
          try
          {
            System.IO.Directory.Delete(rqi.tempDirName, true);
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": removed directory " + rqi.tempDirName);
          }
          catch (Exception ex)
          {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not remove directory " + rqi.tempDirName);
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
          }
        }
        return tr;
      }
      catch(Exception ex)
      {
        Console.Write("\n\n---- {0}\n\n", ex.Message);
        return tr;
      }
    }
    //----< make TestResults Message >-------------------------------

    Message makeTestResultsMessage(ITestResults tr, string fromEndPoint, string toEndPoint)
    {
          Message trMsg = new Message();
          trMsg.author = "TestHarness";
        //trMsg.to = "CL";
        //trMsg.from = "TH";
          trMsg.to = toEndPoint;
          trMsg.from = fromEndPoint;
          XDocument doc = new XDocument();
          XElement root = new XElement("testResultsMsg");
          doc.Add(root);
          XElement testKey = new XElement("testKey");
          testKey.Value = tr.testKey;
          root.Add(testKey);
          XElement timeStamp = new XElement("timeStamp");
          timeStamp.Value = tr.dateTime.ToString();
          root.Add(timeStamp);
          XElement testResults = new XElement("testResults");
          root.Add(testResults);
          foreach(ITestResult test in tr.testResults)
          {
                XElement testResult = new XElement("testResult");
                testResults.Add(testResult);
                XElement testName = new XElement("testName");
                testName.Value = test.testName;
                testResult.Add(testName);
                XElement result = new XElement("result");
                result.Value = test.testResult;
                testResult.Add(result);
                XElement log = new XElement("log");
                log.Value = test.testLog;
                testResult.Add(log);
          }
          trMsg.body = doc.ToString();
          return trMsg;
    }
    //----< wait for all threads to finish >-------------------------

    public void wait_()
    {
      foreach (Thread t in threads_)
        t.Join();
    }
        //----< main activity of TestHarness >---------------------------

        //public void processMessages()
        //{
        //  AppDomain main = AppDomain.CurrentDomain;
        //  Console.Write("\n  Starting in AppDomain " + main.FriendlyName + "\n");

        //  int requestCount = 0;
        //  ThreadStart doTests = () => {
        //    Message testRequest = inQ_.deQ();

        //    if (++requestCount == 1)
        //      Console.Write("\n\n  First Message body:{0}", testRequest.body);

        //    if (testRequest.body == "quit")
        //    {
        //      inQ_.enQ(testRequest);
        //      return;
        //    }
        //    ITestResults testResults = runTests(testRequest);
        //    lock (sync_)
        //    {
        //      client_.sendResults(makeTestResultsMessage(testResults));
        //    }
        //  };

        //  int numThreads = 8;

        //  for(int i = 0; i < numThreads; ++i)
        //  {
        //    Console.Write("\n  Creating AppDomain thread");
        //    Thread t = new Thread(doTests);
        //    threads_.Add(t);
        //    t.Start();
        //  }
        //}

    static public void SendfromTHtoCL( Message msg)
    {
        //string remoteEndPoint = Comm<TestHarness>.makeEndPoint("http://localhost", 8081);
        //msg.body = MessageTest.makeTestRequest();
        //msg = msg.copy();
        //msg.to = remoteEndPoint;
        comm.sndr.PostMessage(msg);
    }

    static public void SendfromTHtoRepo(TestHarness th, Message msg)
    {       
        comm.sndr.PostMessage(msg);
    }

    bool saveResultsAndLogs(ITestResults testResults)
    {
        string logName = testResults.testKey + ".txt";
        System.IO.StreamWriter sr = null;
        try
        {

            sr = new System.IO.StreamWriter(System.IO.Path.Combine(repoPath_, logName));
            sr.WriteLine(logName);
            foreach (ITestResult test in testResults.testResults)
            {
                sr.WriteLine("-----------------------------");
                sr.WriteLine(test.testName);
                sr.WriteLine(test.testResult);
                sr.WriteLine(test.testLog);
            }
            sr.WriteLine("-----------------------------");
        }
        catch
        {
            sr.Close();
            return false;
        }
        sr.Close();
        return true;
    }

    public void processMessages(TestHarness th)
    {
        //DateTime before = System.DateTime.Now;

        AppDomain main = AppDomain.CurrentDomain;
        Console.WriteLine("\n----------------------------Req 4------------------------------");
        Console.Write("\n  Starting in AppDomain " + main.FriendlyName + "\n");
        ITestResults testResults = null;
        ThreadStart doTests = () =>
        {
            Message testRequest = inQ_.deQ();
            if (testRequest.body == "quit")
            {
                inQ_.enQ(testRequest);
                return;
            }
            testResults = runTests(testRequest);
            //repo.saveResultsAndLogs(testResults);
            //return testResults;
            lock (sync_)
            {
                //client_.sendResults(makeTestResultsMessage(testResults,endPoint,endPoint));
                Message testResult_cl = makeTestResultsMessage(testResults, endPoint, remoteEndPoint_cl);
                SendfromTHtoCL(testResult_cl);
                //testResult_cl = testResult_cl.copy();
                //testResult_cl.body = "quit";
                //SendfromTHtoCL(th, testResult_cl);

                Message testResult_repo = makeTestResultsMessage(testResults, endPoint, remoteEndPoint_repo);
                SendfromTHtoRepo(th, testResult_repo);
                //testResult_repo = testResult_repo.copy();
                //testResult_repo.body = "quit";
                //SendfromTHtoRepo(th, testResult_repo);
            }
        };

        int numThreads = 8;

        for (int i = 0; i < numThreads; ++i)
        {
            Console.Write("\n  Creating AppDomain thread");
            Thread t = new Thread(doTests);
            threads_.Add(t);
            t.Start();
        }
        //DateTime after = System.DateTime.Now;
        //TimeSpan ts = after.Subtract(before);
        //Console.WriteLine("\n----------------------------Req 12-----------------------------");
        //Console.WriteLine("  The total execution time is {0} ms.", ts.TotalMilliseconds);
            //return testResults;
        }

    //--------------copy------------------
    //public void processMessages() 
    //{
    //    AppDomain main = AppDomain.CurrentDomain;
    //    Console.Write("\n  Starting in AppDomain " + main.FriendlyName + "\n");

    //    ThreadStart doTests = () =>
    //    {
    //        Message testRequest = inQ_.deQ();
    //        if (testRequest.body == "quit")
    //        {
    //            inQ_.enQ(testRequest);
    //            return;
    //        }
    //        ITestResults testResults = runTests(testRequest);
    //        lock (sync_)
    //        {
    //            client_.sendResults(makeTestResultsMessage(testResults));

    //        }
    //    };

    //    int numThreads = 8;

    //    for (int i = 0; i < numThreads; ++i)
    //    {
    //        Console.Write("\n  Creating AppDomain thread");
    //        Thread t = new Thread(doTests);
    //        threads_.Add(t);
    //        t.Start();
    //    }
    //}
        //----< was used for debugging >---------------------------------

    void showAssemblies(AppDomain ad)
    {
      Assembly[] arrayOfAssems = ad.GetAssemblies();
      foreach (Assembly assem in arrayOfAssems)
        Console.Write("\n  " + assem.ToString());
    }
    //----< create child AppDomain >---------------------------------

    public AppDomain createChildAppDomain()
    {
      try
      {
        Console.WriteLine("\n----------------------------Req 4-----------------------------");
        Console.Write("\n  creating child AppDomain");
        
        AppDomainSetup domaininfo = new AppDomainSetup();
        domaininfo.ApplicationBase
          = "file:///" + System.Environment.CurrentDirectory;  // defines search path for LoadAndTest library

        //Create evidence for the new AppDomain from evidence of current

        Evidence adevidence = AppDomain.CurrentDomain.Evidence;

        // Create Child AppDomain

        AppDomain ad
          = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);

        Console.Write("\n  created AppDomain \"" + ad.FriendlyName + "\"");
        return ad;
      }
      catch (Exception except)
      {
        Console.Write("\n  " + except.Message + "\n\n");
      }
      return null;
    }
    //----< Load and Test is responsible for testing >---------------

    ILoadAndTest installLoader(AppDomain ad)
    {
      ad.Load("LoadAndTest");
      //showAssemblies(ad);
      //Console.WriteLine();

      // create proxy for LoadAndTest object in child AppDomain

      ObjectHandle oh
        = ad.CreateInstance("LoadAndTest", "TestHarness.LoadAndTest");
      object ob = oh.Unwrap();    // unwrap creates proxy to ChildDomain
                                  // Console.Write("\n  {0}", ob);

      // set reference to LoadAndTest object in child

      ILoadAndTest landt = (ILoadAndTest)ob;

      // create Callback object in parent domain and pass reference
      // to LoadAndTest object in child

      landt.setCallback(cb_);
      lock (sync_)
      {
        filePath_ = TLS[Thread.CurrentThread.ManagedThreadId];
        landt.loadPath(filePath_);  // send file path to LoadAndTest
      }
      return landt;
    }
//#if (TEST_TESTHARNESS)
    static void Main(string[] args)
    {
            
            TestHarness th = new TestHarness();
            
            th.processMessages(th);
            
            //THServer thserver = THServer.CreateTHServer();
            //THServer thserver = new THServer();
            //Message msg = Server.makeMessage("Fawcett", Server.endPoint, Server.endPoint);

        }
//#endif
  }
}
