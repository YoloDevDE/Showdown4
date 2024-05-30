namespace Showdown4.Service;

using System;
using System.Drawing; // Füge einen Verweis auf System.Drawing hinzu

public class ColorRangeConverter
{
    public static string ConvertHexToColorName(string hexCode)
    {
        Color color;
        try
        {
            color = ColorTranslator.FromHtml(hexCode);
        }
        catch
        {
            throw new ArgumentException("Invalid hex code.");
        }

        int r = color.R;
        int g = color.G;
        int b = color.B;

        if (r > 200 && g < 50 && b < 50) return "red";
        if (r > 200 && g > 100 && b < 50) return "orange";
        if (r > 150 && g > 150 && b < 50) return "yellow";
        if (r < 60 && g > 150 && b < 60) return "green";
        if (r < 50 && g < 50 && b > 200) return "blue";
        if (r > 100 && g < 100 && b > 150) return "purple";
        if (r > 200 && g > 150 && b > 175) return "pink";
        if (r < 50 && g < 50 && b < 50) return "black";
        if (r > 200 && g > 200 && b > 200) return "white";

        return "unknown"; // Gibt "unknown" zurück, wenn keine übereinstimmung gefunden wird
    }
}
