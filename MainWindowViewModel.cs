using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace roddb
{
    [ObservableObject]
    public partial class MainWindowViewModel
    {
        private readonly WebView2 _webView2;
        private readonly Dispatcher _uiDispatcher;

        public MainWindowViewModel(WebView2 webView2, Dispatcher uiDispatcher)
        {
            var dbLines = File.ReadAllLines(@"RoD_Item.txt").Skip(3);
            foreach (var line in dbLines)
            {
                if (line.Contains('|'))
                {
                    var entries = line.Split('|');
                    var item = RoDItem.FromEntries(entries);
                    _allItems.Add(item);
                }
            }

            timer.Tick += (o, e) => {
                ActualSearch = _searchText;
                timer.Stop();
            };
            _webView2 = webView2;
            _uiDispatcher = uiDispatcher;
        }

        [ObservableProperty]
        private ObservableCollection<RoDItem> _allItems = new();

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(AllItemsCVS))]
        private string _actualSearch;

        [ObservableProperty]
        private RoDItem? _selectedItem;

        private DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(800) };

        private RelayCommand<RoDItem> _rodpSearchCommand;

        private string _currentRoDPediaSearch = string.Empty;

        public MainWindow MainWindow { get; internal set; }
        
        public ICollectionView AllItemsCVS
        {
            get
            {
                var cvs = new CollectionViewSource();
                cvs.Source = _allItems;

                cvs.SortDescriptions.Add(new SortDescription(nameof(RoDItem.WearLoc), ListSortDirection.Ascending));
                cvs.SortDescriptions.Add(new SortDescription(nameof(RoDItem.Name), ListSortDirection.Ascending));

                cvs.GroupDescriptions.Add(new PropertyGroupDescription(nameof(RoDItem.WearLoc)));

                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    var terms = ParseTerms(_searchText);
                    cvs.Filter += (o, e) =>
                    {
                        var item = (RoDItem)e.Item;
                        e.Accepted = terms.All(z => z.Predicate(item));
                    };
                }

                return cvs.View;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            timer.Stop();
            timer.Start();
        }
        
        public record SearchTerm(Predicate<RoDItem> Predicate);

        private SearchTerm[] ParseTerms(string searchText)
        {
            // split into words
            var words = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            var ret = new List<SearchTerm>();
            foreach (var word in words)
            {
                if (word.Contains('>') || word.Contains('<')) // || word.Contains('<') || word.Contains(':')
                {
                    var match = Regex.Match(word, @"^(?<stat>[\w\W]*?)(?<comp>[><])(?<amt>\d*)$");
                    if (match.Success is false)
                    {
                        // Should probably warn about a dropped token
                        continue;
                    }
                    
                    var stat = match.Groups["stat"].Value;
                    var greaterThan = match.Groups["comp"].Value == ">" ? true : false;
                    var amt = int.Parse(match.Groups["amt"].Value);
                    

                    ret.Add(new SearchTerm(item => item.GetStats().ContainsKey(stat) && item.GetStats()[stat] is int statAmt && greaterThan switch
                    {
                        true => statAmt > amt,
                        false => statAmt < amt
                    }));
                }
                else if (word.Contains(':'))
                {
                    // in this case we are looking for a partial string match similar to a regular search
                    var match = Regex.Match(word, @"^(?<stat>[\w\W]*?):(?<key>[\w\W]*?)$");
                    if (match.Success is false)
                    {
                        // Should probably warn about a dropped token
                        continue;
                    }

                    var stat = match.Groups["stat"].Value;
                    var key = match.Groups["key"].Value;

                    ret.Add(new SearchTerm(item => item.GetStats().ContainsKey(stat) && item.GetStats()[stat].ToString().Contains(key)));
                }
                else
                {
                    ret.Add(new SearchTerm(item => item.Name.Contains(word, StringComparison.OrdinalIgnoreCase)));
                }
            }
            return ret.ToArray();
        }

        [RelayCommand]
        private void PerformRoDPediaSearch(RoDItem item)
        {
            var url = $"https://rodpedia.realmsofdespair.info/index.php?search={item.Name}";
            _currentRoDPediaSearch = item.Name;
            _webView2.Source = new Uri(url);

            MainWindow.tabControl.SelectedItem = MainWindow.tabBrowser;
            
        }
    }
}
