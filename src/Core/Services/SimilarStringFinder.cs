using F23.StringSimilarity;

namespace Lynx.Core.Services;

public interface ISimilarStringFinder
{
    IEnumerable<(int startIndex, int endIndex)> FindSimilar(string input, string targetPhrase, double maxNormalizedDistance = 0.25);
    string ReplaceSimilar(string input, string targetPhrase, string replacement = "", double maxNormalizedDistance = 0.25);
}

public sealed class SimilarStringFinder : ISimilarStringFinder
{
    private readonly Levenshtein _levenshtein = new();

    public IEnumerable<(int startIndex, int endIndex)> FindSimilar(string input,
        string targetPhrase, double maxNormalizedDistance = 0.25)
    {
        if (String.IsNullOrWhiteSpace(input)) yield break;

        var lowerTargetPhrase = targetPhrase.ToLowerInvariant();
        var lowerInput = input.ToLowerInvariant();
        var words = lowerInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Keep track of character position in original string
        var originalIndex = 0;

        for (var i = 0; i < words.Length; i++) {
            // Skip leading spaces in original string to find actual word start
            while (originalIndex < input.Length && char.IsWhiteSpace(input[originalIndex])) {
                originalIndex++;
            }

            var startIndex = originalIndex;
            var currentWordLength = words[i].Length;

            // Try single word
            var singleWordPhrase = words[i];
            var singleWordDistance = _levenshtein.Distance(singleWordPhrase, lowerTargetPhrase);
            var singleWordNormalizedDistance = singleWordDistance /
                Math.Max(singleWordPhrase.Length, lowerTargetPhrase.Length);

            // Try two words if possible
            string? twoWordPhrase = null;
            var twoWordNormalizedDistance = double.MaxValue;

            if (i + 1 < words.Length) {
                twoWordPhrase = $"{words[i]} {words[i + 1]}";
                var twoWordDistance = _levenshtein.Distance(twoWordPhrase, lowerTargetPhrase);
                twoWordNormalizedDistance = twoWordDistance /
                    Math.Max(twoWordPhrase.Length, lowerTargetPhrase.Length);
            }

            // Decide which phrase to use (single or two-word)
            var useTwoWordPhrase = twoWordPhrase != null &&
                                   twoWordNormalizedDistance <= maxNormalizedDistance &&
                                   twoWordNormalizedDistance < singleWordNormalizedDistance;

            if (useTwoWordPhrase) {
                // Find end of two-word phrase in original string
                int nextWordStart = originalIndex + currentWordLength;
                while (nextWordStart < input.Length && char.IsWhiteSpace(input[nextWordStart])) {
                    nextWordStart++;
                }
                int secondWordLength = words[i + 1].Length;
                int endIndex = nextWordStart + secondWordLength;

                if (twoWordNormalizedDistance <= maxNormalizedDistance &&
                    twoWordPhrase.Length >= lowerTargetPhrase.Length - 2) {
                    yield return (startIndex, endIndex);
                }

                // Skip next word
                i++;
                originalIndex = endIndex;
            }
            else if (singleWordNormalizedDistance <= maxNormalizedDistance &&
                    singleWordPhrase.Length >= lowerTargetPhrase.Length - 2) {
                // Return single word match
                int endIndex = startIndex + currentWordLength;
                yield return (startIndex, endIndex);
                originalIndex = endIndex;
            }
            else {
                // No match, move to next word
                originalIndex += currentWordLength;
            }
        }
    }

    public string ReplaceSimilar(string input, string targetPhrase, string replacement = "", double maxNormalizedDistance = 0.25)
    {
        var matches = FindSimilar(input, targetPhrase, maxNormalizedDistance).OrderByDescending(m => m.startIndex).ToList();

        string result = input;
        foreach (var (start, end) in matches) {
            result = result.Remove(start, end - start).Insert(start, replacement);
        }
        return result.Trim();
    }
}
