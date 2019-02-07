# TestHarness
## Description
Remote Test Harness is an automated test tool that runs a specified set of tests on multiple packages for clients on server side.
## Structure of Remote Test Harness:
  * TestHarness server
    Implement a Test Harness Program that accepts one or more test requests from multiple clients, then enqueue the requests 
    and handle them serially by multiple threads.The Test Harness will request test drivers and code to test, as cited in a 
    Test Request, from the Repository.
    Each test driver derives from an ITest interface states a method test( ) which returns the test pass status and a 
    getLog( ) function which returns a string statement of the log.
    The test harness could support client queries about test requests from the Log storage. 
  
  * Repository
    Repository server stores test driver and the code which will be tested as dynamic link library(DLL). 
    The repository should in cooperation with the Test Harness, store test results and logs for all of the test executions.
    The repository should support client queries about test results from the Repository storage.
  
  * Client
    One or more clients could concurrently send test dll files to repository before sending test request to TestHarness 
    server.
    One or more client(s) will concurrently extract Test Results and logs from repository and display the replies 
    when they arrive.
    Client activities will be defined by user actions in a Windows Presentation Foundation(WPF) user interface. 
    
    All communication between Test Harness, Repository and clients will be based on message-passing Windows Communication 
    Foundation(WCF) channels.
    
    
