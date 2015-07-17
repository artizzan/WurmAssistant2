using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Aldurcraft.Utility
{
    internal class LoggerOldLogsCleaner
    {
        readonly List<KeyValuePair<DateTime, FileInfo>> _listOFileInfos = new List<KeyValuePair<DateTime, FileInfo>>();

        public LoggerOldLogsCleaner(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);
            foreach (string file in files)
            {
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"(\d\d\d\d)-(\d\d)-(\d\d)");
                if (match.Success)
                {
                    DateTime dt = new DateTime(
                        Convert.ToInt32(match.Groups[1].Value),
                        Convert.ToInt32(match.Groups[2].Value),
                        Convert.ToInt32(match.Groups[3].Value));
                    _listOFileInfos.Add(new KeyValuePair<DateTime, FileInfo>(dt, new FileInfo(file)));
                }
            }
        }

        public long GetByteCount()
        {
            long total = 0;
            foreach (KeyValuePair<DateTime, FileInfo> listOFileInfo in _listOFileInfos)
            {
                total += listOFileInfo.Value.Length;
            }
            return total;
        }

        public long DoCleanup(DateTime maxDate)
        {
            long bytesRemoved = 0;
            var stuffToDel = new List<KeyValuePair<DateTime, FileInfo>>();
            foreach (KeyValuePair<DateTime, FileInfo> keyValuePair in _listOFileInfos)
            {
                if (keyValuePair.Key < maxDate)
                {
                    stuffToDel.Add(keyValuePair);
                }
            }
            foreach (KeyValuePair<DateTime, FileInfo> keyValuePair in stuffToDel)
            {
                bytesRemoved += keyValuePair.Value.Length;
                _listOFileInfos.Remove(keyValuePair);
                keyValuePair.Value.Delete();
            }
            return bytesRemoved;
        }
    }
}
