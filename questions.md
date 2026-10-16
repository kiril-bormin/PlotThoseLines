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

**Éléments attendus d'une réponse convaincante**

- Tous les anciens points compris **entre** le premier et le dernier point importés sont supprimés. Les mois de février à novembre 2020 disparaissent : il reste 2006-2019, janvier et décembre 2020, puis 2021-2026. C'est une perte de données, car la fusion se fait par plage et non par date.
- `plotSeries` est un `Dictionary` : `Add` avec une clé existante lève une `ArgumentException`. Et sans `Remove` sur le graphique, l'ancienne courbe resterait affichée sous la nouvelle.
- Si `dateFin` contient une heure (ex. 15:00), un ancien point du même jour à 00:00 a `.Date` < `dateFin` : il est supprimé. La comparaison est incohérente, mais sans effet pour des données journalières à minuit.

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

**Éléments attendus d'une réponse convaincante**

- `ItemCheck` est levé **avant** que l'état de la case ne change. `GetItemChecked` retournerait l'ancien état, alors que `NewValue` donne le futur état.
- `AddSerieToPlot` appelle `SetItemChecked`, ce qui déclenche `ItemCheck`. Sans le drapeau, `SaveData()` serait appelé à chaque série chargée, alors que `savedSeries` n'en contient qu'une partie (1, puis 2…). Les écritures sont inutiles et risquées : si le chargement plante au milieu, le fichier JSON est réécrit tronqué et des séries sont perdues.
- `finally` garantit la remise à `false` même en cas d'exception ; sinon l'application ne sauvegarderait plus jamais.
