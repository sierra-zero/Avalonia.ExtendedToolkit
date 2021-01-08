using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.ExtendedToolkit.Extensions;
using ReactiveUI;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;
using DynamicData.Binding;

namespace Avalonia.ExtendedToolkit.Controls
{
    /// <summary>
    /// a control with a grouped list and 
    /// buttons to switch to this grouped section
    /// </summary>
    public class IndexItemsControl : TemplatedControl
    {
        /// <summary>
        /// default index entries
        /// </summary>
        private static string[] _defaultIndexes = new string[]
        {"#","A","B","C","D","E","F","G","H",
         "I","J","K","L","M","N","O","P","Q",
         "R","S","T","U","V","W","X","Y","Z"};

        private IObservable<Func<IndexItemModel, bool>> _filter;
        private IObservable<IChangeSet<IndexItemModel>> _items;
        private IndexItemModels _indexEntries = new IndexItemModels();


        private IndexList _contentIndexList;

        /// <summary>
        /// Gets or sets SelectedItem.
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Defines the SelectedItem property.
        /// </summary>
        public static readonly StyledProperty<object> SelectedItemProperty =
        AvaloniaProperty.Register<IndexItemsControl, object>(nameof(SelectedItem));

        /// <summary>
        /// Defines the IndexEntries direct property.
        /// </summary>
        public static readonly DirectProperty<IndexItemsControl, ReadOnlyObservableCollection<IndexItemModel>> IndexItemsProperty =
        AvaloniaProperty.RegisterDirect<IndexItemsControl, ReadOnlyObservableCollection<IndexItemModel>>(
        nameof(IndexItems),
        o => o.IndexItems);

        private ReadOnlyObservableCollection<IndexItemModel> _indexItems;
        private SourceList<IndexItemModel> _sourceList;

        /// <summary>
        /// Gets or sets IndexEntries.
        /// </summary>
        public ReadOnlyObservableCollection<IndexItemModel> IndexItems
        {
            get { return _indexItems; }
            private set
            {
                SetAndRaise(IndexItemsProperty, ref _indexItems, value);
            }
        }



        // /// <summary>
        // /// Defines the IndexEnries direct property.
        // /// </summary>
        // public static readonly DirectProperty<IndexItemsControl, IndexItemModels> IndexEntriesProperty =
        // AvaloniaProperty.RegisterDirect<IndexItemsControl, IndexItemModels>(
        // nameof(IndexEntries),
        // o => o.IndexEntries);



        //private IndexItemModels _indexEnries = new IndexItemModels();

        // /// <summary>
        // /// Gets or sets IndexEnries.
        // /// </summary>
        // public IndexItemModels IndexEntries
        // {
        //     get { return _indexEnries; }
        //     set
        //     {
        //         SetAndRaise(IndexEntriesProperty, ref _indexEnries, value);
        //     }
        // }






        /// <summary>
        /// Gets or sets ShowEmptyItems.
        /// </summary>
        public bool ShowEmptyItems
        {
            get { return (bool)GetValue(ShowEmptyItemsProperty); }
            set { SetValue(ShowEmptyItemsProperty, value); }
        }

        /// <summary>
        /// Defines the ShowEmptyItems property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowEmptyItemsProperty =
        AvaloniaProperty.Register<IndexItemsControl, bool>(nameof(ShowEmptyItems), defaultValue: true);


        /// <summary>
        /// Defines the PressedCommand direct property.
        /// </summary>
        internal static readonly DirectProperty<IndexItemsControl, ICommand> PressedCommandProperty =
        AvaloniaProperty.RegisterDirect<IndexItemsControl, ICommand
        >(nameof(PressedCommand),
        o => o.PressedCommand);

