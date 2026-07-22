using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ZeepSDK.Chat;

namespace Showdown4.Utils;

public class ServerMessage
{
	private readonly StringBuilder _messageBuilder = new(); // Using StringBuilder

	private string _command = "/servermessage white 0 ";

	private int _lineCount; // To track the number of lines

	private string _prefix;

	private string _suffix = "</align>" +
	                         "</size>";

	public ServerMessage(string alignment = "left")
	{
		_prefix = $"<size=\"20%\">" +
		          $"<align=\"{alignment}\">";
	}

	public override string ToString()
	{
		string tmp = "<size=\"0%\">TestTestTest" +
		             "</size>";
		return $"{_command}{tmp}{_prefix}{_messageBuilder}{_suffix}";
	}

	// Add a line with one or more blocks and optional line-wide formatting
	public ServerMessage AddLine(Action<LineBuilder> line)
	{
		LineBuilder lineBuilder = new();
		line(lineBuilder);
		_messageBuilder.Append(lineBuilder.BuildLine());
		AppendLineBreak();
		return this;
	}

	public ServerMessage AddLine(string line)
	{
		LineBuilder lineBuilder = new();
		lineBuilder.AddBlock(line);
		_messageBuilder.Append(lineBuilder.BuildLine());
		AppendLineBreak();
		return this;
	}


	public ServerMessage AddInLine(Action<LineBuilder> line)
	{
		LineBuilder lineBuilder = new();
		line(lineBuilder);
		_messageBuilder.Append(lineBuilder.BuildLine());
		return this;
	}

	public ServerMessage AddMessage(ServerMessage otherMessage)
	{
		// Remove prefix and suffix of the other message
		otherMessage._prefix = "";
		otherMessage._command = "";
		otherMessage._suffix = "";

		// Convert otherMessage's StringBuilder to a string
		string otherMessageContent = otherMessage._messageBuilder.ToString();

		// Remove all leading <br> from otherMessage
		while (otherMessageContent.StartsWith("<br>"))
			otherMessageContent = otherMessageContent.Substring(4); // Remove one <br> (4 characters)

		// Append the cleaned otherMessage's content to the current message
		_messageBuilder.Append(otherMessageContent);

		// Update the line count
		int originalLineCount = _lineCount;
		_lineCount += otherMessage._lineCount;

		// Ensure the current message gets prepended breaks for each added line
		for (int i = originalLineCount; i < _lineCount; i++) PrependBreaksIfNeeded();

		return this;
	}


	public ServerMessage AddSeparator(int length = 20)
	{
		_messageBuilder.Append($"<color=#00000000>{new string('-', length)}</color>");
		AppendLineBreak();
		return this;
	}


	private void AppendLineBreak()
	{
		_messageBuilder.Append("<br>");
		_lineCount++;
		PrependBreaksIfNeeded();
	}

	// Prepend a <br> for each line after the 2nd to push the text down
	private void PrependBreaksIfNeeded()
	{
		if (_lineCount > 2)
		{
			// // messageBuilder.Insert(0, "<br>");
		}
	}


	public ServerMessage ShowdownHeader(bool inline = false, string alignment = "left", int size = 40)
	{
		string season = ToRomanNumeral(MyConfig.Validated.SeasonNumber);
		Action<LineBuilder> headerBuilder = line => line
			.AddBlockNoSpace("<br>", b => b.Size(0))
			.AddBlockNoSpace("Showdown ", b => b.Gradients("#ffffff", "#aaaaaa", "#cccccc", "#ffffff"))
			.AddBlock("Season", b => b.Gradients("#ffffff", "#eeeeee", "#dddddd", "#aaaaaa", "#cccccc", "#ffffff"))
			.AddBlock(season, b => b.Color("#ffaa00"))
			.Bold().AllCaps().Size(size);

		return inline
			? new ServerMessage(alignment).AddInLine(headerBuilder)
			: new ServerMessage(alignment).AddLine(headerBuilder);
	}

	// Converts a positive integer to its Roman numeral representation (e.g. 6 -> "VI").
	// Falls back to the plain number for values outside the supported range.
	private static string ToRomanNumeral(int number)
	{
		if (number <= 0 || number >= 4000)
		{
			return number.ToString();
		}

		int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
		string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

		StringBuilder result = new();
		for (int i = 0; i < values.Length; i++)
			while (number >= values[i])
			{
				result.Append(numerals[i]);
				number -= values[i];
			}

		return result.ToString();
	}

