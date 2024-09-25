using System;
using System.Collections.Generic;
using System.Text;
using ZeepSDK.Chat;

namespace Showdown4.Utils;

public class ServerMessage
{
    private readonly StringBuilder messageBuilder = new StringBuilder(); // Using StringBuilder

    private string command = "/servermessage white 0 ";

    private int lineCount; // To track the number of lines

    private string prefix = "<margin-right=\"50%\">" +
                            "<size=\"30%\">" +
                            "<align=\"left\">";

    private string suffix = "</align>" +
                            "</size>" +
                            "</margin>";

    public override string ToString()
    {
        string tmp = "<size=\"0%\">TestTestTest" +
                     "</size>";
        return $"{command}{tmp}{prefix}{messageBuilder}{suffix}";
    }

    // Add a line with one or more blocks and optional line-wide formatting
    public ServerMessage AddLine(Action<LineBuilder> line)
    {
        LineBuilder lineBuilder = new LineBuilder();
        line(lineBuilder);
        messageBuilder.Append(lineBuilder.BuildLine());
        AppendLineBreak();
        return this;
    }

    public ServerMessage AddLine(string line)
    {
        LineBuilder lineBuilder = new LineBuilder();
        lineBuilder.AddBlock(line);
        messageBuilder.Append(lineBuilder.BuildLine());
        AppendLineBreak();
        return this;
    }

    public ServerMessage AddInLine(Action<LineBuilder> line)
    {
        LineBuilder lineBuilder = new LineBuilder();
        line(lineBuilder);
        messageBuilder.Append(lineBuilder.BuildLine());
        return this;
    }

    public ServerMessage AddMessage(ServerMessage otherMessage)
    {
        // Remove prefix and suffix of the other message
        otherMessage.prefix = "";
        otherMessage.command = "";
        otherMessage.suffix = "";

        // Convert otherMessage's StringBuilder to a string
        string otherMessageContent = otherMessage.messageBuilder.ToString();

        // Remove all leading <br> from otherMessage
        while (otherMessageContent.StartsWith("<br>"))
        {
            otherMessageContent = otherMessageContent.Substring(4); // Remove one <br> (4 characters)
        }

        // Append the cleaned otherMessage's content to the current message
        messageBuilder.Append(otherMessageContent);

        // Update the line count
        int originalLineCount = lineCount;
        lineCount += otherMessage.lineCount;

        // Ensure the current message gets prepended breaks for each added line
        for (int i = originalLineCount; i < lineCount; i++)
        {
            PrependBreaksIfNeeded();
        }

        return this;
    }


    public ServerMessage AddSeparator(int length = 20)
    {
        messageBuilder.Append($"<s><color=#00000000>{new string('-', length)}</color></s>");
        AppendLineBreak();
        return this;
    }

    private void AppendLineBreak()
    {
        messageBuilder.Append("<br>");
        lineCount++;
        PrependBreaksIfNeeded();
    }

    // Prepend a <br> for each line after the 2nd to push the text down
    private void PrependBreaksIfNeeded()
    {
        if (lineCount > 2)
        {
            messageBuilder.Insert(0, "<br>");
        }
    }

    public ServerMessage ShowdownHeader()
    {
        return new ServerMessage()
            .AddLine(line => line
                .AddBlock("Showdown", b => b.Color("#ff0000"))
                .AddBlock("Season", b => b.Color("#ffffff"))
                .AddBlock("4", b => b.Color("#ff8800"))
                .Bold().AllCaps().FontSize(40)
            );
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
        private readonly List<string> closingTag = new List<string>();
        private readonly StringBuilder lineContent = new StringBuilder();
        private readonly List<string> openingTag = new List<string>();

        // Overload for AddBlock without customization
        public LineBuilder AddBlock(string text)
        {
            lineContent.Append(text + " ");
            return this;
        }

        // Add blocks to the line with customization
        public LineBuilder AddBlock(string text, Action<BlockBuilder> customizer)
        {
            BlockBuilder blockBuilder = new BlockBuilder(text + " ");
            customizer(blockBuilder);
            lineContent.Append(blockBuilder.BuildInline());
            return this;
        }

        // Chainable methods for applying formatting to the entire line
        public LineBuilder Bold()
        {
            openingTag.Add("<b>");
            closingTag.Add("</b>");
            return this;
        }

        public LineBuilder Italic()
        {
            openingTag.Add("<i>");
            closingTag.Add("</i>");
            return this;
        }

        public LineBuilder Underline()
        {
            openingTag.Add("<u>");
            closingTag.Add("</u>");
            return this;
        }

        public LineBuilder Color(string color)
        {
            openingTag.Add($"<color={color}>");
            closingTag.Add("</color>");
            return this;
        }

        public LineBuilder AllCaps()
        {
            openingTag.Add("<allcaps>");
            closingTag.Add("</allcaps>");
            return this;
        }

        public LineBuilder FontSize(int size)
        {
            openingTag.Add($"<size=\"{size}%\">");
            closingTag.Add("</size>");
            return this;
        }

        public LineBuilder Align(string alignment)
        {
            openingTag.Add($"<align=\"{alignment}\">");
            closingTag.Add("</align>");
            return this;
        }

        // New Margin methods
        public LineBuilder MarginLeft(int value)
        {
            openingTag.Add($"<margin-left=\"{value}%\">");
            closingTag.Add("</margin>");
            return this;
        }

        // New Indent method (with support for pixels, percentages, or font units)
        public LineBuilder Indent(string value)
        {
            openingTag.Add($"<indent=\"{value}%\">");
            closingTag.Add("</indent>");
            return this;
        }

        public LineBuilder MarginRight(int value)
        {
            openingTag.Add($"<margin-right=\"{value}%\">");
            closingTag.Add("</margin>");
            return this;
        }

        // Build the final formatted line with effects and blocks
        public string BuildLine()
        {
            StringBuilder line = new StringBuilder();

            foreach (string tag in openingTag)
            {
                line.Insert(0, tag);
            }

            line.Append(lineContent.ToString());
            foreach (string tag in closingTag)
            {
                line.Append(tag);
            }

            return line.ToString();
        }
    }

    // BlockBuilder class to encapsulate content and formatting for blocks
    public class BlockBuilder
    {
        private readonly StringBuilder contentBuilder;

        public BlockBuilder(string text)
        {
            contentBuilder = new StringBuilder(text);
        }

        public BlockBuilder WrapWithTag(string tag)
        {
            contentBuilder.Insert(0, $"<{tag}>").Append($"</{tag}>");
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
            contentBuilder.Insert(0, $"<indent=\"{value}%\">").Append("</indent>");
            return this;
        }

        public BlockBuilder Color(string color)
        {
            contentBuilder.Insert(0, $"<color={color}>").Append("</color>");
            return this;
        }

        public BlockBuilder FontSize(int size)
        {
            contentBuilder.Insert(0, $"<size=\"{size}%\">").Append("</size>");
            return this;
        }

        public BlockBuilder Align(string alignment)
        {
            contentBuilder.Insert(0, $"<align=\"{alignment}\">").Append("</align>");
            return this;
        }

        // New Margin methods
        public BlockBuilder MarginLeft(int value)
        {
            contentBuilder.Insert(0, $"<margin-left=\"{value}%\">").Append("</margin-left>");
            return this;
        }

        public BlockBuilder MarginRight(int value)
        {
            contentBuilder.Insert(0, $"<margin-right=\"{value}%\">").Append("</margin-right>");
            return this;
        }

        public string BuildInline()
        {
            return contentBuilder.ToString();
        }

        public void Size(int p0)
        {
            throw new NotImplementedException();
        }
    }
}