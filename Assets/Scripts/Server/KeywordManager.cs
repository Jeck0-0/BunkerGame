using System.Collections.Generic;

namespace Server
{
    public class KeywordManager : Singleton<KeywordManager>
    {
        private readonly HashSet<string> activeKeywords = new();

        public bool Has(string keyword) => activeKeywords.Contains(keyword);

        public void Add(string keyword)
        {
            if (string.IsNullOrEmpty(keyword) || activeKeywords.Contains(keyword))
            return;

            activeKeywords.Add(keyword);
        }

        public void Remove(string keyword)
        {
            if (string.IsNullOrEmpty(keyword) || !activeKeywords.Contains(keyword))
            return;

            activeKeywords.Remove(keyword);
        }

        public void AddMultiple(IEnumerable<string> keywords)
        {
            foreach (var keyword in keywords)
            Add(keyword);
        }

        public void RemoveMultiple(IEnumerable<string> keywords)
        {
            foreach (var keyword in keywords)
            Remove(keyword);
        }

        public IReadOnlyCollection<string> All => activeKeywords;
    }
}
