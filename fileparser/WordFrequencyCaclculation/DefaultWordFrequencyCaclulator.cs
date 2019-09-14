using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fileparser.wordfrequencycaclculation
{
    public class DefaultWordFrequencyCaclulator : IWordFrequencyCalculator
    {
        private readonly string _outputPath;
        private IDictionary<string, int> _result;
        private Object _lock = new Object();
        public DefaultWordFrequencyCaclulator(string outputPath)
        {
            _outputPath = outputPath;
            _result = new Dictionary<string, int>();
        }

        public async Task FlushAsync()
        {
            using (StreamWriter file =
            new StreamWriter(_outputPath))
            {
                foreach (var line in _result.OrderByDescending(pair => pair.Value))
                {
                      await file.WriteLineAsync($"{line.Key},{line.Value}");
                }
            }
        }

        public void MergeInfo(IDictionary<string, int> info)
        {
            lock (_lock)
            {
                foreach (var item in info)
                {
                    if (_result.ContainsKey(item.Key))
                    {
                        _result[item.Key] = _result[item.Key] + item.Value;
                    }
                    else
                    {
                        _result[item.Key] = item.Value;
                    }
                }
            }
        }
    }
}
