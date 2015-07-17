using System;
using System.Runtime.Serialization;

namespace Aldurcraft.WurmAssistantLauncher2
{
    [Serializable]
    public class LauncherException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public LauncherException()
        {
        }

        public LauncherException(string message) : base(message)
        {
        }

        public LauncherException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LauncherException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
