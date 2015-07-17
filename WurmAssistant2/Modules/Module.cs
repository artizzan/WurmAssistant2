using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    /// <summary>
    /// Describes every available module
    /// </summary>
    public static class AssistantModuleDescriptors
    {
        public struct Descriptor
        {
            /// <summary>
            /// Type used to instantiate new module
            /// </summary>
            public readonly Type ModuleType;
            /// <summary>
            /// Name of the module, for presentation
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// Description of the module, for presentation
            /// </summary>
            public readonly string Description;
            /// <summary>
            /// Name of icon file in module dir, for presentation
            /// </summary>
            private readonly string IconFileName;
            /// <summary>
            /// Directory name for module, should mirror module name
            /// </summary>
            private readonly string ModuleDirName;

            //User\Appdata\AldurCraft\WurmAssistant2\Modules\ModuleName\
            /// <summary>
            /// Directory path to persistent data location for this module
            /// </summary>
            public string ModuleDataDir
            {
                get
                {
                    return Path.Combine(AssistantEngine.DataDir, "Modules", ModuleDirName);
                }
            }

            //ProgramDir\Modules\ModuleName\
            /// <summary>
            /// Directory path to assets data location (codebase)
            /// </summary>
            public string ModuleAssetDir
            {
                get
                {
                    string path = Path.Combine(GeneralHelper.PathCombineWithCodeBasePath("Modules"));
                    return Path.Combine(path, ModuleDirName);
                }
            }

            //User\Appdata\AldurCraft\WurmAssistant2\Modules\ModuleName\filename.ext
            /// <summary>
            /// Absolute path to module icon
            /// </summary>
            public string IconPath
            {
                get
                {
                    if (IconFileName == null) return null;
                    return Path.Combine(ModuleAssetDir, IconFileName);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="module">Class inheriting from AssistantModule</param>
            /// <param name="name">Name for presentation</param>
            /// <param name="desc">Description for presentation</param>
            /// <param name="moduleDirName">Directory name for persistent data</param>
            /// <param name="iconFileName">File name of the icon picture</param>
            public Descriptor(Type module, string name, string desc, string moduleDirName, string iconFileName)
            {
                ModuleType = module;
                Name = name;
                Description = desc;
                IconFileName = iconFileName;
                ModuleDirName = moduleDirName;
            }
        }

        static Dictionary<Type, Descriptor> Descriptors = new Dictionary<Type, Descriptor>();

        static AssistantModuleDescriptors()
        {
            //enlist new modules here
            //Descriptors.Add(typeof(ModuleNS.Test.TestModule), new Descriptor(
            //    typeof(ModuleNS.Test.TestModule),
            //    "Test Module",
            //    "Tests the module subsystem",
            //    "ModuleTest",
            //    "icon.png"));
            Descriptors.Add(typeof(ModuleNS.LogSearcher.ModuleLogSearcher), new Descriptor(
                typeof(ModuleNS.LogSearcher.ModuleLogSearcher),
                "Log Searcher",
                "Tired of digging through hundreds or thousands of wurm log files? Why not search from a single window then?",
                "LogSearcher",
                "icon.png"));
            Descriptors.Add(typeof(ModuleNS.Calendar.ModuleCalendar), new Descriptor(
                typeof(ModuleNS.Calendar.ModuleCalendar),
                "Harvest Calendar",
                "Tracks wurm seasonal harvests and will let you know when things are ready for harvesting! And you can modify the season time frames to tune them more to your liking!",
                "Calendar",
                "icon.png"));
            Descriptors.Add(typeof(ModuleNS.Triggers.ModuleTriggers), new Descriptor(
                typeof(ModuleNS.Triggers.ModuleTriggers),
                "Triggers",
                "Ever wished that Wurm could warn you, when something happens in the game? Well, now you can! With this gizmo, you can have a sound and/or popup on practically anything that is saved into Wurm logs.",
                "Triggers",
                "icon.png"));
            Descriptors.Add(typeof(ModuleNS.Timers.ModuleTimers), new Descriptor(
                typeof(ModuleNS.Timers.ModuleTimers),
                "Timers",
                "Use one of the build-in fully-automatic timers for meditation and religion grinds (including support for sermons and all other nuances!). Or just create your own custom timers with a timer creator!",
                "Timers",
                "icon.png"));
            Descriptors.Add(typeof(ModuleNS.Granger.ModuleGranger), new Descriptor(
                typeof(ModuleNS.Granger.ModuleGranger),
                "Granger",
                "Hard time keeping track of dozens/hundreds of your or neighbour horses? Worry no more, Granger is here to save you! All you need to do is smile at the horse, then examine it to have it \"on file\".",
                "Granger",
                "icon.png"));
            Descriptors.Add(typeof(ModuleNS.Backpack.ModuleBackpack), new Descriptor(
                typeof(ModuleNS.Backpack.ModuleBackpack),
                "Backpack",
                "A place for small handy tools, that don't require a dedicated feature.",
                "Backpack",
                "backpack.png"));
        }

        /// <summary>
        /// Throws exc if invalid type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Descriptor GetDescriptor(Type type)
        {
            return Descriptors[type];
        }

        public static Descriptor[] GetAllDescriptors()
        {
            List<Descriptor> allDescriptors = new List<Descriptor>();
            foreach (var item in Descriptors)
            {
                allDescriptors.Add(item.Value);
            }
            return allDescriptors.ToArray();
        }
    }

    public class AssistantModule
    {
        private string _moduleDataDir = null;
        /// <summary>
        /// Data directory to store persistent module data, mirrors descriptor
        /// </summary>
        internal string ModuleDataDir
        {
            get 
            {
                if (_moduleDataDir == null) throw new Exception("field not assigned! be sure to call base.Initialize in Initialize()");
                return _moduleDataDir;
            }
        }

        private string _moduleAssetDir = null;
        /// <summary>
        /// Data directory of module assets, mirrors descriptor
        /// </summary>
        internal string ModuleAssetDir
        {
            get 
            {
                if (_moduleAssetDir == null) throw new Exception("field not assigned! be sure to call base.Initialize in Initialize()");
                return _moduleAssetDir; 
            }
        }

        /// <summary>
        /// Any module initialization routines
        /// </summary>
        public virtual void Initialize()
        {
            //create dirs if not exist
            var descriptor = AssistantModuleDescriptors.GetDescriptor(this.GetType());
            _moduleDataDir = descriptor.ModuleDataDir;

            if (!Directory.Exists(ModuleDataDir))
                Directory.CreateDirectory(ModuleDataDir);

            _moduleAssetDir = descriptor.ModuleAssetDir;

            if (!Directory.Exists(ModuleAssetDir))
                Directory.CreateDirectory(ModuleAssetDir);
        }

        /// <summary>
        /// Open UI of this module
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OpenUI(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called 10 times a second by default
        /// </summary>
        /// <param name="engineSleeping">Not working atm, always false</param>
        public virtual void Update(bool engineSleeping)
        {
        }

        /// <summary>
        /// Cleaning to do, when module is disabled by user
        /// </summary>
        public virtual void Stop()
        {
        }
    }
}
