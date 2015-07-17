using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Aldurcraft.Utility;
using System.Windows.Forms;
using CustomColors;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public class DisabledBreedingEvaluator : BreedingEvaluator
    {
        public override bool EditOptions()
        {
            MessageBox.Show("No advisor selected");
            return false;
        }

        public override object GetOptions()
        {
            return null;
        }

        public override void SetOptions(object options)
        {
            if (options != null)
                Logger.LogError("DisabledBreedingEvaluator received non-null options");
        }

        /// <summary>
        /// null if evaluation did not return any valid results
        /// </summary>
        /// <param name="valuator"></param>
        /// <returns></returns>
        public override BreedingEvalResults? Evaluate(Horse horse1, Horse horse2, TraitValuator valuator)
        {
            return null;
        }

        public override System.Drawing.Color? GetHintColor(Horse horse, double minBreedValue, double maxBreedValue)
        {
            return null;
        }
    }
}