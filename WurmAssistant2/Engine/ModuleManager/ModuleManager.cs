using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    static class ModuleManager
    {
        const string THIS = "ModuleManager";

        static List<AssistantModule> Modules = new List<AssistantModule>();

        internal static void Init()
        {
            UpdateCachedWurmClientRunning();

            if (AssistantEngine.Settings.Value.ModulesInUse != null)
            {
                string[] savedModules = AssistantEngine.Settings.Value.ModulesInUse.ToArray();
                foreach (string moduleType in savedModules)
                {
                    try
                    {
                        Start(Type.GetType(moduleType, true));
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Could not create module with following typestring: " + (moduleType ?? "NULL"), THIS, _e);
                    }
                }
            }
        }

        internal static void ConfigureModules()
        {
            FormModuleManager ui = new FormModuleManager();
            ui.ShowDialog();
        }

        internal static void StopAll()
        {
            var allmodules = Modules.ToArray();
            foreach (var module in allmodules)
            {
                module.Stop();
                Modules.Remove(module);
            }
        }

        internal static void Stop(Type type)
        {
            List<AssistantModule> modulesToRemove = new List<AssistantModule>();

            foreach (var module in Modules)
                if (module.GetType() == type)
                    modulesToRemove.Add(module);

            foreach (var module in modulesToRemove)
            {
                module.Stop();
                Modules.Remove(module);
            }

            UpdatePersistentList();
            AssistantEngine.Modules.RemoveButton(type);
        }

        internal static void Start(Type type)
        {
            Logger.LogDiag("starting module of type: " + type.ToString());
            AssistantModule module = (AssistantModule)Activator.CreateInstance(type);
            module.Initialize();
            Modules.Add(module);

            UpdatePersistentList();
            AssistantEngine.Modules.AddButton(module);
        }

        static void UpdatePersistentList()
        {
            if (AssistantEngine.Settings.Value.ModulesInUse == null)
                AssistantEngine.Settings.Value.ModulesInUse = new List<string>();

            AssistantEngine.Settings.Value.ModulesInUse.Clear();
            foreach (var module in Modules)
            {
                AssistantEngine.Settings.Value.ModulesInUse.Add(module.GetType().ToString());
            }
            AssistantEngine.Settings.DelayedSave();
        }

        static bool cachedWurmClientRunning = false;
        static int counter = 0;

        internal static void Update()
        {
            if (counter >= 10000)
            {
                UpdateCachedWurmClientRunning();
                counter = 0;
            }

            foreach (var module in Modules)
            {
                module.Update(cachedWurmClientRunning);
            }
        }

        static void UpdateCachedWurmClientRunning()
        {
            if (WurmClient.State.WurmClientRunning != WurmClient.State.EnumWurmClientStatus.NotRunning)
                cachedWurmClientRunning = false;
            else cachedWurmClientRunning = true;
        }

        internal static bool IsModuleRunning(Type type)
        {
            foreach (var module in Modules)
            {
                if (module.GetType() == type) return true;
            }
            return false;
        }

        internal static AssistantModule[] GetActiveModules()
        {
            return Modules.ToArray();
        }
    }
}
