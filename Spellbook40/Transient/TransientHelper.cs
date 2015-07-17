using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aldurcraft.Spellbook40.WizardTower;

namespace Aldurcraft.Spellbook40.Transient
{
    public static class TransientHelper
    {
        public static void Compensate(Action action, string customError = null, int retries = 3, TimeSpan? retryDelay = null)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                SpellbookLogger.LogInfo(customError ?? "Action " + action.Method + " failed", "TransientHelper", exception);
                if (retries == 1) throw;
                else
                {
                    retries--;
                    if (retryDelay.HasValue)
                    {
                        Thread.Sleep(retryDelay.Value);
                    }
                    Compensate(action, customError, retries, retryDelay);
                }
            }
        }

        public static T Compensate<T>(Func<T> function, string customError = null, int retries = 3, TimeSpan? retryDelay = null)
        {
            try
            {
                return function();
            }
            catch (Exception exception)
            {
                SpellbookLogger.LogInfo(customError ?? "Function " + function.Method + " failed", "TransientHelper", exception);
                if (retries == 1) throw;
                else
                {
                    retries--;
                    if (retryDelay.HasValue)
                    {
                        Thread.Sleep(retryDelay.Value);
                    }
                    return Compensate(function, customError, retries, retryDelay);
                }
            }
        }

        public static async Task<T> CompensateAsync<T>(
           Func<Task<T>> taskFactory, string customError = null, int retries = 3, TimeSpan? delay = null)
        {
            try
            {
                return await taskFactory();
            }
            catch (Exception exception)
            {
                SpellbookLogger.LogInfo(customError ?? "Task failed, retries to go: " + (retries - 1), "TransientHelper", exception);
                if (retries == 1) 
                    throw;
            }
            retries--;
            if (delay.HasValue)
            {
                await TaskEx.Delay(delay.Value);
            }
            return await CompensateAsync(taskFactory, customError, retries, delay);
        }
    }
}
