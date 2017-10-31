using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using oadrlib.lib.helper;
using oadr2b_ven.ven;
using oadrlib.lib.oadr2b;
using System.Diagnostics;
using oadrlib.lib.http;
using oadr2b_ven;
using System.Configuration;

//Form
//IVenWrapper
//IQueryRegistration
//IManageOptSchedules
//IEvents
//IManageResources
//IucCreateReport

//  : Form, IVenWrapper, IQueryRegistration, IManageOptSchedules, IEvents, IManageResources, IucCreateReport


namespace oadrVenConsoleAppWithDB
{
    public partial class consoleMain : IVenWrapper  // , IQueryRegistration, IManageOptSchedules, IEvents, IManageResources, IucCreateReport
    {
        const string _commandNamespace = "oadrVenConsoleAppWithDB.Commands";

        static Dictionary<string, Dictionary<string, IEnumerable<ParameterInfo>>> _commandLibraries;

        private const string STATUS_SHUTDOWN = "Shutting down...";
        private const string VEN_NOT_REGISTERED = "VEN NOT Registered";
        private const string VEN_REGISTERED = "VEN IS Registered";

        private bool m_running = false;

        private VenWrapper m_venWrapper;

        //        private ClearLog m_clearLog;  // Forms


        public consoleMain(VEN2b ven2b)
        {
            Console.WriteLine($"\nStarting New main console thread:\n");
            Logger.logMessage($"\nStarting New main console thread:\n", "main.log");

            //InitializeComponent();

            //frmSplash splash = new frmSplash();

            //this.FormClosing += new FormClosingEventHandler(frmMain_FormClosing);

            //ucEvents1.setCallbackHandler(this);  // TODO

            //m_clearLog = new ClearLog(this);

            //tsVersion.Text = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //ucQueryRegistration1.setCallbackHandler(this);   // TODO

            //ucManageOptSchedulesView1.setCreateOptScheduleCallback(this);   // TODO
            //oadrucManageOptSchedulesView1.setCreateOptScheduleCallback(this);   // TODO

            //ucResources1.setCallback(this);  // TODO

            //ucReportRequests.setCallback(this);  // TODO

            //VEN2b ven = new VEN2b(new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12), tbURL.TextBoxText, tbVENName.TextBoxText,
            //                      "", tbVENID.TextBoxText, new HttpSecuritySettings());


            //            VEN2b ven2b = new VEN2b(new HttpWebRequestWrapper(false, System.Net.SecurityProtocolType.Tls12), url, venName, venID, password);

            m_venWrapper = new VenWrapper(ven2b, this);
            setVENParameters();

            m_venWrapper.queryRegistration();

            m_venWrapper.register();

            m_venWrapper.registerReports();

            m_venWrapper.startPolling();


            // testConnectAndPoll();


            //ucResources1.addResource("resource1", "Load");  // TODO

            //ucMarketContexts.OnAddItem += new EventHandler(ucMarketContext_addItem);  // TODO
            //ucMarketContexts.OnRemoveItems += new EventHandler(ucMarketContext_removeItems);  // TODO

            //ucCustomSignals.OnAddItem += new EventHandler(ucCustomSignal_addItem);  // TODO
            //ucCustomSignals.OnRemoveItems += new EventHandler(ucCustomSignal_removeItems);  // TODO

            //ucMarketContexts.addItem("http://MarketContext1");

            StartConsole();

            m_venWrapper.stopPolling();

            m_venWrapper.shutdown();

        }

        static void StartConsole()
        {

            Console.Title = typeof(Program).Name;

            // Any static classes containing commands for use from the 
            // console are located in the Commands namespace. Load 
            // references to each type in that namespace via reflection:
            _commandLibraries = new Dictionary<string, Dictionary<string,
                    IEnumerable<ParameterInfo>>>();

            // Use reflection to load all of the classes in the Commands namespace:
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == _commandNamespace
                    select t;
            var commandClasses = q.ToList();

            foreach (var commandClass in commandClasses)
            {
                // Load the method info from each class into a dictionary:
                var methods = commandClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
                var methodDictionary = new Dictionary<string, IEnumerable<ParameterInfo>>();
                foreach (var method in methods)
                {
                    string commandName = method.Name;
                    methodDictionary.Add(commandName, method.GetParameters());
                }
                // Add the dictionary of methods for the current class into a dictionary of command classes:
                _commandLibraries.Add(commandClass.Name, methodDictionary);
            }
            Run();
        }


