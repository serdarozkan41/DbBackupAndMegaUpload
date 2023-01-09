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
}
