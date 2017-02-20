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

//
// Service
//

static string serviceAddress = $"http://{Environment.MachineName}:8000/Service";
static string stsAddress = $"http://{Environment.MachineName}:8000/STS";

void Main()
{
	var url = serviceAddress;
	var binding = new BasicHttpBinding();
	using (var host = new ServiceHost(typeof(MyService), new Uri(url)))
	{
		IncludeExceptionDetails(host);
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

public class MyService : ICrossGatewayQueryITI38
{
	public Message CrossGatewayQuery(Message request)
	{
		return Message.CreateMessage(MessageVersion.Soap11, "urn:ihe:iti:2007:CrossGatewayQueryResponse", "Hello world!");
	}
}

[ServiceContract(Namespace = "urn:ihe:iti:xds-b:2007")]
public interface ICrossGatewayQueryITI38
{
	[OperationContract(Action = "urn:ihe:iti:2007:CrossGatewayQuery", ReplyAction = "urn:ihe:iti:2007:CrossGatewayQueryResponse")]
	Message CrossGatewayQuery(Message request);
}