	public void Send()
	{
		// Simulating sending a message
		Console.WriteLine(ToString());
		ChatApi.SendMessage(ToString());
	}

	// LineBuilder class for formatting entire lines and adding blocks
	public class LineBuilder
	{
		private readonly List<string> _closingTag = new();
		private readonly StringBuilder _lineContent = new();
		private readonly List<string> _openingTag = new();

		// Overload for AddBlock without customization
		public LineBuilder AddBlock(string text)
		{
			return AddBlock(text, _ => { }); // Use empty customizer
		}

		// Add blocks to the line with customization 
		public LineBuilder AddBlock(string text, Action<BlockBuilder> customizer)
		{
			BlockBuilder blockBuilder = new(text + " ");
			customizer(blockBuilder);
			_lineContent.Append(blockBuilder.BuildInline());
			return this;
		}

		public LineBuilder AddBlockNoSpace(string text)
		{
			return AddBlockNoSpace(text, _ => { }); // Use empty customizer
		}

		public LineBuilder AddBlockNoSpace(string text, Action<BlockBuilder> customizer)
		{
			BlockBuilder blockBuilder = new(text);
			customizer(blockBuilder);
			_lineContent.Append(blockBuilder.BuildInline());
			return this;
		}

		// Chainable methods for applying formatting to the entire line
		public LineBuilder Bold()
		{
			_openingTag.Add("<b>");
			_closingTag.Add("</b>");
			return this;
		}

		public LineBuilder Italic()
		{
			_openingTag.Add("<i>");
			_closingTag.Add("</i>");
			return this;
		}

		public LineBuilder Underline()
		{
			_openingTag.Add("<u>");
			_closingTag.Add("</u>");
			return this;
		}

		public LineBuilder StrikeThrough()
		{
			_openingTag.Add("<s>");
			_closingTag.Add("</s>");
			return this;
		}

		public LineBuilder Color(string color)
		{
			_openingTag.Add($"<color={color}>");
			_closingTag.Add("</color>");
			return this;
		}

		public LineBuilder AllCaps()
		{
			_openingTag.Add("<allcaps>");
			_closingTag.Add("</allcaps>");
			return this;
		}

		public LineBuilder Size(int size)
		{
			_openingTag.Add($"<size=\"{size}%\">");
			_closingTag.Add("</size>");
			return this;
		}

		public LineBuilder Align(string alignment)
		{
			_openingTag.Add($"<align=\"{alignment}\">");
			_closingTag.Add("</align>");
			return this;
		}

		// New Margin methods
		public LineBuilder MarginLeft(int value)
		{
			_openingTag.Add($"<margin-left=\"{value}%\">");
			_closingTag.Add("</margin>");
			return this;
		}

		// New Indent method (with support for pixels, percentages, or font units)
		public LineBuilder Indent(string value)
		{
			_openingTag.Add($"<indent=\"{value}%\">");
			_closingTag.Add("</indent>");
			return this;
		}

		public LineBuilder NoBreak()
		{
			_openingTag.Add("<nobr>");
			_closingTag.Add("</nobr>");
			return this;
		}

		public LineBuilder MarginRight(int value)
		{
			_openingTag.Add($"<margin-right=\"{value}%\">");
			_closingTag.Add("</margin>");
			return this;
		}

		// Build the final formatted line with effects and blocks
		public string BuildLine()
		{
			StringBuilder line = new();

			foreach (string tag in _openingTag) line.Insert(0, tag);

			line.Append(_lineContent.ToString());
			foreach (string tag in _closingTag) line.Append(tag);

			return line.ToString();
		}
	}

	// BlockBuilder class to encapsulate content and formatting for blocks
	public class BlockBuilder
	{
		private readonly StringBuilder _contentBuilder;

		public BlockBuilder(string text)
		{
			// int maxLength = 70;
			// if (text.Length > maxLength)
			// {
			//     text = text.Substring(0, maxLength-3) + "..."; // Truncate the text if it's too long'
			// }

			_contentBuilder = new StringBuilder(text);
		}

