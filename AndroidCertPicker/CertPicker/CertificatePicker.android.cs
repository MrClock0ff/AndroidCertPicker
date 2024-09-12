using Java.Security;
using Java.Security.Cert;
using FragmentX = AndroidX.Fragment.App.Fragment;
using FragmentManagerX = AndroidX.Fragment.App.FragmentManager;

namespace MrClock0ff.AndroidCertPicker;

public class CertificatePicker
{
	public Task<X509Certificate?> Pick(FragmentX fragment)
	{
		if (fragment.ChildFragmentManager is null)
		{
			throw new Exception("unable to find suitable fragment manager");
		}
		
		return Pick(fragment.ChildFragmentManager);
	}

	public Task<X509Certificate?> Pick(FragmentManagerX fragmentManager)
	{
		TaskCompletionSource<X509Certificate?> tcs = new TaskCompletionSource<X509Certificate?>();
		KeyStore? store = KeyStore.GetInstance("AndroidCAStore");
		store?.Load(default, default);
		IEnumerable<X509Certificate> certs = store?.GetCertificates() ?? [];
		ListDialogFragment<X509Certificate> dialog = new ListDialogFragment<X509Certificate>(certs);
		dialog.SelectedItemChanged += cert => tcs.SetResult(cert);
		dialog.Show(fragmentManager, nameof(ListDialogFragment<X509Certificate>));
		return tcs.Task;
	}
}