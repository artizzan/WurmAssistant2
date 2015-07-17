using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Aldurcraft.DevTools
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var robot = new Robot(args);
                robot.Execute();
            }
            catch (RobotException exception)
            {
                Helper.WriteOut("Malfunction: " + exception.Message);
                Environment.Exit((int)exception.ExitCode);
            }
            catch (Exception exception)
            {
                Helper.WriteOut("Malfunction: " + exception.Message);
                Environment.Exit(0);
            }
        }
    }
}
