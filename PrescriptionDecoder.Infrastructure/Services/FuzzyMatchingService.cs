using Newtonsoft.Json;
using PrescriptionDecoder.Application.Interfaces;

namespace PrescriptionDecoder.Infrastructure.Services
{
    public class FuzzyMatchingService : IFuzzyMatchingService
    {
        private List<string> _drugDatabase;
        private readonly string _idsPath;

        public FuzzyMatchingService(string idsPath)
        {
            _idsPath = idsPath;
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            if (File.Exists(_idsPath))
            {
                var json = File.ReadAllText(_idsPath);
                _drugDatabase = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            else
            {
                _drugDatabase = new List<string>();
                Console.WriteLine($"Warning: Drug database not found at {_idsPath}");
            }
        }

        public string CorrectDrugName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            if (_drugDatabase.Contains(input, StringComparer.OrdinalIgnoreCase)) return input; 

            string bestMatch = input;
            int lowestDistance = int.MaxValue;

            foreach (var drug in _drugDatabase)
            {
                int distance = LevenshteinDistance(input.ToLower(), drug.ToLower());
                
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    bestMatch = drug;
                }
            }

            double threshold = Math.Min(2, input.Length * 0.2); 

            if (lowestDistance <= threshold)
            {
                Console.WriteLine($"FuzzyMatch: Corrected '{input}' to '{bestMatch}' (Dist: {lowestDistance})");
                return bestMatch; 
            }

            return input;
        }

        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}
