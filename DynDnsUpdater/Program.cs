using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;

namespace DynDnsUpdater
{
    class Program
    {
        const string IP_START_MARKER = "Current IP Address: ";
        const string IP_END_MARKER = "</body>";
        static void Main(string[] args)
        {
            Configuration config;
            try
            {
                config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText("/appdata/config.json"));
            } catch (Exception ex)
            {
                Console.WriteLine("Could not start because of an error while loading the configuration.");
                Console.WriteLine(ex.ToString());
                return;
            }

            var authenticationMethods = new IAuthentication[]
            {
                new BasicAuthorization()
            };

            // check configuration
            var configErrors = CheckConfiguration(config, authenticationMethods);
            if (configErrors.Count > 0)
            {
                Console.WriteLine($"Config check failed due to the following errors:");
                foreach (var err in configErrors)
                {
                    Console.WriteLine(" - " + err);
                }
                return;
            }

            // work
            string lastIP = null;
            while (true)
            {
                try
                {
                    var myIpResponse = new WebClient().DownloadString(config.Settings.IpCheckHost);
                    var start = myIpResponse.IndexOf(IP_START_MARKER);
                    if (start < 0)
                    {
                        throw new Exception("The marker for the IP start could not be found");
                    }
                    start += IP_START_MARKER.Length;
                    var end = myIpResponse.IndexOf(IP_END_MARKER);
                    if (end < 0)
                    {
                        throw new Exception("The marker for the IP end could not be found");
                    }
                    var ip = myIpResponse.Substring(start, end - start);
                    
                    if (ip != lastIP)
                    {
                        Console.WriteLine($"Updating IP from {lastIP} to {ip}");
                        var success = true;
                        foreach (var d in config.Domains)
                        {
                            try
                            {
                                var service = config.Services.First(s => s.Name.Equals(d.Service, StringComparison.OrdinalIgnoreCase));
                                var request = WebRequest.CreateHttp(string.Format(service.Url, ip, d.Fqdn));
                                authenticationMethods.First(a => a.Name.Equals(service.Auth, StringComparison.OrdinalIgnoreCase)).AttachAuthentication(request, d);
                                request.Method = service.HttpMethod ?? "GET";
                                var response = (HttpWebResponse)request.GetResponse();
                                if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
                                {
                                    throw new Exception($"Failed to refresh IP because of response code {response.StatusCode}");
                                }
                                Console.WriteLine($"Updated IP for {d.Fqdn}");
                            } catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to update IP for {d.Fqdn}");
                                Console.WriteLine(ex.ToString());
                                success = false;
                            }
                        }

                        if (success)
                        {
                            lastIP = ip;
                        }
                    } else
                    {
                        Console.WriteLine($"IP ({lastIP}) hasn't changed.");
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine($"Occured an error while checking and updating IPs:");
                    Console.WriteLine(ex.ToString());
                } finally
                {
                    Thread.Sleep(config.Settings.RefreshInterval * 60000); // 60'000 == 1000 (ms in sec) * 60 (secs in min)
                }
            }
        }

        private static List<string> CheckConfiguration(Configuration config, IAuthentication[] authMethods)
        {
            var errs = new List<string>();
            if (string.IsNullOrWhiteSpace(config.Settings.IpCheckHost))
            {
                errs.Add("No host is set for checking the current IP");
            }
            foreach (var provider in config.Services)
            {
                if (string.IsNullOrWhiteSpace(provider.Name))
                {
                    errs.Add("Name for service is missing");
                }
                if (string.IsNullOrWhiteSpace(provider.Url))
                {
                    errs.Add($"Url for {provider.Name} is missing");
                }
                if (!authMethods.Any(auth => auth.Name.Equals(provider.Auth, StringComparison.OrdinalIgnoreCase))) {
                    errs.Add($"Authentication method ({provider.Auth}) not implemented for {provider.Name}");
                }
            }
            foreach(var domain in config.Domains)
            {
                if (string.IsNullOrWhiteSpace(domain.Fqdn))
                {
                    errs.Add("No FQDN provided");
                }
                if (!config.Services.Any(s => s.Name.Equals(domain.Service, StringComparison.OrdinalIgnoreCase)))
                {
                    errs.Add($"Service {domain.Service} not found for {domain.Fqdn}");
                }
            }
            return errs;
        }
    }
}
