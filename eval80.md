# Évaluation intermédiaire (80 %) – Kiril Bormin

> Mise à jour du 16.10.2026 : les méthodes d'extension ne sont **pas exigées** (notion non vue en cours), même si le CdC les mentionne.

---

## En bref

Application fonctionnelle et agréable : import CSV avec détection des colonnes, fusion avec remplacement des dates communes, masquage, suppression, réinitialisation du zoom, export PNG, formatage des grands nombres (T / Md / M), sauvegarde JSON. Données abondantes (10 séries de 4000 à 5000 valeurs). **Mais les exigences techniques du CdC ne sont pas encore traitées** : aucun test, des `foreach`, toute la logique dans `MainForm.cs`. **Le rapport est encore au stade du modèle et le PDF du rapport n'est pas joint à la release 80 %.**

## Axes de travail prioritaires pour la finalisation

1. **Ajouter un projet de tests** (≥ 3 tests) après avoir extrait la logique de `MainForm` : parsing CSV (colonnes trouvées / absentes), fusion avec chevauchement de dates, formatage des grands nombres.
2. **Remplacer les `foreach` par du LINQ** (lecture des lignes CSV avec `Skip(1).Where(...).Select(...)`, chargement JSON).
3. **Compléter le rapport et le livrer en PDF** dans la release : rapport de tests, usage de l'IA (le script de scraping Yahoo généré par IA est à mentionner, avec exemples), bilans, conclusion, et suppression des sections modèle inutiles.
4. Reconvertir `MainForm.cs` en UTF-8 et utiliser (ou supprimer) `DataSerie<T>`.
5. Mettre à jour les US 1, 5 et 6 (JSON, API, saisie manuelle) : les réaliser ou indiquer clairement dans le bilan qu'elles sont abandonnées.
