using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace roddb
{
    [ObservableObject]
    public partial class MainWindowViewModel
    {
        public MainWindowViewModel()
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
        }

        public WebView2 webView2;

        [ObservableProperty]
        private ObservableCollection<RoDItem> _allItems = new();

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(AllItemsCVS))]
        private string _actualSearch;

        [ObservableProperty]
        private RoDItem _selectedItem;

        private DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(800) };

        partial void OnSearchTextChanged(string value)
        {
            timer.Stop();
            timer.Start();
        }

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
                    cvs.Filter += (o, e) =>
                    {
                        var item = (RoDItem)e.Item;
                        if (item.Name.IndexOf(_searchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            e.Accepted = true;
                        }
                        else
                        {
                            e.Accepted = false;
                        }
                    };
                }


                return cvs.View;
            }
        }

        private RelayCommand<RoDItem> _rodpSearchCommand;
        public IRelayCommand<RoDItem> rodpSearchCommand => _rodpSearchCommand ??= new RelayCommand<RoDItem>(rodsearch);

        public MainWindow MainWindow { get; internal set; }

        private void rodsearch(RoDItem item)
        {
            var url = $"https://rodpedia.realmsofdespair.info/wiki/{item.Name}";
            webView2.Source = new Uri(url);

            MainWindow.tabControl.SelectedItem = MainWindow.tabBrowser;
            
        }
    }

    public record struct RoDItem(
        string WearLoc, string Name, int? Level, int? AC, int? NegAC,
        int? d1, int? d2, int? HR, int? DR, int? HP, int? MP, int? Str,
        int? Dex, int? Con, int? Int, int? Wis, int? Lck, int? Cha, int? Moves,
        int? Weight, int? Value, string Properties, string Other, int? Price)
    {

        public IEnumerable<KeyValuePair<string, string>> StatValues
        {
            get
            {
                if (Weight is not null)
                {
                    yield return new("Weight", $"{Weight}");
                }
                if (Value is not null)
                {
                    yield return new("Value", $"{Value}");
                }
                if (Price is not null)
                {
                    yield return new("Price", $"{Value}");
                }
                if (d1 is not null && d2 is not null)
                {
                    yield return new ("Damage", $"{d1}-{d2}");
                }
                if (HR is not null)
                {
                    yield return new ("HitRoll", $"{HR}");
                }
                if (DR is not null)
                {
                    yield return new ("DamRoll", $"{DR}");
                }
                if (HP is not null)
                {
                    yield return new ("HP", $"{HP}");
                }
                if (MP is not null)
                {
                    yield return new ("MP", $"{MP}");
                }
                if (Moves is not null)
                {
                    yield return new("Moves", $"{Moves}");
                }
                if (Str is not null)
                {
                    yield return new("Strength", $"{Str}");
                }
                if (Dex is not null)
                {
                    yield return new("Dexterity", $"{Dex}");
                }
                if (Con is not null)
                {
                    yield return new("Constitution", $"{Con}");
                }
                if (Wis is not null)
                {
                    yield return new("Wisdom", $"{Wis}");
                }
                if (Int is not null)
                {
                    yield return new("Intelligence", $"{Int}");
                }
                if (Lck is not null)
                {
                    yield return new("Luck", $"{Lck}");
                }
                if (string.IsNullOrWhiteSpace(Properties) is false)
                {
                    yield return new("Properties", Properties);
                }
                if (string.IsNullOrWhiteSpace(Other) is false)
                {
                    yield return new("Other", Other);
                }
            }
        }

        private static int? SafeParseInt(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            if (int.TryParse(input, out int ret))
            {
                return ret;
            }
            return null;
        }

        public static RoDItem FromEntries(string[] entries)
        {
            var ret = new RoDItem();
            for (var i = 0; i < entries.Length; i++)
            {
                var cval = entries[i].Trim();
                switch (i)
                {
                    case 0:
                        if (cval == "*")
                        {
                            cval = "All";
                        }
                        ret.WearLoc = cval;
                        break;
                    case 1:
                        ret.Name = cval;
                        break;
                    case 2:
                        ret.Level = SafeParseInt(cval);
                        break;
                    case 3:
                        ret.AC = SafeParseInt(cval);
                        break;
                    case 4:
                        ret.NegAC = SafeParseInt(cval);
                        break;
                    case 5:
                        ret.d1 = SafeParseInt(cval);
                        break;
                    case 6:
                        ret.d2 = SafeParseInt(cval);
                        break;
                    case 7:
                        ret.HR = SafeParseInt(cval);
                        break;
                    case 8:
                        ret.DR = SafeParseInt(cval);
                        break;
                    case 9:
                        ret.HP = SafeParseInt(cval);
                        break;
                    case 10:
                        ret.MP = SafeParseInt(cval);
                        break;
                    case 11:
                        ret.Str = SafeParseInt(cval);
                        break;
                    case 12:
                        ret.Dex = SafeParseInt(cval);
                        break;
                    case 13:
                        ret.Con = SafeParseInt(cval);
                        break;
                    case 14:
                        ret.Int = SafeParseInt(cval);
                        break;
                    case 15:
                        ret.Wis = SafeParseInt(cval);
                        break;
                    case 16:
                        ret.Lck = SafeParseInt(cval);
                        break;
                    case 17:
                        ret.Cha = SafeParseInt(cval);
                        break;
                    case 18:
                        ret.Moves = SafeParseInt(cval);
                        break;
                    case 19:
                        ret.Weight = SafeParseInt(cval);
                        break;
                    case 20:
                        ret.Value = SafeParseInt(cval);
                        break;
                    case 21:
                        ret.Properties = cval;
                        break;
                    case 22:
                        ret.Other = cval;
                        break;
                    case 23:
                        ret.Price = SafeParseInt(cval);
                        break;
                }
            }
            return ret;
        }
    }
}
