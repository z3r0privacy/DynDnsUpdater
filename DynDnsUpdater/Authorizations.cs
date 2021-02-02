using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DynDnsUpdater
{
    public interface IAuthentication
    {
        string Name { get; }
        void AttachAuthentication(HttpWebRequest request, Domain domainData);
    }

    public class BasicAuthorization : IAuthentication
    {
        public string Name => "Basic";

        public void AttachAuthentication(HttpWebRequest request, Domain domainData)
        {
            var authstring = $"{domainData.User ?? ""}:{domainData.Password ?? ""}";
            var base64Str = Convert.ToBase64String(Encoding.UTF8.GetBytes(authstring));
            request.Headers.Add(HttpRequestHeader.Authorization, $"Basic {base64Str}");
        }
    }
}
