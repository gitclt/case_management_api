using System.ComponentModel.DataAnnotations;

namespace case_management_api.Model
{
    public class case_assembly
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public int? account_id { get; set; }

        public int? delete_status { get; set; }
    }
}
