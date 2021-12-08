using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NetTrace
{
    /// <summary>
    /// Interaction logic for TraceDialog.xaml
    /// </summary>
    public partial class TraceDialog
    {
        #region Databound Variables
        // Databound arrays for our three listboxes. 
        public ObservableCollection<DlgTagBinding> TagList { get; }
        private ObservableCollection<string> EnumsList { get; }
        #endregion

        #region Properties
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Gets the EnumInfo for the current enum trace type. </summary>
        ///
        /// <value>	The current EnumInfo. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal EnumInfo TtiCur => NetTrace.TtiFromType(Tags.LstEnums[lbEnums.SelectedIndex]);
        #endregion

        #region Private variables
        private Brush EnumTagColor { get; }
        private Brush NormalTagColor { get; }
        #endregion

        #region Constructor
        public TraceDialog()
        {
            // Allocate our bindings

            // Binding for all tags
            TagList = new ObservableCollection<DlgTagBinding>();

            // Binding for enums
            EnumsList = new ObservableCollection<string>();

            Height = 300;
            Width = 400;

            // We have to initialize component here or our dialog variables will be unset
            InitializeComponent();

            EnumTagColor = new SolidColorBrush((Color)Resources["EnumTagColor"]);
            NormalTagColor = new SolidColorBrush((Color)Resources["NormalTagColor"]);

            ShowInTaskbar = false;
            Tags.DctNamesToBinding = NetTrace.TagNameToBindingDictionary();

            // Initialize the list the dialog binds to for the enum list
            Tags.BindEnums(Tags.DctNamesToBinding);

            foreach (var binding in Tags.DctNamesToBinding.Values)
            {
                TagList.Add(binding);
            }
            lbTags.ItemsSource = TagList;

            foreach (var tp in Tags.LstEnums)
            {
                EnumsList.Add(Tags.StrDescFromTp(tp));
            }
            lbEnums.ItemsSource = EnumsList;

            if (lbEnums.Items.Count > 0)
            {
                lbEnums.SelectedIndex = 0;
            }
        }
        #endregion

        #region Indexers
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns the status of the named tag. </summary>
        ///
        /// <value>	The indexed item. </value>
        ///
        /// ### <param name="strTag">	Tag name being queried. </param>
        /// ### <returns>	Whether the dialog has that tag turned on or off. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool this[string strTag]
        {
            get
            {
                Debug.Assert(Tags.DctNamesToBinding != null, "Tags.DctNamesToBinding != null");
                return Tags.DctNamesToBinding[strTag].FOn;
            }
        }
        #endregion

        private void SetEnumTags(bool fOn)
        {
            var tp = Tags.LstEnums[lbEnums.SelectedIndex];

            foreach (var tsad in TagList.Where(t => t.EnumType == tp))
            {
                tsad.FOn = fOn;
            }
        }

        private void SetAllTags(bool fOn)
        {
            foreach (var tsad in TagList)
            {
                tsad.FOn = fOn;
            }
        }

        private void BtnAllOnClick(object sender, EventArgs e)
        {
            SetAllTags(true);
        }

        private void BtnAllOffClick(object sender, EventArgs e)
        {
            SetAllTags(false);
        }

        private void BtnEnumOnClick(object sender, EventArgs e)
        {
            SetEnumTags(true);
        }

        private void BtnEnumOffClick(object sender, EventArgs e)
        {
            SetEnumTags(false);
        }

        private void BtnOkayClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void EnumSelectionChanged(object sender, EventArgs e)
        {
            SetTagColors();
        }

        private void SetTagColors()
        {
            var ttiCur = TtiCur;

            foreach (var tagInfo in TagList)
            {
                tagInfo.Color = ttiCur.TagNames.Contains(tagInfo.StrTag) ? EnumTagColor : NormalTagColor;

            }
        }
    }
}
