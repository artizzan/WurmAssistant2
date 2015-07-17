using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldurcraft.WurmAssistantMutexes;

namespace WurmAssistantLauncher2.Operators
{
    class BackupCreator
    {
        // run under WA mutex
        public void Create()
        {
            using (var mutex = new WurmAssistantMutex())
            {
                mutex.Enter(1000, "You must close Wurm Assistant before running update");
            }
        }
        // zip build type data dir into backup dir, name the file with high-precision datetime string
    }
}
