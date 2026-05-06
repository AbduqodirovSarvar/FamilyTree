namespace Application.Common.Helpers
{
    /// <summary>
    /// Tiny helpers for building Telegram-flavored HTML messages.
    /// Telegram's HTML parse mode supports a narrow tag whitelist
    /// (<c>&lt;b&gt;</c>, <c>&lt;i&gt;</c>, <c>&lt;u&gt;</c>, <c>&lt;s&gt;</c>,
    /// <c>&lt;code&gt;</c>, <c>&lt;pre&gt;</c>, <c>&lt;a&gt;</c>) and only
    /// requires three characters to be escaped in text content:
    /// <c>&amp;</c>, <c>&lt;</c>, <c>&gt;</c>. Anything else is fine
    /// to embed verbatim — no need for a full HtmlEncoder.
    /// </summary>
    public static class TelegramHtml
    {
        /// <summary>
        /// Escapes the three reserved characters and converts null/empty
        /// inputs to a fallback (default <c>—</c>) so messages stay
        /// readable when fields are missing.
        /// </summary>
        public static string Escape(string? value, string fallback = "—")
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}
