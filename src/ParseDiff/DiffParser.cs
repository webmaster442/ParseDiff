using ParseDiff.Internals;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ParseDiff
{
    internal class DiffParser
    {
        private const string noeol = "\\ No newline at end of file";
        private const string devnull = "/dev/null";

        private readonly List<FileDiff> _files = new();
        private int _in_del;
        private int _in_add;
        private ChunkDiff? _current = null;
        private FileDiff? _file = null;
        private int _oldStart;
        private int _newStart;
        private int _oldLines;
        private int _newLines;
        private readonly HandlerCollection _schema;

        public DiffParser()
        {
            _schema = new HandlerCollection
            {
                { @"^diff\s", OnStart },
                { @"^new file mode \d+$", OnNewFile },
                { @"^deleted file mode \d+$", OnDeletedFile },
                { @"^index\s[\da-zA-Z]+\.\.[\da-zA-Z]+(\s(\d+))?$", OnIndex },
                { @"^---\s", OnFromFile },
                { @"^\+\+\+\s", OnToFile },
                { @"^@@\s+\-(\d+),?(\d+)?\s+\+(\d+),?(\d+)?\s@@", OnChunk },
                { @"^-", OnDeleteLine },
                { @"^\+", OnAddLine }
            };
        }

        public IEnumerable<FileDiff> Run(IEnumerable<string> lines)
        {
            foreach (string? line in lines)
                if (!ParseLine(line))
                    ParseNormalLine(line);

            return _files;
        }

        [MemberNotNull(nameof(_file))]
        private void OnStart(string? line)
        {
            _file = new FileDiff();
            _files.Add(_file);

            if (string.IsNullOrEmpty(_file.To) && string.IsNullOrEmpty(_file.From))
            {
                string[]? fileNames = ParseFileNames(line);

                if (fileNames.Length > 0)
                {
                    _file.From = fileNames[0];
                    _file.To = fileNames[1];
                }
            }
        }

        [MemberNotNull(nameof(_file))]
        private void Restart()
        {
            if (_file == null || _file.Chunks.Count != 0)
                OnStart(null);
        }

        private void OnNewFile()
        {
            Restart();
            _file.ChangeType = ChangeType.Add;
            _file.From = devnull;
        }

        private void OnDeletedFile()
        {
            Restart();
            _file.ChangeType = ChangeType.Delete;
            _file.To = devnull;
        }

        private void OnIndex(string line)
        {
            Restart();
            _file.Index = line.Split(' ').Skip(1).ToArray();
        }

        private void OnFromFile(string line)
        {
            Restart();
            _file.From = ParseFileName(line);
        }

        private void OnToFile(string line)
        {
            Restart();
            _file.To = ParseFileName(line);
        }

        private void OnChunk(string line, Match match)
        {
            _in_del = _oldStart = int.Parse(match.Groups[1].Value);
            _oldLines = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
            _in_add = _newStart = int.Parse(match.Groups[3].Value);
            _newLines = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
            _current = new ChunkDiff
            {
                Content = line,
                OldStart = _oldStart,
                NewStart = _newStart,
                NewLines = _newLines,
                OldLines = _oldLines,
            };
            if (_file != null)
                _file.Chunks.Add(_current);
        }

        private void OnDeleteLine(string line)
        {
            _current?.Changes.Add(new LineDiff
            {
                ChangeType = ChangeType.Delete,
                Index = _in_del++,
                Content = line,
            });
            if (_file != null)
                _file.Deletions++;
        }

        private void OnAddLine(string line)
        {
            _current?.Changes.Add(new LineDiff
            {
                ChangeType = ChangeType.Add,
                Index = _in_add++,
                Content = line,
            });
            if (_file != null)
                _file.Additions++;
        }


        private void ParseNormalLine(string line)
        {
            if (_file == null) return;

            if (string.IsNullOrEmpty(line)) return;

            _current?.Changes.Add(new LineDiff
            {
                OldIndex = line == noeol ? 0 : _in_del++,
                NewIndex = line == noeol ? 0 : _in_add++,
                Content = line
            });
        }

        private bool ParseLine(string line)
        {
            foreach (HandlerRow? p in _schema)
            {
                Match? m = p.Expression.Match(line);
                if (m.Success)
                {
                    p.Action(line, m);
                    return true;
                }
            }

            return false;
        }

        private static string[] ParseFileNames(string? fileNames)
        {
            if (string.IsNullOrEmpty(fileNames)) return Array.Empty<string>();
            return fileNames
                .Split(' ')
                .Reverse().Take(2).Reverse()
                .Select(fileName => Regex.Replace(fileName, @"^(a|b)\/", "")).ToArray();
        }

        private static string ParseFileName(string fileName)
        {
            fileName = fileName.TrimStart('-', '+');
            fileName = fileName.Trim();

            // ignore possible time stamp
            Match? t = new Regex(@"\t.*|\d{4}-\d\d-\d\d\s\d\d:\d\d:\d\d(.\d+)?\s(\+|-)\d\d\d\d").Match(fileName);
            if (t.Success)
            {
                fileName = fileName[..t.Index].Trim();
            }

            // ignore git prefixes a/ or b/
            return Regex.IsMatch(fileName, @"^(a|b)\/")
                ? fileName[2..]
                : fileName;
        }
    }
}
