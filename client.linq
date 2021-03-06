<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Transactions.Bridge.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\SMDiagnostics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.Selectors.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Messaging.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.DurableInstancing.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Activation.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Internals.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Namespace>System.IdentityModel.Protocols.WSTrust</Namespace>
  <Namespace>System.IdentityModel.Tokens</Namespace>
  <Namespace>System.ServiceModel</Namespace>
  <Namespace>System.ServiceModel.Security</Namespace>
  <Namespace>System.ServiceModel.Channels</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Security.Cryptography.X509Certificates</Namespace>
  <Namespace>System.Net.Security</Namespace>
</Query>

static string stsAddress = $"http://{Environment.MachineName}:8000/STS";
static string serviceAddress = $"http://{Environment.MachineName}:8000/Service";

void Main()
{
	var token = GetToken();
	Console.WriteLine("Got token");
	
	var binding = new WSFederationHttpBinding(WSFederationHttpSecurityMode.Message);
	binding.Security.Message.EstablishSecurityContext = false;
	binding.Security.Message.NegotiateServiceCredential = false;
	var factory = new ChannelFactory<ICrossGatewayQueryITI38>(binding, new EndpointAddress(new Uri(serviceAddress), new DnsEndpointIdentity("WCFHOLService")));
	
	factory.Credentials.SupportInteractive = false;
	factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;	
	factory.Credentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectDistinguishedName, "CN=WCFHOLService");

	var proxy = factory.CreateChannelWithIssuedToken(token);
	var response = proxy.CrossGatewayQuery(Message.CreateMessage(MessageVersion.Soap12WSAddressing10, "urn:ihe:iti:2007:CrossGatewayQuery", "Hello world"));
	response.GetBody<string>().Dump();	
}


X509Certificate2 GetServiceSertificate()
{
	return new X509Certificate2(Path.Combine(Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath), "LocalSTS.cer"));
}

SecurityToken GetToken()
{
	var binding = new BasicHttpBinding();
	var factory = new WSTrustChannelFactory(binding, stsAddress);
	factory.TrustVersion = TrustVersion.WSTrustFeb2005;

	var rst =	 new RequestSecurityToken
	{
		RequestType = RequestTypes.Issue,
		KeyType = KeyTypes.Symmetric,
		AppliesTo = new EndpointReference(serviceAddress)
	};
	return factory.CreateChannel().Issue(rst);
}

[ServiceContract(ProtectionLevel = ProtectionLevel.Sign, Namespace = "urn:ihe:iti:xds-b:2007")]
public interface ICrossGatewayQueryITI38
{
	[OperationContract(Action = "urn:ihe:iti:2007:CrossGatewayQuery", ReplyAction = "urn:ihe:iti:2007:CrossGatewayQueryResponse")]
	Message CrossGatewayQuery(Message request);
}
// Define other methods and classes here