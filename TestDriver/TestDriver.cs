/////////////////////////////////////////////////////////////////////////
//  TestDriver.cs - defines testing process                            //
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
 public  class TestDriver : ITest
  {
    public bool test()
    {
      TestHarness.Tested tested = new TestHarness.Tested();
      return tested.myWackyFunction();
    }
    public string getLog()
    {
      return "demo test that always passes";
    }
#if (TEST_TESTDRIVER)
        static void Main(string[] args)
        {
            TestDriver antd = new TestDriver();
            antd.test();
            antd.getLog();
        }
#endif
    }
}
