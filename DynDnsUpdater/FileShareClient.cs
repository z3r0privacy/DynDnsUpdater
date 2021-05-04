using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SMBLibrary;
using SMBLibrary.Client;

namespace DynDnsUpdater
{
    // https://github.com/rebelweb/DotNetCoreFileShare/blob/main/src/FileShare.Access/FileShareClient.cs
    public class FileShareClient : SMB2Client, IDisposable
    {
        private readonly string _domainName;
        private readonly string _username;
        private readonly string _password;
        private readonly string _ipAddress;
        private readonly string _shareName;
        private SMB2FileStore _fileStore;
        private NTStatus _status;
        private bool _connected;


        public FileShareClient(string user, string password, string share)
        {
            _domainName = user.Split('\\')[0];
            _username = user.Split('\\')[1];
            _password = password;
            _ipAddress = share.Split('\\')[0];
            _shareName = share.Split('\\')[1];
        }

        public SMB2FileStore Share => _fileStore;

        public bool Connect()
        {
            _connected = Connect(IPAddress.Parse(_ipAddress), SMBTransportType.DirectTCPTransport);

            if (_connected)
            {
                _status = Login(_domainName, _username, _password);
                if (_status == NTStatus.STATUS_SUCCESS)
                {
                    _fileStore = TreeConnect(_shareName, out _status) as SMB2FileStore;
                    return true;
                }
            }

            return false;
        }

        public new void  Disconnect()
        {
            if (_connected && _status == NTStatus.STATUS_SUCCESS)
                Logoff();

            if (_connected)
                base.Disconnect();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
