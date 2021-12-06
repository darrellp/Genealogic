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
                return Tags.DctNamesToStatus.Keys;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Gets the TraceTypeInfo for the current enum trace type. </summary>
        ///
        /// <value>	The current TraceTypeInfo. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal TraceTypeInfo TtiCur => NetTrace.TtiFromType(Tags.LsttpEnums[lbEnums.SelectedIndex]);
        #endregion

        #region Private variables
        private Brush EnumTagColor { get; set; }
        private Brush NormalTagColor { get; set; }
        #endregion

        #region Constructor
#pragma warning disable CS8618
        public TraceDialog()
#pragma warning restore CS8618
        {
            TagList = new ObservableCollection<DlgTagBinding>();
            EnumsList = new ObservableCollection<string>();
            InitializeComponent();
        }
        public TraceDialog(Dictionary<string, DlgTagBinding> dctNamesToStatus)
            : this()
        {
            EnumTagColor = new SolidColorBrush((Color)Resources["EnumTagColor"]);
            NormalTagColor = new SolidColorBrush((Color)Resources["NormalTagColor"]);

            ShowInTaskbar = false;
            Tags.DctNamesToStatus = new Dictionary<string, DlgTagBinding>(dctNamesToStatus);

            Tags.Init(dctNamesToStatus);
            foreach (string strTag in dctNamesToStatus.Keys)
            {
                TagList.Add(Tags.DctNamesToStatus[strTag]);
            }
            lbTags.ItemsSource = TagList;

            foreach (Type tp in Tags.LsttpEnums)
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
        public bool this[string strTag] => Tags.DctNamesToStatus[strTag].FOn;
        #endregion

        private void SetEnumTags(bool fOn)
        {
            var tp = Tags.LsttpEnums[lbEnums.SelectedIndex];

            foreach (var tsad in TagList.Where(t => t.TpEnum == tp))
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
