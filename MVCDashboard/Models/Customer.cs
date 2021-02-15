using Newtonsoft.Json;

namespace MVCDashboard {
    public class Customer {
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }
        [JsonProperty("contactName")]
        public string ContactName { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
    }
}