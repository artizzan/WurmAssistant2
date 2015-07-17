using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    [DataContract]
    class OtherTool
    {
        public enum ToolType { LocalProgram, Url }

        [DataMember]
        public ToolType Type { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string SpecialArgs { get; set; }
        [DataMember]
        public string WorkDir { get; set; }
        [DataMember]
        public int SortOrder { get; set; }
        [DataMember]
        public Image Icon { get; set; }
    }
}
