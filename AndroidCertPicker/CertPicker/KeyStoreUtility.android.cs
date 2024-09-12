using System.Text;
using Android.Content;
using Android.Security;
using Java.Security;
using Java.Util;
using X509Certificate = Java.Security.Cert.X509Certificate;
using JObject = Java.Lang.Object;

namespace MrClock0ff.AndroidCertPicker;

public static class KeyStoreUtility
{
	public static Task<IPrivateKey?> GetPrivateKey(this KeyStore? store, X509Certificate? certificate)
	{
		return Task.Run(() =>
		{
			if (store is null || certificate is null)
			{
				return null;
			}

			string? alias = store.GetCertificateAlias(certificate);

			if (string.IsNullOrEmpty(alias))
			{
				return null;
			}

			Context context = Application.Context;
			IPrivateKey? privateKey = KeyChain.GetPrivateKey(context, alias);
			X509Certificate[]? certChain = KeyChain.GetCertificateChain(context, alias);

			if (certChain is null || !certChain.Any())
			{
				return null;
			}

			byte[] validationData = Encoding.ASCII.GetBytes("validationData");
			Signature? signature = Signature.GetInstance("SHA1withRSA");

			if (signature is null)
			{
				return null;
			}

			signature.InitSign(privateKey);
			signature.Update(validationData);
			byte[]? signed = signature.Sign();

			IPublicKey? publicKey = certChain[0].PublicKey;
			signature.InitVerify(publicKey);
			signature.Update(validationData);
			bool hasValidSignature = signature.Verify(signed);

			if (!hasValidSignature)
			{
				return null;
			}

			return privateKey;
		});
	}
	
	public static string? GetCertificateAlias(this KeyStore? store, X509Certificate? certificate)
	{
		if (store is null || certificate is null)
		{
			return null;
		}

		string? alias = store.GetCertificateAlias(certificate);

		return alias;
	}
	
	public static IEnumerable<X509Certificate> GetCertificates(this KeyStore? store)
	{
		if (store is null)
		{
			return [];
		}
		
		IEnumerable<string> aliases = store.Aliases()?.ToList<string>() ?? new List<string>();

		List<X509Certificate> certs = aliases
			.Select(alias => store.GetCertificate(alias) as X509Certificate)
			.Where(cert => cert != null)
			.ToList();

		return certs;
	}

	private static List<TType> ToList<TType>(this IEnumeration enumeration)
	{
		if (enumeration == null)
		{
			return [];
		}

		List<TType> casts = new List<TType>();
		
		while (enumeration.HasMoreElements)
		{
			JObject item = enumeration.NextElement();

			if (item is TType cast)
			{
				casts.Add(cast);
			}
		}

		return casts;
	}
}