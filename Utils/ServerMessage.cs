using System;
using ZeepSDK.Chat;

namespace Showdown4.Utils;

public class ServerMessage
{
    private int lineCount; // To track the number of lines
    private string message = "";

    private readonly string prefix = "/servermessage white 0 " +
                                     "<size=\"40%\">" +
                                     "<align=\"left\">";

    public override string ToString()
    {
        return $"{message}";
    }

    // Add a content block with custom formatting
    public ServerMessage AddContent(Action<ContentBuilder> customizer)
    {
        ContentBuilder contentBuilder = new ContentBuilder(); // Create a new content builder for this block
        customizer(contentBuilder); // Apply custom formatting
        message += contentBuilder.BuildInline(); // Append the content without adding a line break yet
        return this;
    }

    // Add a headline that can have multiple content blocks
    public ServerMessage AddHeadline(Action<ServerMessage> headlineContent)
    {
        headlineContent(this); // Let the caller define the headline content
        message += "<br>"; // Add line break after the headline is complete
        lineCount++; // Increment line count after adding a headline
        PrependBreaksIfNeeded(); // Prepend <br> if there are more than 2 lines
        return this;
    }

    // Add a regular line of text
    public ServerMessage AddLine(Action<ServerMessage> lineContent)
    {
        lineContent(this); // Let the caller define the line content
        message += "<br>"; // Add line break after the line is complete
        lineCount++; // Increment line count after adding a line
        PrependBreaksIfNeeded(); // Prepend <br> if there are more than 2 lines
        return this;
    }

    // Optionally, add a separator line
    public ServerMessage AddSeparator()
    {
        message += "----------------------------<br>"; // Add separator with line break
        lineCount++; // Increment line count after adding a separator
        PrependBreaksIfNeeded(); // Prepend <br> if there are more than 2 lines
        return this;
    }

    // Prepend a <br> for each line after the 2nd to push the text down
    private void PrependBreaksIfNeeded()
    {
        if (lineCount > 2)
        {
            // For every line after 2 lines, prepend one <br> to the message
            message = "<br>" + message;
        }
    }

    public void Send()
    {
        message = prefix + message;
        // Simulating sending a message
        Console.WriteLine(message);
        ChatApi.SendMessage(message);
    }

    // ContentBuilder class to encapsulate content and formatting
    public class ContentBuilder
    {
        private string content = "";

        // Chainable methods for formatting
        public ContentBuilder AddText(string text)
        {
            content += text;
            return this;
        }

        public ContentBuilder Bold()
        {
            content = $"<b>{content}</b>";
            return this;
        }

        public ContentBuilder Underline()
        {
            content = $"<u>{content}</u>";
            return this;
        }

        public ContentBuilder Italic()
        {
            content = $"<i>{content}</i>";
            return this;
        }

        public ContentBuilder AllCaps()
        {
            content = $"<allcaps>{content}</allcaps>";
            return this;
        }

        public ContentBuilder Color(string color)
        {
            content = $"<color={color}>{content}</color>";
            return this;
        }

        public string BuildInline()
        {
            return content; // Return content without appending to the message (for inline use)
        }
    }
}