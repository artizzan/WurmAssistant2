using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    [Table(Name = "herds")]
    public class HerdEntity
    {
        [Column(Name = "id_herdname", IsPrimaryKey=true)]
        public string HerdID;

        [Column(Name = "selected")]
        bool? _Selected;
        public bool Selected
        {
            get { return _Selected == null ? false : _Selected.Value; }
            set { _Selected = value; }
        }

        public override string ToString()
        {
            return HerdID;
        }

        internal HerdEntity CloneMe(string newHerdName)
        {
            return new HerdEntity() { _Selected = this._Selected, HerdID = newHerdName };
        }

        public string HerdIDAspect
        {
            get { return this.HerdID; }
        }

        public bool CheckedAspect
        {
            get { return this.Selected; }
        }
    }
}
