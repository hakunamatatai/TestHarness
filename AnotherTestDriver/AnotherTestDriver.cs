/////////////////////////////////////////////////////////////////////////
//  AnotherTestDriver.cs - defines testing process                     //
//  Language:     C#, VS 2015                                          //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis  //
//  Author:       Yuchang Chen                                         //
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace TestHarness
{
  public class AnotherTestDriver : ITest
  {
    public bool test()
    {
      TestHarness.AnotherTested tested = new TestHarness.AnotherTested();
      return tested.myWackyFunction();
    }
    public string getLog()
    {
      return "demo test that always fails";
    }
#if (TEST_ANOTHERTESTDRIVER)
    static void Main(string[] args)
    {
            AnotherTestDriver antd = new AnotherTestDriver();
            antd.test();
            antd.getLog();
    }
#endif
  }
}
