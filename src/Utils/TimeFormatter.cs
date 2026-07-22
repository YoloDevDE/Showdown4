using System;

namespace Showdown4.Utils;

public class TimeFormatter
{
	public static string FormatDuration(int durationInSeconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(durationInSeconds);

		// Check whether hours are present
		if (timeSpan.TotalHours >= 1)
			// Format for hours:minutes:seconds
		{
			return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
		}

		// Format for minutes:seconds
		return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
	}
}