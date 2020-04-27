using Parse.Algorithms;
using Parse.Extensions;
using Parse.WpfControls.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Parse.WpfControls
{
    /// <summary>
    /// XAML 파일에서 이 사용자 지정 컨트롤을 사용하려면 1a 또는 1b단계를 수행한 다음 2단계를 수행하십시오.
    ///
    /// 1a단계) 현재 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:Parse.WpfControls"
    ///
    ///
    /// 1b단계) 다른 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:Parse.WpfControls;assembly=Parse.WpfControls"
    ///
    /// 또한 XAML 파일이 있는 프로젝트의 프로젝트 참조를 이 프로젝트에 추가하고
    /// 다시 빌드하여 컴파일 오류를 방지해야 합니다.
    ///
    ///     솔루션 탐색기에서 대상 프로젝트를 마우스 오른쪽 단추로 클릭하고
    ///     [참조 추가]->[프로젝트]를 차례로 클릭한 다음 이 프로젝트를 찾아서 선택합니다.
    ///
    ///
    /// 2단계)
    /// 계속 진행하여 XAML 파일에서 컨트롤을 사용합니다.
    ///
    ///     <MyNamespace:CompletionList/>
    ///
    /// </summary>
    public class CompletionList : Control
    {
        /********************************************************************************************
         * private field section [Control]
         ********************************************************************************************/
        private TextBox parent;
        private Popup _parentPopup;
        private ListBox listBox;
        private StackPanel stackPanel;
        private ISimilarityComparison similarity = new LikeVSSimilarityComparison();



        /********************************************************************************************
         * private field section [Not Control]
         ********************************************************************************************/
        private int caretIndexWhenCLOccur;
        private bool isIncludeZeroSimilarity;
        private string lastInputString = string.Empty;
        private Dictionary<Enum, KeyData> Keys = new Dictionary<Enum, KeyData>();
        private ObservableCollection<ItemData> AllItems = new ObservableCollection<ItemData>();
        private ObservableCollection<ItemData> VisibleItems = new ObservableCollection<ItemData>();



        #region Properties
        public int CaretIndexWhenCLOccur
        {
            get => caretIndexWhenCLOccur;
            set => caretIndexWhenCLOccur = value;
        }

        public bool IsOpened => (_parentPopup == null) ? false : _parentPopup.IsOpen;

        private bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public bool StaysOpen
        {
            get { return (bool)GetValue(StaysOpenProperty); }
            set { SetValue(StaysOpenProperty, value); }
        }
        #endregion

        #region DependencyProperties
        public Brush FilterButtonCheckedBackgroundColor
        {
            get { return (Brush)GetValue(FilterButtonCheckedBackgroundColorProperty); }
            set { SetValue(FilterButtonCheckedBackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterButtonCheckedBackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterButtonCheckedBackgroundColorProperty =
            DependencyProperty.Register("FilterButtonCheckedBackgroundColor", typeof(Brush), typeof(CompletionList), 
                                                        new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"))));


        public Brush FilterButtonMouseEnterBackgroundColor
        {
            get { return (Brush)GetValue(FilterButtonMouseEnterBackgroundColorProperty); }
            set { SetValue(FilterButtonMouseEnterBackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterButtonMouseEnterBackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterButtonMouseEnterBackgroundColorProperty =
            DependencyProperty.Register("FilterButtonMouseEnterBackgroundColor", typeof(Brush), typeof(CompletionList), 
                                                        new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"))));


        //Placement
        private static readonly DependencyProperty PlacementProperty = Popup.PlacementProperty.AddOwner(typeof(CompletionList));
        private PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        //PlacementTarget
        private static readonly DependencyProperty PlacementTargetProperty = Popup.PlacementTargetProperty.AddOwner(typeof(CompletionList));
        private UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        //PlacementRectangle
        public static readonly DependencyProperty PlacementRectangleProperty = Popup.PlacementRectangleProperty.AddOwner(typeof(CompletionList));
        public Rect PlacementRectangle
        {
            get { return (Rect)GetValue(PlacementRectangleProperty); }
            set { SetValue(PlacementRectangleProperty, value); }
        }

        //HorizontalOffset
        public static readonly DependencyProperty HorizontalOffsetProperty = Popup.HorizontalOffsetProperty.AddOwner(typeof(CompletionList));
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        //VerticalOffset
        public static readonly DependencyProperty VerticalOffsetProperty = Popup.VerticalOffsetProperty.AddOwner(typeof(CompletionList));
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty StaysOpenProperty =
        Popup.StaysOpenProperty.AddOwner(typeof(CompletionList), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                new PropertyChangedCallback(OnStaysOpenChanged)));

        public static readonly DependencyProperty IsOpenProperty =
         Popup.IsOpenProperty.AddOwner(typeof(CompletionList),
                 new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                    new PropertyChangedCallback(OnIsOpenChanged)));
        #endregion

        #region Dependency Property changed event handler
        private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CompletionList ctrl = (CompletionList)d;

            if ((bool)e.NewValue)
            {
                ctrl.StaysOpenParentPopop((bool)e.NewValue);
            }
        }
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CompletionList ctrl = (CompletionList)d;
            if ((bool)e.NewValue)
            {
                if (ctrl._parentPopup == null)
                {
                    ctrl.HookupParentPopup();
                }
            }
        }
        private void StaysOpenParentPopop(bool newValue)
        {
            _parentPopup.StaysOpen = newValue;
        }
        private void HookupParentPopup()
        {
            _parentPopup = new Popup();
            _parentPopup.MinWidth = 100;
            _parentPopup.MinHeight = 16;
            _parentPopup.AllowsTransparency = true;
            Popup.CreateRootPopup(_parentPopup, this);
        }
        #endregion



        public bool IsReady
        {
            get
            {
                if (this.listBox == null) return false;
                if (this.stackPanel == null) return false;

                return true;
            }
        }

        #region Constructor
        static CompletionList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompletionList), new FrameworkPropertyMetadata(typeof(CompletionList)));
        }

        public CompletionList(TextBox textBox)
        {
            this.parent = textBox;

            this.parent.PreviewKeyDown += Parent_PreviewKeyDown;
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.listBox = (ListBox)Template.FindName("PART_ListBox", this);
            this.stackPanel = (StackPanel)Template.FindName("PART_StackPanel", this);

            this.listBox.ItemsSource = VisibleItems;
            this.listBox.SelectionChanged += ((s, le) => this.listBox.ScrollIntoView(this.listBox.SelectedItem));
            this.listBox.SelectionChanged += ((s, le) => this.parent.Focus());
            this.listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
        }

        #region event handler
        private void Parent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.IsOpen)
            {
                if (this.InputProcessOnCompletionList(this.parent, e.Key)) e.Handled = true;
                return;
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.InputProcessOnCompletionList(this.parent, Key.Enter);
        }

        private void FilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            var filterButton = sender as ToggleButton;

            var border = filterButton.Content as Border;
            var brush = FilterButtonMouseEnterBackgroundColor;
            border.Background = brush;
        }

        private void FilterButton_MouseLeave(object sender, MouseEventArgs e)
        {
            var filterButton = sender as ToggleButton;

            if(filterButton.IsChecked == false)
            {
                var border = filterButton.Content as Border;
                border.Background = Brushes.Transparent;
            }
        }

        private void FilteringButton_Checked(object sender, RoutedEventArgs e)
        {
            var filterButton = sender as ToggleButton;

            var border = filterButton.Content as Border;
            var brush = FilterButtonMouseEnterBackgroundColor;
            border.Background = brush;

            this.Refresh();
        }

        private void FilteringButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var filterButton = sender as ToggleButton;

            var border = filterButton.Content as Border;
            border.Background = Brushes.Transparent;

            this.Refresh();
        }
        #endregion

        #region private methods
        private IReadOnlyList<Enum> GetCheckedFilterType()
        {
            List<Enum> result = new List<Enum>();

            foreach(var item in this.stackPanel.Children)
            {
                var filterButton = item as ToggleButton;

                if (filterButton?.IsChecked == true) result.Add(filterButton.Tag as Enum);
            }

            return result;
        }

        private object GetFilterContent(ImageSource imageSource, double width, double height, bool bChecked)
        {
            Border border = new Border
            {
                Width = 20,
                Height = 20,
                BorderThickness = new Thickness(0),
                Child = new Image() { Source = imageSource, Width = width, Height = height }
            };

            if (bChecked) border.Background = FilterButtonCheckedBackgroundColor;
            else border.Background = Brushes.Transparent;

            return border;
        }

        private ToggleButton GetFilterButtonFromEnum(Enum type)
        {
            ToggleButton result = null;

            foreach (var filter in this.stackPanel.Children)
            {
                ToggleButton filterButton = filter as ToggleButton;

                if ((filterButton.Tag as Enum).Equals(type)) result = filterButton;
            }

            return result;
        }

        private void ChangeFilterState(IReadOnlyList<ItemData> itemDatas)
        {
            foreach(var filter in this.stackPanel.Children)
            {
                ToggleButton filterButton = filter as ToggleButton;

                var bitmapImage = this.Keys[filterButton.Tag as Enum].InActiveImgSource;
                bool bChecked = (bool)filterButton.IsChecked;
                filterButton.Content = this.GetFilterContent(bitmapImage, 16, 16, bChecked);
            }

            foreach(var item in itemDatas)
            {
                ToggleButton filterButton = this.GetFilterButtonFromEnum(item.Type);
                var bitmapImage = this.Keys[item.Type as Enum].ActiveImgSource;
                bool bChecked = (bool)filterButton.IsChecked;
                filterButton.Content = this.GetFilterContent(bitmapImage, 16, 16, bChecked);
            }
        }

        private void SelectTopCandidate(IList<double> similarityValues)
        {
            if (similarityValues.Count == 0) return;
            if (this.VisibleItems.Count != similarityValues.Count) return;

            var index = similarityValues.IndexOf(similarityValues.Max());
            this.listBox.SelectedItem = this.VisibleItems[index];
        }

        /// <summary>
        /// This function returns a similarity list by comparing an 'inputString' from 'itemsDatas'.
        /// </summary>
        /// <param name="itemDatas">The list to compare</param>
        /// <param name="inputString">The string to compare with an element of the 'itemDatas'</param>
        /// <returns></returns>
        private IList<double> GetSimilarityList(IReadOnlyList<ItemData> itemDatas, string inputString)
        {
            List<double> similarityValues = new List<double>();

            foreach (var item in itemDatas)
            {
                double value = this.similarity.SimilarityValue(item.Item, inputString, out List<uint> matchedIndex);

                item.MatchedIndexes = matchedIndex;
                this.VisibleItems.Add(item);
                similarityValues.Add(value);
            }

            return similarityValues;
        }

        private bool InputProcessOnCompletionList(TextBox textbox, Key keyType)
        {
            bool result = false;

            if (keyType == Key.Up)
            {
                if (this.listBox.SelectedIndex > 0) this.listBox.SelectedIndex--;
                result = true;
            }
            else if (keyType == Key.Down)
            {
                if (this.listBox.SelectedIndex < this.listBox.Items.Count - 1) this.listBox.SelectedIndex++;
                result = true;
            }
            else if (keyType == Key.Enter || keyType == Key.Tab)
            {
                this.IsOpen = false;
                BringStringFromCompletionList(textbox);
                result = true;
            }
            else if (keyType == Key.Space || keyType == Key.OemPeriod)
            {
                this.IsOpen = false;
                if (keyType == Key.Space) BringStringFromCompletionList(textbox, " ");
                else if (keyType == Key.OemPeriod) BringStringFromCompletionList(textbox, ".");

                result = true;
            }
            else if (keyType == Key.Escape)
            {
                this.IsOpen = false;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This function brings to TextArea a selected text in the completion list.
        /// </summary>
        /// <param name="other"></param>
        private void BringStringFromCompletionList(TextBox textbox, string other = "")
        {
            int startIndex = caretIndexWhenCLOccur;
            int endIndex = textbox.CaretIndex;

            if (this.listBox.SelectedIndex >= 0)
            {
                var addString = this.VisibleItems[this.listBox.SelectedIndex].Item + other;

                textbox.Select(startIndex, endIndex - startIndex);
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textbox, addString));
            }
            else if (other.Length > 0)
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textbox, other));
        }

        private void CreateFilterButtons()
        {
            List<Enum> enums = new List<Enum>();

            // extract enum list from all items without duplicate.
            foreach (var element in this.AllItems)
            {
                if (enums.Contains(element.Type)) continue;

                enums.Add(element.Type);
            }

            // create filter buttons with extracted enum list.
            this.stackPanel.Children.Clear();
            foreach (var @enum in enums)
            {
                if (Keys.ContainsKey(@enum) == false) continue;

                var keyData = Keys[@enum];
                var filterButton = new ToggleButton()
                {
                    Tag = @enum,
                    BorderThickness = new Thickness(0),
                    BorderBrush = Brushes.Transparent,
                    Width = 24,
                    Height = 24,
                    Background = Brushes.Transparent,
                    Focusable = false,
                    ToolTip = keyData.ToolTipData,

                    Content = this.GetFilterContent(keyData.ActiveImgSource, 16, 16, false)
                    //                    Content = new Image() { Source = tgbuttonItem.Item2, Width=16, Height=16 }
                };

                filterButton.Checked += FilteringButton_Checked;
                filterButton.MouseEnter += FilterButton_MouseEnter;
                filterButton.MouseLeave += FilterButton_MouseLeave;
                filterButton.Unchecked += FilteringButton_Unchecked;

                this.stackPanel.Children.Add(filterButton);
            }

            if (enums.Count > 0) _parentPopup.Width = 24 * enums.Count;
        }

        private IReadOnlyList<ItemData> GetFilteredListByFilterButton(IReadOnlyList<ItemData> itemDatas)
        {
            List<ItemData> result = new List<ItemData>();

            var checkedTypeList = this.GetCheckedFilterType();
            foreach (var checkedType in checkedTypeList)
            {
                foreach (var item in itemDatas)
                {
                    if (item.Type.Equals(checkedType)) result.Add(item);
                }
            }

            if (checkedTypeList.Count == 0)
            {
                foreach (var item in itemDatas) result.Add(item);
            }

            return result;
        }

        private bool RegisterItem(ItemData item)
        {
            if (this.Keys.ContainsKey(item.Type) == false) this.Keys.Add(item.Type, new KeyData());
            item.ImageSource = this.Keys[item.Type].ActiveImgSource;

            this.AllItems.Add(item);

            return true;
        }

        private void ClearItems()
        {
            this.AllItems.Clear();
            this.VisibleItems.Clear();
        }
        #endregion

        #region public methods
        public void RegisterKey(Enum key, KeyData keyData) => this.Keys.Add(key, keyData);
        public void ClearKeySet() => this.Keys.Clear();


        public void Create(List<ItemData> items, double x, double y)
        {
            this.ClearItems();
            foreach (var item in items) this.RegisterItem(item);
            foreach (var item in this.AllItems) this.VisibleItems.Add(item);

            this.StaysOpen = false;
            this.Placement = PlacementMode.Relative;
            this.PlacementTarget = this.parent;
            this.HorizontalOffset = x;
            this.VerticalOffset = y;

            this.IsOpen = true;

            this.CreateFilterButtons();
        }

        public void Refresh()
        {
            var filteredList = this.GetFilteredListByFilterButton(this.AllItems);
            this.ChangeFilterState(filteredList);

            this.VisibleItems.Clear();
            foreach (var item in filteredList) this.VisibleItems.Add(item);

            if (this.lastInputString.Length > 0)
            {
                var similarityList = this.GetSimilarityList(filteredList, this.lastInputString);

                this.SelectTopCandidate(similarityList);

                if (this.isIncludeZeroSimilarity == false)
                {
                    List<int> removeIndexes = new List<int>();
                    for (int i = 0; i < similarityList.Count; i++)
                    {
                        if (similarityList[i] == 0) removeIndexes.Add(i);
                    }

                    this.VisibleItems.RemoveList(removeIndexes);
                }
            }
        }

        public void Show(string inputString, double x, double y, bool IsIncludeZeroSimilarity = false)
        {
            this.lastInputString = inputString;
            this.isIncludeZeroSimilarity = IsIncludeZeroSimilarity;

            this.Refresh();

            this.StaysOpen = false;
            this.Placement = PlacementMode.Relative;
            this.PlacementTarget = this.parent;
            this.HorizontalOffset = x;
            this.VerticalOffset = y;
            this.IsOpen = true;
        }

        public void Close() => this.IsOpen = false;
        #endregion
    }
}
