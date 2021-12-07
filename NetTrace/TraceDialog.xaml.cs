using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NetTrace
{
    /// <summary>
    /// Interaction logic for TraceDialog.xaml
    /// </summary>
    public partial class TraceDialog : Window
    {
        #region Databound Variables
        // Databound arrays for our three listboxes. 
        public ObservableCollection<DlgTagBinding> TagList { get; private set; }
        private ObservableCollection<string> EnumsList { get; set; }
        #endregion

        #region Properties
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns the collection of tag names. </summary>
        ///
        /// <value>	A list of names of the tags. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ICollection<string> TagNames
        {
            get
            {
                return Tags.DctNamesToBinding.Keys;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Gets the EnumInfo for the current enum trace type. </summary>
        ///
        /// <value>	The current EnumInfo. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal EnumInfo TtiCur => NetTrace.TtiFromType(Tags.LstEnums[lbEnums.SelectedIndex]);
        #endregion

        #region Private variables
        private Brush EnumTagColor { get; set; }
        private Brush NormalTagColor { get; set; }
        #endregion

        #region Constructor
        public TraceDialog()
        {
            // Allocate our bindings

            // Binding for all tags
            TagList = new ObservableCollection<DlgTagBinding>();

            // Binding for enums
            EnumsList = new ObservableCollection<string>();

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
        public bool this[string strTag] => Tags.DctNamesToBinding[strTag].FOn;
        #endregion

        private void SetEnumTags(bool fOn)
        {
            var tp = Tags.LstEnums[lbEnums.SelectedIndex];

            foreach (var tsad in TagList.Where(t => t.EnumType == tp))
            {
                tsad.FOn = fOn;
            }
        }

        private void BtnAllOnClick(object sender, EventArgs e)
        {
            SetEnumTags(true);
        }

        private void BtnAllOffClick(object sender, EventArgs e)
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
