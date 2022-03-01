using System.Collections;
using System.Text.RegularExpressions;

namespace ParseDiff.Internals
{
    internal class HandlerCollection : IEnumerable<HandlerRow>
    {
        private List<HandlerRow> _handlers = new();

        public void Add(string expression, Action action)
        {
            _handlers.Add(new HandlerRow(new Regex(expression), (line, m) => action()));
        }

        public void Add(string expression, Action<string> action)
        {
            _handlers.Add(new HandlerRow(new Regex(expression), (line, m) => action(line)));
        }

        public void Add(string expression, Action<string, Match> action)
        {
            _handlers.Add(new HandlerRow(new Regex(expression), action));
        }

        public IEnumerator<HandlerRow> GetEnumerator()
        {
            return _handlers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
