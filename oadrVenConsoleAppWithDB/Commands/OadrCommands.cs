using oadrlib.lib.helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// All console commands must be in the sub-namespace Commands:
namespace oadrVenConsoleAppWithDB.Commands
{
    // Must be a public static class:
    public static class OadrCommands
    {
        // Methods used as console commands must be public and must return a string

        public static string start()
        {
            var result = "startPoll";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;


            //bool m_running = true;
            //string result;
            //if (m_running)
            //{
            //    result = $"{m_running.ToString()}";
            //}
            //else
            //{
            //    result = $"{m_running.ToString()}";
            //}


            //return string.Format(ConsoleFormatting.Indent(2) +
            //    "Result:: " + result + "\n" );

            //var result = "Not Implemented";
            //return result;
        }

        public static string stop()
        {
            var result = "stopPoll";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string quit()
        {
            //m_venWrapper.shutdown();

            var result = "quit";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string status()
        {
            var result = "status";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string listEvents()
        {
            var result = "listEvents";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string queryRegistration()
        {
            var result = "queryRegistration";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }

        public static string reRegister()
        {
            var result = "reRegister";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string registerReport()
        {
            var result = "registerReport";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }



        public static string addMarketContext(string data = "http://MarketContext.html")
        {
            var result = $"addMarketContext {data}";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            //var result = "addMarketContext";
            return result;
        }


        public static string listMarketContext()
        {
            var result = "listMarketContext";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string addResource(string data = "Resource1")
        {
            var result = $"addResource {data}";
            Logger.logMessage(result + "\n", "OadrCommands.log");

            //var result = "Not Implemented";
            return result;
        }


        public static string listResources()
        {
            var result = "listResources";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }


        public static string help()
        {
            var result = "help";
            Logger.logMessage(result + "\n", "OadrCommands.log");
            return result;
        }



        /**
                public static string DoSomething(int id, string data)
                {
                    return string.Format(ConsoleFormatting.Indent(2) + 
                        "I did something to the record Id {0} and saved the data '{1}'", id, data);
                }


                public static string DoSomethingElse(DateTime date)
                {
                    return string.Format(ConsoleFormatting.Indent(2) + "I did something else on {0}", date);
                }


                public static string DoSomethingOptional(int id, string data = "No Data Provided")
                {
                    var result = string.Format(ConsoleFormatting.Indent(2) + 
                        "I did something to the record Id {0} and saved the data {1}", id, data);

                    if(data == "No Data Provided")
                    {
                        result = string.Format(ConsoleFormatting.Indent(2) + 
                        "I did something to the record Id {0} but the optinal parameter "
                        + "was not provided, so I saved the value '{1}'", id, data);
                    }
                    return result;
                }
                
                **/

    }
}
