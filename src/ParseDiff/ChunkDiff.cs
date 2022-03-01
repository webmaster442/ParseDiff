namespace ParseDiff
{
    public class ChunkDiff
    {
        public List<LineDiff> Changes { get; init; } = new();

        public string Content { get; init; } = string.Empty;

        public int OldStart { get; init; }

        public int OldLines { get; init; }

        public int NewStart { get; init; }

        public int NewLines { get; init; }
    }
}
