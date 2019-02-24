/////////////////////////////////////////////////////////////////////////
//  AnotherTested.cs - code to test                                    //
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
  public class AnotherTested
  {
    public bool myWackyFunction()
    {
      return false;
    }
#if (TEST_TESTED)
    static void Main(string[] args)
    {
       AnotherTested antest=new AnotherTested();
        if(antest.myWackyFunction())
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
