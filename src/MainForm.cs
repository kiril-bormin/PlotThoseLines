using ScottPlot.WinForms;
using System.Globalization;
using System.Text.Json;

namespace PlotThoseLines
{
    public partial class MainForm : Form
    {
        private FormsPlot plot;
        private CheckedListBox seriesList;

        // Stocke les séries ajoutées dans ScottPlot
        private readonly Dictionary<string, ScottPlot.Plottables.Scatter> plotSeries = new();

        // Liste des séries sauvegardées
        private readonly List<SavedSerie> savedSeries = new();

        // Chemin de fichier (AppData/Local/PlotThoseLines/data.json)
        private readonly string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PlotThoseLines", "data.json"); 

        private bool isLoadingData = false;
        public MainForm()
        {
            InitializeComponent();

            // Instancier la liste des graphiques
            seriesList = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true
            };

            seriesList.ItemCheck += SeriesList_ItemCheck;

            plot = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            // Transformer des grandes nombres en nombre + texte (ex.: 1.2T)
            ScottPlot.TickGenerators.NumericAutomatic tickGenerator = new()
            {
                LabelFormatter = FormaterCapitalisation
            };

            plot.Plot.Axes.Left.TickGenerator = tickGenerator;

            // Le conteneur commun
            SplitContainer splitContainer = new SplitContainer // paramètres de la fenêtre d'affichage
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Panel1MinSize = 100, // largeur minimale du premier composant
                FixedPanel = FixedPanel.Panel1,
            };

