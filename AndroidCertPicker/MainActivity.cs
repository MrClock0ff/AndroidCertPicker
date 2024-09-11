using Android.Views.InputMethods;
using Android.Webkit;
using AndroidX.Fragment.App;
using MrClock0ff.AndroidCertPicker;

namespace AndroidCertPicker;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : FragmentActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		// Set our view from the "main" layout resource
		SetContentView(Resource.Layout.activity_main);

		if (FindViewById(Resource.Id.webview) is WebView webView)
		{
			webView.SetWebViewClient(new DefaultWebViewClient(this));
			
			if (FindViewById(Resource.Id.uri_edittext) is EditText editText)
			{
				editText.EditorAction += (sender, args) =>
				{
					if (args.ActionId == ImeAction.Go)
					{
						editText.ClearFocus();
						
						if (!string.IsNullOrEmpty(editText.Text))
						{
							webView.LoadUrl(editText.Text);
						}
					}
				};
			}
		}
	}
}