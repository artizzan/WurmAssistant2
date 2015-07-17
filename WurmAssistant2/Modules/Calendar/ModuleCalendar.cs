using System;
using System.Collections.Generic;
using System.Ex;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Calendar
{
    public class ModuleCalendar : AssistantModule
    {
        [Obsolete]
        public enum WurmSeasonsEnum { Olive, Oleander, Maple, Camellia, Lavender, Rose, Cherry, Grape, Apple, Lemon }

        public struct WurmSeasonData
        {
            //public WurmSeasonsEnum SeasonEnum;
            public string SeasonName;
            public int DayBegin;
            public int DayEnd;
            public int Length;

            //public WurmSeasonData(WurmSeasonsEnum seasonEnum, string seasonName, int dayBegin, int dayEnd)
            public WurmSeasonData(string seasonName, int dayBegin, int dayEnd)
            {
                //this.SeasonEnum = seasonEnum;
                this.SeasonName = seasonName;
                this.DayBegin = dayBegin;
                this.DayEnd = dayEnd;
                Length = DayEnd - DayBegin;
            }
        }

        static class WurmSeasons
        {
            public static List<WurmSeasonData> Seasons = new List<WurmSeasonData>();
            public const double WurmDaysInYear = 336.0D;
            //static WurmSeasons()
            //{
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Oleander, "Oleander", 85, 91));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Maple, "Maple", 113, 119));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Rose, "Rose", 113, 140));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Rose, "Rose", 141, 147));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Lavender, "Lavender", 113, 119));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Lavender, "Lavender", 141, 147));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Camellia, "Camellia", 113, 126));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Cherry, "Cherry", 176, 196));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Olive, "Olive", 204, 224));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Olive, "Olive", 92, 112));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Grape, "Grape", 225, 252));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Apple, "Apple", 225, 252));
            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Lemon, "Lemon", 288, 308));

            //    // this is not used any more, seasons are loaded from text file

            //    //Seasons.Add(new WurmSeasonData("Oleander", 85, 91));
            //    //Seasons.Add(new WurmSeasonData("Maple", 113, 119));
            //    //Seasons.Add(new WurmSeasonData("Rose", 113, 140));
            //    //Seasons.Add(new WurmSeasonData("Rose", 141, 147));
            //    //Seasons.Add(new WurmSeasonData("Lavender", 113, 119));
            //    //Seasons.Add(new WurmSeasonData("Lavender", 141, 147));
            //    //Seasons.Add(new WurmSeasonData("Camellia", 113, 126));
            //    //Seasons.Add(new WurmSeasonData("Cherry", 176, 196));
            //    //Seasons.Add(new WurmSeasonData("Olive", 204, 224));
            //    //Seasons.Add(new WurmSeasonData("Olive", 92, 112));
            //    //Seasons.Add(new WurmSeasonData("Grape", 225, 252));
            //    //Seasons.Add(new WurmSeasonData("Apple", 225, 252));
            //    //Seasons.Add(new WurmSeasonData("Lemon", 288, 308));

            //    //Seasons.Add(new WurmSeasonData(WurmSeasonsEnum.Test, "Test", 65, 91));
            //}
        }

        public class WurmSeasonOutputItem : IComparable<WurmSeasonOutputItem>
        {
            WurmSeasonData SeasonData;
            int LengthDays;
            bool inSeason = false;
            bool lastInSeasonState = false;
            public bool notifyUser = false;
            TimeSpan RealTimeToSeason;
            TimeSpan WurmTimeToSeason;
            TimeSpan RealTimeToSeasonEnd;
            TimeSpan WurmTimeToSeasonEnd;
            double CompareValue;
            double CompareOffset;

            public WurmSeasonOutputItem(WurmSeasonData wurmSeasonData, double compareOffset, WurmDateTime currentWDT)
            {
                SeasonData = wurmSeasonData;
                LengthDays = SeasonData.DayEnd - SeasonData.DayBegin + 1;
                CompareOffset = compareOffset;
                Update(currentWDT);
            }

            public void Update(WurmDateTime currentWDT)
            {
                lastInSeasonState = inSeason;
                if (currentWDT.DayInYear >= SeasonData.DayBegin
                    && currentWDT.DayInYear <= SeasonData.DayEnd)
                {
                    inSeason = true;
                }
                else inSeason = false;

                if (inSeason)
                {
                    WurmTimeToSeasonEnd = GetTimeToSeasonEnd(SeasonData.DayEnd + 1, currentWDT);
                    RealTimeToSeasonEnd = new TimeSpan(WurmTimeToSeasonEnd.Ticks / 8);
                    CompareValue = (TimeSpan.FromDays(-WurmSeasons.WurmDaysInYear) + WurmTimeToSeasonEnd).TotalSeconds + CompareOffset;
                }
                else
                {
                    WurmTimeToSeason = GetTimeToSeason(SeasonData.DayBegin, currentWDT);
                    RealTimeToSeason = new TimeSpan(WurmTimeToSeason.Ticks / 8);
                    CompareValue = WurmTimeToSeason.TotalSeconds + CompareOffset;
                }

                if (inSeason == true && lastInSeasonState == false)
                    notifyUser = true;
            }

            TimeSpan GetTimeToSeason(int dayBegin, WurmDateTime currentWDT)
            {
                return GetTimeToDay(dayBegin, currentWDT);
            }

            TimeSpan GetTimeToSeasonEnd(int dayEnd, WurmDateTime currentWDT)
            {
                return GetTimeToDay(dayEnd, currentWDT);
            }

            TimeSpan GetTimeToDay(int day, WurmDateTime currentWDT)
            {
                TimeSpan Value = TimeSpan.FromDays(day);
                if (Value < currentWDT.DayAndTimeOfYear)
                {
                    return TimeSpan.FromDays(Value.Days + 336) - currentWDT.DayAndTimeOfYear;
                }
                return Value - currentWDT.DayAndTimeOfYear;
            }

            public string BuildName()
            {
                return SeasonData.SeasonName;
            }

            public string BuildTimeData(bool wurmTime)
            {
                string value;
                if (inSeason)
                {
                    value = "IN SEASON!";
                }
                else
                {
                    if (wurmTime) value = ParseTimeSpanToNiceStringDMS(WurmTimeToSeason);
                    else value = ParseTimeSpanToNiceStringDMS(RealTimeToSeason);
                }
                return value;
            }

            public string BuildLengthData(bool wurmTime)
            {
                string value;
                if (inSeason)
                {
                    if (wurmTime) value = ParseTimeSpanToNiceStringDMS(WurmTimeToSeasonEnd) + "more";
                    else value = ParseTimeSpanToNiceStringDMS(RealTimeToSeasonEnd) + "more";
                }
                else
                {
                    if (wurmTime) value = String.Format("{0} days", LengthDays.ToString());
                    else
                    {
                        TimeSpan ts = TimeSpan.FromDays((double)LengthDays / 8D);
                        value = ParseTimeSpanToNiceStringDMS(ts);
                    }
                }
                return value;
            }

            string ParseTimeSpanToNiceStringDMS(TimeSpan ts, bool noMinutes = false)
            {
                string value = "";
                if (ts.Days > 0)
                {
                    if (ts.Days == 1) value += String.Format("{0} day ", ts.Days);
                    else value += String.Format("{0} days ", ts.Days);
                }
                if (ts.Hours > 0 || noMinutes)
                {
                    if (ts.Hours == 1) value += String.Format("{0} hour ", ts.Hours);
                    else value += String.Format("{0} hours ", ts.Hours);
                }
                if (!noMinutes)
                {
                    if (ts.Minutes == 1) value += String.Format("{0} minute ", ts.Minutes);
                    else value += String.Format("{0} minutes ", ts.Minutes);
                }
                return value;
            }

            public int CompareTo(WurmSeasonOutputItem dtlm)
            {
                return this.CompareValue.CompareTo(dtlm.CompareValue);
            }

            public bool ShouldNotifyUser()
            {
                return notifyUser;
            }

            public bool IsItemTracked(string[] trackedSeasons)
            {
                return trackedSeasons.Contains(SeasonData.SeasonName, StringComparer.InvariantCultureIgnoreCase);
            }

            public string GetSeasonName()
            {
                return SeasonData.SeasonName;
            }

            public DateTime GetSeasonEndDate()
            {
                return DateTime.Now + RealTimeToSeasonEnd;
            }

            public void ResetInSeasonFlag()
            {
                lastInSeasonState = false;
                inSeason = false;
            }

            public void UserNotified()
            {
                notifyUser = false;
            }

            public bool IsItemInSeason()
            {
                return inSeason;
            }
        }

        [DataContract]
        internal class CalendarSettings
        {
            [DataMember]
            public bool UseWurmTimeForDisplay;
            [DataMember]
            public bool SoundWarning;
            [DataMember]
            public string SoundName;
            [DataMember]
            public bool PopupWarning;
            [DataMember]
            [Obsolete]
            public Dictionary<string, WurmSeasonsEnum> TrackedSeasons;
            [DataMember]
            public string[] TrackedSeasons2;
            [DataMember]
            public string ServerName;
            [DataMember]
            public System.Drawing.Size MainWindowSize;

            [OnDeserializing]
            public void Init(StreamingContext context)
            {
                UseWurmTimeForDisplay = false;
                SoundWarning = false;
                SoundName = "";
                PopupWarning = false;
                TrackedSeasons = new Dictionary<string,WurmSeasonsEnum>();
                TrackedSeasons2 = new string[0];
                ServerName = "";
                MainWindowSize = new System.Drawing.Size(487, 414);
            }

            public CalendarSettings()
            {
                Init(new StreamingContext());
            }
        }

        internal PersistentObject<CalendarSettings> Settings;

        List<WurmSeasonOutputItem> WurmSeasonOutput = new List<WurmSeasonOutputItem>();
        FormCalendar CalendarUI;

        const string MOD_FILE_NAME = "SeasonDataV2.txt";
        public string ModFilePath { get; private set; }

        internal WurmDateTime cachedWDT;
        bool isInitialized = false;
        public override void Initialize()
        {
            base.Initialize();

            Settings = new PersistentObject<CalendarSettings>(new CalendarSettings());
            Settings.FilePath = Path.Combine(this.ModuleDataDir, "settings.xml");
            Settings.Load();

            // migrate TrackedSeasons to TrackedSeasons2
            if (Settings.Value.TrackedSeasons.Any())
            {
                Settings.Value.TrackedSeasons2 = Settings.Value.TrackedSeasons.Keys.ToArray();
                Settings.Value.TrackedSeasons = new Dictionary<string, WurmSeasonsEnum>();
                Settings.Save();
            }

            CalendarUI = new FormCalendar(this);
            CalendarUI.UpdateTrackedSeasonsList(Settings.Value.TrackedSeasons2);

            ModFilePath = Path.Combine(this.ModuleDataDir, MOD_FILE_NAME);
            InitSeasonData();

            InitCachedWDT();
        }

        internal void InitSeasonData()
        {
            string modFilePath = this.ModFilePath;
            string defaultFilePath = Path.Combine(this.ModuleAssetDir, MOD_FILE_NAME);

            if (!File.Exists(modFilePath))
            {
                try
                {
                    File.Copy(defaultFilePath, modFilePath);
                }
                catch (Exception _e)
                {
                    Logger.LogError("problem creating season mod file", this, _e);
                }
            }

            try
            {
                if (!ReadSeasonsFromFile(modFilePath))
                    ReadSeasonsFromFile(defaultFilePath, ignoreDefaultListTag: true);
            }
            catch (Exception _e)
            {
                Logger.LogError("failed to read seasons from files, reading hardcoded list instead", this, _e);
            }

            double compareOffset = 0D;
            foreach (WurmSeasonData seasondata in WurmSeasons.Seasons)
            {
                WurmSeasonOutput.Add(new WurmSeasonOutputItem(seasondata, compareOffset, cachedWDT));
                compareOffset += 0.1D;
            }
        }

        bool ReadSeasonsFromFile(string filepath, bool ignoreDefaultListTag = false)
        {
            List<WurmSeasonData> seasons = new List<WurmSeasonData>();
            using (StreamReader sr = new StreamReader(filepath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!ignoreDefaultListTag && line.StartsWith("USE_DEFAULT_LIST", StringComparison.Ordinal))
                    {
                        return false;
                    }
                    if (!line.StartsWith("#", StringComparison.Ordinal))
                    {
                        Match match = Regex.Match(line, @".*?(\w+).*,.*?(\d+).*,.*?(\d+).*?");
                        if (match.Groups.Count == 4)
                        {
                            try
                            {
                                seasons.Add(new WurmSeasonData(
                                    match.Groups[1].Value,
                                    Int32.Parse(match.Groups[2].Value),
                                    Int32.Parse(match.Groups[3].Value)));
                            }
                            catch (Exception _e)
                            {
                                Logger.LogError("failed to add a season from this line: " + (line ?? "NULL"), this, _e);
                            }
                        }
                    }
                }
            }
            WurmSeasons.Seasons = seasons;
            return true;
        }

        internal async Task InitCachedWDT()
        {
            if (!isInitialized && !String.IsNullOrEmpty(Settings.Value.ServerName))
            {
                try
                {
                    cachedWDT = await WurmServer.GetWurmDateTimeAsync(Settings.Value.ServerName);
                    isInitialized = true;
                }
                catch (WurmServer.NoDataException)
                {
                    Logger.LogError("maybe invalid server name: " + (Settings.Value.ServerName ?? "NULL"), this);
                }
            }
        }

        public override void Update(bool engineSleeping)
        {
            Settings.Update();
            if (isInitialized && WurmServer.TryGetWurmDateTime(Settings.Value.ServerName, out cachedWDT))
            {
                UpdateOutputList();
            }
        }

        public override void OpenUI(object sender, EventArgs e)
        {
            CalendarUI.ShowThisDarnWindowDammitEx();
        }

        public override void Stop()
        {
            Settings.Save();
            if (CalendarUI != null) CalendarUI.Close();
        }

        List<KeyValuePair<string, DateTime>> PopupQueue = new List<KeyValuePair<string, DateTime>>();
        bool popupScheduled = false;

        public void UpdateOutputList()
        {
            if (Settings.Value.PopupWarning || Settings.Value.SoundWarning || CalendarUI.Visible)
            {
                foreach (WurmSeasonOutputItem item in WurmSeasonOutput)
                {
                    item.Update(cachedWDT);
                    if (item.ShouldNotifyUser())
                    {
                        if (item.IsItemTracked(Settings.Value.TrackedSeasons2))
                        {
                            if (Settings.Value.SoundWarning)
                            {
                                TriggerSoundWarning();
                                item.UserNotified();
                            }
                            if (Settings.Value.PopupWarning)
                            {
                                PopupQueue.Add(new KeyValuePair<string, DateTime>(item.GetSeasonName(), item.GetSeasonEndDate()));
                                popupScheduled = true;
                                item.UserNotified();
                            }
                        }
                    }
                }
                if (CalendarUI.Visible)
                {
                    WurmSeasonOutput.Sort();
                    CalendarUI.UpdateSeasonOutput(WurmSeasonOutput, Settings.Value.UseWurmTimeForDisplay);
                }
                if (popupScheduled)
                {
                    string output = "";
                    foreach (var item in PopupQueue)
                    {
                        //var endsAt = item.Value.ToString("dd-MM-yyyy hh:mm");
                        var endsIn = item.Value - DateTime.Now;
                        output += item.Key + " is now in season. Season ends in " + endsIn.FormatForConciseDisplayEx() + "\n";
                    }
                    TriggerPopupWarning(output);
                    popupScheduled = false;
                    PopupQueue.Clear();
                }
            }
        }

        public void ChooseTrackedSeasons()
        {
            var trackedSeasons = Settings.Value.TrackedSeasons2;

            FormChooseSeasons seasonsDialog = new FormChooseSeasons(
                WurmSeasons.Seasons.Select(x => x.SeasonName).Distinct().ToArray(), trackedSeasons);
            seasonsDialog.ShowDialog();
            var newTrackedSeasons = new List<string>();
            foreach (var item in seasonsDialog.checkedListBox1.CheckedItems)
            {
                newTrackedSeasons.Add(item.ToString());
            }
            Settings.Value.TrackedSeasons2 = newTrackedSeasons.ToArray();
            Settings.DelayedSave();
            CalendarUI.UpdateTrackedSeasonsList(Settings.Value.TrackedSeasons2);
        }

        void TriggerSoundWarning()
        {
            SoundBank.PlaySound(Settings.Value.SoundName);
        }

        void TriggerPopupWarning(string text)
        {
            Popup.Schedule("Wurm Season Notify", text);
        }

        //deprec?
        internal void OnEngineWakeUp()
        {
            foreach (WurmSeasonOutputItem item in WurmSeasonOutput)
            {
                item.ResetInSeasonFlag();
            }
        }
    }
}
