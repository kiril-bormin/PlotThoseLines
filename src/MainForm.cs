using ScottPlot.WinForms;
using System.Globalization;

namespace PlotThoseLines
{
    public partial class MainForm : Form
    {
        private FormsPlot plot;
        private CheckedListBox seriesList;

        // stocke les séries ajoutées dans ScottPlot
        private readonly Dictionary<string, ScottPlot.Plottables.Scatter> plotSeries = new();
        public MainForm()
        {
            InitializeComponent();
            LoadMenu();

            // instancier la liste des graphiques
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

            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300,
                FixedPanel = FixedPanel.Panel1
            };
            splitContainer.Panel1.Controls.Add(seriesList);
            splitContainer.Panel2.Controls.Add(plot);

            Controls.Add(splitContainer);
        }
        public void LoadMenu()
        {
            MenuStrip menuStrip = new MenuStrip(); // instancier le menu

            ToolStripMenuItem menuFichier = new ToolStripMenuItem("Fichier"); // instancier un élément du menu
            ToolStripMenuItem itemImporter = new ToolStripMenuItem("Importer un fichier");

            menuFichier.DropDownItems.Add(itemImporter); // Ajout du bouton au sous-menu
            menuStrip.Items.Add(menuFichier); // Ajout du bouton au menu 

            itemImporter.Click += (sender, e) => ImportFile();

            this.MainMenuStrip = menuStrip; // déclaration du menu 
            this.Controls.Add(menuStrip); // ajout visuel
        }
        public void ImportFile()
        {
            using OpenFileDialog choiceDialog = new OpenFileDialog()
            {
                Title = "Séléctionner un fichier de format .csv",
                Filter = "Fichiers supportés (*.csv)|*.csv"
            };
            if (choiceDialog.ShowDialog() == DialogResult.OK)
            {
                // pour l'axe y (date)
                string[] aliasDate = { "date", "timestamp", "datetime", "time" };

                // pour l'axe y (valeur)
                string[] aliasPrix = { "market_cap", "cap", "market", "market_capitalisation", "capitalisation" };
                try
                {
                    string extension = Path.GetExtension(choiceDialog.FileName).ToLower();
                    string serieName = Path.GetFileNameWithoutExtension(choiceDialog.FileName);
                    List<DataPoint<double>> points = new();

                    if (extension == ".csv")
                    {
                        var lines = File.ReadAllLines(choiceDialog.FileName);

                        string[] headers = lines.First().Split(',');

                        int colIndex = Array.FindIndex(headers, col => aliasPrix.Contains(col.Trim(), StringComparer.OrdinalIgnoreCase));
                        int dateIndex = Array.FindIndex(headers, col => aliasDate.Contains(col.Trim(), StringComparer.OrdinalIgnoreCase));
                        // tableau d'en-têtes est parcouru, pour trouver les colonnes nécessaires.

                        if (colIndex == -1)
                            throw new InvalidDataException("Colonne de prix est introuvable dans ce fichier.");

                        foreach (string line in lines.Skip(1))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue; // ignore si la ligne est vide

                            string[] colonnes = line.Split(","); // découpe la ligne

                            bool dateOk = DateTime.TryParse(colonnes[dateIndex].Trim(), out DateTime dt); // convertit le texte en une DateTime
                            bool valOk = double.TryParse(colonnes[colIndex].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val);
                            // convertit le texte en un nombre décimal (cultureInfo sert à utiliser les règles de formatage fixe)

                            if (dateOk && valOk)
                            {
                                points.Add(new DataPoint<double>(dt, val));
                            }
                        }

                        // création de datasérie
                        DataSerie<double> Serie = DataSerie<double>.From(serieName, points);
                        double[] x = Serie.Dates.Select(x => x.ToOADate()).ToArray();
                        double[] y = Serie.Values.ToArray();

                        var scottSerie = plot.Plot.Add.Scatter(x, y); 
                        scottSerie.LegendText = serieName; 

                        string uniqueName = GetUniqueSerieName(serieName);

                        scottSerie.LegendText = uniqueName; // affichage la légende
                        

                        plotSeries.Add(uniqueName, scottSerie); // affichage de la série 
                        seriesList.Items.Add(uniqueName, true); // ajout du graphique dans la liste 

                    }

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
        private string GetUniqueSerieName(string originalName)
        {
            string name = originalName;
            int number = 2;

            while (plotSeries.ContainsKey(name))
            {
                name = $"{originalName} ({number})";
                number++;
            }

            return name;
        }
        private void SeriesList_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            string serieName = seriesList.Items[e.Index].ToString()!;

            if (plotSeries.TryGetValue(serieName, out var serie))
            {
                serie.IsVisible = e.NewValue == CheckState.Checked;

                plot.Plot.Axes.AutoScale();
                plot.Refresh();
            }
        }
    }
}