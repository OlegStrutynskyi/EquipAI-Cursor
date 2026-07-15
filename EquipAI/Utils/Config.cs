namespace EquipAI.Utils;

public static class Config
{
    public const string BaseUrl = "https://qa-equipai.devessence.com/";

    public const string MicrosoftEmail = "oleg.strutynskyi03@devessence.com";
    public const string MicrosoftPassword = "wbjy&K5KWcHNM#e1";
    public const string MicrosoftSecretKey = "dypnrvjk67prb6mv";

    public const string TestUser3FirstName = "Oleg3";
    public const string TestUser3LastName = "Strutynskyi3";

    public const string SqlConnectionString =
        "Server=tcp:sql-equipai-eu.database.windows.net,1433;Initial Catalog=equipai-qa;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

    public const string SetupProjectName1 = "DONT-DELETE-Project-1";
    public const string SetupInvoiceNumber1 = "DONT-DELETE-Invoice-1";
    public const string SetupCompanyName1 = "DONT-DELETE-Company-1";
    public const string SetupInvoiceAddress1 = "DONT-DELETE-Address-1";
    public const string SetupInvoiceLineDescription1 = "DONT-DELETE-Description-1";

    public static bool Headless =>
        bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var headless) && headless;
}
