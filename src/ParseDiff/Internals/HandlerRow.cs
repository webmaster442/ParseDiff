using System.Text.RegularExpressions;

namespace ParseDiff.Internals
{
    internal class HandlerRow
    {
        public HandlerRow(Regex expression, Action<string, Match> action)
        {
            Expression = expression;
            Action = action;
        }

        public Regex Expression { get; }

        public Action<string, Match> Action { get; }
    }
}