        static void Run()
        {
            while (true)
            {
                var consoleInput = ReadFromConsole();
                if (string.IsNullOrWhiteSpace(consoleInput)) continue;

                try
                {
                    // Create a ConsoleCommand instance:
                    var cmd = new ConsoleCommand(consoleInput);

                    // Execute the command:
                    string result = Execute(cmd);

                    // Write out the result:
                    WriteToConsole(result);

                    Logger.logMessage(result + "\n", "main.log");

                }
                catch (Exception ex)
                {
                    // OOPS! Something went wrong - Write out the problem:
                    WriteToConsole(ex.Message);

                    Logger.logMessage(ex.Message  + "\n", "main.log");
                }
            }
        }


        static string Execute(ConsoleCommand command)
        {
            // Validate the class name and command name:
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            string badCommandMessage = string.Format(""
                + "Unrecognized command \'{0}.{1}\'. "
                + "Please type a valid command.",
                command.LibraryClassName, command.Name);

            // Validate the command name:
            if (!_commandLibraries.ContainsKey(command.LibraryClassName))
            {
                return badCommandMessage;
            }
            var methodDictionary = _commandLibraries[command.LibraryClassName];
            if (!methodDictionary.ContainsKey(command.Name))
            {
                return badCommandMessage;
            }

            // Make sure the corret number of required arguments are provided:
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            var methodParameterValueList = new List<object>();
            IEnumerable<ParameterInfo> paramInfoList = methodDictionary[command.Name].ToList();

            // Validate proper # of required arguments provided. Some may be optional:
            var requiredParams = paramInfoList.Where(p => p.IsOptional == false);
            var optionalParams = paramInfoList.Where(p => p.IsOptional == true);
            int requiredCount = requiredParams.Count();
            int optionalCount = optionalParams.Count();
            int providedCount = command.Arguments.Count();

            if (requiredCount > providedCount)
            {
                return string.Format(
                    "Missing required argument. {0} required, {1} optional, {2} provided",
                    requiredCount, optionalCount, providedCount);
            }

            // Make sure all arguments are coerced to the proper type, and that there is a 
            // value for every emthod parameter. The InvokeMember method fails if the number 
            // of arguments provided does not match the number of parameters in the 
            // method signature, even if some are optional:
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            if (paramInfoList.Count() > 0)
            {
                // Populate the list with default values:
                foreach (var param in paramInfoList)
                {
                    // This will either add a null object reference if the param is required 
                    // by the method, or will set a default value for optional parameters. in 
                    // any case, there will be a value or null for each method argument 
                    // in the method signature:
                    methodParameterValueList.Add(param.DefaultValue);
                }

                // Now walk through all the arguments passed from the console and assign 
                // accordingly. Any optional arguments not provided have already been set to 
                // the default specified by the method signature:
                for (int i = 0; i < command.Arguments.Count(); i++)
                {
                    var methodParam = paramInfoList.ElementAt(i);
                    var typeRequired = methodParam.ParameterType;
                    object value = null;
                    try
                    {
                        // Coming from the Console, all of our arguments are passed in as 
                        // strings. Coerce to the type to match the method paramter:
                        value = CoerceArgument(typeRequired, command.Arguments.ElementAt(i));
                        methodParameterValueList.RemoveAt(i);
                        methodParameterValueList.Insert(i, value);
                    }
                    catch (ArgumentException ex)
                    {
                        string argumentName = methodParam.Name;
                        string argumentTypeName = typeRequired.Name;
                        string message =
                            string.Format(""
                            + "The value passed for argument '{0}' cannot be parsed to type '{1}'",
                            argumentName, argumentTypeName);
                        throw new ArgumentException(message);
                    }
                }
            }

            // Set up to invoke the method using reflection:
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            Assembly current = typeof(Program).Assembly;

            // Need the full Namespace for this:
            Type commandLibaryClass =
                current.GetType(_commandNamespace + "." + command.LibraryClassName);

            object[] inputArgs = null;
            if (methodParameterValueList.Count > 0)
            {
                inputArgs = methodParameterValueList.ToArray();
            }
            var typeInfo = commandLibaryClass;

            // This will throw if the number of arguments provided does not match the number 
            // required by the method signature, even if some are optional:
            try
            {
                var result = typeInfo.InvokeMember(
                    command.Name,
                    BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                    null, null, inputArgs);
                return result.ToString();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }


        static object CoerceArgument(Type requiredType, string inputValue)
        {
            var requiredTypeCode = Type.GetTypeCode(requiredType);
            string exceptionMessage =
                string.Format("Cannnot coerce the input argument {0} to required type {1}",
                inputValue, requiredType.Name);

            object result = null;
            switch (requiredTypeCode)
            {
                case TypeCode.String:
                    result = inputValue;
                    break;

                case TypeCode.Int16:
                    short number16;
                    if (Int16.TryParse(inputValue, out number16))
                    {
                        result = number16;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;

                case TypeCode.Int32:
                    int number32;
                    if (Int32.TryParse(inputValue, out number32))
                    {
                        result = number32;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;

                case TypeCode.Int64:
                    long number64;
                    if (Int64.TryParse(inputValue, out number64))
                    {
                        result = number64;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;

                case TypeCode.Boolean:
                    bool trueFalse;
                    if (bool.TryParse(inputValue, out trueFalse))
                    {
                        result = trueFalse;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;

                case TypeCode.Byte:
                    byte byteValue;
                    if (byte.TryParse(inputValue, out byteValue))
                    {
                        result = byteValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;

                case TypeCode.Char:
                    char charValue;
                    if (char.TryParse(inputValue, out charValue))
                    {
                        result = charValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;

                case TypeCode.DateTime:
                    DateTime dateValue;
                    if (DateTime.TryParse(inputValue, out dateValue))
                    {
                        result = dateValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Decimal:
                    Decimal decimalValue;
                    if (Decimal.TryParse(inputValue, out decimalValue))
                    {
                        result = decimalValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Double:
                    Double doubleValue;
                    if (Double.TryParse(inputValue, out doubleValue))
                    {
                        result = doubleValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Single:
                    Single singleValue;
                    if (Single.TryParse(inputValue, out singleValue))
                    {
                        result = singleValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.UInt16:
                    UInt16 uInt16Value;
                    if (UInt16.TryParse(inputValue, out uInt16Value))
                    {
                        result = uInt16Value;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.UInt32:
                    UInt32 uInt32Value;
                    if (UInt32.TryParse(inputValue, out uInt32Value))
                    {
                        result = uInt32Value;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.UInt64:
                    UInt64 uInt64Value;
                    if (UInt64.TryParse(inputValue, out uInt64Value))
                    {
                        result = uInt64Value;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                default:
                    throw new ArgumentException(exceptionMessage);
            }
            return result;
        }


        public static void WriteToConsole(string message = "")
        {
            if (message.Length > 0)
            {
                Console.WriteLine(message);
            }
        }


        const string _readPrompt = "console> ";
        public static string ReadFromConsole(string promptMessage = "")
        {
            // Show a prompt, and get input:
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }



        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }




        //

        /**********************************************************/

        /*******
         * start helper function definitions
         * 
         * **/


        /**********************************************************/

        private void ucMarketContext_addItem(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //m_venWrapper.MarketContext.Add(ucMarketContexts.NewItem);
        }

        /**********************************************************/

        private void ucMarketContext_removeItems(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //foreach (string marketContext in ucMarketContexts.RemovedItems)
            //    m_venWrapper.MarketContext.Remove(marketContext);
        }

        /**********************************************************/

        private void ucCustomSignal_addItem(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //m_venWrapper.CustomSingals.Add(ucCustomSignals.NewItem);
        }

        /**********************************************************/

        private void ucCustomSignal_removeItems(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //foreach (string customSignal in ucCustomSignals.RemovedItems)
            //    m_venWrapper.CustomSingals.Remove(customSignal);
        }

        /**********************************************************/

        private void updateStatus(string status)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //MethodInvoker mi = new MethodInvoker(delegate
            //{
            //    tsStatus.Text = status;
            //});

            //// BeginInvoke only needs to be called if we're tyring to update the UI from
            //// a thread that did not create it
            //if (this.InvokeRequired)
            //    this.BeginInvoke(mi);
            //else
            //    mi.Invoke();
        }

        /**********************************************************/

        private void updateError(OadrRequest request)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //MethodInvoker mi = new MethodInvoker(delegate
            //{
            //    // Date: Wed, 12 Jun 2013 20:13:51 GMT
            //    // DateTime t = DateTime.ParseExact(request.serverDate, "ddd, dd MMM yyyy HH:mm:ss GMT", CultureInfo.CurrentCulture).ToLocalTime().ToString();

            //    // use InvariantCulture instead of CurrentCulture.  using CurrentCulture expects date to
            //    // be in local language (but the server is serving en-us)
            //    try
            //    {
            //        string dt = DateTime.ParseExact(request.serverDate, "ddd, dd MMM yyyy HH:mm:ss GMT", CultureInfo.InvariantCulture).ToLocalTime().ToString();

            //        tsError.Text = request.eiResponseCode.ToString() + ": " + request.eiResponseDescription;

            //        string offset = (request.serverOffset > 0 ? "+" : "") + request.serverOffset.ToString();

            //        tsServerTime.Text = "Server time: " + dt + " (" + offset + "s)";
            //    }
            //    catch
            //    {
            //    }

            //});

            //// BeginInvoke only needs to be called if we're tyring to update the UI from
            //// a thread that did not create it
            //if (this.InvokeRequired)
            //    this.BeginInvoke(mi);
            //else
            //    mi.Invoke();
        }

        /**********************************************************/

        private void updateError(Exception ex)
        {
            // TODO: add logging here
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //MethodInvoker mi = new MethodInvoker(delegate
            //{
            //    tsError.Text = "Exception: " + ex.Message;
            //    tsError.ToolTipText = "Exception: " + ex.Message;

            //    try
            //    {
            //        ucLog1.addSystemMessage(ex.Message, UserControls.Log.WebLogView.eWebLogMessageStatus.ERROR);
            //        oadrlib.lib.helper.Logger.logException(ex);
            //    }
            //    catch (Exception logException)
            //    {
            //        MessageBox.Show("Error writing to log file: " + logException.Message);
            //    }

            //});

            //// BeginInvoke only needs to be called if we're tyring to update the UI from
            //// a thread that did not create it
            //if (this.InvokeRequired)
            //    this.BeginInvoke(mi);
            //else
            //    mi.Invoke();
        }

        /**********************************************************/

        private void updateRegistrationStatus(string message)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //MethodInvoker mi = new MethodInvoker(delegate
            //{
            //    tsRegistrationStatus.Text = message;
            //});

            //// BeginInvoke only needs to be called if we're tyring to update the UI from
            //// a thread that did not create it
            //if (this.InvokeRequired)
            //    this.BeginInvoke(mi);
            //else
            //    mi.Invoke();
        }

        /**********************************************************/

        private void setVENParameters()
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            int seconds = 10;
            //            TimeSpan timeout = new TimeSpan(0, 0, tbPollInterval.ValueInt);
            // Check if seconds is between the min and max. Also check what else happens with tbPollInterval.ValueInt
            // Change seconds to read from DB or User input. 

            TimeSpan timeout = new TimeSpan(0, 0, seconds);

            eOptType optType;

            try
            {
                //add logic to decide opt type

                optType = eOptType.OptIn;

                //if (rbManual.Checked == true)
                //    optType = eOptType.Manual;
                //else if (rbOptIn.Checked == true)
                //    optType = eOptType.OptIn;
                //else
                //    optType = eOptType.OptOut;

                //User input or app settings or Database input.
                //Hard Code for now to app settings
                m_venWrapper.VEN.URL = ConfigurationManager.AppSettings["url"];
                m_venWrapper.VEN.VENName = ConfigurationManager.AppSettings["venName"];
                m_venWrapper.VEN.VENID = ConfigurationManager.AppSettings["venID"];
                m_venWrapper.VEN.UseSSL = false; // temporarily set to false. Set up SSL later.

                if (m_venWrapper.VEN.UseSSL)
                {
                    // SSL is true. Find input for SSL Client Certificate.
                    //if (tbClientCertificate.TextBoxText != "")
                    //    m_venWrapper.VEN.loadCertificateFile(tbClientCertificate.TextBoxText, tbClientCertificatePassword.TextBoxText);
                }

                m_venWrapper.optTpye = optType;

                // Add logic for Disabled Hostname Verification and Flag.

                //if (chkDisableHostnameCheck.Checked)
                //{
                //    RemoteCertificateValidationCallback callback = delegate (
                //                System.Object obj, X509Certificate certificate, X509Chain chain,
                //                SslPolicyErrors errors)
                //    {
                //        if (errors == SslPolicyErrors.RemoteCertificateNameMismatch)
                //        {
                //            return (true);
                //        }

                //        return true;
                //    };

                //    m_venWrapper.VEN.setServerCertificateValidationCallback(callback);
                //}
                //else
                //{
                m_venWrapper.VEN.setServerCertificateValidationCallback(null); // Uncomment as default option for now.
                //}

            }
            catch (Exception ex)
            {
                //if (tbClientCertificate.TextBoxText != "" && tbClientCertificatePassword.TextBoxText != "")
                //    MessageBox.Show("Failed to open certificate file: " + ex.Message);

                processException(ex);
            }
        }

        /**********************************************************/

        private void btnCheckPassword_Click(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //try
            //{
            //    m_venWrapper.VEN.loadCertificateFile(tbClientCertificate.TextBoxText, tbClientCertificatePassword.TextBoxText);
            //    MessageBox.Show("Successfully opened certificate", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Failed to open certificate file: " + ex.Message, "Open Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //    processException(ex);
            //}
        }

        /**********************************************************/

        private void handlePollThread()
        {
            //string methodname = GetCurrentMethod();
            //Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            try
            {
                if (m_venWrapper.IsRunning)
                {
                    m_venWrapper.stopPolling();

                    //btnStartStop.Text = "Start Polling";
                    Logger.logMessage($"Start Polling:\n", "main.log");

                    //ucQueryRegistration1.enableQuery();

                    // updateRegistrationStatus(VEN_NOT_REGISTERED);
                }
                else
                {
                    setVENParameters();

                    //if (chkUseSslTls.Checked && tbURL.TextBoxText.StartsWith("http://"))
                    //    updateError(new Exception("SSL/TLS is enabled but 'http:' is used in URL."));
                    //else if (!chkUseSslTls.Checked && tbURL.TextBoxText.StartsWith("https://"))
                    //    updateError(new Exception("SSL/TLS is disabled but 'https:' is used in URL."));

                    m_venWrapper.startPolling();
                    //btnStartStop.Text = "Stop Polling";
                    Logger.logMessage($"Stop Polling:\n", "main.log");

                    //ucQueryRegistration1.disableQuery();

                }
            }
            catch (Exception ex)
            {
                updateError(ex);
            }

        }

        /**********************************************************/

            // Remove reference to form. Change to console closing closes all open threads.

        private void frmMain_FormClosing(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //if (m_venWrapper.IsRunning)
            //{
            //    updateStatus(STATUS_SHUTDOWN);
            //}

            //m_venWrapper.shutdown();
        }

        /**********************************************************/

            // Change from button to console command

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            handlePollThread();
        }

        /**********************************************************/

        //private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    AboutBox1 abx = new AboutBox1();

        //    abx.ShowDialog();
        //}

        /**********************************************************/

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //m_clearLog.getUserOK("Clear Log?", "Select OK to clear all log messages.");
        }

        /**********************************************************/

        public void clearLog()
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucLog1.clearMessages();
        }

        /**********************************************************/

        private void btnClearEvents_Click(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //ucEvents1.removeAllEvents();

            //m_venWrapper.clearEvents();

            //txtHistory.Text = "Events Cleared: " + DateTime.Now.ToLongTimeString() + "\r\n" + txtHistory.Text;
        }

        /**********************************************************/

        private void btnChooseClientCertificate_Click(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //DialogResult diagResult = fdClientCertFile.ShowDialog();
            //if (diagResult == DialogResult.OK)
            //{
            //    tbClientCertificate.TextBoxText = fdClientCertFile.FileName;
            //}
        }

        /**********************************************************/

        private void chkUseSslTls_CheckedChanged(object sender, EventArgs e)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //tbClientCertificate.Enabled = chkUseSslTls.Checked;
            //tbClientCertificatePassword.Enabled = chkUseSslTls.Checked;
            //btnChooseClientCertificate.Enabled = chkUseSslTls.Checked;

            //// Set the URL to either be "http" or "https" depending on whether encryption is used.
            //if (chkUseSslTls.Checked && tbURL.TextBoxText.StartsWith("http://"))
            //{
            //    tbURL.TextBoxText = tbURL.TextBoxText.Replace("http://", "https://");
            //}
            //else if (!chkUseSslTls.Checked && tbURL.textBox1.Text.StartsWith("https://"))
            //{
            //    tbURL.TextBoxText = tbURL.TextBoxText.Replace("https://", "http://");
            //}

            setVENParameters();
        }

        /**********************************************************/
        // ven wrapper callbacks
        /**********************************************************/

        public void processException(Exception ex)
        {
            updateError(ex);
        }

        /**********************************************************/

        private void setPollInterval(oadrCreatedPartyRegistrationType createdPartyRegistration)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //MethodInvoker mi = new MethodInvoker(delegate
            //{
            //    try
            //    {
            //        tbPollInterval.TextBoxText = createdPartyRegistration.oadrRequestedOadrPollFreq.duration;
            //    }
            //    catch
            //    {
            //    }

            //});

            //// BeginInvoke only needs to be called if we're tyring to update the UI from
            //// a thread that did not create it
            //if (this.InvokeRequired)
            //    this.BeginInvoke(mi);
            //else
            //    mi.Invoke();
        }

        /**********************************************************/

        public void processCreatePartyRegisteration(CreatePartyRegistration createPartyRegistration)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //ucLog1.addOadrMessage(createPartyRegistration);
            //updateError(createPartyRegistration);

            //ucQueryRegistration1.updateRegistrationInfo(createPartyRegistration.response);

            //if (m_venWrapper.VEN.IsRegistered)
            //    updateRegistrationStatus(VEN_REGISTERED);

            //setPollInterval(createPartyRegistration.response);
        }

        /**********************************************************/

        public void processQueryRegistration(QueryRegistration queryRegistration)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucLog1.addOadrMessage(queryRegistration);
            //updateError(queryRegistration);

            //ucQueryRegistration1.updateRegistrationInfo(queryRegistration.response);

            //if (m_venWrapper.VEN.IsRegistered)
            //    updateRegistrationStatus(VEN_REGISTERED);

            //setPollInterval(queryRegistration.response);
        }

        /**********************************************************/

        public void processCancelRegistration(CancelPartyRegistration cancelRegistration)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucLog1.addOadrMessage(cancelRegistration);
            //updateError(cancelRegistration);

            //// didn't receive an oadrCancledPartyRegistration response so an error must have occurred
            //// and the cancel registration failed
            //if (cancelRegistration.response == null)
            //    return;

            //ucQueryRegistration1.cancelRegistration();

            //if (!m_venWrapper.VEN.IsRegistered)
            //    updateRegistrationStatus(VEN_NOT_REGISTERED);

        }

        /**********************************************************/

        public void processCanceledRegistration(CanceledPartyRegistration canceledRegistration)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucLog1.addOadrMessage(canceledRegistration);
            //updateError(canceledRegistration);

            //// didn't receive an oadrCancledPartyRegistration response so an error must have occurred
            //// and the cancel registration failed
            //if (canceledRegistration.response == null)
            //    return;

            //ucQueryRegistration1.cancelRegistration();

            //if (!m_venWrapper.VEN.IsRegistered)
            //    updateRegistrationStatus(VEN_NOT_REGISTERED);
        }

