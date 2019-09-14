using System.Collections.Generic;
using System.Threading.Tasks;

namespace fileparser.wordfrequencycaclculation
{
    public interface IWordFrequencyCalculator
    {
        void MergeInfo(IDictionary<string, int> info);

        Task FlushAsync();
    }
}
