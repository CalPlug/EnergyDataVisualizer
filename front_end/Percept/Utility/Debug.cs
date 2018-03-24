using System;
using System.Diagnostics;
namespace Percept.Utility
{
    public class Debug
    {
        // only prints in debug release
        [Conditional("DEBUG")]
        public static void Print(String str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }
        // only prints in debug release
        [Conditional("DEBUG")]
        public static void PrintWT(String str)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("hh.mm.ss.ffffff") + " " + str);
        }
    }
}