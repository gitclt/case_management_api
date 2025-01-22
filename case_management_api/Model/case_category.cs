using System.ComponentModel.DataAnnotations;

namespace api_case_management.Model
{
    public class case_category
    {
        [Key]
        public int? id { get; set; }
        public string? name { get; set; }
        public int? account_id { get; set; }

        public int? delete_status { get; set; }
    }
}
