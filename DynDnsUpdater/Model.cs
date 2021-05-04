using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DynDnsUpdater
{
    public class Settings
    {
        [JsonPropertyName("ipCheckHost")]
        public string IpCheckHost { get; set; }

        [JsonPropertyName("refreshInterval")]
        public int RefreshInterval { get; set; }
    }

    public class Service
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Domain
    {
        [JsonPropertyName("fqdn")]
        public string Fqdn { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class Configuration
    {
        [JsonPropertyName("settings")]
        public Settings Settings { get; set; }

        [JsonPropertyName("services")]
        public List<Service> Services { get; set; }

        [JsonPropertyName("domains")]
        public List<Domain> Domains { get; set; }
    }
}
