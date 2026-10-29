namespace EquipAI.Utils;

public static class Config
{
    public const string BaseUrl = "https://qa-equipai.devessence.com/";

    public const string TestUserEmail = "oleg.strutynskyi03@devessence.com";
    public const string MicrosoftPassword = "wbjy&K5KWcHNM#e1";
    public const string MicrosoftSecretKey = "dypnrvjk67prb6mv";

    public const string TestUserFirstName = "Oleg3";
    public const string TestUserLastName = "Strutynskyi3";

    public const string TestUserEmail2 = "testuser2@devessence.com";
    public const string TestUserFirstName2 = "John";
    public const string TestUserLastName2 = "Smith";
    public const string TestUserEmail3 = "testuser3@devessence.com";
    public const string TestUserFirstName3 = "Jane";
    public const string TestUserLastName3 = "Doe";
    public const string TestUserEmail4 = "testuser4@devessence.com";
    public const string TestUserFirstName4 = "Bob";
    public const string TestUserLastName4 = "Johnson";

    public const string SqlConnectionString =
        "Server=tcp:sql-equipai-eu.database.windows.net,1433;Initial Catalog=equipai-qa;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

    public const string PdfBlobStorageAccountName = "stequipaiinvqa";
    public const string PdfBlobStorageResourceGroup = "rg-equipai-qa";
    public const string PdfBlobStorageSubscriptionId = "1deb1018-b4d6-4861-82cd-546490f3664e";
    public const string PdfBlobStorageContainerName = "invoices-pdf-qa";

    public const string DefaultEnterpriseProjectName = "Haskell";
    public const string SetupProjectName1 = "DONT-DELETE-Project-1";
    public const string SetupInvoiceNumber1 = "DONT-DELETE-Invoice-1";
    public const string SetupCompanyName1 = "DONT-DELETE-Company-1";    
    public const string SetupAddress1 = SetupInvoiceAddress1;
    public const string SetupAddressLine1 = "11 Test street";
    public const string SetupAddressLine2 = "Apt 22";
    public const string SetupCity = "Dallas";
    public const string SetupState = "Texas";
    public const string SetupPostalCode = "12345";
    public const string SetupCountry = "United States";
    public const string SetupOfficeName1 = "DONT-DELETE-Office-1";
    public const string SetupOfficeName2 = "DONT-DELETE-Office-2";
    public const string SetupOfficeArea1 = "1000";
    public const string SetupOfficeArea2 = "2222.22";
    public const string SetupOfficeStartDate1 = "01/01/2025";
    public const string SetupOfficeStartDate2 = "02/02/2024";
    public const string SetupOfficeEndDate1 = "12/31/2029";
    public const string SetupOfficeEndDate2 = "11/30/2025";
    public const string SetupInvoiceAddress1 = "DONT-DELETE-Address-1";
    public const string SetupInvoiceLineDescription1 = "DONT-DELETE-Description-1";
    public const string SetupCode1 = "DONTDELETECODE1";
    public const string SetupCode2 = "DONTDELETECODE2";
    public const string SetupCode3 = "DONTDELETECODE3";
    public const string SetupCode4 = "DONTDELETECODE4";
    public const string SetupCodePropane = "PROPANE";
    public const string SetupDefaultUnitCode1 = "US_GAL";
    public const string SetupDefaultUnitCode2 = "T";
    public const string SetupDefaultUnitCode3 = "KWH";
    public const string SetupDefaultUnitCode4 = "SQFT";
    public const string SetupDefaultUnitName1 = "US Gallon";
    public const string SetupDefaultUnitName2 = "Tonne";
    public const string SetupDefaultUnitName3 = "Kilowatt-hour";
    public const string SetupDefaultUnitName4 = "Square Foot";
    public const string SetupDefaultEmissionCategory1 = "Direct Fuel Emissions (Scope 1)";
    public const string SetupDefaultEmissionCategory2 = "Direct Fleet Emissions (Scope 1)";
    public const string SetupDefaultEmissionCategory3 = "Purchased Electricity (Scope 2)";
    public const string SetupUnitName1 = "DONT DELETE UNIT 1";
    public const string SetupUnitName2 = "DONT DELETE UNIT 2";
    public const string SetupUnitSymbol1 = "SYMBOL 1";
    public const string SetupUnitSymbol2 = "SYMBOL 2";
    public const string SetupEmissionTypeName1 = "DONT DELETE EMISSION TYPE 1";
    public const string SetupEmissionTypeName2 = "DONT DELETE EMISSION TYPE 2";
    public const string SetupEmissionTypeName3 = "DONT DELETE EMISSION TYPE 3";
    public const string SetupEmissionTypeName4 = "DONT DELETE EMISSION TYPE 4";
    public const string SetupEmissionNamePropane = "Propane";
    public const string SetupAliasUnit = "DONT DELETE ALIAS UNIT";
    public const string SetupAliasEmissionType = "DONT DELETE ALIAS EMISSION TYPE";
    public const string SetupAliasFactor1 = "DONT DELETE ALIAS FACTOR1";
    public const string SetupAliasFactorPropane = "Propane";
    public const string SetupAliasProject1 = "DONT DELETE ALIAS PROJECT 1";

    public static bool Headless =>
        bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var headless) && headless;
}
