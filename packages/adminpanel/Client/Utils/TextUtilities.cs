namespace Alvtime.Adminpanel.Client.Utils;

public static class TextUtilities
{
    public static string GetInitialsFromName(string? name, HashSet<string>? ignoreList)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        // split the name into words and filter out company suffixes
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => ignoreList is null || !ignoreList.Contains(w.ToUpper()))
            .ToArray();

        if (words.Length == 0)
            return string.Empty;

        if (words.Length > 1)
        {
            // if there are multiple words (excluding suffixes), return the first letter of the first two words
            return $"{words[0][0]}{words[1][0]}".ToUpper();
        }
        else
        {
            // if there is only one word left, return the first two letters
            return words[0].Length > 1 ? words[0][..2].ToUpper() : words[0].ToUpper();
        }
    }
}