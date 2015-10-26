using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aldurcraft.WurmAssistantMutexes
{
    /// <summary>
    /// Do not dispose on different thread
    /// </summary>
    public class GatewayClosedException : Exception
    {
        public GatewayClosedException(string message)
            : base(message)
        {
        }
    }

    public abstract class GatewayBase : IDisposable
    {
        protected string MutexName { get; private set; }
        protected string UniqueIdentifier { get; private set; }

        public string UniqueId { get; private set; }

        private string mutexId;

        private bool hasHandle = false;
        private readonly Mutex mutex;
        const string DefaultErrorFormat = "{0} is already running";

        [ThreadStatic]
        private static HashSet<string> idsInUse;

        private static HashSet<string> IdsInUse
        {
            get { return idsInUse ?? (idsInUse = new HashSet<string>()); }
        }

        protected GatewayBase(string mutexName, Guid uniqueIdentifier, int timeout, string customError, bool doNotAutoEnter)
        {
            if (string.IsNullOrWhiteSpace(mutexName)) throw new ArgumentException("mutexName cannot be empty");
            if (uniqueIdentifier == Guid.Empty) throw new ArgumentException("uniqueIdentifier cannot be empty");

            MutexName = mutexName;
            UniqueIdentifier = uniqueIdentifier.ToString().ToUpperInvariant();

            UniqueId = MutexName + UniqueIdentifier;
            mutexId = string.Format("Global\\{{{0}}}", UniqueId);
            mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            mutex.SetAccessControl(securitySettings);

            if (!doNotAutoEnter)
            {
                Enter(timeout, customError);
            }
        }

        public void Enter(int timeOut, string customError = null)
        {
            if (hasHandle) return;

            if (IdsInUse.Contains(mutexId))
            {
                // synchronize within thread
                throw new GatewayClosedException(customError ?? string.Format(DefaultErrorFormat, MutexName));
            }
            
            try
            {
                hasHandle = mutex.WaitOne(timeOut <= 0 ? Timeout.Infinite : timeOut, false);

                if (hasHandle == false)
                {
                    throw new GatewayClosedException(customError ?? string.Format(DefaultErrorFormat, MutexName));
                }

                IdsInUse.Add(mutexId);
            }
            catch (AbandonedMutexException)
            {
                hasHandle = true;
            }
        }


        public void Dispose()
        {
            if (mutex != null)
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
                mutex.Dispose();
            }

            IdsInUse.Remove(mutexId);
        }
    }

    public class WurmAssistantGateway : GatewayBase
    {
        private static readonly Guid Id = new Guid("4377383E-C48F-40CF-9E58-6E5D86FD327D");
        private const string Name = "AldurCraftWurmAssistant";
        public WurmAssistantGateway(string customError = null, int timeout = 1000, bool doNotAutoEnter = false)
            : base(Name, Id, timeout, customError, doNotAutoEnter)
        {
        }
        public WurmAssistantGateway()
            : base(Name, Id, 1000, null, true)
        {
        }
    }

    public class WurmAssistantLauncherGateway : GatewayBase
    {
        private static readonly Guid Id = new Guid("39FACB74-7EAC-4C07-AF16-D2EC2F806BCC");
        private const string Name = "AldurCraftWurmAssistantLauncher";
        public WurmAssistantLauncherGateway(string customError = null, int timeout = 1000, bool doNotAutoEnter = false)
            : base(Name, Id, timeout, customError, doNotAutoEnter)
        {
        }
        public WurmAssistantLauncherGateway()
            : base(Name, Id, 1000, null, true)
        {
            
        }
    }

    public class WurmAssistantUnlimitedGateway : GatewayBase
    {
        private static readonly Guid Id = new Guid("652D2233-8E1B-42A8-8B3E-2611532C050B");
        private const string Name = "AldurCraftWurmAssistantUnlimited";
        public WurmAssistantUnlimitedGateway(string customError = null, int timeout = 1000, bool doNotAutoEnter = false)
            : base(Name, Id, timeout, customError, doNotAutoEnter)
        {
        }
        public WurmAssistantUnlimitedGateway()
            : base(Name, Id, 1000, null, true)
        {
        }
    }
}
