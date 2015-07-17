using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    /// <summary>
    /// equal and operator overloaded to compare underlying enum
    /// </summary>
    [DataContract]
    public struct HorseColor
    {
        public enum Color { Unknown = 0, Black = 1, White = 2, Grey = 3, Brown = 4, Gold = 5 }

        [DataMember]
        public readonly Color EnumVal;

        public HorseColor(Color color)
        {
            EnumVal = color;
        }

        public HorseColor(string DBValue)
        {
            if (string.IsNullOrEmpty(DBValue)) EnumVal = Color.Unknown;
            else EnumVal = (Color)int.Parse(DBValue);
        }

        public static HorseColor GetDefaultColor()
        {
            return new HorseColor(Color.Unknown);
        }

        public static string[] GetColorsEnumStrArray()
        {
            return Enum.GetNames(typeof(HorseColor.Color));
        }

        public static IEnumerable<HorseColor> GetAll()
        {
            return Enum.GetValues(typeof (HorseColor.Color)).Cast<HorseColor.Color>().Select(x => new HorseColor(x));
        }

        public override string ToString()
        {
            return EnumVal.ToString();
        }

        public string ToDBValue()
        {
            return ((int)EnumVal).ToString();
        }

        public bool Equals(HorseColor other)
        {
            return EnumVal.Equals(other.EnumVal);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is HorseColor)) return false;
            HorseColor other = (HorseColor)obj;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return EnumVal.GetHashCode();
        }

        public static bool operator ==(HorseColor left, HorseColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HorseColor left, HorseColor right)
        {
            return !(left == right);
        }

        internal static HorseColor CreateColorFromEnumString(string enumStr)
        {
            try
            {
                return new HorseColor((Color)Enum.Parse(typeof(Color), enumStr));
            }
            catch (Exception _e)
            {
                Aldurcraft.Utility.Logger.LogError("Parse error for HorseColor from enumStr: " + enumStr, "HorseColor", _e);
                return new HorseColor();
            }
        }

        internal static string GetDefaultColorStr()
        {
            return Enum.GetName(typeof(Color), Color.Unknown);
        }

        internal System.Drawing.Color? ToSystemDrawingColor()
        {
            switch (EnumVal)
            {
                case Color.Unknown:
                    return null;
                case Color.White:
                    return System.Drawing.Color.GhostWhite;
                case Color.Black:
                    return System.Drawing.Color.DarkSlateGray;
                case Color.Brown:
                    return System.Drawing.Color.Brown;
                case Color.Gold:
                    return System.Drawing.Color.Gold;
                case Color.Grey:
                    return System.Drawing.Color.LightGray;
                default:
                    Aldurcraft.Utility.Logger.LogError("no ARGB match for HorseColor: " + EnumVal, this);
                    return null;
            }
        }
    }

    public struct HorseAge : IComparable, IComparable<HorseAge>
    {
        //enum int value is used for comparable, this is limited design but whatever
        public enum Age : int
        { 
            Unknown=0, YoungFoal=100, AdolescentFoal=200, Young=300, Adolescent=400, Mature=500, Aged=600, Old=700, Venerable=800 
        }

        static Dictionary<string, HorseAge> WurmStringToAgeMap = new Dictionary<string, HorseAge>();

        public Age EnumVal;

        static HorseAge()
        {
            WurmStringToAgeMap.Add(GrangerHelpers.YOUNG.ToUpperInvariant(), new HorseAge(Age.Young));
            WurmStringToAgeMap.Add(GrangerHelpers.ADOLESCENT.ToUpperInvariant(), new HorseAge(Age.Adolescent));
            WurmStringToAgeMap.Add(GrangerHelpers.MATURE.ToUpperInvariant(), new HorseAge(Age.Mature));
            WurmStringToAgeMap.Add(GrangerHelpers.AGED.ToUpperInvariant(), new HorseAge(Age.Aged));
            WurmStringToAgeMap.Add(GrangerHelpers.OLD.ToUpperInvariant(), new HorseAge(Age.Old));
            WurmStringToAgeMap.Add(GrangerHelpers.VENERABLE.ToUpperInvariant(), new HorseAge(Age.Venerable));
        }

        public HorseAge(Age age)
        {
            EnumVal = age;
        }

        public HorseAge(string DBValue)
        {
            if (string.IsNullOrEmpty(DBValue)) EnumVal = Age.Unknown;
            else EnumVal = (Age)int.Parse(DBValue);
        }

        public static string[] GetColorsEnumStrArray()
        {
            return Enum.GetNames(typeof(Age));
        }

        public override string ToString()
        {
            return EnumVal.ToString();
        }

        public string ToDBValue()
        {
            return ((int)EnumVal).ToString();
        }

        public bool Equals(HorseAge other)
        {
            return EnumVal == other.EnumVal;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is HorseAge)) return false;
            HorseAge other = (HorseAge)obj;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)EnumVal;
        }

        internal static HorseAge CreateAgeFromEnumString(string enumStr)
        {
            try
            {
                return new HorseAge((Age)Enum.Parse(typeof(Age), enumStr));
            }
            catch (Exception _e)
            {
                Aldurcraft.Utility.Logger.LogError("Parse error for HorseAge from enumStr: " + enumStr, "HorseColor", _e);
                return new HorseAge();
            }
        }

        internal static string GetDefaultAgeStr()
        {
            return Enum.GetName(typeof(Age), Age.Unknown);
        }

        internal static HorseAge CreateAgeFromRawHorseName(string objectname)
        {
            objectname = objectname.ToUpperInvariant();
            foreach (var agestring in GrangerHelpers.HorseAgesUpcase)
            {
                if (objectname.Contains(agestring))
                {
                    return WurmStringToAgeMap[agestring];
                }
            }

            return new HorseAge(Age.Unknown);
        }

        internal static HorseAge CreateAgeFromRawHorseNameStartsWith(string prefixedobjectname)
        {
            prefixedobjectname = prefixedobjectname.ToUpperInvariant();
            foreach (var agestring in GrangerHelpers.HorseAgesUpcase)
            {
                if (prefixedobjectname.StartsWith(agestring, StringComparison.InvariantCulture))
                {
                    return WurmStringToAgeMap[agestring];
                }
            }

            return new HorseAge(Age.Unknown);
        }

        public void Foalize()
        {
            if (this.EnumVal == Age.Young)
                this.EnumVal = Age.YoungFoal;
            else if (this.EnumVal == Age.Adolescent)
                this.EnumVal = Age.AdolescentFoal;
            else throw new InvalidOperationException("foals do not come with this age type! " + this.EnumVal.ToString());
        }

        public int CompareTo(HorseAge other)
        {
            return ((int)EnumVal).CompareTo((int)other.EnumVal);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is HorseAge)
            {
                HorseAge other = (HorseAge)obj;
                return CompareTo(other);
            }
            else throw new ArgumentException("Object is not a HorseAge");
        }

        public static bool operator <(HorseAge left, HorseAge right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(HorseAge left, HorseAge right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(HorseAge left, HorseAge right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(HorseAge left, HorseAge right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(HorseAge left, HorseAge right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(HorseAge left, HorseAge right)
        {
            return !(left == right);
        }
    }
}
