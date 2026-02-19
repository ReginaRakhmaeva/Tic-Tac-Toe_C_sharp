using System.Text.Json;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.dbcontext.Converters;

public static class MoveHistoryComparer
{
    public static bool Compare(List<MoveDto>? c1, List<MoveDto>? c2)
    {
        if (c1 == null && c2 == null) return true;
        if (c1 == null || c2 == null) return false;
        if (c1.Count != c2.Count) return false;
        var json1 = JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null);
        var json2 = JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null);
        return json1 == json2;
    }

    public static int GetHashCode(List<MoveDto>? c)
    {
        if (c == null) return 0;
        var json = JsonSerializer.Serialize(c, (JsonSerializerOptions?)null);
        return json.GetHashCode();
    }

    public static List<MoveDto> Clone(List<MoveDto>? c)
    {
        if (c == null) return new List<MoveDto>();
        var json = JsonSerializer.Serialize(c, (JsonSerializerOptions?)null);
        return JsonSerializer.Deserialize<List<MoveDto>>(json, (JsonSerializerOptions?)null) ?? new List<MoveDto>();
    }
}
