using Newtonsoft.Json;

public class AppSetting
{
    [JsonConstructor]
    public AppSetting(string sQLDB, string apiKeyBot, string sSHServerAddress, string sSHUser, string sSHPassword)
    {
        SQLDB = sQLDB;
        ApiKeyBot = apiKeyBot;
        SSHPassword = sSHPassword;
        SSHUser = sSHUser;
        SSHServerAddress = sSHServerAddress;
    }
    public static string SQLDB { get; set; }
    public static string ApiKeyBot { get; set; }
    public static string SSHServerAddress { get; set; }
    public static string SSHUser { get; set; }
    public static string SSHPassword { get; set; }
}