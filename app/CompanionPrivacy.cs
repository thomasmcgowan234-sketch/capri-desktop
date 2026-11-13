using System.Text.RegularExpressions;

namespace CapriDesktop
{
    // Shared with legacy chat-memory filtering so automatic reflection cannot bypass
    // the existing personal/contact/location boundary by saving a quoted owner turn.
    internal static class CompanionPrivacy
    {
        internal static readonly Regex Personal = new Regex(@"\b(passwords?|passcodes?|pin (code|number)|log ?in details|credit card|card number|bank (account|details|number)|account number|sort code|routing number|social security|passport|(home|street|postal|email|e-mail) address|(my|their|his|her) address|postcode|post code|zip code|(phone|mobile|cell) number|surname|last name|full name|real name|date of birth|(i|they) live (in|at|on)|(my|their|his|her) school(?: is|'s| is called| called)|school (is )?called)\b|@\w+\.\w+|\d[\d\s-]{6,}\d|\b\d{1,2}[/.-]\d{1,2}[/.-]\d{2,4}\b", RegexOptions.IgnoreCase);
        internal static readonly Regex Whereabouts = new Regex(@"(?i:\baddress(es)?\b)|\b\d{1,5}[A-Za-z]?\s+(?:[A-Z][\p{L}']*\s+){1,3}(?:Street|St|Road|Rd|Avenue|Ave|Lane|Drive|Court|Place|Crescent|Boulevard|Blvd|Way|Terrace|Close|Parade|Highway|Grove|Square)\b");

        internal static bool Revealing(string text)
        {
            return Personal.IsMatch(text ?? "") || Whereabouts.IsMatch(text ?? "");
        }
    }
}
