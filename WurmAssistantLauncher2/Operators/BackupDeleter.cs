using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldurcraft.WurmAssistantMutexes;

namespace WurmAssistantLauncher2.Operators
{
    class BackupDeleter
    {
        // delete zipped backup file
        public void Delete()
        {
            using (var mutex = new WurmAssistantMutex())
            {
                mutex.Enter(1000, "You must close Wurm Assistant before running update");
            }
        }
    }
}
