using System.Linq;
using ZeepSDK.Chat;

namespace Showdown4.Utils;

public class ChatMessage
{
    private ChatMessage()
    {
    }

    public string Message { get; set; }

    public void send()
    {
        ChatApi.SendMessage(Message);
    }

    public class Builder
    {
        private readonly ChatMessage _chatMessage;

        public Builder()
        {
            _chatMessage = new ChatMessage();
        }

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
            return this;
        }

        public ChatMessage build()
        {
            return _chatMessage;
        }
    }
}