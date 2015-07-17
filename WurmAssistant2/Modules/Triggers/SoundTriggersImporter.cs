using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Aldurcraft.Utility;
using Aldurcraft.Utility.Notifier;
using Aldurcraft.WurmOnline.WurmLogsManager;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public class SoundTriggersImporter
    {
        private readonly ModuleTriggers _moduleTriggers;
        private string _oldSoundTriggersPath;

        public SoundTriggersImporter(ModuleTriggers moduleTriggers)
        {
            _moduleTriggers = moduleTriggers;
        }

        public bool Execute()
        {
            var oldSoundTriggersPath = Path.Combine(AssistantEngine.DataDir, "Modules", "SoundNotify");
            if (!Directory.Exists(oldSoundTriggersPath))
            {
                return false;
            }

            _oldSoundTriggersPath = oldSoundTriggersPath;
            var newTriggersPath = _moduleTriggers.ModuleDataDir;

            var queueModFile = Path.Combine(oldSoundTriggersPath, "QueueSoundMod.txt");
            if (File.Exists(queueModFile))
            {
                try
                {
                    var file = new FileInfo(queueModFile);
                    file.CopyTo(Path.Combine(newTriggersPath, "QueueSoundMod.txt"), true);
                }
                catch (Exception exception)
                {
                    Logger.LogError("error while trying to copy queue sound modding file: " + queueModFile, this, exception);
                }
            }

            //var activeChars = new List<string>();
            var moduleSettingsFile = Path.Combine(oldSoundTriggersPath, "settings.xml");
            if (File.Exists(moduleSettingsFile))
            {
                try
                {
                    string xml = File.ReadAllText(moduleSettingsFile);
                    xml = GetRidOfThisSillyNamespace(xml);
                    XDocument doc = XDocument.Parse(xml);
                    //activeChars = doc.Root
                    //    .Element("ActiveCharacterNames")
                    //    .Elements("Elements")
                    //    .Select(element => element.Value)
                    //    .ToList();
                    _moduleTriggers.Settings.Value.GlobalVolume =
                        XmlConvert.ToSingle(doc.Root.Element("globalVolume").Value);
                    _moduleTriggers.Settings.Value.GlobalMute =
                        XmlConvert.ToBoolean(doc.Root.Element("GlobalMute").Value);
                }
                catch (Exception exception)
                {
                    Logger.LogError("error while trying to import old global soundnotify settings: " + moduleSettingsFile, this, exception);
                }
            }

            string[] allExistingPlayers = Directory.GetDirectories(oldSoundTriggersPath);
            foreach (var playerDir in allExistingPlayers)
            {
                var playerName = new FileInfo(playerDir).Name;
                var manager = new TriggerManager(_moduleTriggers, playerName, newTriggersPath);

                var playlistFile = Path.Combine(playerDir, "playlist.txt");
                if (File.Exists(playlistFile))
                {
                    var parser = new FileParser(playlistFile);
                    IEnumerable<TriggerData> triggerDatas = parser.GetData();
                    foreach (var triggerData in triggerDatas)
                    {
                        try
                        {
                            ITrigger trigger;
                            if (triggerData.IsRegex || triggerData.Condition.Contains('*'))
                            {
                                trigger = CreateRegexTrigger(triggerData);
                            }
                            else
                            {
                                trigger = CreateSimpleTrigger(triggerData);

                            }
                            manager.Settings.Value.AddTrigger(trigger);
                        }
                        catch (Exception exception)
                        {
                            Logger.LogError("Failed to create new trigger from parsed soundtrigger playlist!", this, exception);
                        }
                    }
                }

                var settingsFile = Path.Combine(playerDir, "settings.xml");
                if (File.Exists(settingsFile))
                {
                    try
                    {
                        string xml = File.ReadAllText(settingsFile);
                        xml = GetRidOfThisSillyNamespace(xml);
                        XDocument doc = XDocument.Parse(xml);
                        double delay = XmlConvert.ToDouble((doc.Root.Element("QueueDefDelay").Value));
                        bool soundEnabled = XmlConvert.ToBoolean(doc.Root.Element("QueueSoundEnabled").Value);
                        var soundNameElem = doc.Root.Element("QueueSoundName");
                        string soundName = soundNameElem == null ? string.Empty : soundNameElem.Value;
                        var trigger = new ActionQueueTrigger
                        {
                            NotificationDelay = delay,
                            Active = soundEnabled
                        };
                        trigger.AddNotifier(new SoundNotifier(soundName));
                        manager.Settings.Value.Muted = XmlConvert.ToBoolean(doc.Root.Element("Muted").Value);
                        manager.Settings.Value.AddTrigger(trigger);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError("error while parsing old soundnotify settings!", this, exception);
                    }
                }
                _moduleTriggers.AddManager(manager);
            }
            return true;
        }

        private string GetRidOfThisSillyNamespace(string xml)
        {
            return Regex.Replace(xml, @"xmlns\=\""(.+)\""", string.Empty);
        }

        RegexTrigger CreateRegexTrigger(TriggerData data)
        {
            var trigger = new RegexTrigger
                          {
                              Condition = data.Condition,
                              Name = data.Condition.Substring(0, data.Condition.Length <= 25 ? data.Condition.Length : 25),
                              Active = data.Active
                          };
            trigger.AddNotifier(new SoundNotifier(data.SoundName));
            foreach (var logType in data.LogTypes)
            {
                trigger.AddLogType(logType);
            }
            return trigger;
        }

        SimpleTrigger CreateSimpleTrigger(TriggerData data)
        {
            var unescapedCond = Regex.Unescape(data.Condition);
            var trigger = new SimpleTrigger
                          {
                              Condition = ConvertRegexToCondText(data.Condition),
                              Name = unescapedCond.Substring(0, unescapedCond.Length <= 25 ? unescapedCond.Length : 25),
                              Active = data.Active
                          };
            trigger.AddNotifier(new SoundNotifier(data.SoundName));
            foreach (var logType in data.LogTypes)
            {
                trigger.AddLogType(logType);
            }
            return trigger;
        }

        string ConvertRegexToCondText(string regexCond)
        {
            regexCond = regexCond.Replace(@".+", @"\*");
            regexCond = Regex.Unescape(regexCond);
            return regexCond;
        }

        public void RenameDir()
        {
            var dir = new DirectoryInfo(_oldSoundTriggersPath);
            dir.MoveTo(Path.Combine(dir.Parent.FullName, "SoundNotify_AlreadyImported"));
        }

        class FileParser
        {
            private readonly string _filePath;

            public FileParser(string filePath)
            {
                _filePath = filePath;
            }

            public IEnumerable<TriggerData> GetData()
            {
                var triggers = new List<TriggerData>();

                using (var sr = new StreamReader(_filePath))
                {
                    var firstline = sr.ReadLine();
                    // skip any potential outdated version of this file to make this simple
                    if (firstline != null && !firstline.Trim().StartsWith("FILEVERSION 3", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.LogInfo("Found outdated playlist file, skipping import: "+firstline);
                        return null;
                    }
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        try
                        {
                            var triggerData = new TriggerData();
                            string[] entries = line.Split(';');
                            triggerData.Active = Convert.ToBoolean(entries[0]);
                            triggerData.SoundName = entries[1];
                            triggerData.Condition = entries[2];
                            triggerData.LogTypes = new List<GameLogTypes>();
                            for (int i = 3; i < entries.Length; i++)
                            {
                                if (entries[i].Contains("s:CustomRegex"))
                                {
                                    triggerData.IsRegex = true;
                                    continue;
                                }
                                try
                                {
                                    triggerData.LogTypes.Add(ParseLogType(entries[i]));
                                }
                                catch (Exception exception)
                                {
                                    Logger.LogInfo("found unrecognized string in special conditions" + entries[i], this, exception);
                                }
                            }

                            triggers.Add(triggerData);
                        }
                        catch (Exception exception)
                        {
                            Logger.LogInfo("error while parsing imported playlist line, skipping to next, line text: " + line, this, exception);
                        }
                    }
                }

                return triggers.ToArray();
            }

            private GameLogTypes ParseLogType(string text)
            {
                return GameLogTypesEX.GetLogTypeForName(text);
            }
        }

        class TriggerData
        {
            public bool Active;
            public string SoundName;
            public string Condition;
            public List<GameLogTypes> LogTypes;
            public bool IsRegex;
        }
    }
}
