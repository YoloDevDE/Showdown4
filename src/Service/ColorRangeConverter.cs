using System;
using System.Drawing;
using Showdown4.Utils;

namespace Showdown4.Service;
// Füge einen Verweis auf System.Drawing hinzu

public static class ColorRangeConverter
{
    public static ServerMessageColor ConvertHexToColorName(string hexCode)
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

        int red = color.R;
        int green = color.G;
        int blue = color.B;

        // Spezifische Farben zuerst abfragen
        if (IsWhite(red, green, blue))
        {
            return ServerMessageColor.white;
        }

        if (IsBlack(red, green, blue))
        {
            return ServerMessageColor.black;
        }

        if (IsYellow(red, green, blue))
        {
            return ServerMessageColor.yellow;
        }

        if (IsOrange(red, green, blue))
        {
            return ServerMessageColor.orange;
        }

        if (IsPink(red, green, blue))
        {
            return ServerMessageColor.pink;
        }

        if (IsPurple(red, green, blue))
        {
            return ServerMessageColor.purple;
        }

        // Generischere Farben danach abfragen
        if (IsRed(red, green, blue))
        {
            return ServerMessageColor.red;
        }

        if (IsGreen(red, green, blue))
        {
            return ServerMessageColor.green;
        }

        if (IsBlue(red, green, blue))
        {
            return ServerMessageColor.blue;
        }

        if (IsGray(red, green, blue))
        {
            if (red > 128)
            {
                return ServerMessageColor.white; // Helles Grau als Weiß
            }

            return ServerMessageColor.black; // Dunkles Grau als Schwarz
        }

        // Fallback zu dominanter Farbe
        return GetDominantColor(red, green, blue);
    }

    private static bool IsWhite(int red, int green, int blue)
    {
        return red > 240 && green > 240 && blue > 240;
    }

    private static bool IsBlack(int red, int green, int blue)
    {
        return red < 15 && green < 15 && blue < 15;
    }

    private static bool IsRed(int red, int green, int blue)
    {
        return red > blue && red > green && red > 128;
    }

    private static bool IsGreen(int red, int green, int blue)
    {
        return green > red && green > blue && green > 128;
    }

    private static bool IsBlue(int red, int green, int blue)
    {
        return blue > red && blue > green && blue > 128;
    }

    private static bool IsYellow(int red, int green, int blue)
    {
        return red > green && green > blue && red > 200 && green > 200;
    }

    private static bool IsOrange(int red, int green, int blue)
    {
        return red > green && red > 128 && green > 64 && blue < 100;
    }

    private static bool IsPink(int red, int green, int blue)
    {
        return red > blue && blue > green && red > 200 && blue > 150;
    }

    private static bool IsPurple(int red, int green, int blue)
    {
        return blue > red && red > green && blue > 128 && red > 128;
    }

    private static bool IsGray(int red, int green, int blue)
    {
        return Math.Abs(red - green) <= 20 && Math.Abs(red - blue) <= 20 && Math.Abs(green - blue) <= 20;
    }

    private static ServerMessageColor GetDominantColor(int red, int green, int blue)
    {
        int max = Math.Max(red, Math.Max(green, blue));
        if (max == red)
        {
            return ServerMessageColor.red;
        }

        if (max == green)
        {
            return ServerMessageColor.green;
        }

        return ServerMessageColor.blue;
    }
}