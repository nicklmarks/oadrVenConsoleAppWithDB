using oadrlib.lib.helper;
using oadrlib.lib.http;
using oadrlib.lib.oadr2a;
using oadrlib.lib.oadr2b;
using System.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

/// <summary>
/// Test oadrPoll
/// Connect to 
/// http://<hostname>(:port)/(prefix/)OPENADR2/Simple/2.0b/<service>
/// http://oadr.com:8080/OPENADR2/Simple/2.0b/oadrPoll
/// http://172.16.25.51:8080/OPENADR2/Simple/2.0b/oadrPoll
/// Test_VEN_Name
/// 
/// 
/// Download Payload
/// 
/// 
/// Write Payload to DB
/// 
/// 
/// </summary>

/// Intialize Logger
/// Ask for user input
/// Connect to Server
/// Display Payload
/// Start Thread

//HttpWebRequestWrapper wr = new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12);
//IHttp http = wr;
//            string value = ConfigurationManager.AppSettings[key];


// : Form, IVenWrapper, IQueryRegistration, IManageOptSchedules, IEvents, IManageResources, IucCreateReport


namespace oadrVenConsoleAppWithDB
{
    class Program   /// : IVenWrapper  // ??? Can Main Inherit?
    {
        //VEN2b ven2b;

        static void Main(string[] args)
        {
        
            Logger.logMessage( DateTime.Now + "Begin Main...\n", "logger.log");
            
            int i = 0;
            if (args.Length == 0)
            {
                //Console.WriteLine($"No Command Args\n");
                Logger.logMessage( DateTime.Now + "No Command Line Args Passed...\n", "logger.log");
            }
            else
            {
                Logger.logMessage( DateTime.Now + "Logging Commmands:\n", "logger.log");
            }
            while (args.Length != 0)
            {
                // Console.WriteLine($"args[{i}] = {args[i]}\n");
                Logger.logMessage($"args[{i}] = {args[i]}\n", "logger.log");
                i = i + 1;
            }


            // Get App.config data
            
            string url = ConfigurationManager.AppSettings["url"]; // "http://172.16.25.51:8080/OpenADR2/Simple/2.0b";
            string venName = ConfigurationManager.AppSettings["venName"];  // "Test_VEN_Name";
            string venID = ConfigurationManager.AppSettings["venID"];   //  "6f130342def6d658567c";
            string password = ConfigurationManager.AppSettings["password"];   //  "";

            string default_sqlConnString = ConfigurationManager.AppSettings["default_sqlConnString"];   //  "";
            string default_VTNConnString = ConfigurationManager.AppSettings["default_VTNConnString"];   //  "";


            string connectionString = $"{url}::{venName}::{venID}::{password}";

//            Console.WriteLine($"Using {connectionString}");
            Logger.logMessage( DateTime.Now + $"Connection String = [{connectionString}]\n", "logger.log");


            
            // ***TODO::  Connect to DB ***
            // ***TODO::  Get SQLConnString  ***
            
            
            // Get VTN Conn Info
            //      HTTP Request? WebRequest?
            
            // Create New VEN
            
            // Log Create New Ven
            VEN2b ven2b = new VEN2b(
                new HttpWebRequestWrapper(
                    false, 
                    System.Net.SecurityProtocolType.Tls12
                    ), 
                url, 
                venName, 
                venID, 
                password
            );
            
            //this.ven2b = ven2b;
            //            /// VEN = new VEN(http, url, venName, venID, password);
            //            /// VEN2A = new VEN2a();
            //            /// VEN2B = new VEN2b();
            
            
            
            //Pre-Loop Set-Up



            // Save Poll Interval QueryRegistration
            string loggingString = "Create new queryRegistration\n";
            Logger.logMessage( DateTime.Now + loggingString + "\n", "logger.log");

//            oadrlib.lib.oadr2b.QueryRegistration queryRegistration = ven2b.queryRegistration();
            int poll_interval = 10;  // ??queryRegistration.poll_Int??
// *** TODO:: Pull Poll interval from queryRegistration.responseBody  *** 
// oadrCreatedPartyRegistrationType (VTN INFO)
            
            Logger.logMessage( DateTime.Now +  "Log queryRegistration requestBody:: \n" + queryRegistration.requestBody + "\n", "logger.log");
            Logger.logMessage( DateTime.Now +  "Log queryRegistration responseBody:: \n" + queryRegistration.responseBody + "\n", "logger.log");

System.Threading.Thread.Sleep(5000);



// Registration Service
// VEN->VTN  oadrQueryRegistration()
// VEN<-VTN  oadrCreatedPartyRegistration(VTN Info)
// SLEEP
// VEN->VTN  oadrCreatePartyRegistration(VEN Info)
// VEN<-VTN  oadrCreatedPartyRegistration(VTN Info, registrationID)
// SLEEP
//System.Threading.Thread.Sleep(5000);


// Set Up Opt Parameters
// Send Opt In Information for Events
// VEN->VTN  oadrCreateOpt
// VEN<-VTN  oadrCreatedOpt
// SLEEP

// Set Up Report Parameters
// Send Report Registration Info
// VEN->VTN  oadrRegisterReport(METADATA report)
// VEN<-VTN  oadrRegisteredReport(optional oadrReportRequest)
// SLEEP
// VEN->VTN   oadrCreatedReport(If report requested)
// VEN<-VTN  oadrResponse()
// SLEEP
// **OR
// VEN->VTN  oadrCreateReport(reportRequestID, reportSpecifierID)
// VEN<-VTN  oadrCreatedReport()
// SLEEP

// Request First Event Info
// VEN->VTN  oadrPoll  / RequestEvent
// VEN<-VTN  oadrDistributeEvent
// SLEEP
// VEN->VTN  oadrCreatedEvent
// VEN<-VTN  oadrResponse
// SLEEP

            oadrlib.lib.oadr2b.RequestEvent requestEvent = ven2b.requestEvent();
            
            Logger.logMessage( DateTime.Now +  "\nRequest Event Request Body:: \n", "requestEvent.log");
            Logger.logMessage( DateTime.Now +  requestEvent.requestBody, "requestEvent.log");
            Logger.logMessage( DateTime.Now +  "\nRequest Event Response Body:: \n", "requestEvent.log");
            Logger.logMessage( DateTime.Now +  requestEvent.responseBody, "requestEvent.log");
            Logger.logMessage( DateTime.Now +  "\n\n", "requestEvent.log");
            //Logger.logMessage( DateTime.Now +  requestEvent.requestBody.ToString());

System.Threading.Thread.Sleep(10000);


            oadrlib.lib.oadr2b.RequestEvent requestEvent = ven2b.requestEvent();
            
            Logger.logMessage( DateTime.Now +  "\nRequest Event Request Body:: \n", "requestEvent.log");
            Logger.logMessage( DateTime.Now +  requestEvent.requestBody, "requestEvent.log");
            Logger.logMessage( DateTime.Now +  "\nRequest Event Response Body:: \n", "requestEvent.log");
            Logger.logMessage( DateTime.Now +  requestEvent.responseBody, "requestEvent.log");
            Logger.logMessage( DateTime.Now +  "\n\n", "requestEvent.log");
            //Logger.logMessage( DateTime.Now +  requestEvent.requestBody.ToString());

System.Threading.Thread.Sleep(10000);


// VEN->VTN  
// VEN<-VTN  
// SLEEP


// Loop Options for Pull Method. Pull Polls VTN and Awaits Response during INTERVAL
// VEN->VTN  oadrPoll
// VEN<-VTN  oadrResponse OR oadrDistributeEvent OR oadrCreateReport OR oadrRegisterReport OR oadrCancelReport OR oadrUpdateReport OR oadrCancelPartyRegistration OR oadrRequestReregistration
// SLEEP


            //BEGIN-Loop  (Request/Response Options)

            // Start Loop
            // Send Request / Parse Response Loop (Interval and choice of pair)
            // Save Response Data -> DB
            // Send Next Request or Response
            // Sleep (Interval)


// VEN->VTN  
// VEN<-VTN  
// SLEEP

/**   //DEBUG
        
        private const string STATUS_SHUTDOWN = "Shutting down...";
        private const string VEN_NOT_REGISTERED = "VEN NOT Registered";
        private const string VEN_REGISTERED = "VEN IS Registered";

        private bool m_running = false;

        private VenWrapper m_venWrapper;


            m_venWrapper = new VenWrapper(ven2b, this);
            setVENParameters();

            m_venWrapper.queryRegistration();

            m_venWrapper.register();

            m_venWrapper.registerReports();

            m_venWrapper.startPolling();
            
            // StartConsole();
            
            m_venWrapper.stopPolling();

            m_venWrapper.shutdown();


**/

/**
  //REMOVE AND REPLACE
//                    startConsole(ven2b);
//        static void startConsole(VEN2b ven2b)
//        {
//            consoleMain console = new consoleMain(ven2b);
//        }
        
        // ** Start Adding consoleMain Functionality
        

       
//       Logger.logMessage( "Log queryRegistration responseBody:: \n" + queryRegistration.responseBody + "\n", "logger.log");
**/

            return;
        }
        
        
        // Run Command for the Request/Response Loop refactoring 
        
        static void Run()
        {
            while (true)
            {
//                var consoleInput = ReadFromConsole();
//                if (string.IsNullOrWhiteSpace(consoleInput)) continue;

                try
                {
                    // Create a ConsoleCommand instance:
//                    var cmd = new ConsoleCommand(consoleInput);

                    // Execute the command:
//                    string result = Execute(cmd);

                    // Write out the result:
//                    WriteToConsole(result);

                    Logger.logMessage( DateTime.Now +  result + "\n", "logger.log");

                }
                catch (Exception ex)
                {
                    // OOPS! Something went wrong - Write out the problem:
//                    WriteToConsole(ex.Message);

                    Logger.logMessage( DateTime.Now +  ex.Message  + "\n", "logger.log");
                }
            }
        }



   }
}
