using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace WDesk.Widgets.Notes;

public class NoteData
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Color { get; set; } = "#FF8FB339";   // سبز برند
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsPinned { get; set; } = false;

    // ═══════════════════════════════════════════
    //  Serialize / Deserialize (برای ذخیره در Settings)
    // ═══════════════════════════════════════════
    public static string SerializeList(List<NoteData> notes)
    {
        try
        {
            return JsonSerializer.Serialize(notes);
        }
        catch { return "[]"; }
    }

    public static List<NoteData> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<NoteData>();

        try
        {
            return JsonSerializer.Deserialize<List<NoteData>>(json)
                ?? new List<NoteData>();
        }
        catch { return new List<NoteData>(); }
    }
}