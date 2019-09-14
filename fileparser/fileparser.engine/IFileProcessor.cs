using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fileparser.engine
{
    public interface IFileProcessor
    {
        Task ProcessLinesAsync(string inputfilepath);
    }
}
