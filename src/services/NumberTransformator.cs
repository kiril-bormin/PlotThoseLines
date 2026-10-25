using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotThoseLines.Services
{
    public static class NumberFormatter
    {
        // Transformer des grands nombres en nombre + texte (ex.: 1.2T, 500 Md)
        public static string Format(double valeur)
        {
            if (valeur >= 1_000_000_000_000 || valeur <= -1_000_000_000_000)
                return $"{valeur / 1_000_000_000_000:0.##} T";

            if (valeur >= 1_000_000_000 || valeur <= -1_000_000_000)
                return $"{valeur / 1_000_000_000:0.##} Md";

            if (valeur >= 1_000_000 || valeur <= -1_000_000)
                return $"{valeur / 1_000_000:0.##} M";

            return valeur.ToString("N0");
        }
    }
}
