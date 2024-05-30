using System;
using System.Globalization;
using System.Linq;
using Showdown4.Service;

namespace Showdown4.Utils;

public static class MessageFormatter
{
    private const int _keyMaxWidth = 30; // Max Länge des Teamnamens, anpassbar
    private const int _valueMaxWidth = 10; // Max Länge der Zeit, anpassbar

    public static string AlignKeyValue(string key, string value)
    {
        return AlignKeyValue(key, _keyMaxWidth, value, _valueMaxWidth);
    }


    public static string AlignKeyValue(string key, int keyMaxWidth, string value, int valueMaxWidth)
    {
        string paddedTeamName = key.PadRight(keyMaxWidth);
        string paddedTime = value.PadLeft(valueMaxWidth);
        return $"{paddedTeamName} : {paddedTime}";
    }

    public static string ClearChat()
    {
        string br = "<br>";
        int count = 40;
        return string.Concat(Enumerable.Repeat(br, count));
    }

    public static string FormatRoundResult(Round round)
    {
        RoundEvaluator roundEvaluator = round.RoundEvaluator;
        string headline = $"Round {round.RoundNumber} Results:";
        string line1 = AlignKeyValue("#1: " + roundEvaluator.CurrentWinner.GetTag(), 6, roundEvaluator.GetAverageTime(roundEvaluator.CurrentWinner).GetFormattedTime(), 10);
        string line2 = AlignKeyValue("#2: " + roundEvaluator.CurrentLoser.GetTag(), 6, roundEvaluator.GetAverageTime(roundEvaluator.CurrentLoser).GetFormattedTime(), 10);
        return PrintLine() + headline + "<br>" + line1 + "<br>" + line2;
    }

    public static string PrintLine()
    {
        string dash = "-";
        int count = 16;
        return string.Concat(Enumerable.Repeat(dash, count));
    }

    public static string PrintBreak()
    {
        return "<br>";
    }

    public static string FormatTimestamp(double time)
    {
        if (time <= 0)
        {
            return "--:--.---";
        }

        return time.GetFormattedTime();
    }

    public static string FormatTimestampDifference(double time)
    {
        // Überprüfen, ob die Zeit negativ ist und ein entsprechendes Vorzeichen setzen
        char sign = time > 0 ? '-' : time < 0 ? '+' : '\u00b1';

        // Umwandeln der Zeit in einen absoluten Wert, um das Vorzeichen bei der Formatierung zu ignorieren
        double absoluteTime = Math.Abs(time);

        // Erstellen einer CultureInfo Instanz, die immer den Punkt als Dezimaltrennzeichen verwendet
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Formatieren der Zeit in Sekunden mit drei Dezimalstellen
        string formattedTime = absoluteTime.ToString("00.000", culture);

        // Zusammenfügen des Vorzeichens mit der formatierten Zeit
        return $"{sign}{formattedTime}";
    }
}