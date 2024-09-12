using System.Runtime.Versioning;
using Android.Net.Http;
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

	public override void OnReceivedError(WebView? view, IWebResourceRequest? request, WebResourceError? error)
	{
		
	}

	public override void OnReceivedError(WebView? view, ClientError errorCode, string? description, string? failingUrl)
	{
		
	}

	public override void OnReceivedHttpError(WebView? view, IWebResourceRequest? request, WebResourceResponse? errorResponse)
	{
		
	}

	public override void OnReceivedSslError(WebView? view, SslErrorHandler? handler, SslError? error)
	{
		handler.Proceed();
	}

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

		Tuple<IPrivateKey?, X509Certificate[]?> pair = await Task.Run(() =>
		{
			IPrivateKey? privateKey = KeyChain.GetPrivateKey(_activity, alias);
			X509Certificate[]? chain = KeyChain.GetCertificateChain(_activity, alias);
			return Task.FromResult(new Tuple<IPrivateKey?, X509Certificate[]?>(privateKey, chain));
		});
		
		request?.Proceed(pair.Item1, pair.Item2);
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