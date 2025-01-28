namespace case_management_api.Model
{
    public class case_privilage
    {
        public int id {  get; set; }        
        public string module { get; set; }    
        public string menu { get; set; }    
        public string hierarchy_id { get; set; }    
        public int? delete_status { get; set; }    
        public string? platform { get; set; }    
        public int? account_id { get; set; }    
    }
}
