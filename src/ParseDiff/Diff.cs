namespace ParseDiff
{
    public static class Diff
    {
        public static IEnumerable<FileDiff> Parse(string? input, string lineEnding = "\n")
        {
            if (string.IsNullOrWhiteSpace(input)) return Enumerable.Empty<FileDiff>();

            string[]? lines = input.Split(new[] { lineEnding }, StringSplitOptions.None);

            if (lines.Length == 0) return Enumerable.Empty<FileDiff>();

            DiffParser parser = new();

            return parser.Run(lines);
        }
    }
}
