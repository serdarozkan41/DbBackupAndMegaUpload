using CG.Web.MegaApiClient;
using DbBackupAndMegaUpload.Models;
using Ionic.Zip;
using Newtonsoft.Json;
using System.Data.SqlClient;

try
{

    AppSettings appSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText("appsettings.json"));

    var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
    while (await timer.WaitForNextTickAsync())
    {
        timer.Dispose();

        await Task.Delay(5000);

        List<CompanyZipFile> zipFiles = new List<CompanyZipFile>();

        Console.Clear();
        Console.WriteLine($"----------------------------------START----------------------------------");


        Console.WriteLine($"Backup Folder: {appSettings.BackupFolder}");


        foreach (var company in appSettings.Companies)
        {

            Console.WriteLine("Started...");
            try
            {
                string backupFileName = $"{appSettings.FilePrefix}_{company.DatabaseName}_{DateTime.Now.ToString().Replace(".", "_").Replace(":", "_").Replace(" ", "_")}.bak";
                string backupFile = $"{appSettings.BackupFolder}\\{backupFileName}";
                string backupQuery = appSettings.BackupQuery.Replace("@DBNAME", company.DatabaseName).Replace("@BFOLDER", $"{backupFile}");
                string zipFile = $"{appSettings.TempFolder}\\{backupFileName.Replace(".bak", ".zip")}";

                using (SqlConnection sqlConnection = new SqlConnection(company.ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand(backupQuery, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = 5000;

                        var result = sqlCommand.ExecuteNonQuery();

                        if (result == 0)
                            Console.WriteLine($"{company.DatabaseName} backup file don't created.");
                        else
                            Console.WriteLine($"{company.DatabaseName} backup file created.");

                        using (ZipFile zip = new ZipFile())
                        {
                            zip.Password = appSettings.ZipPassword;
                            zip.Comment = "This zip was created at " + DateTime.Now.ToString("G");
                            zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                            zip.AddFile(backupFile, @"\");
                            zip.Save(zipFile);
                        }

                        Console.WriteLine($"Compression ready.");
                        File.Delete(backupFile);
                        Console.WriteLine($"Backup File Deleted.");

                        zipFiles.Add(new CompanyZipFile
                        {
                            ZipFilePath = zipFile,
                            CompanyName = company.Name,
                        });
                    }
                    sqlConnection.Close();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        Console.WriteLine($"Starting backup and compression ready installation for all companies.");

        Console.WriteLine($"----------------------------------MEGA START----------------------------------");

        Console.WriteLine($"Login.");
        MegaApiClient megaApiClient = new MegaApiClient();
        var loginResponse = await megaApiClient.LoginAsync(appSettings.MegaAccount.Email, appSettings.MegaAccount.Password);
        var nodeList = await megaApiClient.GetNodesAsync();
        var rootFolder = nodeList.Single(q => q.Type == NodeType.Root);

        File.WriteAllText("db_backup_logs.txt", $"Last Run Time: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}");

        foreach (var zipFile in zipFiles)
        {
            Console.WriteLine($"Uploading.");
            INode companyFolder;

            List<INode> companyFolders = nodeList.Where(x => x.ParentId == rootFolder.Id && x.Name == zipFile.CompanyName).ToList();
            if (companyFolders is null || companyFolders.Count == 0)
                companyFolder = await megaApiClient.CreateFolderAsync(zipFile.CompanyName, rootFolder);
            else
                companyFolder = companyFolders.First();

            INode myFile = await megaApiClient.UploadFileAsync(zipFile.ZipFilePath, companyFolder);

            Console.WriteLine($"Uploaded.");
        }


        await megaApiClient.LogoutAsync();


        Console.WriteLine($"----------------------------------MEGA END----------------------------------");


        Console.WriteLine($"Clear All Files.");


        Console.WriteLine($"Waiting.... {appSettings.WaitMinute} Minute.");
        Console.WriteLine($"Last Run Time: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}");

        Console.WriteLine($"----------------------------------END----------------------------------");

        timer = new PeriodicTimer(TimeSpan.FromMinutes(appSettings.WaitMinute));
    }

}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.Message);
    Console.WriteLine("HELPPPP");
    Console.ReadLine();
}