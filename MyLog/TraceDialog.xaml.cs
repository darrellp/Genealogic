using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WPFTrace;

namespace MyLog
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Interaction logic for the Trace dialog. </summary>
	///
	/// <remarks>	Darrellp, 10/5/2012. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////
	public partial class TraceDialog
	{
        #region Databound Variables
        // Databound arrays for our three listboxes. 
        public ObservableCollection<TagInfo> TagList { get; private set; }
        private ObservableCollection<string> EnumsList { get; set; }
        #endregion

        //#region Properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>	Returns the collection of tag names. </summary>
        /////
        ///// <value>	A list of names of the tags. </value>
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //public ICollection<string> TagNames
        //{
        //	get
        //	{
        //		return Tags.DctNamesToStatus.Keys;
        //	}
        //}

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>	Gets the TraceTypeInfo for the current enum trace type. </summary>
        /////
        ///// <value>	The current TraceTypeInfo. </value>
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //internal TraceTypeInfo TtiCur
        //{
        //    get
        //    {
        //        return Tracer.TtiFromType(Tags.LsttpEnums[lbEnums.SelectedIndex]);
        //    }
        //}
        //#endregion

        //#region Private variables
        //private Brush EnumTagColor { get; set; }
        //private Brush NormalTagColor { get; set; }
        //#endregion

        //#region Constructors
        public TraceDialog()
        {
            //InitializeComponent();
            TagList = new ObservableCollection<TagInfo>();
            EnumsList = new ObservableCollection<string>();
        }

        //public TraceDialog(Dictionary<string, TagInfo> dctNamesToStatus)
        //	: this()
        //{
        //	EnumTagColor = new SolidColorBrush((Color)Resources[ "EnumTagColor" ]);
        //	NormalTagColor = new SolidColorBrush((Color)Resources["NormalTagColor"]);

        //	ShowInTaskbar = false;
        //	Tags.DctNamesToStatus = new Dictionary<string, TagInfo>(dctNamesToStatus);

        //	Tags.Init(dctNamesToStatus);
        //	foreach (string strTag in dctNamesToStatus.Keys)
        //	{
        //		TagList.Add(Tags.DctNamesToStatus[strTag]);
        //	}
        //	lbTags.ItemsSource = TagList;

        //	foreach (Type tp in Tags.LsttpEnums)
        //	{
        //		EnumsList.Add(Tags.StrDescFromTp(tp));
        //	}
        //	lbEnums.ItemsSource = EnumsList;

        //	SetListeners();
        //	lbListeners.ItemsSource = ListenerList;

        //	if (lbEnums.Items.Count > 0)
        //	{
        //		lbEnums.SelectedIndex = 0;
        //	}
        //	InfoGrid.DataContext = _miscInfo;
        //	_miscInfo.PropertyChanged += MiscInfo_PropertyChanged;
        //}

        //private void MiscInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //	switch (e.PropertyName)
        //	{
        //		case "ShowTimeStamps":
        //			TtiCur.ShowTimeStamps = _miscInfo.ShowTimeStamps;
        //			break;
        //		case "ShowThreadIds":
        //			TtiCur.ShowThreadIds = _miscInfo.ShowThreadIds;
        //			break;
        //		case "ShowSeverity":
        //			TtiCur.ShowSeverity = _miscInfo.ShowSeverity;
        //			break;
        //		case "ShowTagName":
        //			TtiCur.ShowTagName = _miscInfo.ShowTagName;
        //			break;
        //		case "StrSeverityFilter":
        //			TtiCur.TraceLevelFilter = (TraceLevel)Enum.Parse(typeof(TraceLevel), _miscInfo.StrSeverityFilter);
        //			break;
        //		default:
        //			Debug.Assert(false, "Type unhandled in MiscInfo property change");
        //			break;
        //	}
        //}

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>	Check the boxes for the listeners for the current type. </summary>
        /////
        ///// <remarks>	Darrellp, 10/3/2012. </remarks>
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //void CheckListeners()
        //{
        //	Debug.Assert(lbListeners.Items.Count == Trace.Listeners.Count);
        //	var tti = TtiCur;

        //	for (var itlis = 0; itlis < lbListeners.Items.Count; itlis++)
        //	{
        //		ListenerList[itlis].FOn = tti.FIsListening(Trace.Listeners[itlis]);
        //	}
        //}

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>	Put all the listeners into the listener listbox. </summary>
        /////
        ///// <remarks>	Darrellp, 10/3/2012. </remarks>
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //void SetListeners()
        //{
        //	ListenerList.Clear();
        //	for (var i = 0; i < Trace.Listeners.Count; i++)
        //	{
        //		var tlis = Trace.Listeners[i];
        //		var linfo = new ListenerInfo(StrAbbreviatedListenerName(tlis), false, tlis);
        //		ListenerList.Add(linfo);
        //		linfo.PropertyChanged += linfo_PropertyChanged;
        //	}
        //}

        //void linfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //	var linfo = sender as ListenerInfo;

        //	Debug.Assert(linfo != null);

        //	if (linfo.FOn)
        //	{
        //		TtiCur.AddListener(linfo.TListener);
        //	}
        //	else
        //	{
        //		TtiCur.RemoveListener(linfo.TListener);
        //	}
        //}
        //#endregion

        //#region Indexers
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>	Returns the status of the named tag. </summary>
        /////
        ///// <value>	The indexed item. </value>
        /////
        ///// ### <param name="strTag">	Tag name being queried. </param>
        ///// ### <returns>	Whether the dialog has that tag turned on or off. </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //public bool this[string strTag]
        //{
        //	get
        //	{
        //		return Tags.DctNamesToStatus[strTag].FOn;
        //	}
        //}
        //#endregion

        //#region Abbreviated name
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>	
        ///// Returns a name suitable for the dialogs.  File listeners names are only the root file name
        ///// without the directory tacked on. 
        ///// </summary>
        /////
        ///// <remarks>	Darrellp, 10/3/2012. </remarks>
        /////
        ///// <param name="tlis">	Listener we want an abbreviated name for. </param>
        /////
        ///// <returns>	the name. </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //internal static string StrAbbreviatedListenerName(TraceListener tlis)
        //{
        //	string str = tlis.Name;

        //	if (tlis.Name == string.Empty)
        //	{
        //		if (tlis is TextWriterTraceListener)
        //		{
        //			str = "<Unnamed TextWriterTraceListener>";
        //		}
        //	}
        //	else if (tlis is InternalTextWriterListener)
        //	{
        //		str = Path.GetFileName(tlis.Name);
        //	}
        //	return str;
        //}
        //#endregion

        //#region Event handlers
        //private void SetEnumTags(bool fOn)
        //{
        //	Type tp = Tags.LsttpEnums[lbEnums.SelectedIndex];

        //	foreach (TagInfo tsad in TagList.Where(t => t.TpEnum == tp))
        //	{
        //		tsad.FOn = fOn;
        //	}
        //}

        //private void BtnAllOnClick(object sender, EventArgs e)
        //{
        //	SetEnumTags(true);
        //}

        //private void BtnAllOffClick(object sender, EventArgs e)
        //{
        //	SetEnumTags(false);
        //}

        //private void EnumSelectionChanged(object sender, EventArgs e)
        //{
        //	TraceTypeInfo ttiThisEnum = TtiCur;

        //	_miscInfo.ShowTimeStamps = ttiThisEnum.ShowTimeStamps;
        //	_miscInfo.ShowThreadIds = ttiThisEnum.ShowThreadIds;
        //	_miscInfo.ShowSeverity = ttiThisEnum.ShowSeverity;
        //	_miscInfo.ShowTagName = ttiThisEnum.ShowTagName;
        //	_miscInfo.StrSeverityFilter = ttiThisEnum.TraceLevelFilter.ToString();
        //	SetTagColors();

        //	CheckListeners();
        //}

        //private void SetTagColors()
        //{
        //	var ttiCur = TtiCur;

        //	foreach (var tagInfo in TagList)
        //	{
        //		tagInfo.Color = ttiCur.TagNames.Contains(tagInfo.StrTag) ? EnumTagColor : NormalTagColor;

        //	}
        //}

        //private void BtnOkayClick(object sender, RoutedEventArgs e)
        //{
        //	DialogResult = true;
        //}

        //private void btnManageListeners_Click(object sender, RoutedEventArgs e)
        //{
        //	var tdlg = new ListenerDialog();
        //	var fOk = tdlg.ShowDialog();
        //	if (fOk ?? false)
        //	{
        //		// Add any new listeners
        //		foreach (var tlis in tdlg.ListenerList.Where(tlis => !Trace.Listeners.Contains(tlis)))
        //		{
        //			Trace.Listeners.Add(tlis);
        //		}
        //		// ...and remove any deleted listeners
        //		var lstlistToBeDeleted =
        //			Trace.Listeners.Cast<TraceListener>().Where(
        //				tlis => Tracer.FIsInternalListener(tlis) &&
        //				!tdlg.ListenerList.Contains(tlis)).ToList();
        //		foreach (var tlis in lstlistToBeDeleted)
        //		{
        //			Tracer.EliminateInternalListener(tlis);
        //		}
        //		SetListeners();
        //		CheckListeners();
        //	}
        //}
        //#endregion
    }
}
