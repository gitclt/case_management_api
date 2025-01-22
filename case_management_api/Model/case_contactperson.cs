namespace case_management_api.Model
{
    public class case_contactperson
    {
        public int? id {  get; set; }
        public int? account_id { get; set; }
        public int? case_id { get; set; }

        public string type { get; set; }
        public string name { get; set; }        
        public string mobile { get; set; }        
        public string designation { get; set; }        
    }
}
