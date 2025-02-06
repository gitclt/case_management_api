namespace case_management_api.Model
{
    public class appversionmodel
    {
        public int? Id { get; set; }
        public string? version_name { get; set; }

        public string? version_code { get; set; }
        public DateTime? added_on { get; set; }
        public string? ios_url { get; set; }
        public string? android_url { get; set; }

    }
}
