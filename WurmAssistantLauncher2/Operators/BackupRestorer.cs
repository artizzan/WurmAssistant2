using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldurcraft.WurmAssistantMutexes;

namespace WurmAssistantLauncher2.Operators
{
    class BackupRestorer
    {
        // run under WA mutex
        public void Restore()
        {
            using (var mutex = new WurmAssistantMutex())
            {
                mutex.Enter(1000, "You must close Wurm Assistant before running update");
            }
        }
        // delete data dir for build type
        // unzip backup into data dir
    }
}
