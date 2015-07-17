using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    [Table(Name = DBSchema.TraitValuesTableName)]
    public class TraitValueEntity
    {
        [Column(Name = "id", IsPrimaryKey=true)]
        public int ID;
        [Column(Name = "valuemapid")]
        public string ValueMapID;
        [Column(Name = "traitid")]
        string _TraitEnumINTStr;
        public HorseTrait Trait
        {
            get
            {
                return HorseTrait.FromEnumINTStr(_TraitEnumINTStr);
            }
            set
            {
                _TraitEnumINTStr = value.ToInt32().ToString();
            }
        }
        [Column(Name = "traitvalue")]
        public int Value;

        static public int GenerateNewTraitValueID(GrangerContext context)
        {
            try
            {
                return context.TraitValues.Max(x => x.ID) + 1;
            }
            catch (InvalidOperationException)
            {
                return 1;
            }
        }
    }
}