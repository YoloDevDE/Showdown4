using System.Linq;
using ZeepkistClient;

namespace Showdown4.Entities;

public class ChatMessage
{
	private ChatMessage()
	{
	}

	public string Message { get; set; }

	public static void SendCustomMessage(string message)
	{
		ZeepkistNetwork.SendCustomChatMessage(true, 0, "<br><#E0E0E0><size=-2>" + message + "</size></color>",
			"--------SHOWDOWN--------");
	}

	public static void SendCustomMessage(string message, params ulong[] steamIds)
	{
		foreach (ulong steamId in steamIds.Distinct())
			ZeepkistNetwork.SendCustomChatMessage(false, steamId,
				"<br><#E0E0E0><size=-2>" + message + "</size></color>", "--------SHOWDOWN--------</align>");
	}

	public class Builder
	{
		private readonly ChatMessage _chatMessage = new();

		private int _maxLineWidth = 16;

		public Builder DashedLine()
		{
			string dash = "-";
			int count = 16;
			_chatMessage.Message += string.Concat(Enumerable.Repeat(dash, count));
			return this;
		}

		public Builder ClearChat()
		{
			string br = "<br>";
			int count = 50;
			_chatMessage.Message += string.Concat(Enumerable.Repeat(br, count));
			return this;
		}

		public Builder NewLine()
		{
			_chatMessage.Message += "<br>";
			return this;
		}

		public Builder TextLine(string text)
		{
			_chatMessage.Message += text;
			if (text.Length > _maxLineWidth)
			{
				_maxLineWidth = text.Length;
			}

			return this;
		}

		public Builder CenterTextLine(string text)
		{
			// Calculate the number of spaces on both sides
			int padding = (_maxLineWidth - text.Length) / 2;

			// If maxLineWidth is smaller than the text length, no padding is added
			if (padding > 0)
			{
				_chatMessage.Message += new string(' ', padding) + text + new string(' ', padding);
				// If the length is odd, add an extra space on the right
				if ((_maxLineWidth - text.Length) % 2 != 0)
				{
					_chatMessage.Message += " ";
				}
			}
			else
			{
				_chatMessage.Message += text;
			}

			return this;
		}

		public ChatMessage Build()
		{
			return _chatMessage;
		}
	}
}