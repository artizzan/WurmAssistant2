using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WurmAssistantLauncher
{
    [DataContract]
    public class LauncherSettings
    {
        public LauncherSettings()
        {
            Init();
        }

        [OnDeserializing]
        void OnDes(StreamingContext context)
        {
            Init();
        }

        private void Init()
        {
        }
    }
}