        /// <summary>
        /// Gets or sets PressedCommand.
        /// </summary>
        internal ICommand PressedCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets IndexSectionItems.
        /// </summary>
        public ObservableCollection<string> IndexSectionItems
        {
            get { return (ObservableCollection<string>)GetValue(IndexSectionItemsProperty); }
            set { SetValue(IndexSectionItemsProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="IndexSectionItems"/> property.
        /// </summary>
        public static readonly StyledProperty<ObservableCollection<string>> IndexSectionItemsProperty =
            AvaloniaProperty.Register<IndexItemsControl, ObservableCollection<string>>(nameof(IndexSectionItems),
            defaultValue: new ObservableCollection<string>(_defaultIndexes));

        /// <summary>
        /// Gets or sets ShowSearch.
        /// </summary>
        public bool ShowSearch
        {
            get { return (bool)GetValue(ShowSearchProperty); }
            set { SetValue(ShowSearchProperty, value); }
        }

        /// <summary>
        /// Defines the ShowSearch property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowSearchProperty =
        AvaloniaProperty.Register<IndexItemsControl, bool>(nameof(ShowSearch), defaultValue: true);

        /// <summary>
        /// Gets or sets SearchText.
        /// </summary>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>
        /// Defines the SearchText property.
        /// </summary>
        public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<IndexItemsControl, string>(nameof(SearchText));


        /// <summary>
        /// initilaizes som changed events
        /// and initilize the pressed command
        /// </summary>
        public IndexItemsControl()
        {
            PressedCommand = ReactiveCommand.Create<IndexItemModel>(x => ExecutePressedCommand(x), outputScheduler: RxApp.MainThreadScheduler);

            DataContextProperty.Changed.AddClassHandler<IndexItemsControl>((o, e) => OnDataContextChanged(o, e));

            IndexSectionItemsProperty.Changed.AddClassHandler<IndexItemsControl>((o, e) => OnIndexSectionItemsChanged(o, e));

            ShowEmptyItemsProperty.Changed.AddClassHandler<IndexItemsControl>((o, e) => OnShowEmptyItemsChanged(o, e));

            _filter = this.WhenAnyValue(x => x.SearchText)
                        .Delay(TimeSpan.FromMilliseconds(100))
                        .Select(BuildFilter);

            this.WhenAnyValue(x => x.ShowSearch).Subscribe((showSearch) =>
                      {
                          //reset the filter if showSearch is false
                          if (showSearch == false && string.IsNullOrEmpty(SearchText) == false)
                          {
                              SearchText = string.Empty;

                          }


                      });

        }

        private Func<IndexItemModel, bool> BuildFilter(string searchText)
        {
            return entry => (
                                entry.ApplyFilter(searchText)
                            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OnShowEmptyItemsChanged(IndexItemsControl o, AvaloniaPropertyChangedEventArgs e)
        {
            if (_contentIndexList == null || e.NewValue == null)
            {
                return;
            }

            _contentIndexList.ShowEmptyItems = (bool)e.NewValue;
        }

        /// <summary>
        /// clears the <see cref="_indexEntries"/>
        /// and raises the datacontext changed event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OnIndexSectionItemsChanged(IndexItemsControl o, object e)
        {
            CreateBaseSections();
            UpdateData(DataContext);
        }

        private void CreateBaseSections()
        {
            _indexEntries.Clear();

            IndexSectionItems.ForEach(item => _indexEntries.Add(new IndexItemModel { Text = item }));
        }


        /// <summary>
        /// scrolls the item into the view
        /// </summary>
        /// <param name="item"></param>
        private void ExecutePressedCommand(IndexItemModel item)
        {
            _contentIndexList.ScrollIntoView(item);
        }

        /// <summary>
        /// if indexentries.count is zero return
        /// else fills the index entries
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OnDataContextChanged(IndexItemsControl o, AvaloniaPropertyChangedEventArgs e)
        {
            if (_indexEntries.Count == 0)
            {
                CreateBaseSections();
            }

            UpdateData(e.NewValue);


        }

        private void UpdateData(object datacontext)
        {
            if (datacontext is IEnumerable<IndexItemModel> items)
            {
                var tempItems = items.OrderBy(x => x.Text, new AlphanumComparatorFast())
                                .Select(item => item.Text.First())
                                .Distinct()
                                .ToDictionary(l => l.ToString(), l => items.Where(x => x.Text.StartsWith(l.ToString())));
                foreach (var item in tempItems)
                {
                    IndexItemModel mainIndex = null;

                    if (char.IsDigit(item.Key.First()))
                    {
                        mainIndex = _indexEntries.Where(x => x.Text == "#").FirstOrDefault();
                    }
                    else
                    {
                        mainIndex = _indexEntries.Where(x => x.Text == item.Key).FirstOrDefault();
                    }

                    if (mainIndex == null)
                    {
                        Debug.WriteLine($"WARNING: {item.Key} not found");
                        continue;
                    }

                    item.Value.ForEach(it => mainIndex.SubItems.Add(it));
                }

                var comparer = SortExpressionComparer<IndexItemModel>.Ascending(l => l.Text);

                if (_sourceList != null)
                {
                    _sourceList.Dispose();
                }

                _sourceList = new SourceList<IndexItemModel>();
                _sourceList.AddRange(_indexEntries);
                _items = _sourceList.Connect();
                _items.Filter(_filter)
                .Sort(comparer) /*need this here because after filtering the list is incorrect ordered*/
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _indexItems)
                .DisposeMany()
                .Subscribe();


                RaisePropertyChanged(IndexItemsProperty, null, _indexItems);
            }
        }



        /// <summary>
        /// gets the needed controls
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);
            _contentIndexList = e.NameScope.Find<IndexList>("contentListBox");
            _contentIndexList.ShowEmptyItems = ShowEmptyItems;
            _contentIndexList.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = e.AddedItems.OfType<object>().FirstOrDefault();
        }
    }
}