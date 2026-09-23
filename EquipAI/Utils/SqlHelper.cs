using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;

namespace EquipAI.Utils;

public static class SqlHelper
{
    private static StorageSharedKeyCredential? _pdfStorageCredential;
    private static readonly SemaphoreSlim PdfStorageCredentialLock = new(1, 1);

    public static async Task SetUserCapabilitiesAsync(string email, int capabilities)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [dbo].[AspNetUsers] SET Capabilities = @capabilities WHERE Email = @email",
            connection);
        command.Parameters.AddWithValue("@capabilities", capabilities);
        command.Parameters.AddWithValue("@email", email);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<IReadOnlyList<string>> GetProjectNamesAsync()
    {
        var projectNames = new List<string>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Name] FROM [projects].[Project] WHERE IsDeleted = 0",
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
            {
                projectNames.Add(reader.GetString(0).Trim());
            }
        }

        return projectNames;
    }

    public static async Task<bool?> TryGetProjectIsDeletedByNameAsync(string name)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [IsDeleted] FROM [projects].[Project] WHERE [Name] = @name",
            connection);
        command.Parameters.AddWithValue("@name", name);

        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            return null;

        return Convert.ToBoolean(result);
    }

    public static async Task RestoreProjectByNameAsync(string name)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [projects].[Project] SET [IsDeleted] = 0 WHERE [Name] = @name",
            connection);
        command.Parameters.AddWithValue("@name", name);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<bool> ProjectExistsByNameAsync(string name)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Name] FROM [projects].[Project] WHERE [Name] = @name",
            connection);
        command.Parameters.AddWithValue("@name", name);

        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }

    public static async Task CreateSetupTestProjectAsync()
    {
        const string createdByUserId = "367DA46B-C6A9-4FE0-8854-D5CFBCF545AA";
        const string addressLine1 = "Test str";
        var timestamp = "2026-06-25 08:02:27.3477977 +00:00";

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var insertAddressCommand = new SqlCommand(
            """
            INSERT INTO [projects].[Address]
                (AddressLine1, AddressLine2, City, StateProvince, PostalCode, CountryCode, CreatedAt, UpdatedAt, IsDeleted, CreatedByUserId)
            VALUES
                (@addressLine1, @addressLine2, @city, @stateProvince, @postalCode, @countryCode, @createdAt, @updatedAt, 0, @createdByUserId)
            """,
            connection);
        insertAddressCommand.Parameters.AddWithValue("@addressLine1", addressLine1);
        insertAddressCommand.Parameters.AddWithValue("@addressLine2", "Test flat");
        insertAddressCommand.Parameters.AddWithValue("@city", "Test city");
        insertAddressCommand.Parameters.AddWithValue("@stateProvince", "Test state");
        insertAddressCommand.Parameters.AddWithValue("@postalCode", "123456");
        insertAddressCommand.Parameters.AddWithValue("@countryCode", "US");
        insertAddressCommand.Parameters.AddWithValue("@createdAt", timestamp);
        insertAddressCommand.Parameters.AddWithValue("@updatedAt", timestamp);
        insertAddressCommand.Parameters.AddWithValue("@createdByUserId", Guid.Parse(createdByUserId));
        await insertAddressCommand.ExecuteNonQueryAsync();

        await using var insertProjectCommand = new SqlCommand(
            """
            INSERT INTO [projects].[Project]
                (Code, Name, AddressId, CreatedAt, UpdatedAt, IsDeleted, CreatedByUserId)
            VALUES
                ('TEST-1', @projectName, (SELECT Id FROM [projects].[Address] WHERE AddressLine1 = @addressLine1), @createdAt, @updatedAt, 0, @createdByUserId)
            """,
            connection);
        insertProjectCommand.Parameters.AddWithValue("@projectName", Config.SetupProjectName1);
        insertProjectCommand.Parameters.AddWithValue("@addressLine1", addressLine1);
        insertProjectCommand.Parameters.AddWithValue("@createdAt", timestamp);
        insertProjectCommand.Parameters.AddWithValue("@updatedAt", timestamp);
        insertProjectCommand.Parameters.AddWithValue("@createdByUserId", Guid.Parse(createdByUserId));
        await insertProjectCommand.ExecuteNonQueryAsync();
    }

    public static async Task<bool> InvoiceExistsByNumberAsync(string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [InvoiceNumber] FROM [invoices].[Invoice] WHERE [InvoiceNumber] = @invoiceNumber",
            connection);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }

    public static async Task RestoreInvoiceByNumberAsync(string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [invoices].[Invoice] SET [IsDeleted] = 0 WHERE [InvoiceNumber] = @invoiceNumber",
            connection);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<DateTime> SetInvoiceRejectedAsync(string invoiceNumber, string rejectionReason)
    {
        var updatedAt = DateTime.Now;

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [invoices].[Invoice]
            SET [Status] = 'Rejected',
                [RejectionReason] = @rejectionReason,
                [UpdatedAt] = @updatedAt
            WHERE [InvoiceNumber] = @invoiceNumber
            """,
            connection);
        command.Parameters.AddWithValue("@rejectionReason", rejectionReason);
        command.Parameters.AddWithValue("@updatedAt", updatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await command.ExecuteNonQueryAsync();
        return updatedAt;
    }

    public static async Task SetInvoiceDraftAsync(string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [invoices].[Invoice]
            SET [Status] = 'Draft',
                [RejectionReason] = NULL,
                [ApprovedAt] = NULL,
                [ApprovedByUserId] = NULL
            WHERE [InvoiceNumber] = @invoiceNumber
            """,
            connection);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<DateTime> SetInvoiceApprovedAsync(string invoiceNumber)
    {
        const string approvedByUserId = "367DA46B-C6A9-4FE0-8854-D5CFBCF545AA";
        var approvedAt = DateTime.Now;

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [invoices].[Invoice]
            SET [Status] = 'Approved',
                [ApprovedAt] = @approvedAt,
                [ApprovedByUserId] = @approvedByUserId
            WHERE [InvoiceNumber] = @invoiceNumber
            """,
            connection);
        command.Parameters.AddWithValue("@approvedAt", approvedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@approvedByUserId", Guid.Parse(approvedByUserId));
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await command.ExecuteNonQueryAsync();
        return approvedAt;
    }

    public static async Task<object> GetInvoiceIdByInvoiceNumberAsync(string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Id] FROM [invoices].[Invoice] WHERE [InvoiceNumber] = @invoiceNumber",
            connection);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found.");

        return result;
    }

    public static async Task RestoreSetupManualInvoiceAsync(object invoiceId, string addedLineDescription)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var updateInvoiceCommand = new SqlCommand(
            """
            UPDATE [invoices].[Invoice]
            SET Status = 'Draft',
                TotalCost = '1562.99',
                InvoiceDate = '2026-06-02',
                InvoiceNumber = @invoiceNumber,
                CompanyName = @companyName,
                Address = @address,
                CurrencyCode = 'USD',
                EmissionCategoryId = 1,
                ProjectId = (SELECT [Id] FROM [projects].[Project] WHERE Name = @projectName)
            WHERE Id = @invoiceId
            """,
            connection);
        updateInvoiceCommand.Parameters.AddWithValue("@invoiceNumber", Config.SetupInvoiceNumber1);
        updateInvoiceCommand.Parameters.AddWithValue("@companyName", Config.SetupCompanyName1);
        updateInvoiceCommand.Parameters.AddWithValue("@address", Config.SetupInvoiceAddress1);
        updateInvoiceCommand.Parameters.AddWithValue("@projectName", Config.SetupProjectName1);
        updateInvoiceCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
        await updateInvoiceCommand.ExecuteNonQueryAsync();

        await using var deleteLineItemCommand = new SqlCommand(
            """
            DELETE FROM [invoices].[InvoiceLineItem]
            WHERE InvoiceId = @invoiceId
              AND LineDescription = @lineDescription
            """,
            connection);
        deleteLineItemCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
        deleteLineItemCommand.Parameters.AddWithValue("@lineDescription", addedLineDescription);
        await deleteLineItemCommand.ExecuteNonQueryAsync();

        await using var updateLineItemCommand = new SqlCommand(
            """
            UPDATE [invoices].[InvoiceLineItem]
            SET LineDescription = @lineDescription,
                Quantity = '421.2900',
                UnitPrice = '3.710000',
                Cost = '1562.99',
                EmissionTypeId = 1,
                UnitOfMeasureId = 1
            WHERE InvoiceId = @invoiceId
            """,
            connection);
        updateLineItemCommand.Parameters.AddWithValue("@lineDescription", Config.SetupInvoiceLineDescription1);
        updateLineItemCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
        await updateLineItemCommand.ExecuteNonQueryAsync();
    }

    public static async Task DeleteInvoiceByInvoiceNumberAsync(string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var deleteLineItemsCommand = new SqlCommand(
            "DELETE FROM [invoices].[InvoiceLineItem] WHERE InvoiceId = (SELECT Id FROM [invoices].[Invoice] WHERE InvoiceNumber = @invoiceNumber)",
            connection);
        deleteLineItemsCommand.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);
        await deleteLineItemsCommand.ExecuteNonQueryAsync();

        await using var deleteInvoiceCommand = new SqlCommand(
            "DELETE FROM [invoices].[Invoice] WHERE InvoiceNumber = @invoiceNumber",
            connection);
        deleteInvoiceCommand.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);
        await deleteInvoiceCommand.ExecuteNonQueryAsync();
    }

    public static async Task DeleteImportedInvoiceByInvoiceNumberAsync(string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        object? invoiceId = null;
        object? sourceId = null;
        await using (var getIdsCommand = new SqlCommand(
            """
            SELECT [Id], [SourceId]
            FROM [invoices].[Invoice]
            WHERE [InvoiceNumber] = @invoiceNumber
            """,
            connection))
        {
            getIdsCommand.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);
            await using var reader = await getIdsCommand.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                invoiceId = reader.GetValue(0);
                sourceId = reader.IsDBNull(1) ? null : reader.GetValue(1);
            }
        }

        if (invoiceId is not null)
        {
            await using (var deleteLineRecognitionsCommand = new SqlCommand(
                """
                DELETE FROM [ingestion].[InvoiceLineRecognition]
                WHERE [InvoiceLineItemId] IN (
                    SELECT [Id] FROM [invoices].[InvoiceLineItem] WHERE [InvoiceId] = @invoiceId
                )
                """,
                connection))
            {
                deleteLineRecognitionsCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                await deleteLineRecognitionsCommand.ExecuteNonQueryAsync();
            }

            await using (var deleteInvoiceRecognitionCommand = new SqlCommand(
                "DELETE FROM [ingestion].[InvoiceRecognition] WHERE [InvoiceId] = @invoiceId",
                connection))
            {
                deleteInvoiceRecognitionCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                await deleteInvoiceRecognitionCommand.ExecuteNonQueryAsync();
            }

            await using (var deleteLineItemsCommand = new SqlCommand(
                "DELETE FROM [invoices].[InvoiceLineItem] WHERE InvoiceId = @invoiceId",
                connection))
            {
                deleteLineItemsCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                await deleteLineItemsCommand.ExecuteNonQueryAsync();
            }

            await using var deleteInvoiceCommand = new SqlCommand(
                "DELETE FROM [invoices].[Invoice] WHERE Id = @invoiceId",
                connection);
            deleteInvoiceCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
            await deleteInvoiceCommand.ExecuteNonQueryAsync();
        }

        if (sourceId is not null)
        {
            await using var deleteActivitySourceCommand = new SqlCommand(
                "DELETE FROM [sources].[ActivitySource] WHERE Id = @sourceId",
                connection);
            deleteActivitySourceCommand.Parameters.AddWithValue("@sourceId", sourceId);
            await deleteActivitySourceCommand.ExecuteNonQueryAsync();
        }
    }

    public static async Task DeleteActivitySourceByCsvFileAsync(string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Test data file was not found: {filePath}");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileBytes)).ToLowerInvariant();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            DELETE FROM [sources].[ActivitySource]
            WHERE [SourceType] = 'BulkCsv'
              AND [OriginalDocumentBlobUrl] LIKE '%' + @hash + '%'
            """,
            connection);
        command.Parameters.AddWithValue("@hash", hash);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteActivitySourceByPdfFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileBytes)).ToLowerInvariant();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            DELETE FROM [sources].[ActivitySource]
            WHERE [SourceType] = 'AiInvoice'
              AND (
                    [OriginalDocumentBlobUrl] LIKE '%' + @hash + '%'
                 OR [OriginalDocumentBlobUrl] LIKE '%' + @fileName + '%' ESCAPE '\'
              )
            """,
            connection);
        command.Parameters.AddWithValue("@hash", hash);
        command.Parameters.AddWithValue("@fileName", EscapeLikePattern(fileName));
        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteImportedInvoiceByPdfFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileBytes)).ToLowerInvariant();
        var likeFileName = EscapeLikePattern(fileName);

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        var sourceIds = new List<object>();
        await using (var getSourcesCommand = new SqlCommand(
            """
            SELECT [Id]
            FROM [sources].[ActivitySource]
            WHERE [SourceType] = 'AiInvoice'
              AND (
                    [OriginalDocumentBlobUrl] LIKE '%' + @hash + '%'
                 OR [OriginalDocumentBlobUrl] LIKE '%' + @fileName + '%' ESCAPE '\'
              )
            """,
            connection))
        {
            getSourcesCommand.Parameters.AddWithValue("@hash", hash);
            getSourcesCommand.Parameters.AddWithValue("@fileName", likeFileName);
            await using var reader = await getSourcesCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                sourceIds.Add(reader.GetValue(0));
        }

        foreach (var sourceId in sourceIds)
        {
            object? invoiceId = null;
            await using (var getInvoiceCommand = new SqlCommand(
                """
                SELECT [Id]
                FROM [invoices].[Invoice]
                WHERE [SourceId] = @sourceId
                """,
                connection))
            {
                getInvoiceCommand.Parameters.AddWithValue("@sourceId", sourceId);
                invoiceId = await getInvoiceCommand.ExecuteScalarAsync();
            }

            if (invoiceId is not null && invoiceId is not DBNull)
            {
                await using (var deleteLineRecognitionsCommand = new SqlCommand(
                    """
                    DELETE FROM [ingestion].[InvoiceLineRecognition]
                    WHERE [InvoiceLineItemId] IN (
                        SELECT [Id] FROM [invoices].[InvoiceLineItem] WHERE [InvoiceId] = @invoiceId
                    )
                    """,
                    connection))
                {
                    deleteLineRecognitionsCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                    await deleteLineRecognitionsCommand.ExecuteNonQueryAsync();
                }

                await using (var deleteInvoiceRecognitionCommand = new SqlCommand(
                    "DELETE FROM [ingestion].[InvoiceRecognition] WHERE [InvoiceId] = @invoiceId",
                    connection))
                {
                    deleteInvoiceRecognitionCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                    await deleteInvoiceRecognitionCommand.ExecuteNonQueryAsync();
                }

                await using (var deleteLineItemsCommand = new SqlCommand(
                    "DELETE FROM [invoices].[InvoiceLineItem] WHERE InvoiceId = @invoiceId",
                    connection))
                {
                    deleteLineItemsCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                    await deleteLineItemsCommand.ExecuteNonQueryAsync();
                }

                await using var deleteInvoiceCommand = new SqlCommand(
                    "DELETE FROM [invoices].[Invoice] WHERE Id = @invoiceId",
                    connection);
                deleteInvoiceCommand.Parameters.AddWithValue("@invoiceId", invoiceId);
                await deleteInvoiceCommand.ExecuteNonQueryAsync();
            }

            await using (var deleteActivitiesCommand = new SqlCommand(
                "DELETE FROM [emissions].[Activity] WHERE [SourceId] = @sourceId",
                connection))
            {
                deleteActivitiesCommand.Parameters.AddWithValue("@sourceId", sourceId);
                await deleteActivitiesCommand.ExecuteNonQueryAsync();
            }

            await using var deleteActivitySourceCommand = new SqlCommand(
                "DELETE FROM [sources].[ActivitySource] WHERE Id = @sourceId",
                connection);
            deleteActivitySourceCommand.Parameters.AddWithValue("@sourceId", sourceId);
            await deleteActivitySourceCommand.ExecuteNonQueryAsync();
        }
    }

    public static async Task DeleteTelemetryByExternalReferenceAsync(string externalReferenceId)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using (var deleteReadingsCommand = new SqlCommand(
            """
            DELETE FROM [telemetry].[EquipmentReading]
            WHERE SourceId = (
                SELECT Id FROM [sources].[ActivitySource]
                WHERE ExternalReferenceId = @externalReferenceId
            )
            """,
            connection))
        {
            deleteReadingsCommand.Parameters.AddWithValue("@externalReferenceId", externalReferenceId);
            await deleteReadingsCommand.ExecuteNonQueryAsync();
        }

        await using var deleteSourceCommand = new SqlCommand(
            """
            DELETE FROM [sources].[ActivitySource]
            WHERE ExternalReferenceId = @externalReferenceId
            """,
            connection);
        deleteSourceCommand.Parameters.AddWithValue("@externalReferenceId", externalReferenceId);
        await deleteSourceCommand.ExecuteNonQueryAsync();
    }

    public static async Task DeleteCustomFactorImportAsync(string batchId, int year = 2001)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        var batchIdValue = long.Parse(batchId);

        await using (var deleteFactorsCommand = new SqlCommand(
            """
            DELETE FROM [factors].[EmissionFactor]
            WHERE FactorLibraryVersionId IN (
                SELECT Id FROM [factors].[FactorLibraryVersion] WHERE Year = @year
            )
            """,
            connection))
        {
            deleteFactorsCommand.Parameters.AddWithValue("@year", year);
            await deleteFactorsCommand.ExecuteNonQueryAsync();
        }

        await using (var deleteLinesCommand = new SqlCommand(
            """
            DELETE FROM [factors].[FactorImportLine]
            WHERE BatchId = @batchId
            """,
            connection))
        {
            deleteLinesCommand.Parameters.AddWithValue("@batchId", batchIdValue);
            await deleteLinesCommand.ExecuteNonQueryAsync();
        }

        await using (var deleteBatchCommand = new SqlCommand(
            """
            DELETE FROM [factors].[FactorImportBatch]
            WHERE Id = @batchId
            """,
            connection))
        {
            deleteBatchCommand.Parameters.AddWithValue("@batchId", batchIdValue);
            await deleteBatchCommand.ExecuteNonQueryAsync();
        }

        await using var deleteLibraryCommand = new SqlCommand(
            """
            DELETE FROM [factors].[FactorLibraryVersion]
            WHERE Year = @year
            """,
            connection);
        deleteLibraryCommand.Parameters.AddWithValue("@year", year);
        await deleteLibraryCommand.ExecuteNonQueryAsync();
    }

    public static async Task DeleteCustomFactorImportByYearAsync(int year = 2001)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using (var deleteFactorsCommand = new SqlCommand(
            """
            DELETE FROM [factors].[EmissionFactor]
            WHERE FactorLibraryVersionId IN (
                SELECT Id FROM [factors].[FactorLibraryVersion] WHERE Year = @year
            )
            """,
            connection))
        {
            deleteFactorsCommand.Parameters.AddWithValue("@year", year);
            await deleteFactorsCommand.ExecuteNonQueryAsync();
        }

        await using (var deleteLinesCommand = new SqlCommand(
            """
            DELETE FROM [factors].[FactorImportLine]
            WHERE BatchId IN (
                SELECT Id FROM [factors].[FactorImportBatch] WHERE Year = @year
            )
            """,
            connection))
        {
            deleteLinesCommand.Parameters.AddWithValue("@year", year);
            await deleteLinesCommand.ExecuteNonQueryAsync();
        }

        await using (var deleteBatchesCommand = new SqlCommand(
            """
            DELETE FROM [factors].[FactorImportBatch]
            WHERE Year = @year
            """,
            connection))
        {
            deleteBatchesCommand.Parameters.AddWithValue("@year", year);
            await deleteBatchesCommand.ExecuteNonQueryAsync();
        }

        await using var deleteLibraryCommand = new SqlCommand(
            """
            DELETE FROM [factors].[FactorLibraryVersion]
            WHERE Year = @year
            """,
            connection);
        deleteLibraryCommand.Parameters.AddWithValue("@year", year);
        await deleteLibraryCommand.ExecuteNonQueryAsync();
    }

    public static async Task DeletePdfBlobByFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileBytes)).ToLowerInvariant();
        var likeFileName = EscapeLikePattern(fileName);

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        var blobUrls = new List<string>();
        await using (var command = new SqlCommand(
            """
            SELECT [OriginalDocumentBlobUrl]
            FROM [sources].[ActivitySource]
            WHERE [SourceType] = 'AiInvoice'
              AND (
                    [OriginalDocumentBlobUrl] LIKE '%' + @hash + '%'
                 OR [OriginalDocumentBlobUrl] LIKE '%' + @fileName + '%' ESCAPE '\'
              )
            """,
            connection))
        {
            command.Parameters.AddWithValue("@hash", hash);
            command.Parameters.AddWithValue("@fileName", likeFileName);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                {
                    var url = reader.GetString(0).Trim();
                    if (!string.IsNullOrWhiteSpace(url))
                        blobUrls.Add(url);
                }
            }
        }

        if (blobUrls.Count == 0)
        {
            blobUrls.Add(
                $"https://{Config.PdfBlobStorageAccountName}.blob.core.windows.net/{Config.PdfBlobStorageContainerName}/{hash}.pdf");
        }

        var sharedKey = await GetPdfStorageSharedKeyCredentialAsync();
        foreach (var blobUrl in blobUrls.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var blobClient = new BlobClient(new Uri(blobUrl), sharedKey);
            await blobClient.DeleteIfExistsAsync();
        }
    }

    private static async Task<StorageSharedKeyCredential> GetPdfStorageSharedKeyCredentialAsync()
    {
        if (_pdfStorageCredential is not null)
            return _pdfStorageCredential;

        await PdfStorageCredentialLock.WaitAsync();
        try
        {
            if (_pdfStorageCredential is not null)
                return _pdfStorageCredential;

            var token = await new DefaultAzureCredential().GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/.default"]));

            var listKeysUrl =
                $"https://management.azure.com/subscriptions/{Config.PdfBlobStorageSubscriptionId}" +
                $"/resourceGroups/{Config.PdfBlobStorageResourceGroup}" +
                $"/providers/Microsoft.Storage/storageAccounts/{Config.PdfBlobStorageAccountName}" +
                "/listKeys?api-version=2023-01-01";

            using var http = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, listKeysUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            using var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var key = doc.RootElement.GetProperty("keys")[0].GetProperty("value").GetString();
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Storage account key was empty.");

            _pdfStorageCredential = new StorageSharedKeyCredential(Config.PdfBlobStorageAccountName, key);
            return _pdfStorageCredential;
        }
        finally
        {
            PdfStorageCredentialLock.Release();
        }
    }

    private static string ResolveTestDataPath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "TestData", fileName),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", fileName)),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Test data file was not found: {fileName}");
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    public static async Task DeleteUnitOfMeasureByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "DELETE FROM [emissions].[UnitOfMeasure] WHERE Code = @code",
            connection);
        command.Parameters.AddWithValue("@code", code);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<object> GetUnitOfMeasureIdByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @code",
            connection);
        command.Parameters.AddWithValue("@code", code);

        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            throw new InvalidOperationException($"Unit of measure '{code}' was not found.");

        return result;
    }

    public static async Task<(int? Dimension, decimal? ScaleToCanonical)> GetUnitOfMeasureDimensionAndScaleAsync(object id)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Dimension], [ScaleToCanonical] FROM [emissions].[UnitOfMeasure] WHERE Id = @id",
            connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException($"Unit of measure id '{id}' was not found.");

        int? dimension = reader.IsDBNull(0) ? null : reader.GetInt32(0);
        decimal? scaleToCanonical = reader.IsDBNull(1) ? null : reader.GetDecimal(1);
        return (dimension, scaleToCanonical);
    }

    public static async Task RestoreUnitOfMeasureAsync(
        object id,
        string code,
        string displayName,
        int? dimension = null,
        decimal? scaleToCanonical = null)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[UnitOfMeasure]
            SET Code = @code,
                DisplayName = @displayName,
                Dimension = @dimension,
                ScaleToCanonical = @scaleToCanonical
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@displayName", displayName);
        command.Parameters.AddWithValue("@dimension", dimension is null ? DBNull.Value : dimension);
        command.Parameters.AddWithValue("@scaleToCanonical", scaleToCanonical is null ? DBNull.Value : scaleToCanonical);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RestoreUnitOfMeasureByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [emissions].[UnitOfMeasure] SET IsDeleted = 0 WHERE Code = @code",
            connection);
        command.Parameters.AddWithValue("@code", code);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteEmissionTypeByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "DELETE FROM [emissions].[EmissionType] WHERE Code = @code",
            connection);
        command.Parameters.AddWithValue("@code", code);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<object> GetEmissionTypeIdByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Id] FROM [emissions].[EmissionType] WHERE Code = @code",
            connection);
        command.Parameters.AddWithValue("@code", code);

        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            throw new InvalidOperationException($"Emission type '{code}' was not found.");

        return result;
    }

    public static async Task RestoreEmissionTypeAsync(object id, string code, string displayName, string defaultUnitCode, string defaultEmissionCategory)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[EmissionType]
            SET Code = @code,
                DisplayName = @displayName,
                DefaultUnitOfMeasureId = (SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @defaultUnitCode),
                DefaultCategoryId = (
                    SELECT [Id]
                    FROM [emissions].[EmissionCategory]
                    WHERE CONCAT(DisplayName, ' (Scope ', CAST(GhgScope AS varchar(10)), ')') = @defaultEmissionCategory
                )
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@displayName", displayName);
        command.Parameters.AddWithValue("@defaultUnitCode", defaultUnitCode);
        command.Parameters.AddWithValue("@defaultEmissionCategory", defaultEmissionCategory);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<object> GetEmissionCategoryIdByDisplayNameAsync(string displayName)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Id] FROM [emissions].[EmissionCategory] WHERE [DisplayName] = @displayName",
            connection);
        command.Parameters.AddWithValue("@displayName", displayName);

        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            throw new InvalidOperationException($"Emission category '{displayName}' was not found.");

        return result;
    }

    public static async Task RestoreEmissionCategoryAsync(object id, string displayName, int ghgScope)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[EmissionCategory]
            SET [DisplayName] = @displayName,
                [GhgScope] = @ghgScope
            WHERE [Id] = @id
            """,
            connection);
        command.Parameters.AddWithValue("@displayName", displayName);
        command.Parameters.AddWithValue("@ghgScope", ghgScope);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RestoreEmissionTypeByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [emissions].[EmissionType] SET IsDeleted = 0 WHERE Code = @code",
            connection);
        command.Parameters.AddWithValue("@code", code);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteReferenceAliasByAliasTextAsync(string aliasText)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "DELETE FROM [emissions].[ReferenceAlias] WHERE AliasText = @aliasText",
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<object> GetReferenceAliasIdByAliasTextAsync(string aliasText)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Id] FROM [emissions].[ReferenceAlias] WHERE AliasText = @aliasText",
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);

        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            throw new InvalidOperationException($"Reference alias '{aliasText}' was not found.");

        return result;
    }

    public static async Task RestoreReferenceAliasUnitAsync(object id, string aliasText, string unitOfMeasureCode)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET AliasText = @aliasText,
                UnitOfMeasureId = (SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @unitOfMeasureCode)
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);
        command.Parameters.AddWithValue("@unitOfMeasureCode", unitOfMeasureCode);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RestoreReferenceAliasEmissionTypeAsync(object id, string aliasText, string emissionTypeCode)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET AliasText = @aliasText,
                EmissionTypeId = (SELECT [Id] FROM [emissions].[EmissionType] WHERE Code = @emissionTypeCode)
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);
        command.Parameters.AddWithValue("@emissionTypeCode", emissionTypeCode);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RestoreReferenceAliasFactorAsync(
        object id,
        string aliasText,
        string emissionTypeCode,
        int factorSource = 0)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET AliasText = @aliasText,
                EmissionTypeId = (SELECT [Id] FROM [emissions].[EmissionType] WHERE Code = @emissionTypeCode),
                FactorSource = @factorSource
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);
        command.Parameters.AddWithValue("@emissionTypeCode", emissionTypeCode);
        command.Parameters.AddWithValue("@factorSource", factorSource);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RestoreReferenceAliasProjectAsync(object id, string aliasText, string projectName)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET AliasText = @aliasText,
                ProjectId = (SELECT [Id] FROM [projects].[Project] WHERE Name = @projectName)
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);
        command.Parameters.AddWithValue("@projectName", projectName);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task EnsureSetupUnitAliasAsync()
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using (var restoreCommand = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET IsDeleted = 0,
                AliasText = @aliasText,
                UnitOfMeasureId = (SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @unitCode),
                EmissionTypeId = NULL,
                FactorSource = NULL
            WHERE UnitOfMeasureId IS NOT NULL
              AND (
                    UnitOfMeasureId = (SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @unitCode)
                 OR AliasText = @aliasText
                 OR AliasText = @aliasText + N' UPDATED'
              )
            """,
            connection))
        {
            restoreCommand.Parameters.AddWithValue("@aliasText", Config.SetupAliasUnit);
            restoreCommand.Parameters.AddWithValue("@unitCode", Config.SetupCode1);
            var updated = await restoreCommand.ExecuteNonQueryAsync();
            if (updated > 0)
                return;
        }

        await using var insertCommand = new SqlCommand(
            """
            INSERT INTO [emissions].[ReferenceAlias]
                (AliasText, UnitOfMeasureId, IsDeleted)
            VALUES
                (@aliasText, (SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @unitCode), 0)
            """,
            connection);
        insertCommand.Parameters.AddWithValue("@aliasText", Config.SetupAliasUnit);
        insertCommand.Parameters.AddWithValue("@unitCode", Config.SetupCode1);
        await insertCommand.ExecuteNonQueryAsync();
    }

    public static async Task EnsureSetupEmissionTypeAliasAsync()
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using (var restoreCommand = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET IsDeleted = 0,
                AliasText = @aliasText,
                EmissionTypeId = (SELECT [Id] FROM [emissions].[EmissionType] WHERE Code = @emissionTypeCode),
                UnitOfMeasureId = NULL,
                FactorSource = NULL
            WHERE UnitOfMeasureId IS NULL
              AND (
                    AliasText = @aliasText
                 OR AliasText = @aliasText + N' UPDATED'
                 OR (
                        EmissionTypeId = (SELECT [Id] FROM [emissions].[EmissionType] WHERE Code = @emissionTypeCode)
                    AND FactorSource IS NULL
                    AND AliasText LIKE N'DONT DELETE ALIAS EMISSION TYPE%'
                    )
              )
            """,
            connection))
        {
            restoreCommand.Parameters.AddWithValue("@aliasText", Config.SetupAliasEmissionType);
            restoreCommand.Parameters.AddWithValue("@emissionTypeCode", Config.SetupCode1);
            var updated = await restoreCommand.ExecuteNonQueryAsync();
            if (updated > 0)
                return;
        }

        await using var insertCommand = new SqlCommand(
            """
            INSERT INTO [emissions].[ReferenceAlias]
                (AliasText, EmissionTypeId, IsDeleted)
            VALUES
                (@aliasText, (SELECT [Id] FROM [emissions].[EmissionType] WHERE Code = @emissionTypeCode), 0)
            """,
            connection);
        insertCommand.Parameters.AddWithValue("@aliasText", Config.SetupAliasEmissionType);
        insertCommand.Parameters.AddWithValue("@emissionTypeCode", Config.SetupCode1);
        await insertCommand.ExecuteNonQueryAsync();
    }

    public static async Task RestoreReferenceAliasByAliasTextAsync(string aliasText)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [emissions].[ReferenceAlias] SET IsDeleted = 0 WHERE AliasText = @aliasText",
            connection);
        command.Parameters.AddWithValue("@aliasText", aliasText);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<decimal> GetTotalCarbonEmissionsTonnesAsync(int year)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            DECLARE @PeriodStart date = DATEFROMPARTS(@Year, 1, 1);
            DECLARE @PeriodEnd   date = IIF(@Year = YEAR(GETUTCDATE()),
                                            CAST(GETUTCDATE() AS date),
                                            DATEFROMPARTS(@Year, 12, 31));

            WITH ActivityTotals AS (
                SELECT
                    a.CategoryId,
                    COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) AS Co2eTonnes
                FROM projects.Project AS p
                INNER JOIN emissions.Activity AS a
                    ON a.ProjectId = p.Id AND a.IsDeleted = 0
                    AND a.ActivityDate >= @PeriodStart
                    AND a.ActivityDate <= @PeriodEnd
                LEFT JOIN emissions.EmissionType AS t ON t.Id = a.TypeId AND t.IsDeleted = 0
                LEFT JOIN factors.FactorLibraryVersion AS flv
                    ON flv.IsDeleted = 0
                    AND flv.Year = COALESCE(
                        (
                            SELECT TOP (1) flvExact.Year
                            FROM factors.FactorLibraryVersion AS flvExact
                            WHERE flvExact.IsDeleted = 0
                              AND flvExact.Year = YEAR(a.ActivityDate)
                              AND EXISTS (
                                  SELECT 1
                                  FROM factors.EmissionFactor AS efExact
                                  WHERE efExact.FactorLibraryVersionId = flvExact.Id
                                    AND efExact.IsDeleted = 0)
                        ),
                        (
                            SELECT MAX(flvLatest.Year)
                            FROM factors.FactorLibraryVersion AS flvLatest
                            WHERE flvLatest.IsDeleted = 0
                              AND EXISTS (
                                  SELECT 1
                                  FROM factors.EmissionFactor AS efLatest
                                  WHERE efLatest.FactorLibraryVersionId = flvLatest.Id
                                    AND efLatest.IsDeleted = 0)
                        ))
                LEFT JOIN factors.EmissionFactor AS ef
                    ON ef.FactorLibraryVersionId = flv.Id
                    AND ef.TypeId = t.Id
                    AND ef.UnitOfMeasureId = t.DefaultUnitOfMeasureId
                    AND ef.IsDeleted = 0
                WHERE p.IsDeleted = 0
                GROUP BY a.CategoryId
            )
            SELECT
                COALESCE(SUM(totals.Co2eTonnes), 0) AS Co2eTonnes
            FROM emissions.EmissionCategory AS ec
            LEFT JOIN ActivityTotals AS totals ON totals.CategoryId = ec.Id
            WHERE ec.IsDeleted = 0;
            """,
            connection);
        command.Parameters.AddWithValue("@Year", year);

        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? 0m : Convert.ToDecimal(result);
    }

    public static async Task<decimal> GetCarbonEmissionsTonnesByScopeAsync(int year, int ghgScope)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            DECLARE @PeriodStart date = DATEFROMPARTS(@Year, 1, 1);
            DECLARE @PeriodEnd   date = IIF(@Year = YEAR(GETUTCDATE()),
                                            CAST(GETUTCDATE() AS date),
                                            DATEFROMPARTS(@Year, 12, 31));

            WITH ActivityTotals AS (
                SELECT
                    a.CategoryId,
                    COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) AS Co2eTonnes
                FROM projects.Project AS p
                INNER JOIN emissions.Activity AS a
                    ON a.ProjectId = p.Id AND a.IsDeleted = 0
                    AND a.ActivityDate >= @PeriodStart
                    AND a.ActivityDate <= @PeriodEnd
                LEFT JOIN emissions.EmissionType AS t ON t.Id = a.TypeId AND t.IsDeleted = 0
                LEFT JOIN factors.FactorLibraryVersion AS flv
                    ON flv.IsDeleted = 0
                    AND flv.Year = COALESCE(
                        (
                            SELECT TOP (1) flvExact.Year
                            FROM factors.FactorLibraryVersion AS flvExact
                            WHERE flvExact.IsDeleted = 0
                              AND flvExact.Year = YEAR(a.ActivityDate)
                              AND EXISTS (
                                  SELECT 1
                                  FROM factors.EmissionFactor AS efExact
                                  WHERE efExact.FactorLibraryVersionId = flvExact.Id
                                    AND efExact.IsDeleted = 0)
                        ),
                        (
                            SELECT MAX(flvLatest.Year)
                            FROM factors.FactorLibraryVersion AS flvLatest
                            WHERE flvLatest.IsDeleted = 0
                              AND EXISTS (
                                  SELECT 1
                                  FROM factors.EmissionFactor AS efLatest
                                  WHERE efLatest.FactorLibraryVersionId = flvLatest.Id
                                    AND efLatest.IsDeleted = 0)
                        ))
                LEFT JOIN factors.EmissionFactor AS ef
                    ON ef.FactorLibraryVersionId = flv.Id
                    AND ef.TypeId = t.Id
                    AND ef.UnitOfMeasureId = t.DefaultUnitOfMeasureId
                    AND ef.IsDeleted = 0
                WHERE p.IsDeleted = 0
                GROUP BY a.CategoryId
            )
            SELECT COALESCE(SUM(COALESCE(totals.Co2eTonnes, 0)), 0) AS Co2eTonnes
            FROM emissions.EmissionCategory AS ec
            LEFT JOIN ActivityTotals AS totals ON totals.CategoryId = ec.Id
            WHERE ec.IsDeleted = 0 AND ec.GhgScope = @GhgScope;
            """,
            connection);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@GhgScope", ghgScope);

        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? 0m : Convert.ToDecimal(result);
    }

    public static async Task<int> GetTotalActiveProjectsCountAsync()
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT COUNT(*)
            FROM [projects].[Project]
            WHERE IsActive = 1 AND IsDeleted = 0
            """,
            connection);

        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? 0 : Convert.ToInt32(result);
    }

    public static async Task<int> GetReportingActiveProjectsCountAsync()
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT COALESCE(SUM(CASE WHEN reporting.ProjectId IS NOT NULL THEN 1 ELSE 0 END), 0) AS Reporting
            FROM projects.Project AS p
            LEFT JOIN (
                SELECT DISTINCT a.ProjectId
                FROM emissions.Activity AS a
                WHERE a.IsDeleted = 0
            ) AS reporting ON reporting.ProjectId = p.Id
            WHERE p.IsDeleted = 0
              AND p.IsActive = 1
            """,
            connection);

        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? 0 : Convert.ToInt32(result);
    }

    public static async Task<IReadOnlyList<object>> GetInactiveProjectIdsAsync()
    {
        var ids = new List<object>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT Id
            FROM [projects].[Project]
            WHERE IsActive = 0
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
                ids.Add(reader.GetValue(0));
        }

        return ids;
    }

    public static async Task DeactivateAllProjectsAsync()
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE [projects].[Project] SET IsActive = 0",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RestoreActiveProjectsExceptAsync(IReadOnlyList<object> inactiveProjectIds)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        if (inactiveProjectIds.Count == 0)
        {
            await using var restoreAllCommand = new SqlCommand(
                "UPDATE [projects].[Project] SET IsActive = 1",
                connection);
            await restoreAllCommand.ExecuteNonQueryAsync();
            return;
        }

        var parameterNames = inactiveProjectIds
            .Select((_, index) => $"@id{index}")
            .ToList();
        await using var command = new SqlCommand(
            $"""
            UPDATE [projects].[Project]
            SET IsActive = 1
            WHERE Id NOT IN ({string.Join(", ", parameterNames)})
            """,
            connection);

        for (var i = 0; i < inactiveProjectIds.Count; i++)
            command.Parameters.AddWithValue(parameterNames[i], inactiveProjectIds[i]);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<IReadOnlyList<ActiveProjectEmissionsRow>> GetTopActiveProjectsByEmissionsAsync(int limit = 8)
    {
        var rows = new List<ActiveProjectEmissionsRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT TOP (@Limit)
                p.Name,
                COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) AS TotalCo2eTonnes
            FROM projects.Project AS p
            LEFT JOIN emissions.Activity AS a
                ON a.ProjectId = p.Id AND a.IsDeleted = 0
            LEFT JOIN emissions.EmissionType AS t ON t.Id = a.TypeId AND t.IsDeleted = 0
            LEFT JOIN factors.FactorLibraryVersion AS flv
                ON flv.IsDeleted = 0
                AND flv.Year = COALESCE(
                    (
                        SELECT TOP (1) flvExact.Year
                        FROM factors.FactorLibraryVersion AS flvExact
                        WHERE flvExact.IsDeleted = 0
                          AND flvExact.Year = YEAR(a.ActivityDate)
                          AND EXISTS (
                              SELECT 1
                              FROM factors.EmissionFactor AS efExact
                              WHERE efExact.FactorLibraryVersionId = flvExact.Id
                                AND efExact.IsDeleted = 0)
                    ),
                    (
                        SELECT MAX(flvLatest.Year)
                        FROM factors.FactorLibraryVersion AS flvLatest
                        WHERE flvLatest.IsDeleted = 0
                          AND EXISTS (
                              SELECT 1
                              FROM factors.EmissionFactor AS efLatest
                              WHERE efLatest.FactorLibraryVersionId = flvLatest.Id
                                AND efLatest.IsDeleted = 0)
                    ))
            LEFT JOIN factors.EmissionFactor AS ef
                ON ef.FactorLibraryVersionId = flv.Id
                AND ef.TypeId = t.Id
                AND ef.UnitOfMeasureId = t.DefaultUnitOfMeasureId
                AND ef.IsDeleted = 0
            WHERE p.IsDeleted = 0
              AND p.IsActive = 1
            GROUP BY p.Id, p.Code, p.Name, p.ProjectType, p.StartDate, p.CompletionDate
            HAVING COUNT(a.Id) > 0
            ORDER BY TotalCo2eTonnes DESC, p.Name ASC;
            """,
            connection);
        command.Parameters.AddWithValue("@Limit", limit);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var tonnes = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
            rows.Add(new ActiveProjectEmissionsRow(name, tonnes));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<string>> GetActivityYearsAsync()
    {
        var years = new List<string>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT DISTINCT YEAR(ActivityDate) AS ActivityYear
            FROM [emissions].[Activity] WHERE IsDeleted=0
            ORDER BY ActivityYear DESC
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
                years.Add(reader.GetInt32(0).ToString());
        }

        return years;
    }

    public static async Task<IReadOnlyList<ChartMonthEmissionsRow>> GetChartScopeMonthlyEmissionsAsync(int year, int ghgScope)
    {
        var rows = new List<ChartMonthEmissionsRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            DECLARE @PeriodStart date = DATEFROMPARTS(@Year, 1, 1);
            DECLARE @PeriodEnd   date = IIF(@Year = YEAR(GETUTCDATE()),
                                            CAST(GETUTCDATE() AS date),
                                            DATEFROMPARTS(@Year, 12, 31));

            SELECT
                CAST(MONTH(a.ActivityDate) AS tinyint) AS Month,
                COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) AS Co2eTonnes
            FROM projects.Project AS p
            INNER JOIN emissions.Activity AS a
                ON a.ProjectId = p.Id AND a.IsDeleted = 0
                AND a.ActivityDate >= @PeriodStart
                AND a.ActivityDate <= @PeriodEnd
            INNER JOIN emissions.EmissionCategory AS ec
                ON ec.Id = a.CategoryId AND ec.IsDeleted = 0
            LEFT JOIN emissions.EmissionType AS t ON t.Id = a.TypeId AND t.IsDeleted = 0
            LEFT JOIN factors.FactorLibraryVersion AS flv
                ON flv.IsDeleted = 0
                AND flv.Year = COALESCE(
                    (
                        SELECT TOP (1) flvExact.Year
                        FROM factors.FactorLibraryVersion AS flvExact
                        WHERE flvExact.IsDeleted = 0
                          AND flvExact.Year = YEAR(a.ActivityDate)
                          AND EXISTS (
                              SELECT 1
                              FROM factors.EmissionFactor AS efExact
                              WHERE efExact.FactorLibraryVersionId = flvExact.Id
                                AND efExact.IsDeleted = 0)
                    ),
                    (
                        SELECT MAX(flvLatest.Year)
                        FROM factors.FactorLibraryVersion AS flvLatest
                        WHERE flvLatest.IsDeleted = 0
                          AND EXISTS (
                              SELECT 1
                              FROM factors.EmissionFactor AS efLatest
                              WHERE efLatest.FactorLibraryVersionId = flvLatest.Id
                                AND efLatest.IsDeleted = 0)
                    ))
            LEFT JOIN factors.EmissionFactor AS ef
                ON ef.FactorLibraryVersionId = flv.Id
                AND ef.TypeId = t.Id
                AND ef.UnitOfMeasureId = t.DefaultUnitOfMeasureId
                AND ef.IsDeleted = 0
            WHERE p.IsDeleted = 0 AND ec.GhgScope = @GhgScope
            GROUP BY ec.GhgScope, MONTH(a.ActivityDate)
            HAVING COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) > 0
            ORDER BY Month;
            """,
            connection);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@GhgScope", ghgScope);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var month = Convert.ToInt32(reader.GetValue(0));
            var tonnes = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
            rows.Add(new ChartMonthEmissionsRow(month, tonnes));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<CategoryEmissionsRow>> GetEmissionsByCategoryAsync(int year, int ghgScope)
    {
        var rows = new List<CategoryEmissionsRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            DECLARE @PeriodStart date = DATEFROMPARTS(@Year, 1, 1);
            DECLARE @PeriodEnd   date = IIF(@Year = YEAR(GETUTCDATE()),
                                            CAST(GETUTCDATE() AS date),
                                            DATEFROMPARTS(@Year, 12, 31));

            WITH ActivityTotals AS (
                SELECT
                    a.CategoryId,
                    COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) AS Co2eTonnes
                FROM projects.Project AS p
                INNER JOIN emissions.Activity AS a
                    ON a.ProjectId = p.Id AND a.IsDeleted = 0
                    AND a.ActivityDate >= @PeriodStart
                    AND a.ActivityDate <= @PeriodEnd
                LEFT JOIN emissions.EmissionType AS t ON t.Id = a.TypeId AND t.IsDeleted = 0
                LEFT JOIN factors.FactorLibraryVersion AS flv
                    ON flv.IsDeleted = 0
                    AND flv.Year = COALESCE(
                        (
                            SELECT TOP (1) flvExact.Year
                            FROM factors.FactorLibraryVersion AS flvExact
                            WHERE flvExact.IsDeleted = 0
                              AND flvExact.Year = YEAR(a.ActivityDate)
                              AND EXISTS (
                                  SELECT 1
                                  FROM factors.EmissionFactor AS efExact
                                  WHERE efExact.FactorLibraryVersionId = flvExact.Id
                                    AND efExact.IsDeleted = 0)
                        ),
                        (
                            SELECT MAX(flvLatest.Year)
                            FROM factors.FactorLibraryVersion AS flvLatest
                            WHERE flvLatest.IsDeleted = 0
                              AND EXISTS (
                                  SELECT 1
                                  FROM factors.EmissionFactor AS efLatest
                                  WHERE efLatest.FactorLibraryVersionId = flvLatest.Id
                                    AND efLatest.IsDeleted = 0)
                        ))
                LEFT JOIN factors.EmissionFactor AS ef
                    ON ef.FactorLibraryVersionId = flv.Id
                    AND ef.TypeId = t.Id
                    AND ef.UnitOfMeasureId = t.DefaultUnitOfMeasureId
                    AND ef.IsDeleted = 0
                WHERE p.IsDeleted = 0
                GROUP BY a.CategoryId
            )
            SELECT
                ec.DisplayName AS CategoryName,
                COALESCE(totals.Co2eTonnes, 0) AS Co2eTonnes
            FROM emissions.EmissionCategory AS ec
            LEFT JOIN ActivityTotals AS totals ON totals.CategoryId = ec.Id
            WHERE ec.IsDeleted = 0 AND ec.GhgScope = @GhgScope
            ORDER BY Co2eTonnes DESC, CategoryName ASC;
            """,
            connection);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@GhgScope", ghgScope);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var tonnes = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
            rows.Add(new CategoryEmissionsRow(name, tonnes));
        }

        return rows;
    }
}

public sealed record ActiveProjectEmissionsRow(string Name, decimal TotalCo2eTonnes);

public sealed record ChartMonthEmissionsRow(int Month, decimal Co2eTonnes);

public sealed record CategoryEmissionsRow(string CategoryName, decimal Co2eTonnes);
