using Android.Content;
using DialogFragmentX = AndroidX.Fragment.App.DialogFragment;

namespace MrClock0ff.AndroidCertPicker;

public class ListDialogFragment<TItem>(IEnumerable<TItem> source) : DialogFragmentX
{
	private ArrayAdapter<TItem>? _listAdapter;
	private int _selectedItemIndex;
	private bool _disposed;

	public event Action<TItem?>? SelectedItemChanged;
	public event Action Cancelled;

	public TItem? SelectedItem
	{
		get
		{
			if (_listAdapter == null)
			{
				return default;
			}
			
			return _listAdapter.GetItem(_selectedItemIndex);
		}
	}

	public override Dialog OnCreateDialog(Bundle? savedInstanceState)
	{
		_listAdapter = new ArrayAdapter<TItem>(Context, Android.Resource.Layout.SimpleListItem1, source.ToArray());
		Dialog dialog = new AlertDialog.Builder(Context)
			.SetTitle("Select Certificate")
			.SetSingleChoiceItems(_listAdapter, _selectedItemIndex, Dialog_OnItemSelected)
			.SetCancelable(false)
			.Create();
		return dialog;
	}

	protected virtual void Dialog_OnItemSelected(object? sender, DialogClickEventArgs args)
	{
		if (_selectedItemIndex != args.Which)
		{
			_selectedItemIndex = args.Which;
			SelectedItemChanged?.Invoke(SelectedItem);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		if (disposing)
		{
			_listAdapter?.Dispose();
			_listAdapter = null;
		}
		
		base.Dispose(disposing);
	}
}