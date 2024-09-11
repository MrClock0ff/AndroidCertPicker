using Android.Security;
using JObject = Java.Lang.Object;

namespace MrClock0ff.AndroidCertPicker;

public class DefaultKeyChainAliasCallback(Action<string?> callback) : JObject, IKeyChainAliasCallback
{
	private readonly Action<string?> _callback = callback;

	public void Alias(string? alias)
	{
		_callback?.Invoke(alias);
	}
}