using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public class BreedingAdvisor
    {
        public const string DISABLED_id = "DISABLED";
        public const string DEFAULT_id = "default";
        public static string[] DefaultAdvisorIDs = new string[] { DISABLED_id, DEFAULT_id };

        public bool IsDisabled { get; private set; }
        public string AdvisorID { get; private set; }
        private GrangerContext Context;
        private FormGrangerMain MainForm;

        private BreedingEvaluator BreedEvalutator;

        public BreedingAdvisor(FormGrangerMain mainForm, string advisorID, GrangerContext Context)
        {
            this.MainForm = mainForm;
            this.AdvisorID = advisorID;
            this.Context = Context;

            IsDisabled = false;
            if (advisorID == DISABLED_id)
            {
                BreedEvalutator = new DisabledBreedingEvaluator();
                IsDisabled = true;
            }

            if (advisorID == DEFAULT_id)
            {
                BreedEvalutator = new DefaultBreedingEvaluator();
            }

            if (BreedEvalutator != null)
            {
                object options = mainForm.Settings.Value.GetBreedingEvalOptions(BreedEvalutator.GetType());
                if (options != null)
                    BreedEvalutator.SetOptions(options);
            }
        }

        internal BreedingEvalResults? GetBreedingValue(Horse horse)
        {
            if (IsDisabled) return null;
            if (MainForm.SelectedSingleHorse != null) //this is cached value
            {
                // this is the currently user-selected horse, while parameter horses are iterated by display process
                Horse evaluatedHorse = MainForm.SelectedSingleHorse;
                return BreedEvalutator.Evaluate(evaluatedHorse, horse, MainForm.CurrentValuator);
            }
            else return null;
        }

        internal System.Drawing.Color? GetColorForThisValue(int? CompareValue)
        {
            return null;
        }

        /// <summary>
        /// Options persistence is handled automatically,
        /// advisor rebuild is not needed
        /// </summary>
        /// <returns></returns>
        internal bool ShowOptions()
        {
            if (BreedEvalutator != null)
            {
                if (BreedEvalutator.EditOptions())
                {
                    MainForm.Settings.Value.SetBreedingEvalOptions(BreedEvalutator.GetType(), BreedEvalutator.GetOptions());
                    MainForm.Settings.DelayedSave();
                    return true;
                }
                else return false;
            }
            else
            {
                Logger.LogError("BreedEvalutator was null on ShowOptions", this);
                return false;
            }
        }

        internal System.Drawing.Color? GetHintColor(Horse horse, double minBreedValue, double maxBreedValue)
        {
            if (BreedEvalutator == null) return null;
            else
            {
                return BreedEvalutator.GetHintColor(horse, minBreedValue, maxBreedValue);
            }
        }
    }
}
