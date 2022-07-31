namespace ParseDiff
{
    public record class FileDiff
    {
        public ICollection<ChunkDiff> Chunks { get; } = new List<ChunkDiff>();

        public int Deletions { get; set; }
        public int Additions { get; set; }

        public string To { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public ChangeType ChangeType { get; set; }

        public string[] Index { get; set; } = Array.Empty<string>();
    }
}
