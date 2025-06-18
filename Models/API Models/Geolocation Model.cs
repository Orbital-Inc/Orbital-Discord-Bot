using System.Text.Json.Serialization;

namespace MainBot.Models.APIModels;

public class GeolocationModel
{
    public class CombinedGeoLocationData
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("responseTime")]
        public double ResponseTime { get; set; }

        [JsonPropertyName("orbitalGeoData")]
        public OrbitalGeoData? OrbitalGeoData { get; set; }

        //[JsonPropertyName("dbIp")]
        //public DBIP? DbIp { get; set; }

        [JsonPropertyName("ipRegistryDb")]
        public IPRegistryDB.IPRegistryDBRoot? IpRegistryDb { get; set; }

        [JsonPropertyName("ipData")]
        public IPData.IPDataRoot? IpData { get; set; }

        [JsonPropertyName("ipInfo")]
        public IPInfo? IpInfo { get; set; }

        //[JsonPropertyName("columitas")]
        //public Columitas.ColumitasRoot? Columitas { get; set; }

        [JsonPropertyName("ipQualityScore")]
        public IPQualityScore? IpQualityScore { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class OrbitalGeoData
    {
        [JsonPropertyName("ipAddress")]
        public string? IPAddress { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("route")]
        public string? Route { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("cloudProvider")]
        public bool? CloudProvider { get; set; }

        [JsonPropertyName("icloudRelay")]
        public bool? IcloudRelay { get; set; }

        [JsonPropertyName("datacenter")]
        public bool? Datacenter { get; set; }

        [JsonPropertyName("tor")]
        public bool? Tor { get; set; }

        [JsonPropertyName("proxy")]
        public bool? Proxy { get; set; }

        [JsonPropertyName("abuser")]
        public bool? Abuser { get; set; }

        [JsonPropertyName("attacker")]
        public bool? Attacker { get; set; }

        [JsonPropertyName("torExit")]
        public bool? TorExit { get; set; }

        [JsonPropertyName("bogon")]
        public bool? Bogon { get; set; }

        [JsonPropertyName("relay")]
        public bool? Relay { get; set; }

        [JsonPropertyName("anonymous")]
        public bool? Anonymous { get; set; }

        [JsonPropertyName("threat")]
        public bool? Threat { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("flag")]
        public string? Flag { get; set; }

        [JsonPropertyName("asName")]
        public string? ASName { get; set; }

        [JsonPropertyName("asNumber")]
        public int? ASNumber { get; set; }

        [JsonPropertyName("isp")]
        public string? ISP { get; set; }

        [JsonPropertyName("organization")]
        public string? Organization { get; set; }

        [JsonPropertyName("accuracy")]
        public GeoDataAccuracy? Accuracy { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class GeoDataAccuracy
    {
        [JsonPropertyName("ipAddressAccuracy")]
        public double IPAddressAccuracy { get; set; }

        [JsonPropertyName("countryAccuracy")]
        public double CountryAccuracy { get; set; }

        [JsonPropertyName("regionAccuracy")]
        public double RegionAccuracy { get; set; }

        [JsonPropertyName("cityAccuracy")]
        public double CityAccuracy { get; set; }

        [JsonPropertyName("districtAccuracy")]
        public double DistrictAccuracy { get; set; }

        [JsonPropertyName("domainAccuracy")]
        public double DomainAccuracy { get; set; }

        [JsonPropertyName("hostnameAccuracy")]
        public double HostnameAccuracy { get; set; }

        [JsonPropertyName("ispAccuracy")]
        public double ISPAccuracy { get; set; }

        [JsonPropertyName("organizationAccuracy")]
        public double OrganizationAccuracy { get; set; }

        [JsonPropertyName("typeAccuracy")]
        public double TypeAccuracy { get; set; }

        [JsonPropertyName("routeAccuracy")]
        public double RouteAccuracy { get; set; }

        [JsonPropertyName("abuserAccuracy")]
        public double AbuserAccuracy { get; set; }

        [JsonPropertyName("cloudProviderAccuracy")]
        public double CloudProviderAccuracy { get; set; }

        [JsonPropertyName("proxyAccuracy")]
        public double ProxyAccuracy { get; set; }

        [JsonPropertyName("torAccuracy")]
        public double TorAccuracy { get; set; }

        [JsonPropertyName("relayAccuracy")]
        public double RelayAccuracy { get; set; }

        [JsonPropertyName("bogonAccuracy")]
        public double BogonAccuracy { get; set; }

        [JsonPropertyName("threatAccuracy")]
        public double ThreatAccuracy { get; set; }

        [JsonPropertyName("torExitAccuracy")]
        public double TorExitAccuracy { get; set; }

        [JsonPropertyName("asNumberAccuracy")]
        public double ASNumberAccuracy { get; set; }

        [JsonPropertyName("asNameAccuracy")]
        public double ASNameAccuracy { get; set; }

        [JsonPropertyName("attackerAccuracy")]
        public double AttackerAccuracy { get; set; }

        [JsonPropertyName("anonymousAccuracy")]
        public double AnonymousAccuracy { get; set; }

        [JsonPropertyName("icloudRelayAccuracy")]
        public double IcloudRelayAccuracy { get; set; }

        [JsonPropertyName("datacenterAccuracy")]
        public double DatacenterAccuracy { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class IPRegistryDB
    {
        /// <summary>
        ///
        /// </summary>
        public record Carrier
        {
            public object? name { get; set; }
            public object? mcc { get; set; }
            public object? mnc { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Company
        {
            public string? domain { get; set; }
            public string? name { get; set; }
            public string? type { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Connection
        {
            public int asn { get; set; }
            public string? domain { get; set; }
            public string? organization { get; set; }
            public string? route { get; set; }
            public string? type { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Negative
        {
            public string? prefix { get; set; }
            public string? suffix { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Positive
        {
            public string? prefix { get; set; }
            public string? suffix { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Format
        {
            public Negative? negative { get; set; }
            public Positive? positive { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Currency
        {
            public string? code { get; set; }
            public string? name { get; set; }
            public string? name_native { get; set; }
            public string? plural { get; set; }
            public string? plural_native { get; set; }
            public string? symbol { get; set; }
            public string? symbol_native { get; set; }
            public Format? format { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Continent
        {
            public string? code { get; set; }
            public string? name { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Flag
        {
            public string? emoji { get; set; }
            public string? emoji_unicode { get; set; }
            public string? emojitwo { get; set; }
            public string? noto { get; set; }
            public string? twemoji { get; set; }
            public string? wikimedia { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Language
        {
            public string? code { get; set; }
            public string? name { get; set; }
            public string? native { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Country
        {
            public int area { get; set; }
            public List<string>? borders { get; set; }
            public string? calling_code { get; set; }
            public string? capital { get; set; }
            public string? code { get; set; }
            public string? name { get; set; }
            public int population { get; set; }
            public double population_density { get; set; }
            public Flag? flag { get; set; }
            public List<Language>? languages { get; set; }
            public string? tld { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Region
        {
            public string? code { get; set; }
            public string? name { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Language2
        {
            public string? code { get; set; }
            public string? name { get; set; }
            public string? native { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Location
        {
            public Continent? continent { get; set; }
            public Country? country { get; set; }
            public Region? region { get; set; }
            public string? city { get; set; }
            public string? postal { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public Language? language { get; set; }
            public bool in_eu { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record Security
        {
            public bool is_abuser { get; set; }
            public bool is_attacker { get; set; }
            public bool is_bogon { get; set; }
            public bool is_cloud_provider { get; set; }
            public bool is_proxy { get; set; }
            public bool is_relay { get; set; }
            public bool is_tor { get; set; }
            public bool is_tor_exit { get; set; }
            public bool is_anonymous { get; set; }
            public bool is_threat { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record TimeZone
        {
            public string? id { get; set; }
            public string? abbreviation { get; set; }
            public DateTime current_time { get; set; }
            public string? name { get; set; }
            public int offset { get; set; }
            public bool in_daylight_saving { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public record IPRegistryDBRoot
        {
            public string? ip { get; set; }
            public string? type { get; set; }
            public string? hostname { get; set; }
            public Carrier? carrier { get; set; }
            public Company? company { get; set; }
            public Connection? connection { get; set; }
            public Currency? currency { get; set; }
            public Location? location { get; set; }
            public Security? security { get; set; }
            public TimeZone? time_zone { get; set; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class IPData
    {
        /// <summary>
        ///
        /// </summary>
        public class Asn
        {
            public string? asn { get; set; }
            public string? name { get; set; }
            public string? domain { get; set; }
            public string? route { get; set; }
            public string? type { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class Currency
        {
            public string? name { get; set; }
            public string? code { get; set; }
            public string? symbol { get; set; }
            public string? native { get; set; }
            public string? plural { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class Language
        {
            public string? name { get; set; }
            public string? native { get; set; }
            public string? code { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class IPDataRoot
        {
            public string? ip { get; set; }
            public bool? is_eu { get; set; }
            public string? city { get; set; }
            public string? region { get; set; }
            public string? region_code { get; set; }
            public string? region_type { get; set; }
            public string? country_name { get; set; }
            public string? country_code { get; set; }
            public string? continent_name { get; set; }
            public string? continent_code { get; set; }
            public double? latitude { get; set; }
            public double? longitude { get; set; }
            public string? postal { get; set; }
            public string? calling_code { get; set; }
            public string? flag { get; set; }
            public string? emoji_flag { get; set; }
            public string? emoji_unicode { get; set; }
            public Asn? asn { get; set; }
            public List<Language>? languages { get; set; }
            public Currency? currency { get; set; }
            public TimeZone? time_zone { get; set; }
            public Threat? threat { get; set; }
            public string? count { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class Threat
        {
            public bool? is_tor { get; set; }
            public bool? is_icloud_relay { get; set; }
            public bool? is_proxy { get; set; }
            public bool? is_datacenter { get; set; }
            public bool? is_anonymous { get; set; }
            public bool? is_known_attacker { get; set; }
            public bool? is_known_abuser { get; set; }
            public bool? is_threat { get; set; }
            public bool? is_bogon { get; set; }
            public List<object>? blocklists { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class TimeZone
        {
            public string? name { get; set; }
            public string? abbr { get; set; }
            public string? offset { get; set; }
            public bool? is_dst { get; set; }
            public DateTime? current_time { get; set; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class IPInfo
    {
        [JsonPropertyName("ip")]
        public string? Ip { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("loc")]
        public string? Loc { get; set; }

        [JsonPropertyName("org")]
        public string? Org { get; set; }

        [JsonPropertyName("postal")]
        public string? Postal { get; set; }

        [JsonPropertyName("timezone")]
        public string? TimeZone { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class IPQualityScore
    {
        [JsonPropertyName("success")]
        public bool? success { get; set; }

        [JsonPropertyName("message")]
        public string? message { get; set; }

        [JsonPropertyName("fraud_score")]
        public int? fraud_score { get; set; }

        [JsonPropertyName("country_code")]
        public string? country_code { get; set; }

        [JsonPropertyName("region")]
        public string? region { get; set; }

        [JsonPropertyName("city")]
        public string? city { get; set; }

        [JsonPropertyName("ISP")]
        public string? ISP { get; set; }

        [JsonPropertyName("ASN")]
        public int? ASN { get; set; }

        [JsonPropertyName("organization")]
        public string? organization { get; set; }

        [JsonPropertyName("is_crawler")]
        public bool? is_crawler { get; set; }

        [JsonPropertyName("timezone")]
        public string? timezone { get; set; }

        [JsonPropertyName("mobile")]
        public bool? mobile { get; set; }

        [JsonPropertyName("host")]
        public string? host { get; set; }

        [JsonPropertyName("proxy")]
        public bool? proxy { get; set; }

        [JsonPropertyName("vpn")]
        public bool? vpn { get; set; }

        [JsonPropertyName("tor")]
        public bool? tor { get; set; }

        [JsonPropertyName("active_vpn")]
        public bool? active_vpn { get; set; }

        [JsonPropertyName("active_tor")]
        public bool? active_tor { get; set; }

        [JsonPropertyName("recent_abuse")]
        public bool? recent_abuse { get; set; }

        [JsonPropertyName("bot_status")]
        public bool? bot_status { get; set; }

        [JsonPropertyName("connection_type")]
        public string? connection_type { get; set; }

        [JsonPropertyName("abuse_velocity")]
        public string? abuse_velocity { get; set; }

        [JsonPropertyName("shared_connection")]
        public bool? shared_connection { get; set; }

        [JsonPropertyName("dynamic_connection")]
        public bool? dynamic_connection { get; set; }

        [JsonPropertyName("frequent_abuser")]
        public bool? frequent_abuser { get; set; }

        [JsonPropertyName("high_risk_attacks")]
        public bool? high_risk_attacks { get; set; }

        [JsonPropertyName("security_scanner")]
        public bool? security_scanner { get; set; }

        [JsonPropertyName("trusted_network")]
        public bool? trusted_network { get; set; }

        [JsonPropertyName("zip_code")]
        public string? zip_code { get; set; }

        [JsonPropertyName("latitude")]
        public double? latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? longitude { get; set; }

        [JsonPropertyName("request_id")]
        public string? request_id { get; set; }
    }
}