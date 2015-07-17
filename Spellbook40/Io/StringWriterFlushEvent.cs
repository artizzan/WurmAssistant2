using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aldurcraft.Spellbook40.Io
{
    public class StringWriterFlushEventArgs : EventArgs
    {
        public readonly string Value;

        public StringWriterFlushEventArgs(string value)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// StringWriter with auto-flushing capability.
    /// </summary>
    public class StringWriterFlushEvent : StringWriter
    {
        /// <summary>
        /// Triggers when flush occours and provides text that was flushed
        /// </summary>
        public event EventHandler<StringWriterFlushEventArgs> Flushed;
        public virtual bool AutoFlush { get; set; }

        public StringWriterFlushEvent()
            : base() { }

        public StringWriterFlushEvent(bool autoFlush)
            : base() { this.AutoFlush = autoFlush; }

        private void OnFlush()
        {
            var eh = Flushed; //tsafe
            if (eh != null)
            {
                string newText = this.GetStringBuilder().ToString();
                eh(this, new StringWriterFlushEventArgs(newText));
            }
            this.GetStringBuilder().Clear();
        }

        public override void Flush()
        {
            base.Flush();
            OnFlush();
        }

        public override void Write(char value)
        {
            base.Write(value);
            if (AutoFlush) Flush();
        }

        public override void Write(string value)
        {
            base.Write(value);
            if (AutoFlush) Flush();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            if (AutoFlush) Flush();
        }
    }
}
