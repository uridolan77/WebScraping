using System;

namespace WebScraper
{
    public class ScrapedPage
    {
        public string Url { get; set; }
        public DateTime ScrapedDateTime { get; set; }
        public string TextContent { get; set; }
        public int Depth { get; set; }
    }

    public class CustomStringBuilder
    {
        private readonly System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
        private readonly Action<string> _logAction;

        public CustomStringBuilder(Action<string> logAction)
        {
            _logAction = logAction;
        }

        // Property to expose the internal StringBuilder
        public System.Text.StringBuilder InternalBuilder => _stringBuilder;

        // Forward needed methods to the internal StringBuilder
        public CustomStringBuilder Append(string value)
        {
            _stringBuilder.Append(value);
            return this;
        }

        public CustomStringBuilder AppendLine(string value)
        {
            _stringBuilder.AppendLine(value);
            return this;
        }

        public override string ToString()
        {
            var result = _stringBuilder.ToString();
            _logAction(result);
            _stringBuilder.Clear();
            return result;
        }

        public void Clear()
        {
            _stringBuilder.Clear();
        }
    }
}