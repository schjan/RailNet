using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Core
{
    public enum Digitalsystem
    {
        Motorola,
        DCC
    }

    public static class DigitalsystemHelper
    {
        public static string ToEcoSString(this Digitalsystem digitalsystem)
        {
            switch (digitalsystem)
            {
                case Digitalsystem.Motorola:
                    return "MOT";
                case Digitalsystem.DCC:
                    return "DCC";
                default:
                    return "DCC";
            }
        }
    }
}