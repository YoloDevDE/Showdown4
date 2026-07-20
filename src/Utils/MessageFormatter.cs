using System;
using System.Globalization;
using System.Linq;

namespace Showdown4.Utils;

public static class MessageFormatter
{
	private const int KeyMaxWidth = 30; // Max Länge des Teamnamens, anpassbar
	private const int ValueMaxWidth = 10; // Max Länge der Zeit, anpassbar

	public static string AlignKeyValue(string key, string value)
	{
		return AlignKeyValue(key, KeyMaxWidth, value, ValueMaxWidth);
	}


	public static string AlignKeyValue(string key, int keyMaxWidth, string value, int valueMaxWidth)
	{
		var paddedTeamName = key.PadRight(keyMaxWidth);
		var paddedTime = value.PadLeft(valueMaxWidth);
		return $"{paddedTeamName} : {paddedTime}";
	}

	public static string ClearChat()
	{
		var br = "<br>";
		var count = 40;
		return string.Concat(Enumerable.Repeat(br, count));
	}

	// public static string FormatRoundResult(Round round)
	// {
	//     RoundEvaluator roundEvaluator = round.RoundEvaluator;
	//     string headline = $"Round {round.RoundNumber} Results:";
	//     string line1 = AlignKeyValue("#1: " + roundEvaluator.CurrentWinner.GetTag(), 6, roundEvaluator.GetAverageTime(roundEvaluator.CurrentWinner).GetFormattedTime(), 10);
	//     string line2 = AlignKeyValue("#2: " + roundEvaluator.CurrentLoser.GetTag(), 6, roundEvaluator.GetAverageTime(roundEvaluator.CurrentLoser).GetFormattedTime(), 10);
	//     return PrintLine() + headline + "<br>" + line1 + "<br>" + line2;
	// }

	public static string PrintLine()
	{
		var dash = "-";
		var count = 16;
		return string.Concat(Enumerable.Repeat(dash, count));
	}

	public static string PrintBreak()
	{
		return "<br>";
	}

	public static string FormatTimestamp(double time)
	{
		if (time <= 0) return "--:--.---";

		return time.GetFormattedTime();
	}

	public static string FormatTimestampDifference(double time)
	{
		// Überprüfen, ob die Zeit negativ ist und ein entsprechendes Vorzeichen setzen
		var sign = time > 0
			? '-'
			: time < 0
				? '+'
				: '\u00b1';

		// Umwandeln der Zeit in einen absoluten Wert, um das Vorzeichen bei der Formatierung zu ignorieren
		var absoluteTime = Math.Abs(time);

		// Erstellen einer CultureInfo Instanz, die immer den Punkt als Dezimaltrennzeichen verwendet
		var culture = CultureInfo.InvariantCulture;

		// Formatieren der Zeit in Sekunden mit drei Dezimalstellen
		var formattedTime = absoluteTime.ToString("00.000", culture);

		// Zusammenfügen des Vorzeichens mit der formatierten Zeit
		return $"{sign}{formattedTime}";
	}
}