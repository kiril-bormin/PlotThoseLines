# Questions de compréhension – Kiril Bormin

## Question 1 – Fusion d'un nouvel import dans une série existante

**Passage** : `src/MainForm.cs`, lignes 237-262

```csharp
DateTime dateDebut = nouveauxPoints.Min(p => p.Timestamp);
DateTime dateFin = nouveauxPoints.Max(p => p.Timestamp);

List<SavedPoint> anciensPointsConserves = serieExistante.Points
    .Where(p => p.Timestamp.Date < dateDebut || p.Timestamp.Date > dateFin)
    .ToList();

serieExistante.Points = anciensPointsConserves
    .Concat(nouveauxPoints)
    .OrderBy(p => p.Timestamp)
    .ToList();

if (plotSeries.TryGetValue(companyAcronym, out var ancienneCourbe))
{
    plot.Plot.Remove(ancienneCourbe);
    plotSeries.Remove(companyAcronym);
}
AddSerieToPlot(serieExistante);
```

- La série AAPL contient des données de 2006 à 2026. Vous importez un fichier AAPL qui ne contient que janvier 2020 et décembre 2020. Que contient la série après la fusion ? Est-ce le comportement voulu ?
- Pourquoi il faut retirer l'ancienne courbe et l'entrée du dictionnaire **avant** d'appeler `AddSerieToPlot`.

---

## Question 2 – `isLoadingData`

**Passage** : `src/MainForm.cs`, lignes 278-305 et 326-357 (`LoadData`)

```csharp
private void SeriesList_ItemCheck(object? sender, ItemCheckEventArgs e)
{
    string nom = seriesList.Items[e.Index].ToString()!;
    bool estVisible = e.NewValue == CheckState.Checked;
    ...
    if (!isLoadingData)
        SaveData();
}
...
isLoadingData = true;
foreach (SavedSerie serie in seriesChargees)
{
    savedSeries.Add(serie);
    AddSerieToPlot(serie);   // appelle seriesList.SetItemChecked(...)
}
...
finally { isLoadingData = false; }
```

- À quoi sert `isLoadingData` ? comment ça marche ? quel scénario est-il sensé éviter ?
- Pourquoi `isLoadingData = false` est-il dans un bloc `finally` ?
