using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    /// <summary>
    /// Arrays used to help parsing log messages
    /// </summary>
    static public class LogQueueParseHelper
    {
        // queue sound parsing helper arrays

        // if an event starts with these, it informs Sound Notify that action has started
        static public List<string> ActionStart = new List<string>
        {
            "You start", 
            "You continue to", 
            "You throw out the line and start fishing.",
            "You start to string"
        };

        // this is analogous to ActionStart, 
        // with the exception that it searches for this sentence in entire event line
        // and not just at the beginning
        static public List<string> ActionStart_contains = new List<string> { };

        // if an event starts with these, it informs Sound Notify this is actually not start of an action
        static public List<string> ActionFalstart = new List<string> { "You start dragging", "You start leading" };

        // unused
        //static public string[] QueueAdd_contains_and = { "After", "you will start" };

        // if an event starts with these, it informs Sound Notify that action has finished
        static public List<string> ActionEnd = new List<string>  { "You improve", 
                                              "You continue on",
                                              "You nail",
                                              "You dig",
                                              "You attach",
                                              "You repair", 
                                              "You fail", 
                                              "You stop",
                                              "You mine some", 
                                              "You damage",
                                              "You harvest",
                                              "You plant", 
                                              "You pull", 
                                              "You push", 
                                              "You almost made it", 
                                              "You create", 
                                              "You will want to",
                                              "You need to temper",
                                              "You finish",
                                              "Roof completed",
                                              "Metal needs to be glowing hot while smithing",
                                              "You continue to build on a floor",
                                              "You have now tended",
                                              "You sow",
                                              "You turn",
                                              "You realize you harvested",
                                              "The ground is cultivated",
                                              "You gather",
                                              "You cut down",
                                              "You cut a sprout",
                                              "You must use a",
                                              "You notice some notches",
                                              "The dirt is packed",
                                              "You lay the foundation",
                                              "You bandage",
                                              "You prune", 
                                              "You make a lot of errors", 
                                              "You try to bandage",
                                              "Sadly, the sprout does not",
                                              "You chip away",
                                              "The last parts of the",
                                              "The ground is paved",
                                              "The ground is no longer paved",
                                              "You hit rock",
                                              "You must rest",
                                              "You use some of the dirt in one corner",
                                              "You proudly close the gift",
                                              "You add a stone and some mortar",
                                              "The field is now nicely groomed",
                                              "You find", 
                                              "You let your love change the area",
                                              "It is now flat",
                                              "You catch a ",
                                              "You frown as you fail to improve the power.", 
                                              "The field is now tended.", 
                                              "The crops are growing nicely", 
                                              "The field looks better after your tending.",
                                              "The ore will run out soon.", 
                                              "You succeed", 
                                              "You fill the small bucket",
                                              "The field is tended.", 
                                              "The line snaps", 
                                              "You string the", 
                                              "You seem to catch something but it escapes", 
                                              "You fill the gem with the power of your determination", 
                                              "The gem is of too low quality to store any power and is damaged a bit"};

        // this is analogous to ActionEnd, 
        // with the exception that it searches for this sentence in entire event line
        // and not just at the beginning
        static public List<string> ActionEnd_contains = new List<string> {   "has some irregularities", 
                                                        "has some dents", //bug: fires for examining lamp //fixed via ActionFalsEndDueToLastAction
                                                        "needs to be sharpened",
                                                        "is finished",
                                                        "will probably give birth",
                                                        "shys away and interrupts",
                                                        "falls apart with a crash",
                                                        "feed you more.",
                                                        "is already well tended.", 
                                                        "too little material", 
                                                        "You pick some flowers."};

        // if an event starts with these, it informs Sound Notify this is actually not an end of action
        static public List<string> ActionFalsEnd = new List<string> { "You stop dragging", 
                                                   "A forge made from", 
                                                   "It is made from", 
                                                   "A small, very rudimentary", 
                                                   "You stop leading", 
                                                   "You fail to produce", 
                                                   "A tool for",
                                                   "The roof is finished already",
                                                   "A high guard tower",
                                                   "You create a box side",
                                                   "You create another box side",
                                                   "You create yet another box side",
                                                   "You create the last box side",
                                                   "You create a bottom",
                                                   "You create a top"};

        // this is analogous to ActionFalsEnd, with the exception that
        // these will be checked against previous log line parsed by Sound Notify
        static public List<string> ActionFalsEndDueToLastAction = new List<string>
        {
            "A decorative lamp", 
            "A high guard tower",
            "that you dispose of."
        };

        public const string LevelingModeStart = "You start to level the ground";
        static public List<string> LevelingEnd = new List<string>
        {
            "You stop leveling", 
            "You can only level tiles that you are adjacent to",
            "You need to be standing on flat ground",
            "It is now flat",
            "You must rest",
            "Done.",
            "You are not strong enough to carry one more dirt pile",
            "You assemble some dirt from a corner",
            "The ground is flat here."
        };

        // unused
        //static public string[] QueueReset = { "You ride", "You mount" };

        enum ParseMode
        {
            None,
            ACTION_START,
            ACTION_START_CONTAINS,
            ACTION_FALSTART,
            ACTION_END,
            ACTION_END_CONTAINS,
            ACTION_FALSEND,
            ACTION_FALSEND_LAST_EVENT
        }

        static string ModFilePath;

        public static void Build(string modFilePath, string defaultFilePath)
        {
            try
            {
                ModFilePath = modFilePath;
                if (!File.Exists(modFilePath))
                {
                    File.Copy(defaultFilePath, modFilePath);
                }

                using (StreamReader sr = new StreamReader(modFilePath))
                {
                    string line;
                    ParseMode parseMode = ParseMode.None;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!line.StartsWith("#", StringComparison.Ordinal))
                        {
                            if (line.StartsWith("+", StringComparison.Ordinal)) parseMode = GetParsingMode(line);
                            else
                            {
                                if (line.StartsWith("=", StringComparison.Ordinal))
                                {
                                    RemoveCondition(line, parseMode);
                                }
                                else if (line.Trim() != string.Empty)
                                {
                                    AddCondition(line, parseMode);
                                }
                                else
                                {
                                    //ignore blank lines
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("problem while parsing queue sound mod file, operation aborted", "LogQueueParseHelper", _e);
            }
        }

        static ParseMode GetParsingMode(string line)
        {
            if (line.Contains("ACTION_START_CONTAINS")) return ParseMode.ACTION_START_CONTAINS;
            else if (line.Contains("ACTION_START")) return ParseMode.ACTION_START;
            else if (line.Contains("ACTION_FALSTART")) return ParseMode.ACTION_FALSTART;
            else if (line.Contains("ACTION_END_CONTAINS")) return ParseMode.ACTION_END_CONTAINS;
            else if (line.Contains("ACTION_END")) return ParseMode.ACTION_END;
            else if (line.Contains("ACTION_FALSEND_LAST_EVENT")) return ParseMode.ACTION_FALSEND_LAST_EVENT;
            else if (line.Contains("ACTION_FALSEND")) return ParseMode.ACTION_FALSEND;

            else return ParseMode.None;
        }

        static void AddCondition(string line, ParseMode parseMode)
        {
            ModifyCondition(line, parseMode, false);
        }

        static void RemoveCondition(string line, ParseMode parseMode)
        {
            ModifyCondition(line, parseMode, true);
        }

        static void ModifyCondition(string line, ParseMode parseMode, bool disableMode)
        {
            switch (parseMode)
            {
                case ParseMode.ACTION_START:
                    ModifyList(line, ActionStart, disableMode); break;
                case ParseMode.ACTION_START_CONTAINS:
                    ModifyList(line, ActionStart_contains, disableMode); break;
                case ParseMode.ACTION_FALSTART:
                    ModifyList(line, ActionFalstart, disableMode); break;
                case ParseMode.ACTION_END:
                    ModifyList(line, ActionEnd, disableMode); break;
                case ParseMode.ACTION_END_CONTAINS:
                    ModifyList(line, ActionEnd_contains, disableMode); break;
                case ParseMode.ACTION_FALSEND:
                    ModifyList(line, ActionFalsEnd, disableMode); break;
                case ParseMode.ACTION_FALSEND_LAST_EVENT:
                    ModifyList(line, ActionFalsEndDueToLastAction, disableMode); break;
                default:
                    break;
            }
        }

        static void ModifyList(string line, List<string> list, bool disableMode)
        {
            if (!disableMode)
            {
                list.Add(line);
            }
            else
            {
                try
                {
                    line = Regex.Match(line, @"=DISABLE(.+)").Groups[1].Value.Trim();
                    if (line != string.Empty)
                    {
                        list.Remove(line);
                    }
                }
                catch (Exception _e)
                {
                    Logger.LogError("problem while parsing mod line, discarded: " + (line ?? "NULL"), "LogQueueParseHelper", _e);
                }
            }
        }

        public static void EditModFile()
        {
            try
            {
                System.Diagnostics.Process.Start(ModFilePath);
            }
            catch (Exception _e)
            {
                Logger.LogError("could not open mod file " + (ModFilePath ?? "NULL"), "LogQueueParseHelper", _e);
            }
        }
    }
}
