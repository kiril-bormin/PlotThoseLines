using System;
using System.Collections.Generic;
using System.Linq;

namespace PlotThoseLines.Services
{
    public static class SeriesMerge
    {
        // Fusionne les données existantes et nouvelles en remplaçant la plage de dates communes
        public static List<SavedPoint> Merge(List<SavedPoint> anciensPoints, List<SavedPoint> nouveauxPoints)
        {
            // Si la série existante n'a pas de points, on renvoie directement les nouveaux triés
            if (anciensPoints == null || anciensPoints.Count == 0)
                return nouveauxPoints.OrderBy(p => p.Timestamp).ToList();

            // Si le nouvel import est vide, on garde les anciens
            if (nouveauxPoints == null || nouveauxPoints.Count == 0)
                return anciensPoints;

            // Trouver la date de début et de fin du nouveau fichier
            DateTime dateDebut = nouveauxPoints.Min(p => p.Timestamp);
            DateTime dateFin = nouveauxPoints.Max(p => p.Timestamp);

            // Conserver uniquement les anciens points situés STRICTEMENT avant ou après cette nouvelle période
            List<SavedPoint> anciensPointsConserves = anciensPoints
                .Where(p =>
                    p.Timestamp.Date < dateDebut.Date ||
                    p.Timestamp.Date > dateFin.Date)
                .ToList();

            // Fusionner les anciens conservés avec les nouveaux points, et trier par date chronologique
            return anciensPointsConserves
                .Concat(nouveauxPoints)
                .OrderBy(p => p.Timestamp)
                .ToList();
        }
    }
}
