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
    class Program   // : IVenWrapper  // ??? Can Main Inherit?
    {
        //VEN2b ven2b;

        static void Main(string[] args)
        {
        
            Logger.logMessage( DateTime.Now + ":: " + "Begin Main...\n", "logger.log");
            
            int i = 0;
            if (args.Length == 0)
            {
                //Console.WriteLine($"No Command Args\n");
                Logger.logMessage( DateTime.Now + ":: " + "No Command Line Args Passed...\n", "logger.log");
            }
            else
            {
                Logger.logMessage( DateTime.Now + ":: " + "Logging Commmands:\n", "logger.log");
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
            Logger.logMessage( DateTime.Now + ":: " + $"Connection String = [{connectionString}]\n", "logger.log");


            
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

            string DEBUGSTR = "";

            // Save Poll Interval QueryRegistration
            string loggingString = "Create new queryRegistration\n";
            Logger.logMessage( DateTime.Now + ":: " + loggingString + "\n", "logger.log");

            oadrlib.lib.oadr2b.QueryRegistration queryRegistration = ven2b.queryRegistration();

            DEBUGSTR = queryRegistration.response.oadrRequestedOadrPollFreq.duration.ToString();
            // *** TODO:: WRITE TO DB POLL FREQUENCY!
            Logger.logMessage("\nDEBUG:: Poll Frequency:: " +  DEBUGSTR + "\n", "logger.log");

            //            Convert Poll Frequency "PT10" into 10000
            //            pollduration = queryRegistration.response.oadrRequestedOadrPollFreq.duration
            int totalSeconds = (int)System.Xml.XmlConvert.ToTimeSpan(queryRegistration.response.oadrRequestedOadrPollFreq.duration).TotalSeconds;
            Logger.logMessage("\nDEBUG:: Total Seconds::  " + totalSeconds + "\n", "logger.log");

            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
            Logger.logMessage("\nDEBUG:: Total MilliSeconds::  " + time + "\n", "logger.log");

            int poll_interval = 10;// 10;  // ??queryRegistration.poll_Int??
// *** TODO:: Pull Poll interval from queryRegistration.responseBody  *** 
// oadrCreatedPartyRegistrationType (VTN INFO)
            
            Logger.logMessage( DateTime.Now +  ":: Log queryRegistration requestBody:: \n" + queryRegistration.requestBody + "\n", "logger.log");
            Logger.logMessage( DateTime.Now +  ":: Log queryRegistration responseBody:: \n" + queryRegistration.responseBody + "\n", "logger.log");


            Logger.logMessage("SLEEPING:: Total MilliSeconds::  " + time + " " + DateTime.Now +"\n", "logger.log");
            System.Threading.Thread.Sleep(time);

            // public CreatePartyRegistration createPartyRegistration(string requestID, oadrProfileType profileType, oadrTransportType transportType, string oadrTransportAddress, bool oadrReportOnly, bool oadrXmlSignature, bool oadrHttpPullModel)

            // Create Party Input Params
            string requestID = queryRegistration.response.eiResponse.requestID;
            oadrProfileType profileType = oadrProfileType.Item20b;
            oadrTransportType transportType = oadrTransportType.simpleHttp;
            string oadrTransportAddress = "";
            bool oadrReportOnly = false;
            bool oadrXmlSignature = false;
            bool oadrHttpPullModel = true;


            DEBUGSTR = queryRegistration.response.registrationID;
            if (DEBUGSTR == "" || DEBUGSTR == null)
            {
                // No Registration yet
                // Create Party Registration
                Logger.logMessage("DEBUG:: createPartyRegistration:: \n", "logger.log");
                CreatePartyRegistration createPartyRegistration = ven2b.createPartyRegistration(requestID, profileType, transportType, oadrTransportAddress, oadrReportOnly, oadrXmlSignature, oadrHttpPullModel);
                Logger.logMessage(DateTime.Now + ":: Log createPartyRegistration requestBody:: \n" + createPartyRegistration.requestBody + "\n", "logger.log");
                Logger.logMessage(DateTime.Now + ":: Log createPartyRegistration responseBody:: \n" + createPartyRegistration.responseBody + "\n", "logger.log");
                Logger.logMessage("SLEEPING:: Total MilliSeconds::  " + time + " " + DateTime.Now + "\n", "logger.log");
                System.Threading.Thread.Sleep(time);

            }
            else
            {
                Logger.logMessage("\nDEBUG:: RegistrationID:: " + DEBUGSTR + "\n", "logger.log");
            }




//            Logger.logMessage("Proccessing queryRegistration ResponseBody... \n \n", "logger.log");










//            Logger.logMessage("Log queryRegistration PollInterval:: \n" + queryRegistration.responseBody + "\n", "logger.log");






//            Logger.logMessage("SLEEPING:: Total MilliSeconds::  " + time + " " + DateTime.Now + "\n", "logger.log");
//            System.Threading.Thread.Sleep(time);

            //            System.Threading.Thread.Sleep(5000);



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


            Logger.logMessage("DEBUG:: requestEvent:: \n", "logger.log");
            Logger.logMessage(DateTime.Now + ":: Log requestEvent requestBody:: \n" + requestEvent.requestBody + "\n", "logger.log");
            Logger.logMessage(DateTime.Now + ":: Log requestEvent responseBody:: \n" + requestEvent.responseBody + "\n", "logger.log");
            // SAVE EVENT DATA TO DB OR CHECK IF EXISTS


            Logger.logMessage( DateTime.Now +  "\nRequest Event Request Body:: \n", "requestEvent.log");
            Logger.logMessage( DateTime.Now +  requestEvent.requestBody, "requestEvent.log");
            Logger.logMessage( DateTime.Now +  "\nRequest Event Response Body:: \n", "requestEvent.log");
            Logger.logMessage( DateTime.Now +  requestEvent.responseBody, "requestEvent.log");
            Logger.logMessage( DateTime.Now +  "\n\n", "requestEvent.log");
            //Logger.logMessage( DateTime.Now +  requestEvent.requestBody.ToString());

            Logger.logMessage("SLEEPING:: Total MilliSeconds::  " + time + " " + DateTime.Now + "\n", "logger.log");
            System.Threading.Thread.Sleep(time);



            requestID = requestEvent.response.eiResponse.requestID;


            //** TODO:: ADD EVENTS TO LIST **
            List<oadrDistributeEventTypeOadrEvent> evts = new List<oadrDistributeEventTypeOadrEvent>();

            //            oadrDistributeEventTypeOadrEvent evt = requestEvent.responseBody; // m_lviToEvent[lvi];
            //            request.response = (oadrDistributeEventType)postRequest(requestBody, "/EiEvent", request);

            //            oadrDistributeEventTypeOadrEvent evt = (oadrDistributeEventType)requestEvent.responseBody;

            //            evts.Add(evt);

            //            List<oadrDistributeEventTypeOadrEvent> evts = getSelectedEvents();

            //            List<oadrDistributeEventTypeOadrEvent> evts = ;


            oadrlib.lib.oadr2b.OptTypeType optType = oadrlib.lib.oadr2b.OptTypeType.optIn;
            int responseCode = 200;
            string responseDescription = "OK";

//            public CreatedEvent createdEvent(string requestID, List<oadrDistributeEventTypeOadrEvent> evts, OptTypeType optType, int responseCode = 200, string responseDescription = "OK")

            oadrlib.lib.oadr2b.CreatedEvent createdEvent = ven2b.createdEvent( requestID, evts, optType, responseCode, responseDescription);


            Logger.logMessage("DEBUG:: CreatedEvent:: \n", "logger.log");
            Logger.logMessage(DateTime.Now + ":: Log CreatedEvent requestBody:: \n" + createdEvent.requestBody + "\n", "logger.log");
            Logger.logMessage(DateTime.Now + ":: Log CreatedEvent responseBody:: \n" + createdEvent.responseBody + "\n", "logger.log");



            Logger.logMessage("SLEEPING:: Total MilliSeconds::  " + time + " " + DateTime.Now + "\n", "logger.log");
            System.Threading.Thread.Sleep(time);




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

            /// Move UP
            /// 



            const string STATUS_SHUTDOWN = "Shutting down...";
            const string VEN_NOT_REGISTERED = "VEN NOT Registered";
            const string VEN_REGISTERED = "VEN IS Registered";

            bool m_running = false;

            // Begin Polling Loop

            oadrlib.lib.oadr2b.OadrPoll poll = ven2b.poll();


            Logger.logMessage("DEBUG:: OadrPoll:: \n", "logger.log");
            Logger.logMessage(DateTime.Now + ":: Log OadrPoll requestBody:: \n" + poll.requestBody + "\n", "logger.log");
            Logger.logMessage(DateTime.Now + ":: Log OadrPoll responseBody:: \n" + poll.responseBody + "\n", "logger.log");



            Logger.logMessage("SLEEPING:: Total MilliSeconds::  " + time + " " + DateTime.Now + "\n", "logger.log");
            System.Threading.Thread.Sleep(time);



            //Set Up Polling Loop

            OadrPoll oadrPoll = null;

            m_running = true;

            while (m_running)
            {
                try
                {
                    //            updateStatus(STATUS_POLLING);

                    //            if (handleRegistration())
                    //            {
                    oadrPoll = ven2b.poll();
                    //            }
                }
                catch (Exception ex)
                {
                    Logger.logMessage("ERROR:: " + ex + "\n", "logger.log");

                    //            m_callbacks.processException(ex);
                }

                if (oadrPoll.responseTypeIs(typeof(oadrDistributeEventType)))
                    Logger.logMessage("TODO:: Implement oadrPoll.responseTypeIs: oadrDistributeEventType\n", "logger.log");
                //                    processDistributeEvent(oadrPoll.getDistributeEventResponse());

                else if (oadrPoll.responseTypeIs(typeof(oadrRegisterReportType)))
                    Logger.logMessage("TODO:: Implement oadrPoll.responseTypeIs: oadrRegisterReportType\n", "logger.log");
                //processRegisterReport(oadrPoll.getRegisterReportResponse());

                else if (oadrPoll.responseTypeIs(typeof(oadrCreateReportType)))
                    Logger.logMessage("TODO:: Implement oadrPoll.responseTypeIs: oadrCreateReportType \n", "logger.log");
                //processCreateReport(oadrPoll.getCreateReportResponse());

                else if (oadrPoll.responseTypeIs(typeof(oadrCancelReportType)))
                    Logger.logMessage("TODO:: Implement oadrPoll.responseTypeIs: oadrCancelReportType \n", "logger.log");
                //processCancelReport(oadrPoll.getCancelReportResponse());

                else if (oadrPoll.responseTypeIs(typeof(oadrCancelPartyRegistrationType)))
                    Logger.logMessage("TODO:: Implement oadrPoll.responseTypeIs: oadrCancelPartyRegistrationType \n", "logger.log");
                //processCancelRegistration(oadrPoll.getCancelPartyRegistrationResponse());

                else if (oadrPoll.responseTypeIs(typeof(oadrRequestReregistrationType)))
                {
                    m_running = false;
                    Logger.logMessage("TODO:: Implement oadrPoll.responseTypeIs: oadrRequestReregistrationType \n", "logger.log");
                    //handleRegistration(true);
                }

                //Handle Event Changes
                //try
                //{
                //    handleEventStatusChanges();
                //}
                //catch (Exception ex)
                //{
                //    m_callbacks.processException(ex);
                //}

                try
                {
                    // only sleep if the last oadrpoll was an OadrResponse or an error occured
                    if (oadrPoll == null || oadrPoll.response == null || oadrPoll.responseTypeIs(typeof(oadrResponseType)) || oadrPoll.eiResponseCode != 200)
                    {
                        //updateStatus(STATUS_IDLE);

                        //Thread.Sleep(m_timeout);
                        System.Threading.Thread.Sleep(time);

                    }
                    else
                    {

                        System.Threading.Thread.Sleep(1000);
                        //Thread.Sleep(1000);
                    }

                }
                //catch (ThreadInterruptedException)
                //{
                //}
                catch (Exception ex)
                {
                    Logger.logMessage("ERROR:: " + ex + "\n", "logger.log");

                    //m_callbacks.processException(ex);
                }



            }

            /**

                    VenWrapper m_venWrapper;  // Replace and Remove Wrapper References?



                        m_ven = ven;

                        m_callbacks = callbacks;

                        m_resources.setCallback(this);

                        m_processEvents = new ProcessEvents(callbacks);

                        m_resources.startThread();

                        TimeSpan timeout = new TimeSpan(0, 0, tbPollInterval.ValueInt);

                        eOptType optType;

                        optType = eOptType.OptIn;

                        **/



            /** 
             * /// DEBUG SSL AND VENWRAPPER INFO
            try
            {

                            m_venWrapper.VEN.URL = tbURL.TextBoxText;
                            m_venWrapper.VEN.VENName = tbVENName.TextBoxText;
                            m_venWrapper.VEN.VENID = tbVENID.TextBoxText;
                            m_venWrapper.VEN.UseSSL = chkUseSslTls.Checked;
                if (m_venWrapper.VEN.UseSSL)
            {
                if (tbClientCertificate.TextBoxText != "")
                    m_venWrapper.VEN.loadCertificateFile(tbClientCertificate.TextBoxText, tbClientCertificatePassword.TextBoxText);
            }

            m_venWrapper.optTpye = optType;
                if (chkDisableHostnameCheck.Checked)
                {
                    RemoteCertificateValidationCallback callback = delegate (
                                System.Object obj, X509Certificate certificate, X509Chain chain,
                                SslPolicyErrors errors)
                    {
                        if (errors == SslPolicyErrors.RemoteCertificateNameMismatch)
                        {
                            return (true);
                        }

                        return true;
                    };

                    m_venWrapper.VEN.setServerCertificateValidationCallback(callback);
                }
                else
                {
                    m_venWrapper.VEN.setServerCertificateValidationCallback(null);
                }
            }
            catch (Exception ex)
            {
                if (tbClientCertificate.TextBoxText != "" && tbClientCertificatePassword.TextBoxText != "")
                    MessageBox.Show("Failed to open certificate file: " + ex.Message);

                processException(ex);
            }
            **/

            //m_venWrapper.queryRegistration();




            /**   //DEBUG

            setVENParameters();




        


                 
            private bool doQueryPartyRegistration()
        {
            QueryRegistration queryRegisteration;

            lock (m_ven) { queryRegisteration = m_ven.queryRegistration(); }

            m_callbacks.processQueryRegistration(queryRegisteration);

            if (queryRegisteration.eiResponseCode != 200)
                return false;

            setPollinterval(queryRegisteration.response);

            return true;
        }



             public void processQueryRegistration(QueryRegistration queryRegistration)
            {
                ucLog1.addOadrMessage(queryRegistration);
                updateError(queryRegistration);

                ucQueryRegistration1.updateRegistrationInfo(queryRegistration.response);

                if (m_venWrapper.VEN.IsRegistered)
                    updateRegistrationStatus(VEN_REGISTERED);

                setPollInterval(queryRegistration.response);
            }


            private void setPollInterval(oadrCreatedPartyRegistrationType createdPartyRegistration)
            {
                MethodInvoker mi = new MethodInvoker(delegate
                {
                    try
                    {
                        tbPollInterval.TextBoxText = createdPartyRegistration.oadrRequestedOadrPollFreq.duration;
                    }
                    catch
                    {
                    }

                });

                // BeginInvoke only needs to be called if we're tyring to update the UI from
                // a thread that did not create it
                if (this.InvokeRequired)
                    this.BeginInvoke(mi);
                else
                    mi.Invoke();
            }




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
                    string result = ""; // Execute(cmd);

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


        static void initializeVen()
        {

        }

        static void doQueryRegistration()
        {

        }

        static void doQueryEvent()
        {

        }



        ///**********************************************************************************/

        //private OadrPoll doPoll()
        //{
        //    OadrPoll oadrPoll = null;

        //    lock (m_ven) { oadrPoll = m_ven.poll(); }

        //    m_callbacks.processPoll(oadrPoll);

        //    if (oadrPoll == null)
        //        return null;

        //    m_serverOffsetSeconds = oadrPoll.serverOffset;

        //    if (oadrPoll.responseTypeIs(typeof(oadrDistributeEventType)))
        //        processDistributeEvent(oadrPoll.getDistributeEventResponse());

        //    else if (oadrPoll.responseTypeIs(typeof(oadrRegisterReportType)))
        //        processRegisterReport(oadrPoll.getRegisterReportResponse());

        //    else if (oadrPoll.responseTypeIs(typeof(oadrCreateReportType)))
        //        processCreateReport(oadrPoll.getCreateReportResponse());

        //    else if (oadrPoll.responseTypeIs(typeof(oadrCancelReportType)))
        //        processCancelReport(oadrPoll.getCancelReportResponse());

        //    else if (oadrPoll.responseTypeIs(typeof(oadrCancelPartyRegistrationType)))
        //        processCancelRegistration(oadrPoll.getCancelPartyRegistrationResponse());

        //    else if (oadrPoll.responseTypeIs(typeof(oadrRequestReregistrationType)))
        //        handleRegistration(true);

        //    return oadrPoll;
        //}

        ///**********************************************************************************/


        //private void execute()
        //{
        //    OadrPoll oadrPoll = null;

        //    while (m_running)
        //    {
        //        try
        //        {
        //            updateStatus(STATUS_POLLING);

        //            if (handleRegistration())
        //            {
        //                oadrPoll = doPoll();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            m_callbacks.processException(ex);
        //        }

        //        try
        //        {
        //            handleEventStatusChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            m_callbacks.processException(ex);
        //        }

        //        try
        //        {
        //            // only sleep if the last oadrpoll was an OadrResponse or an error occured
        //            if (oadrPoll == null || oadrPoll.response == null || oadrPoll.responseTypeIs(typeof(oadrResponseType)) || oadrPoll.eiResponseCode != 200)
        //            {
        //                updateStatus(STATUS_IDLE);

        //                Thread.Sleep(m_timeout);
        //            }
        //            else
        //            {
        //                Thread.Sleep(1000);
        //            }

        //        }
        //        catch (ThreadInterruptedException)
        //        {
        //        }
        //        catch (Exception ex)
        //        {
        //            m_callbacks.processException(ex);
        //        }
        //    }

        //}

        ///**********************************************************************************/

        //public void startPolling()
        //{
        //    if (m_running)
        //        return;

        //    m_running = true;

        //    m_pollThread = new Thread(delegate ()
        //    {
        //        execute();
        //    });

        //    m_pollThread.Start();

        //}

        ///**********************************************************************************/

        //public void shutdown()
        //{
        //    stopPolling();
        //    m_resources.stopThread();
        //}

        ///**********************************************************************************/

        //public void stopPolling()
        //{
        //    if (!m_running)
        //        return;

        //    m_running = false;
        //    m_pollThread.Interrupt();
        //    m_pollThread.Join();
        //}

        ///**********************************************************************************/


    }
}
