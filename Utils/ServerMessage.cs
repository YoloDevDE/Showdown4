using System;
using ZeepSDK.Chat;

namespace Showdown4.Utils;

public class ServerMessage
{
    private readonly string prefix = "/servermessage white 0 " +
                                     "<size=\"30%\">" +
                                     "<align=\"left\">";

    private int lineCount; // To track the number of lines
    private string lineEffects = ""; // Default line-wide effects
    private string message = "";


    public override string ToString()
    {
        return $"{message}";
    }

    // Apply line-wide effects, but allow the lambda to override them
    public ServerMessage SetLineEffects(Action<ContentBuilder> lineEffectCustomizer)
    {
        ContentBuilder contentBuilder = new ContentBuilder();
        lineEffectCustomizer(contentBuilder);
        lineEffects = contentBuilder.BuildInline(); // Store effects for this line
        return this;
    }

    // Add a content block with custom formatting
    public ServerMessage AddContent(Action<ContentBuilder> customizer)
    {
        ContentBuilder contentBuilder = new ContentBuilder();
        customizer(contentBuilder);
        string inlineContent = contentBuilder.BuildInline();

        // Apply line-wide effects if there are any, but allow overriding
        message += lineEffects + inlineContent;
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
    } // Add a method to append another ServerMessage object

    public ServerMessage AddMessage(ServerMessage otherMessage)
    {
        message += otherMessage.ToString(); // Append the message from the other ServerMessage object
        lineCount += otherMessage.lineCount + 2;
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

    public ServerMessage AddLine()
    {
        message += "<br>"; // Add line break after the line is complete
        lineCount++; // Increment line count after adding a line
        PrependBreaksIfNeeded(); // Prepend <br> if there are more than 2 lines
        return this;
    }

    // Optionally, add a separator line
    public ServerMessage AddSeparator(int length = 20)
    {
        message += "<s><color=#00000000>" + new string('-', length) + "</color></s><br>"; // Add separator with line break
        lineCount++; // Increment line count after adding a separator
        PrependBreaksIfNeeded(); // Prepend <br> if there are more than 2 lines
        return this;
    }

    // Prepend a <br> for each line after the 2nd to push the text down
    private void PrependBreaksIfNeeded()
    {
        if (lineCount > 3)
        {
            // For every line after 2 lines, prepend one <br> to the message
            message = "<br>" + message;
        }
    }

    public ServerMessage ShowdownHeader()
    {
        return new ServerMessage()
            .AddHeadline(h => h
                .AddContent(c => c
                    .AddText("Showdown ")
                    .Color("#ff0000")
                    .Bold()
                    .AllCaps().FontSize(40)
                )
                .AddContent(c => c
                    .AddText("Season ")
                    .Color("#ffffff")
                    .Bold()
                    .AllCaps().FontSize(40)
                )
                .AddContent(c => c
                    .AddText("4")
                    .Color("#ff8800")
                    .Bold()
                    .AllCaps().FontSize(40)
                )
            );
    }

    public void Send()
    {
        message = prefix + message;
        // Simulating sending a message
        // Console.WriteLine(message);
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

        public ContentBuilder Italic()
        {
            content = $"<i>{content}</i>";
            return this;
        }

        public ContentBuilder Underline()
        {
            content = $"<u>{content}</u>";
            return this;
        }

        public ContentBuilder Strikethrough()
        {
            content = $"<s>{content}</s>";
            return this;
        }

        public ContentBuilder Superscript()
        {
            content = $"<sup>{content}</sup>";
            return this;
        }

        public ContentBuilder Subscript()
        {
            content = $"<sub>{content}</sub>";
            return this;
        }

        public ContentBuilder AllCaps()
        {
            content = $"<allcaps>{content}</allcaps>";
            return this;
        }

        public ContentBuilder SmallCaps()
        {
            content = $"<smallcaps>{content}</smallcaps>";
            return this;
        }

        public ContentBuilder Color(string color)
        {
            content = $"<color={color}>{content}</color>";
            return this;
        }

        public ContentBuilder FontSize(int size)
        {
            content = $"<size=\"{size}%\">{content}</size>";
            return this;
        }

        public ContentBuilder MarginLeft(int value)
        {
            content = $"<margin-left=\"{value}\">{content}</margin-left>";
            return this;
        }

        public ContentBuilder MarginRight(int value)
        {
            content = $"<margin-right=\"{value}\">{content}</margin-right>";
            return this;
        }

        // Neue Methoden für die Ausrichtung (alignment)
        public ContentBuilder Center()
        {
            content = $"<align=\"center\">{content}</align>";
            return this;
        }

        public ContentBuilder Left()
        {
            content = $"<align=\"left\">{content}</align>";
            return this;
        }

        public ContentBuilder Right()
        {
            content = $"<align=\"right\">{content}</align>";
            return this;
        }

        public ContentBuilder Indent()
        {
            content = $"<indent>{content}</indent>";
            return this;
        }

        public ContentBuilder NoParsing()
        {
            content = $"<noparse>{content}</noparse>";
            return this;
        }

        public ContentBuilder FontWeight(int weight)
        {
            content = $"<font-weight=\"{weight}\">{content}</font-weight>";
            return this;
        }

        public string BuildInline()
        {
            return content; // Rückgabe des formatierten Inhalts
        }
    }
}