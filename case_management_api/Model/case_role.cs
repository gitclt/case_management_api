namespace case_management_api.Model
{
    public class case_role
    {
        public int? id {  get; set; }       
        public string? name { get; set; }
        public int? hierarchy_id { get; set; }
        public int? account_id { get; set; }
        public int? delete_status { get;  set; }   

    }
}
