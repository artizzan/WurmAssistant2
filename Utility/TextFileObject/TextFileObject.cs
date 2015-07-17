using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// Somewhat useful wrapper for reading text files "on the fly" as they are being written. 
    /// Uses polling method for checking file updates.
    /// Maintaining for some WurmAssistant legacy code.
    /// </summary>
    public class TextFileObject
    {
        // address of the wraped file
        string fileAdress;

        // current numer of lines in this file
        int LinesMaxIndex = 0;

        // list of all lines in this file
        List<string> Lines = new List<string>();

        // primitive iterator for getnextline method
        int currentReadIndex = 0;

        // used to determine if to read new lines from file
        long lastSizeOfThisFile = 0;

        // should instance be allowed to update its Lines list
        bool doUpdate = false;

        // should instance do full update regardless of anything
        bool alwaysUpdate = false;

        // does file exist on drive
        bool fileExists = false;
        /// <summary>
        /// Does the monitored file exist?
        /// </summary>
        public bool FileExists
        {
            get { return fileExists; }
        }

        // whether this file should be handled as read only
        bool isReadOnly = false;

        // whether this file is only appended to (like log files)
        bool isGrowingFileOnly = false;

        // if display log when file found
        bool WrapNotify = true;

        bool LogReaderMode = false;

        bool firstUpdate = true;

        bool NewLineOnCRLF = false;

        FileInfo currentFileInfo;

        /// <summary>
        /// Constructs new text file wrapper.
        /// </summary>
        /// <param name="fileAdress">full path to the file</param>
        /// <param name="doUpdate">true to enable reading from this file</param>
        /// <param name="alwaysUpdate">true if update should happen regardless of file size changes</param>
        /// <param name="createIfNotExists">true if file should be created if not exists</param>
        /// <param name="isReadOnly">true if writing to file should be disabled, also disables creating file</param>
        /// <param name="isGrowingOnly">true if text is only appended to this file (more efficient file reading)</param>
        /// <param name="notifyWhenWrapped">true if should log when file is aquired</param>
        public TextFileObject(string fileAdress, bool doUpdate, bool alwaysUpdate, bool createIfNotExists, bool isReadOnly, bool isGrowingOnly, bool notifyWhenWrapped = true, bool logReaderMode = false, bool newLineOnlyOnCRLF = false)
        {
            this.doUpdate = doUpdate;
            this.fileAdress = fileAdress;
            this.alwaysUpdate = alwaysUpdate;
            this.isReadOnly = isReadOnly;
            this.isGrowingFileOnly = isGrowingOnly;
            this.WrapNotify = notifyWhenWrapped;
            this.LogReaderMode = logReaderMode;
            this.NewLineOnCRLF = newLineOnlyOnCRLF;

            this.CheckIfFileExists();

            this.currentFileInfo = new FileInfo(fileAdress);

            if (this.fileExists == false)
            {
                if (createIfNotExists && !isReadOnly) this.CreateIfNotExists();
            }

            this.Update();
        }

        private void CreateIfNotExists()
        {

            try
            {
                if (!File.Exists(fileAdress))
                {
                    File.WriteAllText(fileAdress, "");
                    fileExists = true;
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("file does not exist, failed to create "+fileAdress, "TextFileObject", _e);
            }
        }

        /// <summary>
        /// Reads the file and updates List of cached text lines
        /// </summary>
        public void Update()
        {
            if (doUpdate)
            {
                if (!fileExists)
                {
                    CheckIfFileExists();
                }
                
                if (fileExists)
                {
                    if (CheckForFileChanged())
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(fileAdress, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                if (isGrowingFileOnly) fs.Seek(lastSizeOfThisFile, SeekOrigin.Begin);
                                else Lines.Clear();

                                if (LogReaderMode)
                                {
                                    Lines.Clear();
                                    resetReadPos();
                                    if (firstUpdate)
                                    {
                                        fs.Seek(0, SeekOrigin.End);
                                        firstUpdate = false;
                                    }
                                }

                                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                                {
                                    if (NewLineOnCRLF)
                                    {
                                        string thefile = sr.ReadToEnd();
                                        string[] strlist = thefile.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                        Lines.AddRange(strlist);
                                    }
                                    else
                                    {
                                        while (!sr.EndOfStream)
                                        {
                                            Lines.Add(sr.ReadLine());
                                            //Debug.WriteLine(DateTime.Now + " line added to list at TextFileObject update");
                                        }
                                    }
                                    lastSizeOfThisFile = fs.Position;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("The file could not be updated", this, e);
                            fileExists = false;
                        }
                        LinesMaxIndex = Lines.Count() - 1;
                    }
                }
            }
        }

        private void CheckIfFileExists()
        {
            if (File.Exists(fileAdress))
            {
                fileExists = true;
                if (WrapNotify) Logger.LogInfo("Wrapped file: " + fileAdress, this);
            }
        }

        private bool CheckForFileChanged()
        {
            if (!alwaysUpdate)
            {
                try
                {
                    currentFileInfo.Refresh();
                    if (currentFileInfo.Length != lastSizeOfThisFile)
                        return true;
                    else return false;
                }
                catch (Exception e)
                {
                    Logger.LogError("The file info could not be read", this, e);
                    //Debug.WriteLine("The file info could not be read:");
                    //Debug.WriteLine(e.Message);
                    return false;
                }
            }
            else return true;
        }

        /// <summary>
        /// Returns one text line from file at specified index. Returns null if index out of bounds.
        /// Null on any errors.
        /// </summary>
        /// <param name="index">indexing starts at 0</param>
        /// <returns></returns>
        public string ReadLine(int index)
        {
            try
            {
                if (index <= LinesMaxIndex)
                    return Lines[index];
                else return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns next text line from file, starting from the beginning and using internal counter.  
        /// Returns null if index out of bounds. Null on any errors.
        /// Can be reset with resetReadPos()
        /// </summary>
        /// <returns></returns>
        public string ReadNextLine()
        {
            try
            {
                if (currentReadIndex <= LinesMaxIndex)
                {
                    currentReadIndex++;
                    return Lines[currentReadIndex - 1];
                }
                else return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns next line from text file, with an index offset. Returns null if index out of bounds. Null on any errors.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string ReadNextLineOffset(int offset)
        {
            try
            {
                if (currentReadIndex <= LinesMaxIndex - offset)
                {
                    currentReadIndex++;
                    return Lines[currentReadIndex - 1];
                }
                else return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns last text line in file. Null on any error.
        /// </summary>
        public string ReadLastLine()
        {
            try
            {
                return Lines[LinesMaxIndex];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns last text line in file with index offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string GetLastLine(int offset)
        {
            try
            {
                return Lines[LinesMaxIndex - offset];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resets internal iterator for getNextLine methods to 0
        /// </summary>
        public void resetReadPos()
        {
            currentReadIndex = 0;
        }

        /// <summary>
        /// Returns index of the last line in this file, (indexing starts at 0)
        /// </summary>
        /// <returns></returns>
        public int getLastIndex()
        {
            return LinesMaxIndex;
        }

        /// <summary>
        /// Appends a new text line to the file. False if any problems encountered, reason is logged.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool WriteLine(string text)
        {
            if (fileExists && !isReadOnly)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(fileAdress))
                    {
                        sw.WriteLine(text);
                    }
                    Update();
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogError("error while adding line to file: " + fileAdress, this, _e);
                    return false;
                }
            }
            else return false;
        }

        /// <summary>
        /// Appends a List of new lines to the file. False if any problems encountered, reason is logged.
        /// </summary>
        /// <param name="textlist"></param>
        /// <returns></returns>
        public bool WriteLines(List<string> textlist)
        {
            if (fileExists && !isReadOnly)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(fileAdress))
                    {
                        foreach (string text in textlist)
                        {
                            sw.WriteLine(text);
                        }
                    }
                    Update();
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogError("error while adding lines to file: " + fileAdress, this, _e);
                    return false;
                }
            }
            else return false;
        }

        /// <summary>
        /// Clears the file and writes supplied list to the file. Returns false if this wrapper is read-only,
        /// clearing or writing failed, reason is logged.
        /// </summary>
        /// <param name="textlist"></param>
        /// <returns></returns>
        public bool RewriteFile(List<string> textlist)
        {
            if (fileExists && !isReadOnly)
            {
                ClearFile();
                WriteLines(textlist);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Clears the file. False if any problems encountered, reason is logged.
        /// </summary>
        /// <returns></returns>
        public bool ClearFile()
        {
            if (fileExists && !isReadOnly)
            {
                try
                {
                    File.WriteAllText(fileAdress, String.Empty);
                    resetReadPos();
                    Update();
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogError("error while clearing file: " + fileAdress, this, _e);
                    return false;
                }
            }
            else return false;
        }

        /// <summary>
        /// Returns all cached text lines as string array.
        /// </summary>
        /// <returns></returns>
        public string[] getAllLines()
        {
            return Lines.ToArray();
        }

        /// <summary>
        /// Creates a backup of wrapped text file in the same directory with appended .bak extension.
        /// Overwrites old .bak if present.
        /// </summary>
        public void BackupFile()
        {
            File.Copy(fileAdress, fileAdress + ".bak", true);
        }
    }
}