            // Menus
            GroupBox groupBox = new GroupBox
            {
                Text = "Graphiques",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            Panel listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5, 5, 5, 10)
            };

            // Supprimer bouton
            Button deleteButton = new Button
            {
                Text = "Supprimer",
                Dock = DockStyle.Bottom,
                Height = 25
            };

            deleteButton.Click += (sender, e) =>
                DeleteSelectedSerie();

            // Reset zoom
            Button resetZoomButton = new Button
            {
                Text = "Réinit. le zoom",
                Dock = DockStyle.Bottom,
                Height = 25
            };
            resetZoomButton.Click += (sender, e) =>
            {
                plot.Plot.Axes.AutoScale();
                plot.Refresh();
            };

            // Export en png
            Button exportPngButton = new Button
            {
                Text = "Exporter en png",
                Dock = DockStyle.Bottom,
                Height = 25
            };
            exportPngButton.Click += (sender, e) => ExportToPng();


            seriesList.Dock = DockStyle.Fill;

            // Ajout des composants
            groupBox.Controls.Add(seriesList);
            groupBox.Controls.Add(deleteButton);
            groupBox.Controls.Add(resetZoomButton);
            groupBox.Controls.Add(exportPngButton);
            listPanel.Controls.Add(groupBox);

            splitContainer.Panel1.Controls.Add(listPanel);
            splitContainer.Panel2.Controls.Add(plot);

            Controls.Add(splitContainer);

            LoadMenu();
            LoadData();

            // Définir la taille de fenêtre par défaut
            Shown += (sender, e) =>
            {
                splitContainer.SplitterDistance = 130;
            };
        }
        // Charger le menu ruban
        public void LoadMenu()
        {
            MenuStrip menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top
            }; // Instancier le menu

            ToolStripMenuItem menuFichier = new ToolStripMenuItem("Fichier"); // Instancier un élément du menu
            ToolStripMenuItem itemImporter = new ToolStripMenuItem("Importer un fichier");

            menuFichier.DropDownItems.Add(itemImporter); // Ajout du bouton au sous-menu
            menuStrip.Items.Add(menuFichier); // Ajout du bouton au menu 

            itemImporter.Click += (sender, e) => ImportFile();

            this.MainMenuStrip = menuStrip; // Déclaration du menu 
            this.Controls.Add(menuStrip); // Ajout visuel
        }
        // Méthode pour l'import des données depuis un fichier
        public void ImportFile()
        {
            using OpenFileDialog choiceDialog = new OpenFileDialog()
            {
                Title = "Séléctionner un fichier de format .csv",
                Filter = "Fichiers supportés (*.csv)|*.csv"
            };
            if (choiceDialog.ShowDialog() == DialogResult.OK)
            {
                // Pour l'axe y (date)
                string[] aliasDate = { "date", "timestamp", "datetime", "time" };

                // Pour l'axe y (valeur)
                string[] aliasPrix = { "market_cap", "cap", "market", "market_capitalisation", "capitalisation" };
                try
                {
                    string extension = Path.GetExtension(choiceDialog.FileName).ToLower();
                    string serieName = Path.GetFileNameWithoutExtension(choiceDialog.FileName);
                    string companyAcronym = serieName.Split("_")[0]; // Garde seulement l'acronyme commercial de l'entreprise

                    List<DataPoint<double>> points = new();

                    if (extension == ".csv")
                    {
                        var lines = File.ReadAllLines(choiceDialog.FileName);

                        string[] headers = lines.First().Split(',');

                        int colIndex = Array.FindIndex(headers, col => aliasPrix.Contains(col.Trim(), StringComparer.OrdinalIgnoreCase));
                        int dateIndex = Array.FindIndex(headers, col => aliasDate.Contains(col.Trim(), StringComparer.OrdinalIgnoreCase));
                        // Tableau d'en-têtes est parcouru, pour trouver les colonnes nécessaires.

                        if (colIndex == -1)
                            throw new InvalidDataException("Colonne de prix est introuvable dans ce fichier.");

                        foreach (string line in lines.Skip(1))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue; // Ignore si la ligne est vide

                            string[] colonnes = line.Split(","); // Découpe la ligne

                            bool dateOk = DateTime.TryParse(colonnes[dateIndex].Trim(), out DateTime dt); // Convertit le texte en une DateTime
                            bool valOk = double.TryParse(colonnes[colIndex].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val);
                            // Convertit le texte en un nombre décimal (cultureInfo sert à utiliser les règles de formatage fixe)

                            if (dateOk && valOk)
                            {
                                points.Add(new DataPoint<double>(dt, val));
                            }
                        }

                        // Création de la DataSerie
                        DataSerie<double> serie = DataSerie<double>.From(serieName, points);


                        // Retrouve la série existante
                        SavedSerie? serieExistante = savedSeries.FirstOrDefault(s => s.Name == companyAcronym);


                        // Créer l'objets qui sera sauvegardé dans le JSON
                        List<SavedPoint> nouveauxPoints = points
                            .Select(p => new SavedPoint
                            {
                                Timestamp = p.Timestamp,
                                Value = p.Value
                            })
                            .ToList();

                        if (nouveauxPoints.Count == 0)
                        {
                            throw new InvalidDataException(
                            "Le fichier ne contient aucun point valide.");
                        }

                        if (serieExistante is null)
                        {
                            SavedSerie nouvelleSerie = new SavedSerie
                            {
                                Name = companyAcronym,
                                IsVisible = true,
                                Points = nouveauxPoints
                            };

                            savedSeries.Add(nouvelleSerie);
                            AddSerieToPlot(nouvelleSerie);
                        }
                        else
                        {
                            DateTime dateDebut = nouveauxPoints.Min(p => p.Timestamp);
                            DateTime dateFin = nouveauxPoints.Max(p => p.Timestamp);

                            List<SavedPoint> anciensPointsConserves = serieExistante.Points
                                .Where(p =>
                                    p.Timestamp.Date < dateDebut ||
                                    p.Timestamp.Date > dateFin)
                                .ToList();

                            // Fusionner et trier les données
                            serieExistante.Points = anciensPointsConserves
                                .Concat(nouveauxPoints)
                                .OrderBy(p => p.Timestamp)
                                .ToList();

                            // Retirer l'ancienne courbe du graphique
                            if (plotSeries.TryGetValue(
                            companyAcronym,
                            out var ancienneCourbe))
                            {
                                plot.Plot.Remove(ancienneCourbe);
                                plotSeries.Remove(companyAcronym);
                            }

                            // Recréer la courbe avec les données fusionnées
                            AddSerieToPlot(serieExistante);
                        }
                    }
                    SaveData();

                    plot.Plot.Axes.DateTimeTicksBottom();
                    plot.Plot.Axes.AutoScale();
                    plot.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la lecture du fichier :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // Masquer / afficher le graphique
        private void SeriesList_ItemCheck(object? sender,ItemCheckEventArgs e)
        {
            // Récupérer le nom de la série cliquée
            string nom = seriesList.Items[e.Index].ToString()!;

            // Savoir si la case va être cochée ou décochée
            bool estVisible = e.NewValue == CheckState.Checked;

            // Afficher ou masquer la courbe
            if (plotSeries.ContainsKey(nom))
            {
                var serie = plotSeries[nom];
                serie.IsVisible = estVisible;
            }

            // Retrouver la série dans les données sauvegardées
            SavedSerie? serieSauvegardee = savedSeries.FirstOrDefault(serie => serie.Name == nom);
            // Sauvegarder son nouvel état
            if (serieSauvegardee != null)
            {
                serieSauvegardee.IsVisible = estVisible;
            }

            plot.Plot.Axes.AutoScale();
            plot.Refresh();
            if (!isLoadingData)
                SaveData();
        }
        // Sauvegarde des données en json
        private void SaveData()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(savedSeries, options);

            string folder = Path.GetDirectoryName(savePath)!;

            Directory.CreateDirectory(folder);

            File.WriteAllText(savePath, json);
        }
        // Charger les données du fichier JSON
        private void LoadData()
        {
            // Ne rien faire si le fichier JSON n'existe pas
            if (!File.Exists(savePath))
                return;
            try
            {
                // Évite de sauvegarder pendant le chargement
                isLoadingData = true;

                // Lire le contenu du fichier
                string json = File.ReadAllText(savePath);

                // Transformer le JSON en liste de séries
                List<SavedSerie>? seriesChargees = JsonSerializer.Deserialize<List<SavedSerie>>(json);

                if (seriesChargees == null)
                    return;

                // Ajouter chaque série dans l'application
                foreach (SavedSerie serie in seriesChargees)
                {
                    savedSeries.Add(serie);
                    AddSerieToPlot(serie);
                }

                plot.Plot.Axes.DateTimeTicksBottom();
                plot.Plot.Axes.AutoScale();
                plot.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données :\n{ex.Message}","Erreur",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            finally
            {
                // Le chargement est terminé
                isLoadingData = false;
            }
        }
        // Rajout des séries sur le graph
        private void AddSerieToPlot(SavedSerie savedSerie)
        {
            double[] dates = savedSerie.Points
                .Select(point => point.Timestamp.ToOADate())
                .ToArray();
            double[] values = savedSerie.Points
                .Select(point => point.Value)
                .ToArray();

            var scottSerie = plot.Plot.Add.Scatter(dates, values);

            scottSerie.LegendText = savedSerie.Name;
            scottSerie.IsVisible = savedSerie.IsVisible;

            plotSeries.Add(savedSerie.Name, scottSerie);

            if (!seriesList.Items.Contains(savedSerie.Name))
            {
                int index = seriesList.Items.Add(savedSerie.Name);

                seriesList.SetItemChecked(
                index,
                savedSerie.IsVisible);
            }
        }
        // Supprimer une série de graphique et de fichier json
        private void DeleteSelectedSerie()
        {
            // Vérifier qu'une série est sélectionnée
            if (seriesList.SelectedItem == null)
            {
                MessageBox.Show(
                    "Sélectionnez une série à supprimer.",
                    "Aucune sélection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // Nom de la série à supprimer
            string nom = seriesList.SelectedItem.ToString()!;

            // Demander confirmation
            DialogResult result = MessageBox.Show(
                $"Voulez-vous vraiment supprimer {nom} ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            // Supprimer la courbe du graphique
            if (plotSeries.TryGetValue(nom, out var courbe))
            {
                plot.Plot.Remove(courbe);
                plotSeries.Remove(nom);
            }

            // Supprimer les données sauvegardées   
            SavedSerie? serieSauvegardee = savedSeries.FirstOrDefault(serie => serie.Name == nom);

            if (serieSauvegardee != null)
                savedSeries.Remove(serieSauvegardee);

            // Supprimer l'élément de la liste
            seriesList.Items.Remove(nom);

            // Mettre à jour le json
            SaveData();

            plot.Plot.Axes.AutoScale();
            plot.Refresh();
        }
        // Exporter image en pdf
        private void ExportToPng()
        {
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Exporter le graphique",
                Filter = "Image PNG (*.png)|*.png",
                FileName = "chart.png"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                plot.Plot.SavePng(
                    dialog.FileName,
                    plot.Width,
                    plot.Height);

                MessageBox.Show(
                    "Le graphique a été exporté avec succès.",
                    "Export terminé",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        // Transformer des grandes nombres en nombre + texte (ex.: 1.2T)
        private static string FormaterCapitalisation(double valeur)
        {
            if (valeur >= 1_000_000_000_000)
            {
                return $"{valeur / 1_000_000_000_000:0.##} T";
            }

            if (valeur >= 1_000_000_000)
            {
                return $"{valeur / 1_000_000_000:0.##} Md";
            }

            if (valeur >= 1_000_000)
            {
                return $"{valeur / 1_000_000:0.##} M";
            }

            return valeur.ToString("0");
        }
    }
}