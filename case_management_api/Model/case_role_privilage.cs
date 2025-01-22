namespace case_management_api.Model
{
    public class case_role_privilage
    {
        public int? id { get; set; }
        public int? privilage_id { get; set; }
        public int? is_add { get; set; }
        public int? is_delete { get; set; }
        public int? is_view { get; set; }
        public int? is_edit { get; set; }
        public int? account_id { get; set; }
        public int? role_id { get; set; }
    }
}
