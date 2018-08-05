using FuzzySearchConsole;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;

namespace FuzzySearchEngineDesktop
{

    public class MainWindowViewModel : BindableObject
    {
        QGramIndex qgi;
        public MainWindowViewModel()
        {
            qgi = new QGramIndex(3);
            qgi.BuildFromFile("../../../wikidata.tsv");
            DatabaseNotLoaded = false;
            TextToSearch = string.Empty;
            ResultsCollection = new ObservableCollection<SingleResult>();

        }

        public ObservableCollection<SingleResult> ResultsCollection { get; set; }
        private bool _databaseNotLoaded = true;
        public bool DatabaseNotLoaded
        {
            get
            {
                return _databaseNotLoaded;
            }
            set
            {
                _databaseNotLoaded = value;
                OnPropertyChanged(nameof(DatabaseNotLoaded));
            }
        }

        private string _textToSearch; 
        public string TextToSearch
        {
            get
            {
                return _textToSearch;
            }
            set
            {
                _textToSearch = value;
                OnPropertyChanged(nameof(TextToSearch));
                SearchInQGramIndex();
            }
        }

        private string _deltaValueText;
        public string DeltaValueText
        {
            get
            {
                return _deltaValueText;
            }
            set
            {
                _deltaValueText = value;
                OnPropertyChanged(nameof(DeltaValueText));
                SearchInQGramIndex();
            }
        }
        private string _searchTime;
        public string SearchTime { get
            {
                return _searchTime;
            }
            set
            {
                _searchTime = value;
                OnPropertyChanged(nameof(SearchTime));
            }
        }
        private string _resultsCount;
        public string ResultsCount
        {
            get
            {
                return _resultsCount;
            }
            set
            {
                _resultsCount = value;
                OnPropertyChanged(nameof(ResultsCount));
            }
        }

        private void SearchInQGramIndex()
        {
            DateTime start = DateTime.Now;
            var results = GetResultsFromQGramIndex(TextToSearch, DeltaValueText);
            DateTime end = DateTime.Now;

            ResultsCount = String.Format("{0} result{1}", results.Count, results.Count == 1 ? ' ' : 's');
            SearchTime = String.Format("{0} millisec.", (end - start).Milliseconds);

            RefreshResultCollection(results);
        }

        private void RefreshResultCollection(List<KeyValuePair<Entity, int>> results)
        {
            ResultsCollection = new ObservableCollection<SingleResult>();
            foreach (var result in results)
            {
                ResultsCollection.Add(new SingleResult()
                {
                    Name = result.Key.name,
                    PED = result.Value.ToString(),
                    Desc = result.Key.description
                });
            }

            OnPropertyChanged(nameof(ResultsCollection));
        }

        private List<KeyValuePair<Entity, int>> GetResultsFromQGramIndex(string textToSearch, string deltaValueText)
        {
            string query = QGramIndex.NormalizeString(TextToSearch);
            int delta;
            try
            {
                delta = int.Parse(deltaValueText);
            }
            catch (Exception)
            {
                delta = query.Length / 4;
            }
            return QGramIndex.RankMatches(qgi.FindMatches(query, delta).Key);
        }
    }
}
