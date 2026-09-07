using ScottPlot.WinForms;

namespace PlotThoseLines
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // 1. Création et ancrage du graphique en plein écran
            var plot = new FormsPlot { Dock = DockStyle.Fill };
            Controls.Add(plot);

            // 2. Ajout de données d'exemple
            double[] testData = { 12.5, 14.0, 11.8, 15.2, 13.9, 16.0, 12,2143,54,3543,5,43,5,435,34,5,34,5,435,34,3,43,4,3 };
            plot.Plot.Add.Signal(testData);
            double[] testData2 = { 12.52, 14.20, 121.8, 15.22, 213.9, 126.0, 12, 2143, 524, 3543,2, 43, 52, 435, 324, 5, 342, 5, 435, 34, 3, 43, 42, 3 };
            plot.Plot.Add.Signal(testData2);

            // 3. Rafraîchissement du tracé
            plot.Refresh();
        }
    }
}