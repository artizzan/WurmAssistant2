using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmLogsManager.Searcher
{
    public partial class FormLogSearcher : Form
    {
        LogSearchData HandleToCurrentLogSearchData;
        LogSearchManager LogSearchMan;

        internal FormLogSearcher(LogSearchManager logsearchman)
        {
            InitializeComponent();
            this.LogSearchMan = logsearchman;

            //init all search boxes and choose default values for them
            dateTimePickerTimeFrom.Value = DateTime.Now;
            dateTimePickerTimeTo.Value = DateTime.Now;

            comboBoxPlayerName.Items.AddRange(WurmState.WurmClient.WurmPaths.GetAllPlayersNames());
            //comboBoxPlayerName.Text = parentModule.GetCurrentPlayer(); //TODO
            comboBoxLogType.Items.AddRange(GameLogTypesEX.GetAllNames());
            comboBoxLogType.Text = GameLogTypesEX.GetNameForLogType(GameLogTypes.Event);
            comboBoxSearchType.Items.AddRange(SearchTypesEX.GetAllNames());
            comboBoxSearchType.Text = SearchTypesEX.GetNameForSearchType(SearchTypes.RegexEscapedCaseIns);
        }

        internal delegate void DisplaySearchResultsCallback(LogSearchData logSearchData);

        internal void DisplaySearchResults(LogSearchData logSearchData)
        {
            buttonCommitSearch.Text = "Loading results...";
            this.Refresh();
            if (!logSearchData.StopSearching)
            {
                labelAllResults.Text = "All results: " + logSearchData.SearchResults.Count;

                richTextBoxAllLines.Visible = false;
                listBoxAllResults.Visible = false;
                labelWorking.Show();
                this.Refresh();

                richTextBoxAllLines.Clear();
                listBoxAllResults.Items.Clear();
                richTextBoxAllLines.Lines = logSearchData.AllLinesArray;
                bool tooManyToProcess = false;
                bool tooManyToHighlight = false;
                if (logSearchData.SearchResults.Count > 20000) tooManyToProcess = true;
                if (logSearchData.SearchResults.Count > 5000) tooManyToHighlight = true;
                if (!tooManyToProcess)
                {
                    foreach (LogSearchData.SingleSearchMatch searchmatch in logSearchData.SearchResults)
                    {
                        if (!logSearchData.StopSearching) //&& !ParentModule.isAppClosing) //avoid app exit exceptions due to Application.DoEvents
                        {
                            string matchDesc = "";
                            matchDesc += searchmatch.MatchDate;
                            if (!tooManyToHighlight)
                            {
                                richTextBoxAllLines.Select((int)searchmatch.Begin, (int)searchmatch.Length);
                                richTextBoxAllLines.SelectionBackColor = Color.LightBlue;
                            }
                            listBoxAllResults.Items.Add(matchDesc);
                            try { Application.DoEvents(); } //improve interface responsiveness and allow cancelling
                            catch { Debug.WriteLine("ERROR in appclosing workaround"); }
                        }
                    }
                }
                else
                {
                    listBoxAllResults.Items.Add("too many matches");
                    listBoxAllResults.Items.Add("narrow the search");
                }

                //if (!ParentModule.isAppClosing) //avoid app exit exceptions due to Application.DoEvents
                {
                    try
                    {
                        labelWorking.Hide();
                        buttonCancelSearch.Visible = false;
                        listBoxAllResults.Visible = true;
                        richTextBoxAllLines.Visible = true;
                        richTextBoxAllLines.Select(0, 0);
                        richTextBoxAllLines.ScrollToCaret();
                    }
                    catch { Debug.WriteLine("ERROR in appclosing workaround"); }
                }
            }
            //if (!ParentModule.isAppClosing) //avoid app exit exceptions due to Application.DoEvents
            {
                try
                {
                    buttonCommitSearch.Text = "Search";
                    buttonCommitSearch.Enabled = true;
                }
                catch { Debug.WriteLine("ERROR in appclosing workaround"); }
            }
        }

        void PerformSearch()
        {
            try
            {
                // create new search data container
                LogSearchData logSearchData = new LogSearchData();
                HandleToCurrentLogSearchData = logSearchData;
                // enable cancel button
                buttonCancelSearch.Visible = true;
                // clear old results
                richTextBoxAllLines.Clear();
                listBoxAllResults.Items.Clear();
                // write container with return address
                logSearchData.CallerControl = this;
                // adjust timeto if necessary (monitor)
                dateTimePickerTimeFrom.Value = new DateTime(
                    dateTimePickerTimeFrom.Value.Year,
                    dateTimePickerTimeFrom.Value.Month,
                    dateTimePickerTimeFrom.Value.Day,
                    0, 0, 0);
                dateTimePickerTimeTo.Value = new DateTime(
                    dateTimePickerTimeTo.Value.Year,
                    dateTimePickerTimeTo.Value.Month,
                    dateTimePickerTimeTo.Value.Day,
                    23, 59, 59);
                Debug.WriteLine("Search begin");
                Debug.WriteLine("from " + dateTimePickerTimeFrom.Value + " to " + dateTimePickerTimeTo.Value);
                // drop all search criteria into the container
                logSearchData.SetSearchCriteria(
                    comboBoxPlayerName.Text,
                    GameLogTypesEX.GetLogTypeForName(comboBoxLogType.Text),
                    dateTimePickerTimeFrom.Value,
                    dateTimePickerTimeTo.Value,
                    textBoxSearchKey.Text,
                    SearchTypesEX.GetSearchTypeForName(comboBoxSearchType.Text)
                    );
                // add pm player if applies
                if (comboBoxLogType.Text == GameLogTypesEX.GetNameForLogType(GameLogTypes.PM))
                {
                    logSearchData.SetPM_Player(textBoxPM.Text);
                }
                // pass the container further
                DoSearch(logSearchData);
            }
            catch (Exception _e)
            {
                MessageBox.Show("Error while starting search, this is a bug please report!");
                Logger.LogError("LogSearcher: Error while performing search", this, _e);
            }
        }

        internal void RestoreFromMin()
        {
            if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
        }

        private void FormLogSearcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void buttonCommitSearch_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        internal enum SearchStatus { Busy, Searching, None }

        internal void UpdateUIAboutScheduledSearch(SearchStatus searchStatus)
        {
            if (searchStatus == SearchStatus.Busy)
            {
                buttonCommitSearch.Enabled = false;
                buttonCommitSearch.Text = "Busy, please wait...";
            }
            else if (searchStatus == SearchStatus.Searching)
            {
                buttonCommitSearch.Enabled = false;
                buttonCommitSearch.Text = "Searching...";
            }
        }

        private void buttonCancelSearch_Click(object sender, EventArgs e)
        {
            if (HandleToCurrentLogSearchData != null)
            {
                HandleToCurrentLogSearchData.StopSearching = true;
            }
        }

        private void listBoxAllResults_Click(object sender, EventArgs e)
        {
            try
            {
                LogSearchData.SingleSearchMatch matchdata = HandleToCurrentLogSearchData.SearchResults[listBoxAllResults.SelectedIndex];
                richTextBoxAllLines.Select((int)matchdata.Begin, (int)matchdata.Length);
                richTextBoxAllLines.Focus();
            }
            catch
            {

            }
        }

        private void textBoxSearchKey_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void textBoxSearchKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }

        }

        private void dateTimePickerTimeFrom_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void dateTimePickerTimeTo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void comboBoxLogType_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void comboBoxPlayerName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void comboBoxSearchType_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void textBoxPM_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ProcessTabKey(true);
            }
        }

        private void comboBoxLogType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLogType.Text == GameLogTypesEX.GetNameForLogType(GameLogTypes.PM))
            {
                labelPM.Visible = true;
                textBoxPM.Visible = true;
                textBoxPM.TabStop = true;
            }
            else
            {
                labelPM.Visible = false;
                textBoxPM.Visible = false;
                textBoxPM.TabStop = false;
            }
        }

        private void buttonForceRecache_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show("This may take a while, continue?", "Confirm recache", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Asterisk) == System.Windows.Forms.DialogResult.OK)
            {
                TryExecuteForceRecache();
            }
        }

        private object _tryRecacheLocker = new object();
        private void TryExecuteForceRecache()
        {
            // exec only when no recache scheduled or running
            lock (_tryRecacheLocker)
            {
                Logger.LogInfo("Attempting to start log searcher recache");
                if (buttonForceRecache.Enabled)
                {
                    Logger.LogInfo("Starting log searcher recache");
                    buttonForceRecache.Text = "Working...";
                    buttonForceRecache.Enabled = false;
                    buttonCommitSearch.Enabled = false;
                    ForceRecache();
                }
                else
                {
                    Logger.LogInfo("Log searcher recache is already scheduled or running");
                }
            }
        }

        public void TryForceRecache()
        {
            TryExecuteForceRecache();
        }

        internal delegate void OnRecacheCompleteCallback();

        internal void InvokeOnRecacheComplete()
        {
            buttonCommitSearch.Enabled = true;
            buttonForceRecache.Text = "Fix Broken Searcher Database";
            buttonForceRecache.Enabled = true;
        }

        int CacheUpdateCounter = 1000;
        bool isInternalRecacheScheduled = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            LogSearchMan.UpdateLoop();

            CacheUpdateCounter -= timer1.Interval;
            if (CacheUpdateCounter < 0) CacheUpdateCounter = 0;

            if (CacheUpdateCounter == 0)
            {
                if (LogSearchMan.incorrectLogsDir)
                {
                    isInternalRecacheScheduled = true;
                    Logger.__WriteLine("LogSearcher: Attempting to recache log data due to previous errors");
                    CacheUpdateCounter = 10000;
                }
                else
                {
                    if (LogSearchMan.UpdateCache())
                    {
                        CacheUpdateCounter = 30000;
                    }
                }
            }

            //if (LogSearchMan != null)
            //    LogSearchMan.UpdateSearchQueueHandler();

            //100ms
            // if scheduled, try to perform every tick
            if (isSearchScheduled)
            {
                DoSearch(ScheduledLogSearchData);
            }

            if (isRecacheScheduled)
            {
                ForceRecache();
            }

            if (isInternalRecacheScheduled)
            {
                ForceRecache(true);
            }
        }

        LogSearchData ScheduledLogSearchData;
        internal bool isSearchScheduled = false;

        internal void DoSearch(LogSearchData logsearchdata)
        {
            LogSearchMan.PerformSearchAsync(logsearchdata);
            ScheduledLogSearchData = logsearchdata;
            UpdateUIAboutScheduledSearch(SearchStatus.Searching);
            isSearchScheduled = false;
        }

        internal bool isRecacheScheduled = false;

        internal void ForceRecache(bool internalRecache = false)
        {
            if (LogSearchMan != null && LogSearchMan.ForceRecache(this, internalRecache))
            {
                isRecacheScheduled = false;
                if (internalRecache) isInternalRecacheScheduled = false;
            }
            else
            {
                isRecacheScheduled = true;
                if (internalRecache) isInternalRecacheScheduled = true;
            }
        }

        internal void StartUpdateLoop()
        {
            timer1.Enabled = true;
        }

        internal void StopUpdateLoop()
        {
            timer1.Enabled = false;
        }

        private void FormLogSearcher_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(this.textBoxSearchKey, "For \"Match\" search use * to indicate 1 or more of any characters");
            toolTip1.SetToolTip(this.comboBoxSearchType, "Match simply searches for the keyword,\r\nRegex searches for anything matching regex pattern");
            toolTip1.SetToolTip(this.buttonForceRecache, "This button will fix searcher, if for example\r\nyou have moved/reinstalled Wurm to another directory\r\nor deleted some player folders");
            toolTip1.SetToolTip(this.textBoxPM, "Leave this empty to see all PM's sorted by day by recipient,\r\nor set a single name to look for");
        }

        private void textBoxSearchKey_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBoxAllLines_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
            }
            catch (Exception _e)
            {
                Logger.LogError("could not open link", this, _e);
            }
        }
    }
}
