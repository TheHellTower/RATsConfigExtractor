using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATsConfigExtractor.Interfaces
{
    internal interface IRatDetector
    {
        void Execute(string filePath);
    }
}
