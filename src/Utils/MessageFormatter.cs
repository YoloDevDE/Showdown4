using System;
using System.Globalization;
using System.Linq;

namespace Showdown4.Utils;

public static class MessageFormatter
{
	private const int KeyMaxWidth = 30; // Max length of the team name, adjustable
	private const int ValueMaxWidth = 10; // Max length of the time, adjustable

	public static string AlignKeyValue(string key, string value)
	{
		return AlignKeyValue(key, KeyMaxWidth, value, ValueMaxWidth);
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
		return time <= 0 ? "--:--.---" : time.GetFormattedTime();
	}

	public static string FormatTimestampDifference(double time)
	{
		// Check whether the time is negative and set the corresponding sign
		char sign = time > 0
			? '-'
			: time < 0
				? '+'
				: '\u00b1';

		// Convert the time to an absolute value to ignore the sign during formatting
		double absoluteTime = Math.Abs(time);

		// Create a CultureInfo instance that always uses the dot as the decimal separator
		CultureInfo culture = CultureInfo.InvariantCulture;

		// Format the time in seconds with three decimal places
		string formattedTime = absoluteTime.ToString("00.000", culture);

		// Combine the sign with the formatted time
		return $"{sign}{formattedTime}";
	}
}