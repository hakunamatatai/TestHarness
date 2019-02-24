/////////////////////////////////////////////////////////////////////////
//  TestExec.cs - Demonstrate TestHarness, Client, and Repository      //
//  ver 1.1                                                            //
//  Language:     C#, VS 2015                                          //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis  //
//  Author:       Yuchang Chen                                         //
/////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TestExec package orchestrates TestHarness, Client, and Repository
 * operations to show that all requirements for Project #2 have been
 * satisfied. 
 *
 * Required files:
 * ---------------
 * - TestExec.cs
 * - ITest.cs
 * - Client.cs, Repository.cs, TestHarness.cs
 * - LoadAndTest, Logger, Messages
 * 
 * Maintanence History:
 * --------------------
 * ver 1.2 : 26 Nov 2016
 * - added main activity
 * ver 1.1 : 13 Nov 2016
 * - removed logger statements
 *   Lot's of logging messages were confusing in multi-threaded environment
 * ver 1.0 : 16 Oct 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace TestHarness
{
      public class TestExec
      {
            public TestHarness testHarness { get; set; }
            public Client client { get; set; }
            public Repository repository { get; set; }
            public TestExec()
            {
                  
                  
                  testHarness = new TestHarness(repository);
                  client = new Client(testHarness as ITestHarness);
                  repository = new Repository();
                  //MainWindow mainwindow = new MainWindow();

                  testHarness.setClient(client);
                  client.setRepository(repository);
            }
            void sendTestRequest(Message testRequest)
            {
                  client.sendTestRequest(testRequest);
            }
            Message buildTestMessage(string author, string fromEndPoint, string toEndPoint)
            {
                  Message msg = new Message();
                  msg.to = toEndPoint;
                  msg.from = fromEndPoint;
                  msg.author = author;

                  testElement te1 = new testElement("test1");
                  te1.addDriver("testdriver.dll");
                  te1.addCode("testedcode.dll");
                  testElement te2 = new testElement("test2");
                  te2.addDriver("td1.dll");
                  te2.addCode("tc1.dll");
                  testElement te3 = new testElement("test3");
                  te3.addDriver("anothertestdriver.dll");
                  te3.addCode("anothertestedcode.dll");
                  testElement tlg = new testElement("loggerTest");
                  tlg.addDriver("logger.dll");
                  testRequest tr = new testRequest();
                  tr.author = "Jim Fawcett";
                  tr.tests.Add(te1);
                  tr.tests.Add(te2);
                  tr.tests.Add(te3);
                  //tr.tests.Add(tlg);
                  msg.body = tr.ToString();
                  return msg;
            }
            //[STAThread]
            static void Main(string[] args)
            {
                //MainWindow mainwindow = new MainWindow();
                //ThreadStart doTests = () => mainwindow.Show();
                DateTime before= System.DateTime.Now;
                try
                  {
                        //Console.Write("\n  Demonstrating TestHarness - Project #2 with Threading");
                        Console.Write("\n ======================Remote TestHarness======================");
                        //Client client = Client.CreateClient();       
                        //THServer thserver = THServer.CreateTHServer();
                        
                        /////////////////////////////////////////////////////////////////////////////////////////////
                        Console.WriteLine("\n----------------------------Req 13------------------------------");
                        Console.WriteLine("  creating Test Executive");
                        TestExec te = new TestExec();

                        DateTime beforeSend = System.DateTime.Now;

                        Message msg1 = te.client.makeMessage("Yuchang Chen", te.client.endPoint, te.client.remoteEndPoint_TH);
                        msg1.body = MessageTest.makeTestRequest();
                        Client.SendfromCltoTH( msg1);
                        DateTime afterSend = System.DateTime.Now;
                        TimeSpan time = afterSend.Subtract(beforeSend);
                        Console.WriteLine("\n----------------------------Req 12-----------------------------");
                        Console.WriteLine("  The time of sending message is {0} ms.", time.TotalMilliseconds);
                        //te.testHarness.sendTestRequest(msg1);
                        //msg1 = msg1.copy();
                        //msg1.body = "quit";
                        //Client.SendfromCltoTH(te.client, msg1);
                        //te.testHarness.sendTestRequest(msg1);
                        te.testHarness.processMessages(te.testHarness);
                        //te.repository.saveResultsAndLogs(tr);

                        te.client.makeQuery( "test1");
                        //te.testHarness.wait();
                        /////////////////////////////////////////////////////////////////////////////////////////////////

                //TestExec te = new TestExec();
                //Message msg = te.buildTestMessage("Yuchang Chen", te.client.endPoint, te.client.endPoint);
                //te.sendTestRequest(msg);
                //te.sendTestRequest(msg);
                //msg = msg.copy();
                //msg.body = "quit";
                //te.sendTestRequest(msg);
                //te.testHarness.processMessages();
                ////te.testHarness.wait();
                ////te.client.makeQuery("test1");
                ////Console.Write("\n\n");
                Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n\n", ex.Message);
                }
            DateTime after= System.DateTime.Now;
            TimeSpan ts = after.Subtract(before);
            Console.WriteLine("\n----------------------------Req 12-----------------------------");
            Console.WriteLine("  The total execution time is {0} ms.",ts.TotalMilliseconds);
            
            }
      }
}
