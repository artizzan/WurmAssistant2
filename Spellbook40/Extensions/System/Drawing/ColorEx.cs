using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Drawing
{
    public static class ColorEx
    {
        /// <summary>
        /// Gets a black or white color based on input color value.
        /// Intended for making foreground font color more visible 
        /// against non-static background color.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetContrastingWhiteOrBlackColorEx(this System.Drawing.Color input)
        {
            if (input.R + input.G + input.B < 3 * (256 / 2))
                return System.Drawing.Color.White;
            else return System.Drawing.Color.Black;
        }
    }
}
