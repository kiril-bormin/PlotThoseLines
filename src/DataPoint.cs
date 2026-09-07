using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotThoseLines
{
    public class DataPoint<Type>
    {
        public DateTime Timestamp { get; }
        public Type Value { get; }
        public DataPoint(DateTime timestamp, Type value)
        {
            Timestamp = timestamp;
            Value = value;
        }

    }
}
