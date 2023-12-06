using AO;

public class Util 
{
    private static readonly string[] graduations = {"Unused", "K", "M", "B", "T"};
    public static string FormatDouble(double value, int roundAtDigits = 4, int roundToDigits = 2, string separator = "", bool forceSign = false)
    {
        value = Math.Floor(value);
        string number = value.ToString("F0");

        if (number.Length < roundAtDigits)
        {
            return number;
        }

        int graduationLevel = (number.Length - 1);

        // Round graduation level to nearest step of 3.
        graduationLevel = graduationLevel - (graduationLevel % 3);

        // Graduation level is already a multiple of 3.
        int modified_level = graduationLevel / 3;

        // The string which will hold the numbers units.
        string graduation = "";

        if (modified_level < graduations.Length)
            graduation = graduations[modified_level];
        else
        {
            modified_level -= graduations.Length;

            graduation =  modified_level < 26 ? (new string((char) ('A' + modified_level), 2)) : string.Format("e+{0}", graduationLevel);
        }

        //return graduation;

        int decimalPlacement = number.Length - graduationLevel;

        roundToDigits = Math.Min(roundToDigits, number.Length - decimalPlacement);

        string beforeDecimal = number.Substring(0, decimalPlacement);
        string afterDecimal = number.Substring(decimalPlacement, roundToDigits);

        if (afterDecimal.Length > 0)
            afterDecimal = "." + afterDecimal;

        string sign = forceSign && Math.Sign(value) == 1 ? "+" : "";

        return sign + beforeDecimal + afterDecimal + separator + graduation;
    }
}