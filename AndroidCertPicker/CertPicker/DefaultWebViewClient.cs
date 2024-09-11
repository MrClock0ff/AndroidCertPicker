using System.Runtime.Versioning;
using Android.Security;
using Android.Webkit;
using Java.Security;
using Java.Security.Cert;
using Uri = Android.Net.Uri;

namespace MrClock0ff.AndroidCertPicker;

public class DefaultWebViewClient(Activity? activity) : WebViewClient
{
	private Activity? _activity = activity;
	private bool _disposed;
	
	public override async void OnReceivedClientCertRequest(WebView? view, ClientCertRequest? request)
	{
		if (_activity == null || !OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			base.OnReceivedClientCertRequest(view, request);
			return;
		}
		
		string? alias = await ChoosePrivateKeyAlias(_activity, view?.Url, request);

		if (string.IsNullOrEmpty(alias))
		{
			base.OnReceivedClientCertRequest(view, request);
			return;
		}
		
		IPrivateKey? privateKey = KeyChain.GetPrivateKey(_activity, alias);
		X509Certificate[]? chain = KeyChain.GetCertificateChain(_activity, alias);
		request?.Proceed(privateKey, chain);
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
			_activity = null;
		}
		
		base.Dispose(disposing);
	}

	[SupportedOSPlatform("android23.0")]
	private Task<string?> ChoosePrivateKeyAlias(Activity activity, string? url, ClientCertRequest? request)
	{
		Uri? uri = TryParseUrl(url);
		
		TaskCompletionSource<string?> tcs = new TaskCompletionSource<string?>();
		KeyChain.ChoosePrivateKeyAlias(activity, new DefaultKeyChainAliasCallback(alias => tcs.SetResult(alias)), 
			request?.GetKeyTypes(), request?.GetPrincipals(), uri, default);
		return tcs.Task;
	}

	private Uri? TryParseUrl(string? url)
	{
		try
		{
			return Uri.Parse(url);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return default;
		}
	}
}