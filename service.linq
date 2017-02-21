<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.dll</Reference>
  <Namespace>System.IdentityModel</Namespace>
  <Namespace>System.IdentityModel.Configuration</Namespace>
  <Namespace>System.IdentityModel.Protocols.WSTrust</Namespace>
  <Namespace>System.IdentityModel.Services</Namespace>
  <Namespace>System.IdentityModel.Tokens</Namespace>
  <Namespace>System.Security.Claims</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Security.Cryptography.X509Certificates</Namespace>
  <Namespace>System.ServiceModel</Namespace>
  <Namespace>System.ServiceModel.Channels</Namespace>
  <Namespace>System.ServiceModel.Description</Namespace>
  <Namespace>System.ServiceModel.Security</Namespace>
  <Namespace>System.Net.Security</Namespace>
  <Namespace>System.Net</Namespace>
  <AppConfig>
    <Content>
      <configuration>
        <system.diagnostics>
          <sources>
            <source name="System.ServiceModel" switchValue="Verbose, ActivityTracing">
              <listeners>
                <add name="wif" />
              </listeners>
            </source>
            <source name="System.IdentityModel" switchValue="Verbose">
              <listeners>
                <add name="wif" />
              </listeners>
            </source>
            <source name="Microsoft.IdentityModel" switchValue="Verbose">
              <listeners>
                <add name="wif" />
              </listeners>
            </source>
          </sources>
          <sharedListeners>
            <add name="wif" type="System.Diagnostics.XmlWriterTraceListener" initializeData="D:\Temp\WIF.svclog" />
          </sharedListeners>
          <trace autoflush="true" />
        </system.diagnostics>
      </configuration>
    </Content>
  </AppConfig>
</Query>

//
// Service
//

static string serviceAddress = $"http://{Environment.MachineName}:8000/Service";
static string stsAddress = $"http://{Environment.MachineName}:8000/STS";

void Main()
{
	ServicePointManager.CheckCertificateRevocationList = false;
	ServicePointManager.ServerCertificateValidationCallback =
				delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
				{ return true; };
				
	var url = serviceAddress;
	var binding = new WSFederationHttpBinding(WSFederationHttpSecurityMode.Message);
	binding.Security.Message.EstablishSecurityContext = false;
	binding.Security.Message.NegotiateServiceCredential = false;
	
	using (var host = new ServiceHost(typeof(MyService)))
	{
		host.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectDistinguishedName, "CN=WCFHOLService");
		host.Credentials.UseIdentityConfiguration = true;
		host.Credentials.IdentityConfiguration = CreateIdentityConfig();
		/*
		host.Credentials.IdentityConfiguration.CertificateValidationMode = X509CertificateValidationMode.None;
		host.Credentials.IdentityConfiguration.AudienceRestriction.AudienceMode = System.IdentityModel.Selectors.AudienceUriMode.Never;
		*/
		host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
		host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
		host.AddServiceEndpoint(typeof(ICrossGatewayQueryITI38), binding, serviceAddress);
		IncludeExceptionDetails(host);
		host.Open();
		Console.WriteLine($"Running on {url}");
		Console.ReadLine();
	}
	Console.WriteLine("Finished");
}

IdentityConfiguration CreateIdentityConfig()
{
	IdentityConfiguration identityConfig = new IdentityConfiguration(false);

	//AUDIENCE URI                
	//this value can be a fantasy name and not a real URL, it identifies our service whitin the ADFS server.
	//It must be equal to the one defined in ADFS Console ("RELAYING PARTY trust identifiers" section)
	//the token we receive contains this value, so if do not match we fail
	identityConfig.AudienceRestriction.AllowedAudienceUris.Add(new Uri($"http://{Environment.MachineName}:8000/Service"));

	//ISSUER NAME REGISTRY explicit the thumbprint of the accepted certificates, if the token coming in is not signed with any of these certificates then is considered invalid
	var issuerNameRegistry = new ConfigurationBasedIssuerNameRegistry();
	//issuerNameRegistry.AddTrustedIssuer();
	issuerNameRegistry.AddTrustedIssuer("81 5b 06 b2 7f 5b 26 30 47 3b 8a b9 56 bb 9f 9f 8c 36 20 76", "signing certificate sts"); //STS signing certificate thumbprint
	identityConfig.IssuerNameRegistry = issuerNameRegistry;
	identityConfig.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
	return identityConfig;

}


void IncludeExceptionDetails(ServiceHost host)
{
	host.Description.Behaviors.Remove(
		typeof(ServiceDebugBehavior));
	host.Description.Behaviors.Add(
		new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
}

public class MyService : ICrossGatewayQueryITI38
{
	public Message CrossGatewayQuery(Message request)
	{
		return Message.CreateMessage(MessageVersion.Soap12WSAddressing10, "urn:ihe:iti:2007:CrossGatewayQueryResponse", "Hello world!");
	}
}

[ServiceContract(ProtectionLevel = ProtectionLevel.Sign, Namespace = "urn:ihe:iti:xds-b:2007")]
public interface ICrossGatewayQueryITI38
{
	[OperationContract(Action = "urn:ihe:iti:2007:CrossGatewayQuery", ReplyAction = "urn:ihe:iti:2007:CrossGatewayQueryResponse")]
	Message CrossGatewayQuery(Message request);
}

static X509Certificate2 GetCertificate()
{
	var filename = Path.Combine(Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath), "LocalSTS.pfx");
	var password = "LocalSTS";
	return new X509Certificate2(filename, password, X509KeyStorageFlags.PersistKeySet);
}