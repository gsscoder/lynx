using F23.StringSimilarity;

namespace Lynx.Core.Utilities;

public sealed class SimilarStringFinder(string targetPhrase, double maxNormalizedDistance = 0.25)
{
    private readonly string _targetPhrase = targetPhrase.ToLowerInvariant();
    private readonly double _maxNormalizedDistance = maxNormalizedDistance;
    private readonly Levenshtein _levenshtein = new();

    public IEnumerable<(int startIndex, int endIndex)> FindSimilar(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) yield break;

        string lowerInput = input.ToLowerInvariant();
        string[] words = lowerInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Keep track of character position in original string
        int charIndex = 0;
        int originalIndex = 0;

        for (int i = 0; i < words.Length; i++) {
            // Skip leading spaces in original string to find actual word start
            while (originalIndex < input.Length && char.IsWhiteSpace(input[originalIndex])) {
                originalIndex++;
            }

            int startIndex = originalIndex;
            int currentWordLength = words[i].Length;

            // Try single word
            string singleWordPhrase = words[i];
            double singleWordDistance = _levenshtein.Distance(singleWordPhrase, _targetPhrase);
            double singleWordNormalizedDistance = singleWordDistance /
                Math.Max(singleWordPhrase.Length, _targetPhrase.Length);

            // Try two words if possible
            string twoWordPhrase = null;
            double twoWordNormalizedDistance = double.MaxValue;

            if (i + 1 < words.Length) {
                twoWordPhrase = $"{words[i]} {words[i + 1]}";
                double twoWordDistance = _levenshtein.Distance(twoWordPhrase, _targetPhrase);
                twoWordNormalizedDistance = twoWordDistance /
                    Math.Max(twoWordPhrase.Length, _targetPhrase.Length);
            }

            // Decide which phrase to use (single or two-word)
            bool useTwoWordPhrase = twoWordPhrase != null &&
                                   twoWordNormalizedDistance <= _maxNormalizedDistance &&
                                   twoWordNormalizedDistance < singleWordNormalizedDistance;

            if (useTwoWordPhrase) {
                // Find end of two-word phrase in original string
                int nextWordStart = originalIndex + currentWordLength;
                while (nextWordStart < input.Length && char.IsWhiteSpace(input[nextWordStart])) {
                    nextWordStart++;
                }
                int secondWordLength = words[i + 1].Length;
                int endIndex = nextWordStart + secondWordLength;

                if (twoWordNormalizedDistance <= _maxNormalizedDistance &&
                    twoWordPhrase.Length >= _targetPhrase.Length - 2) {
                    yield return (startIndex, endIndex);
                }

                // Skip next word
                i++;
                originalIndex = endIndex;
            }
            else if (singleWordNormalizedDistance <= _maxNormalizedDistance &&
                    singleWordPhrase.Length >= _targetPhrase.Length - 2) {
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

    public static string ReplaceSimilar(string input, string targetPhrase, string replacement = "")
    {
        var finder = new SimilarStringFinder(targetPhrase);
        var matches = finder.FindSimilar(input).OrderByDescending(m => m.startIndex).ToList();

        string result = input;
        foreach (var (start, end) in matches) {
            result = result.Remove(start, end - start).Insert(start, replacement);
        }
        return result.Trim();
    }
}
