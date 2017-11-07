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
    class Program
    {
        //VEN2b ven2b;

        static void Main(string[] args)
        {
            // specify where appsettings comes from as input parameter?


            //List Args
            Console.WriteLine("Begin Main...\n");
            Logger.logMessage("Begin Main...\n", "main.log");

            int i = 0;
            if (args.Length == 0)
            {
                Console.WriteLine($"No Command Args\n");
                Logger.logMessage($"No Command Args\n", "main.log");
            }
            while (args.Length != 0)
            {
                Console.WriteLine($"args[{i}] = {args[i]}\n");
                Logger.logMessage($"args[{i}] = {args[i]}\n", "main.log");
                i = i + 1;
            }


            // initialize components for http connections
            // from app.config

            string url = ConfigurationManager.AppSettings["url"]; // "http://172.16.25.51:8080/OpenADR2/Simple/2.0b";
            string venName = ConfigurationManager.AppSettings["venName"];  // "Test_VEN_Name";
            string venID = ConfigurationManager.AppSettings["venID"];   //  "6f130342def6d658567c";
            string password = ConfigurationManager.AppSettings["password"];   //  "";

            string connectionString = $"{url}::{venName}::{venID}::{password}";

            Console.WriteLine($"Using {connectionString}");
            Logger.logMessage($"Connection String = [{connectionString}]\n", "main.log");


            VEN2b ven2b = new VEN2b(new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12), url, venName, venID, password);
            //this.ven2b = ven2b;





            startConsole(ven2b);


            //            //string url = "http://172.16.25.51:8080/OpenADR2/Simple/2.0b";
            //            //string venName = "Test_VEN_Name";
            //            //string venID = "6f130342def6d658567c";
            //            //string password = "";


            //            /// VEN = new VEN(http, url, venName, venID, password);
            //            /// VEN2A = new VEN2a();
            //            /// VEN2B = new VEN2b();
            //            /// 
            //            //Logger.logMessage($"Create new VEN with params {url} {venName}\n");
            //            //Console.WriteLine($"Create new VEN with params {url} {venName}\n");
            //            //Logger.logMessage($"Create new VEN with params {url} {venName}\n", "ven.log");

            //            //VEN ven = new VEN(http, url, venName, venID, password);
            //            string str = "Create new VEN2A\n";
            //            Logger.logMessage(str);
            //            Logger.logMessage(str, "ven.log");

            ////            oadrlib.lib.oadr2a.VEN2a ven2a = new VEN2a(new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12), url, venID, password);
            //            VEN2b ven2b = new VEN2b(new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12), url, venName, venID, password);

            //            str = "Create new RequestEvent\n";
            //            Logger.logMessage(str);
            //            Console.WriteLine(str);
            //            //Console.WriteLine($"Press Enter (or any key) to continue:\n");
            //            //string x = Console.ReadLine();


            ////            oadrlib.lib.oadr2a.RequestEvent requestEvent = ven2a.requestEvent();
            //            oadrlib.lib.oadr2b.RequestEvent requestEvent = ven2b.requestEvent();

            //            str = "Read new RequestEvent\n";

            //            Console.WriteLine(str);
            //            Console.WriteLine(requestEvent.requestBody);

            //            Logger.logMessage(str);

            //            Logger.logMessage("\nRequest Event Request Body:: \n", "requestEvent.log");
            //            Logger.logMessage(requestEvent.requestBody, "requestEvent.log");
            //            Logger.logMessage("\nRequest Event Response Body:: \n", "requestEvent.log");
            //            Logger.logMessage(requestEvent.responseBody, "requestEvent.log");
            //            Logger.logMessage("\n\n", "requestEvent.log");


            //            Logger.logMessage(distributeEvent.requestBody, "requestEvent.log");

            ////Logger.logMessage(requestEvent);
            //Logger.logMessage(requestEvent.requestBody.ToString());


            //Console.WriteLine($"Press Enter (or any key) to continue:\n");
            //string x = Console.ReadLine();


            /// VEN( new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12) , string url = "http://172.16.25.51:8080/OpenADR2/Simple/2.0b", string venName  = "Test_VEN_Name", string venID = "6f130342def6d658567c", string password = "")
            /// http://172.16.25.51:8080/OPENADR2/Simple/2.0b/oadrPoll
            /// 


            //Console.WriteLine($"Press Enter (or any key) to continue:\n");
            //string y = Console.ReadLine();


            //Console.WriteLine($"\nStart New Command: {y}\n");
            //Logger.logMessage($"\nStart New Command: {y}\n");

            ////            chooseCommand(y);


            //Console.WriteLine($"\nStart New main console thread:\n");
            //Logger.logMessage($"\nStart New main console thread:\n");


            //            consoleMain console = new consoleMain();



            return;

            ///            Application.EnableVisualStyles();
            ///            Application.SetCompatibleTextRenderingDefault(false);

            ///#if (!DEBUG)
            ///           frmSplash splash = new frmSplash();

            ///            DialogResult result = splash.ShowDialog();

            ///           if (result != System.Windows.Forms.DialogResult.OK)
            ///               return;
            ///#endif
            ///           Application.Run(new frmMain());
        }

        
        
        static void startConsole(VEN2b ven2b)
        {
            consoleMain console = new consoleMain(ven2b);
        }



    }
}
