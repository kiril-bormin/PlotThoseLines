using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotThoseLines
{
    public class DataSerie<Type>
    {
        public string Name { get; } // nom de la serie
        private readonly IReadOnlyList<DataPoint<Type>> _points; // conserve les données dans la mémoire de manière non modifiable, et permet d'y acceder instantanément 
        private DataSerie(string name, IEnumerable<DataPoint<Type>> points) // constructeur
        {
            Name = name;
            _points = points.ToList();
        }
        public static DataSerie<Type> From(string name, IEnumerable<DataPoint<Type>> points) // fabrique et renvoie directement une nouvelle série à partir du nom et des points fournis. (créetion d'un objet) 
        => new(name, points);

        public IReadOnlyList<DataPoint<Type>> Points => _points; // accès à la liste complète (date et values)
        public IEnumerable<DateTime> Dates => _points.Select(p => p.Timestamp); // accès au dates
        public IEnumerable<Type> Values => _points.Select(p => p.Value); // accès au valuers
        public int Count => _points.Count; // renvoie le nombre d'éléments
    }
}
