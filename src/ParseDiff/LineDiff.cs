﻿namespace ParseDiff
{
    public record class LineDiff
    {
        public string Content { get; init; } = string.Empty;

        public int Index { get; init; }

        public int OldIndex { get; init; }

        public int NewIndex { get; init; }

        public ChangeType ChangeType { get; init; }
    }
}
