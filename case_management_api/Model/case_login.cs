namespace case_management_api.Model
{
    public class case_login
    {
        public int? id { get; set; }
        public int? account_id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? type { get; set; }
        public string? active_status { get; set; }
        public string? enc_key { get; set; }
        public DateTime? enc_key_date { get; set; }
        public string? name { get; set; }
        public string? contact_person { get; set; }
        public string? mobile { get; set; }
        public int? otp { get; set; }

        public int? delete_status { get; set; }
    }
}
