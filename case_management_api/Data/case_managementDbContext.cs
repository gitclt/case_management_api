using api_case_management.Model;
using case_management_api.Model;
using Microsoft.EntityFrameworkCore;

namespace api_case_management.Data
{
    public class case_managementDbContext:DbContext
    {
        public case_managementDbContext(DbContextOptions options) : base(options)
        {

        }

        public string base64Decode(string data)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }
        public string base64Encode(string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }


        public DbSet<case_category> tbl_case_category { get; set; }
        public DbSet<case_priority> tbl_case_priority { get; set; }
        public DbSet<case_assembly> tbl_case_assembly { get; set; }
        public DbSet<case_login> tbl_case_login { get; set; }
        public DbSet<case_subadmin_assembly> tbl_case_subadminassembly { get; set; }
        public DbSet<case_cases> tbl_case_cases { get; set; }
        public DbSet<case_contactperson> tbl_case_contactperson { get; set; }
        public DbSet<case_documents> tbl_case_documents { get; set; }
        public DbSet<case_role> tbl_case_role { get; set; }
        public DbSet<case_hierarchy> tbl_case_hierarchy { get; set; }
        public DbSet<case_role_privilage> tbl_case_role_privilage { get; set; }
        public DbSet<case_status> tbl_case_status { get; set; }
        public DbSet<case_privilage> tbl_case_privilage { get; set; }

        public DbSet<appversionmodel> tbl_case_app_version { get; set; }
        public DbSet<case_account> tbl_case_account { get; set; }





    }
}
