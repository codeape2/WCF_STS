For THUMBPRINT/certstorname, use a certificate in the machine cert store w/ private key.

Appid must be a unique GUID.

C:\> netsh http add sslcert hostnameport=MACHINENAME:8000 appid={d4382442-a88a-4ac8-93c6-1128d97420ba} certhash=THUMBPRINT certstorename=MY


Proper way is described here:
https://blogs.msdn.microsoft.com/james_osbornes_blog/2010/12/10/selfhosting-a-wcf-service-over-https/