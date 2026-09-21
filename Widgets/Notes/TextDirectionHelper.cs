using System;
using System.Text;

namespace WDesk.Widgets.Notes;

/// <summary>
/// تشخیص خودکار جهت متن (RTL برای فارسی/عربی، LTR برای انگلیسی).
/// </summary>
public static class TextDirectionHelper
{
    /// <summary>
    /// بررسی می‌کنه آیا متن عمدتاً فارسی/عربیه.
    /// </summary>
    public static bool IsRtl(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        int rtlCount = 0;
        int ltrCount = 0;

        foreach (char c in text)
        {
            // ── حروف فارسی/عربی ──
            if ((c >= '\u0600' && c <= '\u06FF') ||   // Arabic
                (c >= '\u0750' && c <= '\u077F') ||   // Arabic Supplement
                (c >= '\uFB50' && c <= '\uFDFF') ||   // Arabic Presentation Forms-A
                (c >= '\uFE70' && c <= '\uFEFF'))     // Arabic Presentation Forms-B
            {
                rtlCount++;
            }
            // ── حروف انگلیسی ──
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                ltrCount++;
            }
        }

        return rtlCount > ltrCount;
    }

    /// <summary>
    /// جهت مناسب برای متن.
    /// </summary>
    public static System.Windows.FlowDirection GetFlowDirection(string text)
    {
        return IsRtl(text)
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;
    }

    /// <summary>
    /// آیا متن شامل حروف فارسیه؟
    /// </summary>
    public static bool ContainsPersian(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        foreach (char c in text)
        {
            if ((c >= '\u0600' && c <= '\u06FF') ||
                (c >= '\uFB50' && c <= '\uFDFF') ||
                (c >= '\uFE70' && c <= '\uFEFF'))
            {
                return true;
            }
        }
        return false;
    }
}