        /**********************************************************/

        public void processRequestEvent(RequestEvent requestEvent)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //updateError(requestEvent);
            //// ucEvents1.updateEvents(requestEvent.response);
            //ucLog1.addOadrMessage(requestEvent, chkAutoScroll.Checked);
        }

        /**********************************************************/

        public void processCreatedEvent(CreatedEvent createdEvent, Dictionary<string, OadrEventWrapper> activeEvents, string requestID)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //// update list of active events
            //// this must be called before ucEvents1.updateOptType(createdEvent.request);
            //// updateOptType will lookup event by ID and, well, update the optType
            //// new events won't be in the list of updateEvents isn't called first
            //ucEvents1.updateEvents(activeEvents, requestID);

            //// createdEvent will be null if the VTN sent an empty distributeEvent message,
            //// clearing all events for this ven
            //// createdEvent is NULL because there's no response to send when distributeEvent
            //// is empty
            //// it can also be NULL if the message contained only updates to event status
            //if (createdEvent != null)
            //{
            //    ucLog1.addOadrMessage(createdEvent);

            //    ucEvents1.updateOptType(createdEvent.request);

            //    updateError(createdEvent);
            //}
        }

        /**********************************************************/

        public void processUpdateStatus(string status, bool threadStopped)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //updateStatus(status);

            //MethodInvoker mi = new MethodInvoker(delegate
            //{
            //    if (threadStopped)
            //    {
            //        btnStartStop.Text = "Start Polling";
            //        ucQueryRegistration1.enableQuery();
            //    }
            //});

            //// BeginInvoke only needs to be called if we're tyring to update the UI from
            //// a thread that did not create it
            //if (this.InvokeRequired)
            //    this.BeginInvoke(mi);
            //else
            //    mi.Invoke();
        }

        /**********************************************************/

        public void processPoll(OadrPoll oadrPoll)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //updateError(oadrPoll);
            //ucLog1.addOadrMessage(oadrPoll, chkAutoScroll.Checked);

            ///*if (oadrPoll.responseTypeIs(typeof(oadrDistributeEventType)))
            //{
            //    ucEvents1.updateEvents(oadrPoll.getDistributeEventResponse());
            //}*/
        }

        /**********************************************************/

        public void processCreateOptSchedule(CreateOpt createOpt)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //updateError(createOpt);
            //ucLog1.addOadrMessage(createOpt, chkAutoScroll.Checked);

            //ucManageOptSchedulesView1.addOptSchedule(createOpt);
            //oadrucManageOptSchedulesView1.addOptSchedule(createOpt);
        }

        /**********************************************************/

        public void processCancelOpt(CancelOpt cancelOpt)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //updateError(cancelOpt);
            //ucLog1.addOadrMessage(cancelOpt, chkAutoScroll.Checked);

            //ucManageOptSchedulesView1.cancelOptSchedule(cancelOpt);
            //oadrucManageOptSchedulesView1.cancelOptSchedule(cancelOpt);
        }

        /**********************************************************/

        public void processCreateOpt(CreateOpt createOpt)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //ucLog1.addOadrMessage(createOpt);

            //updateError(createOpt);

            //if (createOpt.eiResponseCode == 200)
            //{
            //    ucEvents1.updateOptType(createOpt.request);
            //}
        }

        /**********************************************************/

        public void processRegisteredReport(RegisteredReport registeredReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //updateError(registeredReport);
            //ucLog1.addOadrMessage(registeredReport, chkAutoScroll.Checked);
        }

        /**********************************************************/

        public void processRegisterReport(RegisterReport registerReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //updateError(registerReport);
            //ucLog1.addOadrMessage(registerReport, chkAutoScroll.Checked);

            //ucRegisteredReports1.handleRegisterReports(registerReport.request);
        }

        /**********************************************************/

        public void processUpdateReportList(oadrRegisterReportType registerReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucRegisteredReports1.handleRegisterReports(registerReport);
        }

        /**********************************************************/

        public void processCreateReport(oadrReportRequestType[] reportRequests)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucReportRequests.handleCreateReport(reportRequests);
        }

        /**********************************************************/

        public void processCreatedReport(CreatedReport createdReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //updateError(createdReport);
            //ucLog1.addOadrMessage(createdReport, chkAutoScroll.Checked);
        }

        /**********************************************************/

        public void processUpdateReport(UpdateReport updateReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //updateError(updateReport);
            //ucLog1.addOadrMessage(updateReport, chkAutoScroll.Checked);
        }

        /**********************************************************************************/

        public void processCanceledReport(CanceledReport canceledReport, oadrCancelReportType cancelReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //updateError(canceledReport);
            //ucLog1.addOadrMessage(canceledReport, chkAutoScroll.Checked);

            //ucReportRequests.processCancelReport(cancelReport);
        }

        /**********************************************************************************/

        /// <summary>
        /// called when a cancel report is piggy backed in an updated report response
        /// </summary>
        /// <param name="cancelReport"></param>
        public void processCancelReport(oadrCancelReportType cancelReport)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucReportRequests.processCancelReport(cancelReport);
        }

        /**********************************************************************************/

        public void processCreateReportComplete(string reportRequestID)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //ucReportRequests.processCreateReportComplete(reportRequestID);
        }

        /**********************************************************/

        public void updateEventStatus(List<oadrDistributeEventTypeOadrEvent> oadrEvents)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");


            //if (oadrEvents != null && oadrEvents.Count > 0)
            //    ucEvents1.updateEventsTimeTriggered(oadrEvents);
        }

        /**********************************************************/

        public void logSystemMessage(string message /*, oadr2b_ven.UserControls.Log.WebLogView.eWebLogMessageStatus status */  )
        {
            // **TODO** Add Status to logging message input and stream. (Either OK 200, Bad 500, or Info/Warning/Message)
            Logger.logMessage($"Logging Message:: {message}\n", "logSystemMessage.log");

            //ucLog1.addSystemMessage(message, status);
        }

        /**********************************************************/
        // ucQueryRegistration callbacks
        /**********************************************************/

        public void processQueryRegistration()
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                m_venWrapper.queryRegistration();
            }
            catch (Exception ex)
            {
                updateError(ex);
            }
        }

        /**********************************************************/

        public void processCancelRegistration()
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                m_venWrapper.cancelRegistration();
            }
            catch (Exception ex)
            {
                updateError(ex);
            }
        }

        /**********************************************************/

        public void processRegister()
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                m_venWrapper.register();
            }
            catch (Exception ex)
            {
                updateError(ex);
            }
        }

        /**********************************************************/

        public void processClearRegistration()
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                //m_venWrapper.clearRegistration(tbVENID.TextBoxText.Trim());

                updateRegistrationStatus(VEN_NOT_REGISTERED);
            }
            catch (Exception ex)
            {
                updateError(ex);
            }
        }

        /**********************************************************/
        // ucManageOptSchedules callbacks
        /**********************************************************/

        public void createOptSchedule(oadrlib.lib.oadr2b.OptSchedule optSchedule)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                m_venWrapper.createOptSchedule(optSchedule);
            }
            catch (Exception ex)
            {
                updateError(ex);
            }

        }

        /**********************************************************/

        public void cancelOptSchedule(string optID)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                m_venWrapper.cancelOptSchedule(optID);
            }
            catch (Exception ex)
            {
                updateError(ex);
            }
        }

        /**********************************************************/
        // ucManageOptSchedules callbacks
        /**********************************************************/

        public void processEventOpt(List<oadrDistributeEventTypeOadrEvent> evts, OptTypeType optType, string requestID, int responseCode, string responseDescription)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();


                m_venWrapper.updateEventOpt(evts, optType, requestID, responseCode, responseDescription);
            }
            catch (Exception ex)
            {
                updateError(ex);
            }

        }

        /**********************************************************/

        public void processCreateEventOpt(List<oadrDistributeEventTypeOadrEvent> evts, OptTypeType optType, OptReasonEnumeratedType optReason, string resourceID)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            try
            {
                setVENParameters();

                m_venWrapper.updateEventOpt(evts, optType, optReason, resourceID);
            }
            catch (Exception ex)
            {
                updateError(ex);
            }
        }

        /**********************************************************/

        public void populateLists(List<string> marketContexts, List<string> resources)
        {
            string methodname = GetCurrentMethod();
            Logger.logMessage($"Implement function:: {methodname}\n", "main.log");

            //foreach (string mc in m_venWrapper.MarketContext)
            //    marketContexts.Add(mc);

            //foreach (ven.resources.Resource resource in m_venWrapper.Resources.ResourceDictionary.Values)
            //    resources.Add(resource.ResourceID);
        }

        /**********************************************************/
        // ucResources callbacks
        /**********************************************************/

        public void processRegisterReports()
        {
            m_venWrapper.registerReports();
        }

        /**********************************************************/

        public void addResource(oadr2b_ven.ven.resources.Resource resource)
        {
            m_venWrapper.addResource(resource);
        }

        /**********************************************************/

        public void removeResource(oadr2b_ven.ven.resources.Resource resource)
        {
            m_venWrapper.removeResource(resource);
        }

        /**********************************************************/
        // ucCreateReport callbacks
        /**********************************************************/

        public void clearAllReports()
        {
            m_venWrapper.clearAllReports();
        }

        private void ucLog1_Load(object sender, EventArgs e)
        {

        }        

    }
}
