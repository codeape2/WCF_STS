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
</Query>

static string stsAddress = $"http://{Environment.MachineName}:8000/STS";
static string serviceAddress = $"http://{Environment.MachineName}:8000/Service";

void Main()
{
	var token = GetToken();
	Console.WriteLine("Got token");
	
	var binding = new WSFederationHttpBinding(WSFederationHttpSecurityMode.Message);	
	var factory = new ChannelFactory<ICrossGatewayQueryITI38>(binding, new EndpointAddress(new Uri(serviceAddress), new DnsEndpointIdentity("LocalSTS")));
	
	factory.Credentials.SupportInteractive = false;
	factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;	
	
	var proxy = factory.CreateChannelWithIssuedToken(token);
	var response = proxy.CrossGatewayQuery( Message.CreateMessage(MessageVersion.Soap11, "urn:ihe:iti:2007:CrossGatewayQuery", "Hello world"));
	response.GetBody<string>().Dump();
}

SecurityToken GetToken()
{
	var binding = new BasicHttpBinding();
	var factory = new WSTrustChannelFactory(binding, stsAddress);
	factory.TrustVersion = TrustVersion.WSTrustFeb2005;

	var rst = new RequestSecurityToken
	{
		RequestType = RequestTypes.Issue,
		KeyType = KeyTypes.Symmetric,
		AppliesTo = new EndpointReference(serviceAddress)
	};
	return factory.CreateChannel().Issue(rst);
}

[ServiceContract(Namespace = "urn:ihe:iti:xds-b:2007")]
public interface ICrossGatewayQueryITI38
{
	[OperationContract(Action = "urn:ihe:iti:2007:CrossGatewayQuery", ReplyAction = "urn:ihe:iti:2007:CrossGatewayQueryResponse")]
	Message CrossGatewayQuery(Message request);
}
// Define other methods and classes here