<Query Kind="Program">
  <Namespace>System.Security.Cryptography.X509Certificates</Namespace>
</Query>

void Main()
{
	var cert = GetCertificate();
	cert.Thumbprint.Dump();
}

X509Certificate2 GetCertificate()
{
	var filename = Path.Combine(Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath), "LocalSTS.pfx");
	var password = "LocalSTS";
	return new X509Certificate2(filename, password, X509KeyStorageFlags.PersistKeySet);
}
