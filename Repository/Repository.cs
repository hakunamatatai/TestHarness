/////////////////////////////////////////////////////////////////////////
//  Repository.cs - holds test code for TestHarness                    //
//  Language:     C#, VS 2015                                          //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis  //
//  Author:       Yuchang Chen                                         //
/////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Almost no functionality now.  Will be expanded to accept
 * Queries for Logs and Libraries.
 * 
 * Required Files:
 * - Client.cs, ITest.cs, Logger.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.2 : 23 Nov 2016
 * - added function queryLogs and extractTestResults
 * ver 1.1 : 20 Nov 2016
 * - added communication sender and receiver
 * ver 1.0 : 20 Oct 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace TestHarness
{
  public class TestResult : ITestResult
  {
    public string testName { get; set; }
    public string testResult { get; set; }
    public string testLog { get; set; }
  }

  public class TestResults: ITestResults
  {
    public string testKey { get; set; }
    public DateTime dateTime { get; set; }
    public List<ITestResult> testResults { get; set; }
  }

  public class Repository : IRepository
  {
    string repoStoragePath = "../../../Repository/RepositoryStorage/";
    private string repoPath_ = "../../../Repository/RepositoryStorage/";
    //build endpoint
    public Comm<Repository> comm { get; set; } = new Comm<Repository>();
    public string endPoint { get; } = Comm<Repository>.makeEndPoint("http://localhost", 8082);
    public string endPoint_TH { get; } = Comm<Repository>.makeEndPoint("http://localhost", 8080);
    public string remoteEndPoint_cl = Comm<Repository>.makeEndPoint("http://localhost", 8081);
    private Thread rcvThread = null;
    //private Thread rcvThread_CLtoRepo = null;
    //-----------------
    public Repository()
    {
        Console.Write("\n  Creating instance of Repository");
        //build receiver
        comm.rcvr.CreateRecvChannel(endPoint);
        rcvThread = comm.rcvr.start(rcvThreadProc);
        //Thread.Sleep(1000);
        //rcvThread.Join();
        //rcvThread_CLtoRepo = comm.rcvr.start(rcvThreadProc_CLtoRepo);
    }


    void rcvThreadProc()
    {
        while (true)
        {
            
            Message msg = comm.rcvr.GetMessage();
            msg.time = DateTime.Now;
            Console.Write("\n  {0} received message:   ------>Req 6", comm.name);
                //msg.showMsg();
            if (msg.body == "quit")
            {
                msg.showMsgInfo();
                break;
            }
            if (msg.from == endPoint_TH)
            {
                DateTime beforeSend = System.DateTime.Now;
                Console.WriteLine("\n----------------------------Req 7-----------------------------");
                //Console.Write("\n  {0} received message:", comm.name);
                //msg.showMsg();
                msg.showMsgInfo();
                Console.WriteLine("Storing logs and results with a key ------>Req 8");
                saveResultsAndLogs(extractTestResults(msg));
                DateTime afterSend = System.DateTime.Now;
                TimeSpan time = afterSend.Subtract(beforeSend);
                Console.WriteLine("\n----------------------------Req 12-----------------------------");
                Console.WriteLine("  The total test time is {0} ms.", time.TotalMilliseconds);
                }
            else if (msg.from == remoteEndPoint_cl)
            {
                DateTime beforeSend = System.DateTime.Now;
                //msg.showMsgInfo();
                string queryText = msg.body;
                
                List<string> files = queryLogs(queryText);
                    //rcvThread_RepotoCl = comm.rcvr.start(rcvThreadProc_RepotoCl);
                Thread.Sleep(1000);
                Console.WriteLine("\n----------------------------Req 9-----------------------------");
                Console.Write("\n  first 10 reponses to query " + msg.body + "\n");
                for (int i = 0; i < 10; ++i)
                {
                    if (i == files.Count())
                        break;
                    Console.Write("\n  " + files[i]);
                }
                DateTime afterSend = System.DateTime.Now;
                TimeSpan time = afterSend.Subtract(beforeSend);
                Console.WriteLine("\n----------------------------Req 12-----------------------------");
                Console.WriteLine("  The time of making and receiving query is {0} ms.", time.TotalMilliseconds);
                }
            
            }
        }

    //void rcvThreadProc()
    //{
    //    while (true)
    //    {
    //        Message msg = comm.rcvr.GetMessage();
    //        msg.time = DateTime.Now;
    //        Console.Write("\n  {0} received message:", comm.name);
    //        //msg.showMsg();
    //        msg.showMsgInfo();

    //        if (msg.body == "quit")
    //            { break; }
    //        saveResultsAndLogs(extractTestResults(msg));           
    //    }
    //}

    public void wait()
    {
        rcvThread.Join();
    }

        //----< search for text in log files >---------------------------
        /*
         * This function should return a message.  I'll do that when I
         * get a chance.
         */
    public List<string> queryLogs(string queryText)
    {
      List<string> queryResults = new List<string>();
      string path = System.IO.Path.GetFullPath(repoStoragePath);
      string[] files = System.IO.Directory.GetFiles(repoStoragePath, "*.txt");
      foreach(string file in files)
      {
        string contents = File.ReadAllText(file);
        if (contents.Contains(queryText))
        {
          string name = System.IO.Path.GetFileName(file);
          queryResults.Add(name);
        }
      }
      queryResults.Sort();
      queryResults.Reverse();
      return queryResults;
    }
    //----< send files with names on fileList >----------------------
    /*
     * This function is not currently being used.  It may, with a
     * Message interface, become part of Project #4.
     */
    public bool getFiles(string path, string fileList)
    {
      string[] files = fileList.Split(new char[] { ',' });
      //string repoStoragePath = "..\\..\\RepositoryStorage\\";

      foreach (string file in files)
      {
        string fqSrcFile = repoStoragePath + file;
        string fqDstFile = "";
        try
        {
          fqDstFile = path + "\\" + file;
          File.Copy(fqSrcFile, fqDstFile);
        }
        catch
        {
          Console.Write("\n  could not copy \"" + fqSrcFile + "\" to \"" + fqDstFile);
          return false;
        }
      }
      return true;
    }

    ITestResults extractTestResults(Message testResult)
    {
        //Console.Write("\n  parsing test result");
        //List<ITestResult> testResults = new List<ITestResult>();
        ITestResults TestResults = new TestResults();
        List<ITestResult> testResults = new List<ITestResult>();
        TestResults.testResults = testResults;
        TestResults.dateTime = DateTime.Now;
        //TestResults.testKey = testResult.author + TestResults.dateTime.ToString();
        XDocument doc = XDocument.Parse(testResult.body);
        foreach (XElement testElem in doc.Descendants("testResultsMsg"))
        {
            TestResults.testKey = testElem.Element("testKey").Value;
        }
        foreach (XElement testElem in doc.Descendants("testResult"))
        {
            //Test test = new Test();
            ITestResult tr = new TestResult();
            tr.testName = testElem.Element("testName").Value;
            tr.testResult = testElem.Element("result").Value;
            tr.testLog = testElem.Element("log").Value;
            //test.testName = testElem.Attribute("name").Value;
            //test.files.Add(testDriverName);
            //foreach (XElement lib in testElem.Elements("library"))
            //{
            //    test.files.Add(lib.Value);
            //}
            TestResults.testResults.Add(tr);
        }
        //TestResults.testKey = doc.Element("testKey").Value;
        return TestResults;
    }

    public bool saveResultsAndLogs(ITestResults testResults)
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
//----< intended for Project #4 >--------------------------------
        public void sendLog(string Log)
    {

    }
//#if (TEST_REPOSITORY)
    static void Main(string[] args)
    {
            /*
             * ToDo: add code to test 
             * - Test code in Repository class that sends files to TestHarness.
             * - Modify TestHarness code that now copies files from RepositoryStorage folder
             *   to call Repository.getFiles.
             * - Add code to respond to client queries on files and logs.
             * - Add RepositoryTest class that implements ITest so Repo
             *   functionality can be tested in TestHarness.
             */
            Repository repo = new Repository();
    }
//#endif
  }
}
