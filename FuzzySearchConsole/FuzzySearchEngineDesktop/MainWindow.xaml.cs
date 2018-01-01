using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using FuzzySearchConsole;
namespace FuzzySearchEngineDesktop
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        QGramIndex qgi;
        public MainWindow()
        {
            qgi = new QGramIndex(3);
            InitializeComponent();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.Text = String.Empty;
            box.GotFocus -= TextBox_GotFocus;
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox box = sender as TextBox;
            if(String.IsNullOrWhiteSpace(box.Text))
            {
                resultsCountLabel.Visibility = Visibility.Hidden;
                timeLabel.Visibility = Visibility.Hidden;
            }
            string query = QGramIndex.NormalizeString(box.Text);
            int delta;
            try
            {
                delta = int.Parse(deltaTextBox.Text);
            }
            catch(Exception)
            {
                delta = query.Length / 4;
            }
            DateTime start = DateTime.Now;
            var results = QGramIndex.RankMatches(qgi.FindMatches(query, delta).Key);
            DateTime end = DateTime.Now;

            resultsCountLabel.Content = String.Format("{0} result{1}", results.Count, results.Count == 1 ? ' ' : 's');
            resultsCountLabel.Visibility = Visibility.Visible;
            timeLabel.Content = String.Format("{0} millisec.", (end - start).Milliseconds);
            timeLabel.Visibility = Visibility.Visible;

            resultsListView.Items.Clear();
            foreach(var result in results)
            {
                resultsListView.Items.Add(new SingleResult() { Name = result.Key.name,
                    PED = result.Value.ToString(), Desc = result.Key.description});
            }

        }
        class SingleResult
        {
            public string Name { get; set; }
            public string PED { get; set; }
            public string Desc { get; set; }
        }

        private void DeltaTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox_KeyUp(queryTextBox, e);
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            qgi.BuildFromFile("../../../wikidata.tsv");
            queryTextBox.IsEnabled = true;
            queryTextBox.Text= "Type something...";
            deltaTextBox.IsEnabled = true;
        }
    }
}
