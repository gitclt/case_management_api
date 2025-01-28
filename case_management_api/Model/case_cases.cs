using System.ComponentModel.DataAnnotations.Schema;

namespace case_management_api.Model
{
    public class case_cases
    {
        public int? id { get; set; } 

        public string? type { get; set; } 

        public string? name { get; set; } 

        public string? address { get; set; } 

        public string? email { get; set; } 

        public string? mobile { get; set; } 

        public string? location { get; set; } 

        public int? category_id { get; set; } 

        public int? priority_id { get; set; } 

        public int? assembly_id { get; set; } 

        public DateTime? date { get; set; }
        public string? time { get; set; } 

        public string? title { get; set; } 

        public string? comment { get; set; } 

        public int? set_reminder { get; set; } 

        public DateTime? reminder_date { get; set; } 

        public string? description { get; set; } 

        public int? delete_status { get; set; } 

        public int? addedby { get; set; } 

        public DateTime? addedon { get; set; } 

        public string? addedtype { get; set; } 

        public int? account_id { get; set; } 

        public string? status { get; set; } 
        public string? subject { get; set; } 
    }
}
