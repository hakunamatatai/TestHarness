/////////////////////////////////////////////////////////////////////////
//  Tested.cs - code to test                                           //
//  Language:     C#, VS 2015                                          //
//  Application:  Project #2 in CSE681 - Software Modeling & Analysis  //
//  Author:       Yuchang Chen                                         //
/////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
  public class Tested
  {
    public bool myWackyFunction()
    {
      return true;
    }
#if (TEST_TESTED)
        static void Main(string[] args)
        {
            Tested antest = new Tested();
            if (antest.myWackyFunction())
            {
                Console.Write("This is a test");
            }
            else
            {
                Console.Write("This is a test");
            }
        }
#endif
    }
}
