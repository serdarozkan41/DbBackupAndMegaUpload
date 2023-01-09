namespace DbBackupAndMegaUpload.Models
{
    internal class AppSettings
    {
        public string BackupFolder { get; set; }
        public string BackupQuery { get; set; }
        public string TempFolder { get; set; }
        public string FilePrefix { get; set; }
        public string ZipPassword { get; set; }
        public int WaitMinute { get; set; }
        public MegaAccount MegaAccount { get; set; }
        public List<Company> Companies { get; set; }
    }

    internal class Company
    {
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }

    internal class MegaAccount
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
