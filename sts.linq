<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.dll</Reference>
  <Namespace>System.IdentityModel</Namespace>
  <Namespace>System.IdentityModel.Services</Namespace>
  <Namespace>System.ServiceModel</Namespace>
  <Namespace>System.ServiceModel.Channels</Namespace>
  <Namespace>System.IdentityModel.Protocols.WSTrust</Namespace>
  <Namespace>System.ServiceModel.Description</Namespace>
  <Namespace>System.IdentityModel.Tokens</Namespace>
  <Namespace>System.Security.Claims</Namespace>
  <Namespace>System.IdentityModel.Configuration</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Security.Cryptography.X509Certificates</Namespace>
</Query>

void Main()
{
	var stsAddress = $"http://{Environment.MachineName}:8000/STS";
	
	var url = stsAddress;
	var binding = new BasicHttpBinding();
	using (var host = new ServiceHost(typeof(SecurityTokenWCFService)))
	{
		IncludeExceptionDetails(host);
		host.AddServiceEndpoint(typeof(ISecurityTokenService), binding, url);
		host.Open();
		Console.WriteLine($"Running on {url}");
		Console.ReadLine();
	}
	Console.WriteLine("Finished");
}


void IncludeExceptionDetails(ServiceHost host)
{
	host.Description.Behaviors.Remove(
		typeof(ServiceDebugBehavior));
	host.Description.Behaviors.Add(
		new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
}


const string ACTION = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue";
const string REPLYACTION = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue";


[ServiceContract(Name = "SecurityTokenService", Namespace = "http://foobar.org")]
public interface ISecurityTokenService
{
	[OperationContract(Action = ACTION, ReplyAction = REPLYACTION)]
	Message Issue(Message message);
}

static SigningCredentials CreateSigningCredentials()
{
	return new X509SigningCredentials(GetCertificate());
}


static X509Certificate2 GetCertificate()
{
	var filename = Path.Combine(Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath), "LocalSTS.pfx");
	var password = "LocalSTS";
	return new X509Certificate2(filename, password, X509KeyStorageFlags.PersistKeySet);
}

public class SecurityTokenWCFService : ISecurityTokenService
{
	public Message Issue(Message message)
	{
		var sContext = new WSTrustSerializationContext();
		var rst = new WSTrustFeb2005RequestSerializer().ReadXml(message.GetReaderAtBodyContents(), sContext);

		var sts = new STS(new SecurityTokenServiceConfiguration("Custom token issuer", CreateSigningCredentials()));
		var rstr = sts.Issue(null, rst);

		var ms = new MemoryStream();
		var writer = XmlWriter.Create(ms);
		new WSTrustFeb2005ResponseSerializer().WriteXml(rstr, writer, sContext);
		writer.Flush();
		ms.Position = 0;
		
		var xelmt = XElement.Parse(new StreamReader(ms).ReadToEnd());
		xelmt.Dump();
		return Message.CreateMessage(MessageVersion.Soap11, REPLYACTION, xelmt);
	}
}


public class STS : SecurityTokenService
{
	public STS(SecurityTokenServiceConfiguration configuration) : base(configuration)
	{
	}
	
	protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
	{
		Console.WriteLine("GetOutputClaimsIdentity");
		return new ClaimsIdentity(new[] { 
			new Claim(ClaimTypes.Name, "Here Is A Name"),
			new Claim(ClaimTypes.DateOfBirth, "2000-01-01"),
			new Claim(ClaimTypes.NameIdentifier, "12345678901")
		});
	}

	protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
	{		
		Console.WriteLine("GetScope");
		var serviceAddress = $"http://{Environment.MachineName}:8000/Service";
		return new Scope(serviceAddress, SecurityTokenServiceConfiguration.SigningCredentials) { 
			TokenEncryptionRequired = false,
			SymmetricKeyEncryptionRequired = false
		};
	}
}