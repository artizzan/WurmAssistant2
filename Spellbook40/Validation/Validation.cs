using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Spellbook40.Validation
{
    public static class Validation
    {
        /// <summary>
        /// If the value is less than min or more than max, returns min or max respectively, else returns the value.
        /// </summary>
        /// <typeparam name="T">Any comparable type</typeparam>
        public static T ConstrainValue<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }
    }
}
