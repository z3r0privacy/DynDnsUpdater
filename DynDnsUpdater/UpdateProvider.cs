using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using SMBLibrary;

namespace DynDnsUpdater
{
    public interface IUpdateProvider
    {
        string ProviderName { get; }
        bool UpdateIP(string newIP, Domain domain, Service service);
    }

    public class AllInklUpdateProvider : IUpdateProvider
    {
        public string ProviderName => "All-Inkl";

        public bool UpdateIP(string newIP, Domain domain, Service service)
        {
            var authenticator = new BasicAuthorization();
            var request = WebRequest.CreateHttp(string.Format(service.Path, newIP, domain.Fqdn));
            authenticator.AttachAuthentication(request, domain);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
        }
    }

    public class MsDnsUpdateProvider : IUpdateProvider
    {
        public string ProviderName => "MSDNS";

        public bool UpdateIP(string newIP, Domain domain, Service service)
        {
            using (var smbClient = new FileShareClient(domain.User, domain.Password, service.Path))
            {
                if (smbClient.Connect())
                {
                    var status = smbClient.Share.CreateFile(out var handle, out _, "ip.txt", AccessMask.GENERIC_WRITE,
                        0, ShareAccess.Write, CreateDisposition.FILE_OPEN_IF, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
                    if (status == NTStatus.STATUS_SUCCESS)
                    {
                        var data = Encoding.ASCII.GetBytes(newIP);
                        status = smbClient.Share.WriteFile(out _, handle, 0, data);
                        if (status == NTStatus.STATUS_SUCCESS)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public class NginxSSHUpdateProvider : IUpdateProvider
    {
        public string ProviderName => "NginxSSH";

        public bool UpdateIP(string newIP, Domain domain, Service service)
        {
            var connStrs = service.Path.Split('@');
            var conInfo = new ConnectionInfo(connStrs[1], connStrs[0], new PrivateKeyAuthenticationMethod("mgmt", new PrivateKeyFile(domain.Password)));
            using (var client = new SshClient(conInfo))
            {
                client.Connect();
                var cmd = client.RunCommand($"echo {newIP} > ip_update/ip");
                if (cmd.ExitStatus != 0)
                {
                    return false;
                }
                cmd = client.RunCommand("sudo ./update_ip.sh");
                if (cmd.ExitStatus != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
