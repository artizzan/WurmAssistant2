using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aldurcraft.WurmAssistantMutexBarrier
{
    public class WurmAssistantMutexTimeoutException : Exception
    {
        public WurmAssistantMutexTimeoutException(string message)
            : base(message)
        {

        }
    }

    public class WurmAssistantMutex : IDisposable
    {
        private bool _hasHandle = false;
        private const string UniqueID = "AldurCraftWurmAssistant";
        private const string UniqueToken = "4377383E-C48F-40CF-9E58-6E5D86FD327D";
        private Mutex _mutex;

        private void InitMutex()
        {
            const string appId = UniqueID + UniqueToken;
            string mutexId = string.Format("Global\\{{{0}}}", appId);
            _mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        public WurmAssistantMutex()
        {
            InitMutex();
        }

        public void Enter(int timeOut)
        {
            try
            {
                _hasHandle = _mutex.WaitOne(timeOut <= 0 ? Timeout.Infinite : timeOut, false);

                if (_hasHandle == false)
                    throw new WurmAssistantMutexTimeoutException("Wurm Assistant is already running");
            }
            catch (AbandonedMutexException)
            {
                _hasHandle = true;
            }
        }


        public void Dispose()
        {
            if (_mutex != null)
            {
                if (_hasHandle)
                    _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }
    }
}
