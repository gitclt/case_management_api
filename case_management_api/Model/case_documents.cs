namespace case_management_api.Model
{
    public class case_documents
    {
        public int? id {  get; set; }    
        public string? type {  get; set; }    
        public int? account_id {  get; set; }
        public int? case_id { get; set; }

        public string? document {get; set; }    
    }
}
