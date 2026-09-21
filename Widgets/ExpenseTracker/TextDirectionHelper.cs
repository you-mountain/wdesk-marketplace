using System.Globalization;

namespace WDesk.Widgets.ExpenseTracker;

public static class TextDirectionHelper
{
    public static bool IsRtlText(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        int rtlCount = 0, ltrCount = 0;
        foreach (var ch in text)
        {
            if (IsRtlChar(ch)) rtlCount++;
            else if (char.IsLetter(ch)) ltrCount++;
        }
        return rtlCount > ltrCount;
    }

    public static bool IsRtlChar(char c)
    {
        // Arabic + Hebrew + Persian ranges
        return (c >= 0x0590 && c <= 0x05FF) ||  // Hebrew
               (c >= 0x0600 && c <= 0x06FF) ||  // Arabic
               (c >= 0x0750 && c <= 0x077F) ||  // Arabic Supplement
               (c >= 0xFB50 && c <= 0xFDFF) ||  // Arabic Presentation Forms-A
               (c >= 0xFE70 && c <= 0xFEFF);    // Arabic Presentation Forms-B
    }

    public static bool CurrentCultureIsRtl()
    {
        return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
    }
}