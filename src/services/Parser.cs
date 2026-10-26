using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PlotThoseLines.Services
{
    public static class Parser
    {
        // Pour l'axe x (date)
        private static readonly string[] AliasDate = { "date", "timestamp", "datetime", "time" };

        // Pour l'axe y (valeur)
        private static readonly string[] AliasPrix = { "market_cap", "cap", "market", "market_capitalisation", "capitalisation" };

        // Méthode qui transforme les lignes de texte CSV en liste de points exploitables
        public static List<SavedPoint> Parse(IEnumerable<string> lines)
        {
            var lineList = lines.ToList();

            // Vérifier que le fichier contient au moins un en-tête et une ligne de données
            if (lineList.Count < 2)
                throw new InvalidDataException("Le fichier ne contient aucun point valide.");

            // Tableau d'en-têtes est découpé par virgule pour trouver les colonnes nécessaires
            string[] headers = lineList[0].Split(',');

            int colIndex = Array.FindIndex(headers, col => AliasPrix.Contains(col.Trim(), StringComparer.OrdinalIgnoreCase));
            int dateIndex = Array.FindIndex(headers, col => AliasDate.Contains(col.Trim(), StringComparer.OrdinalIgnoreCase));

            // Si la colonne de valeur n'existe pas, on arrête tout
            if (colIndex == -1)
                throw new InvalidDataException("Colonne de prix est introuvable dans ce fichier.");

            // Transformation des lignes en points via LINQ
            List<SavedPoint> points = lineList
                .Skip(1)                                                    
                .Where(line => !string.IsNullOrWhiteSpace(line)) // Ignore si la ligne est vide
                .Select(line => line.Split(',')) // Découpe la ligne par virgule
                .Where(cols => cols.Length > Math.Max(colIndex, dateIndex)) // S'assurer que les index existent bien sur cette ligne
                .Select(cols =>
                {
                    // Convertit le texte en une DateTime
                    bool dateOk = DateTime.TryParse(cols[dateIndex].Trim(), out DateTime dt);

                    // Convertit le texte en un nombre décimal (cultureInfo sert à utiliser les règles de formatage fixe)
                    bool valOk = double.TryParse(cols[colIndex].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val);

                    // Si les deux conversions réussissent, on crée le point, sinon on renvoie null
                    return dateOk && valOk ? new SavedPoint { Timestamp = dt, Value = val } : null;
                })
                .Where(point => point is not null) // Filtre et garde uniquement les points valides (élimine les null)
                .Select(point => point!) // Récupère les points
                .ToList();

            // Vérifier qu'au moins un point a pu être converti
            if (points.Count == 0)
                throw new InvalidDataException("Le fichier ne contient aucun point valide.");

            return points;
        }
    }
}
