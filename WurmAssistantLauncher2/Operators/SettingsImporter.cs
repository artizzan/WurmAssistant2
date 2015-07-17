using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldurcraft.WurmAssistantMutexes;

namespace WurmAssistantLauncher2.Operators
{
    class SettingsImporter
    {
        // run under WA mutex
        public void Import()
        {
            using (var mutex = new WurmAssistantMutex())
            {
                mutex.Enter(1000, "You must close Wurm Assistant before running update");
            }
        }
        // specify source build type and target build type
        // delete existing settings for target build type
        // copy from source build type

        // subclass for stable and beta
    }
}