		public BlockBuilder Gradients(params string[] colors)
		{
			if (colors == null || colors.Length < 2)
			{
				return this;
			}

			// Validate hex codes
			foreach (string color in colors)
			{
				string cleanColor = color.TrimStart('#');
				if (!Regex.IsMatch(cleanColor, "^[0-9A-Fa-f]{3}$|^[0-9A-Fa-f]{4}$|^[0-9A-Fa-f]{6}$|^[0-9A-Fa-f]{8}$"))
				{
					return this;
				}
			}

			string content = ExtractTextContent(_contentBuilder.ToString(), out string before, out string after);
			StringBuilder gradientText = new();
			int textLength = content.Length;

			for (int i = 0; i < textLength; i++)
			{
				float progress = (float)i / (textLength - 1);
				int colorIndex = (int)(progress * (colors.Length - 1));
				float colorProgress = progress * (colors.Length - 1) - colorIndex;

				string startColor = colors[colorIndex].TrimStart('#');
				string endColor = colors[Math.Min(colorIndex + 1, colors.Length - 1)].TrimStart('#');
				string interpolatedColor = InterpolateColors(startColor, endColor, colorProgress);

				gradientText.Append($"<color=#{interpolatedColor}>{content[i]}</color>");
			}

			_contentBuilder.Clear();
			_contentBuilder.Append(before);
			_contentBuilder.Append(gradientText.ToString());
			_contentBuilder.Append(after);
			return this;
		}

		private string InterpolateColors(string startColor, string endColor, float progress)
		{
			int r1 = Convert.ToInt32(startColor.Substring(0, 2), 16);
			int g1 = Convert.ToInt32(startColor.Substring(2, 2), 16);
			int b1 = Convert.ToInt32(startColor.Substring(4, 2), 16);

			int r2 = Convert.ToInt32(endColor.Substring(0, 2), 16);
			int g2 = Convert.ToInt32(endColor.Substring(2, 2), 16);
			int b2 = Convert.ToInt32(endColor.Substring(4, 2), 16);

			int r = (int)(r1 + (r2 - r1) * progress);
			int g = (int)(g1 + (g2 - g1) * progress);
			int b = (int)(b1 + (b2 - b1) * progress);

			return $"{r:X2}{g:X2}{b:X2}";
		}

		private string ExtractTextContent(string input, out string before, out string after)
		{
			int startIndex = input.LastIndexOf('>') + 1;
			int endIndex = input.IndexOf("</", StringComparison.Ordinal);

			if (startIndex >= 0 && endIndex >= 0)
			{
				before = input.Substring(0, startIndex);
				after = input.Substring(endIndex);
				return input.Substring(startIndex, endIndex - startIndex);
			}

			before = "";
			after = "";
			return input;
		}

		public BlockBuilder WrapWithTag(string tag)
		{
			_contentBuilder.Insert(0, $"<{tag}>").Append($"</{tag}>");
			return this;
		}

		public BlockBuilder Bold()
		{
			return WrapWithTag("b");
		}

		public BlockBuilder Italic()
		{
			return WrapWithTag("i");
		}

		public BlockBuilder Underline()
		{
			return WrapWithTag("u");
		}

		public BlockBuilder Strikethrough()
		{
			return WrapWithTag("s");
		}

		public BlockBuilder Superscript()
		{
			return WrapWithTag("sup");
		}

		public BlockBuilder Subscript()
		{
			return WrapWithTag("sub");
		}

		public BlockBuilder AllCaps()
		{
			return WrapWithTag("allcaps");
		}

		public BlockBuilder SmallCaps()
		{
			return WrapWithTag("smallcaps");
		}

		// New Indent method (with support for pixels, percentages, or font units)
		public BlockBuilder Indent(string value)
		{
			_contentBuilder.Insert(0, $"<indent=\"{value}%\">").Append("</indent>");
			return this;
		}

		public BlockBuilder Color(string color)
		{
			_contentBuilder.Insert(0, $"<color={color}>").Append("</color>");
			return this;
		}

		public BlockBuilder Mark(string color)
		{
			_contentBuilder.Insert(0, $"<mark={color}>").Append("</color>");
			return this;
		}

		public BlockBuilder Size(int size)
		{
			_contentBuilder.Insert(0, $"<size=\"{size}%\">").Append("</size>");
			return this;
		}

		public BlockBuilder Align(string alignment)
		{
			_contentBuilder.Insert(0, $"<align=\"{alignment}\">").Append("</align>");
			return this;
		}

		// New Margin methods
		public BlockBuilder MarginLeft(int value)
		{
			_contentBuilder.Insert(0, $"<margin-left=\"{value}%\">").Append("</margin-left>");
			return this;
		}

		public BlockBuilder MarginRight(int value)
		{
			_contentBuilder.Insert(0, $"<margin-right=\"{value}%\">").Append("</margin-right>");
			return this;
		}

		public string BuildInline()
		{
			return _contentBuilder.ToString();
		}
	}
}