using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotThoseLines
{
    public class SavedSerie
    {
        public string Name { get; set; } = "";
        public bool IsVisible { get; set; } = true;
        public List<SavedPoint> Points { get; set; } = new();
    }

    public class SavedPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }
}