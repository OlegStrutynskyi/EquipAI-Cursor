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

    public static async Task<IReadOnlyList<ProjectGridRow>> GetProjectGridRowsAsync()
    {
        var rows = new List<ProjectGridRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT
                p.Code,
                p.Name,
                CONCAT_WS(', ',
                    NULLIF(a.AddressLine1, ''),
                    NULLIF(a.AddressLine2, ''),
                    NULLIF(a.City, ''),
                    NULLIF(CONCAT_WS(' ', a.StateProvince, a.PostalCode), ''),
                    NULLIF(a.CountryCode, '')) AS Address,
                p.StartDate,
                p.IsActive
            FROM [projects].[Project] AS p
            LEFT JOIN [projects].[Address] AS a
                ON p.AddressId = a.Id
            WHERE p.IsDeleted = 0
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var code = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            var address = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
            DateTime? startDate = reader.IsDBNull(3) ? null : Convert.ToDateTime(reader.GetValue(3));
            var isActive = !reader.IsDBNull(4) && Convert.ToBoolean(reader.GetValue(4));
            rows.Add(new ProjectGridRow(code, name, address, startDate, isActive));
        }

        return rows;
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

    public static Task SetUtilityBillRejectedAsync(string projectName, string companyName, DateTime billDate, string rejectionReason) =>
        UpdateUtilityBillStatusAsync(
            projectName,
            companyName,
            billDate,
            rejectionReason,
            """
            UPDATE i
            SET i.[Status] = 'Rejected',
                i.[RejectionReason] = @rejectionReason,
                i.[UpdatedAt] = @updatedAt
            """);

    public static Task SetUtilityBillApprovedAsync(string projectName, string companyName, DateTime billDate) =>
        UpdateUtilityBillStatusAsync(
            projectName,
            companyName,
            billDate,
            rejectionReason: null,
            """
            UPDATE i
            SET i.[Status] = 'Approved',
                i.[ApprovedAt] = @approvedAt,
                i.[ApprovedByUserId] = @approvedByUserId
            """);

    public static Task SetUtilityBillDraftAsync(string projectName, string companyName, DateTime billDate) =>
        UpdateUtilityBillStatusAsync(
            projectName,
            companyName,
            billDate,
            rejectionReason: null,
            """
            UPDATE i
            SET i.[Status] = 'Draft',
                i.[RejectionReason] = NULL,
                i.[ApprovedAt] = NULL,
                i.[ApprovedByUserId] = NULL
            """);

    private static async Task UpdateUtilityBillStatusAsync(
        string projectName,
        string companyName,
        DateTime billDate,
        string? rejectionReason,
        string setClause)
    {
        const string approvedByUserId = "367DA46B-C6A9-4FE0-8854-D5CFBCF545AA";
        var timestamp = DateTime.Now;

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            $"""
            {setClause}
            FROM [invoices].[Invoice] AS i
            INNER JOIN [projects].[Project] AS p
                ON i.ProjectId = p.Id
            INNER JOIN [ingestion].[InvoiceRecognition] AS r
                ON i.Id = r.InvoiceId
            WHERE p.Name = @projectName
              AND i.CompanyName = @companyName
              AND CAST(i.InvoiceDate AS date) = @billDate
              AND i.IsDeleted = 0
              AND r.DocumentKindId = 2
            """,
            connection);
        command.Parameters.AddWithValue("@projectName", projectName);
        command.Parameters.AddWithValue("@companyName", companyName);
        command.Parameters.AddWithValue("@billDate", billDate.Date);
        command.Parameters.AddWithValue("@rejectionReason", (object?)rejectionReason ?? DBNull.Value);
        command.Parameters.AddWithValue("@updatedAt", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@approvedAt", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@approvedByUserId", Guid.Parse(approvedByUserId));

        var updated = await command.ExecuteNonQueryAsync();
        if (updated == 0)
            throw new InvalidOperationException(
                $"Utility bill for '{companyName}' on {billDate:yyyy-MM-dd} was not found.");
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

    public static async Task DeleteActivitySourceByCsvFileAsync(string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Test data file was not found: {filePath}");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileBytes)).ToLowerInvariant();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using (var deleteActivitiesCommand = new SqlCommand(
            """
            DELETE FROM [emissions].[Activity]
            WHERE [SourceId] IN (
                SELECT [Id] FROM [sources].[ActivitySource]
                WHERE [SourceType] = 'BulkCsv'
                  AND [OriginalDocumentBlobUrl] LIKE '%' + @hash + '%'
            )
            """,
            connection))
        {
            deleteActivitiesCommand.Parameters.AddWithValue("@hash", hash);
            await deleteActivitiesCommand.ExecuteNonQueryAsync();
        }

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

        await using (var deleteActivitiesCommand = new SqlCommand(
            """
            DELETE FROM [emissions].[Activity]
            WHERE [SourceId] IN (
                SELECT [Id] FROM [sources].[ActivitySource]
                WHERE [SourceType] IN ('AiInvoice', 'UtilityBill', 'AiUtilityBill')
                  AND (
                        [OriginalDocumentBlobUrl] LIKE '%' + @hash + '%'
                     OR [OriginalDocumentBlobUrl] LIKE '%' + @fileName + '%' ESCAPE '\'
                  )
            )
            """,
            connection))
        {
            deleteActivitiesCommand.Parameters.AddWithValue("@hash", hash);
            deleteActivitiesCommand.Parameters.AddWithValue("@fileName", EscapeLikePattern(fileName));
            await deleteActivitiesCommand.ExecuteNonQueryAsync();
        }

        await using var command = new SqlCommand(
            """
            DELETE FROM [sources].[ActivitySource]
            WHERE [SourceType] IN ('AiInvoice', 'UtilityBill', 'AiUtilityBill')
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
            WHERE [SourceType] IN ('AiInvoice', 'UtilityBill', 'AiUtilityBill')
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
            WHERE [SourceType] IN ('AiInvoice', 'UtilityBill', 'AiUtilityBill')
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

    public static async Task<IReadOnlyList<EmissionCategoryGridRow>> GetEmissionCategoryGridRowsAsync()
    {
        var rows = new List<EmissionCategoryGridRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT DisplayName, GhgScope
            FROM [emissions].[EmissionCategory]
            WHERE IsDeleted = 0
            ORDER BY DisplayName
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var displayName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var ghgScope = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
            rows.Add(new EmissionCategoryGridRow(displayName, ghgScope));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<EmissionTypeGridRow>> GetEmissionTypeGridRowsAsync()
    {
        var rows = new List<EmissionTypeGridRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT e.Code, e.DisplayName, CONCAT_WS(' — ', u.Code, u.DisplayName) AS [Default Unit]
            FROM [emissions].[EmissionType] AS e
            LEFT JOIN [emissions].[UnitOfMeasure] AS u
                ON e.DefaultUnitOfMeasureId = u.Id
            WHERE e.IsDeleted = 0
            ORDER BY e.Code
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var code = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var displayName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            var defaultUnit = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
            rows.Add(new EmissionTypeGridRow(code, displayName, defaultUnit));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<FactorImportBatchGridDbRow>> GetFactorImportBatchGridRowsAsync()
    {
        var rows = new List<FactorImportBatchGridDbRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT b.Id, b.Source, b.Year, v.Label, COUNT(i.Id) AS N, b.CreatedAt
            FROM [factors].[FactorImportBatch] AS b
            LEFT JOIN [factors].[FactorLibraryVersion] AS v
                ON b.FactorLibraryVersionId = v.Id
            LEFT JOIN [factors].[FactorImportLine] AS i
                ON b.Id = i.BatchId
                AND i.IsDeleted = 0
            WHERE b.IsDeleted = 0
            GROUP BY b.Id, b.Source, b.Year, v.Label, b.CreatedAt
            ORDER BY b.Id DESC
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt64(reader.GetValue(0));
            var source = FormatFactorImportSource(reader.GetValue(1));
            int? year = reader.IsDBNull(2) ? null : Convert.ToInt32(reader.GetValue(2));
            var library = reader.IsDBNull(3) ? string.Empty : reader.GetString(3).Trim();
            var lines = Convert.ToInt32(reader.GetValue(4));
            var createdAt = ReadLocalDateTime(reader.GetValue(5));
            rows.Add(new FactorImportBatchGridDbRow(id, source, year, library, lines, createdAt));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<InvoiceGridDbRow>> GetInvoiceGridRowsAsync()
    {
        var rows = new List<InvoiceGridDbRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.Id, p.Name, i.InvoiceNumber, i.CompanyName, i.InvoiceDate, i.Status, i.CreatedAt,
                   CASE i.Status
                       WHEN 'Approved' THEN i.ApprovedAt
                       WHEN 'Rejected' THEN i.UpdatedAt
                   END AS ApproveDate,
                   s.SourceType
            FROM [invoices].[Invoice] AS i
            FULL JOIN [ingestion].[InvoiceRecognition] AS r
                ON i.Id = r.InvoiceId
            LEFT JOIN [projects].[Project] AS p
                ON i.ProjectId = p.Id
            LEFT JOIN [sources].[ActivitySource] AS s
                ON i.SourceId = s.Id
            WHERE i.IsDeleted = 0
              AND (r.DocumentKindId != 2 OR r.DocumentKindId IS NULL)
            ORDER BY i.InvoiceDate DESC, i.CreatedAt DESC, i.Id DESC
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt64(reader.GetValue(0));
            var project = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            var invoiceNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
            var company = reader.IsDBNull(3) ? string.Empty : reader.GetString(3).Trim();
            DateTime? invoiceDate = reader.IsDBNull(4) ? null : ReadLocalDateTime(reader.GetValue(4));
            var status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5).Trim();
            DateTime? importDate = reader.IsDBNull(6) ? null : ReadLocalDateTime(reader.GetValue(6));
            DateTime? approveDate = reader.IsDBNull(7) ? null : ReadLocalDateTime(reader.GetValue(7));
            var source = reader.IsDBNull(8) ? string.Empty : reader.GetValue(8).ToString()?.Trim() ?? string.Empty;
            rows.Add(new InvoiceGridDbRow(id, project, invoiceNumber, company, invoiceDate, status, importDate, approveDate, source));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<UtilityBillGridDbRow>> GetUtilityBillGridRowsAsync()
    {
        var rows = new List<UtilityBillGridDbRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.Id, p.Name, i.CompanyName, i.InvoiceDate, i.Status, i.CreatedAt,
                   CASE i.Status
                       WHEN 'Approved' THEN i.ApprovedAt
                       WHEN 'Rejected' THEN i.UpdatedAt
                   END AS ApproveDate,
                   s.SourceType
            FROM [invoices].[Invoice] AS i
            FULL JOIN [ingestion].[InvoiceRecognition] AS r
                ON i.Id = r.InvoiceId
            LEFT JOIN [projects].[Project] AS p
                ON i.ProjectId = p.Id
            LEFT JOIN [sources].[ActivitySource] AS s
                ON i.SourceId = s.Id
            WHERE i.IsDeleted = 0
              AND r.DocumentKindId = 2
            ORDER BY i.InvoiceDate DESC, i.CreatedAt DESC, i.Id DESC
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt64(reader.GetValue(0));
            var project = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            var company = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
            DateTime? billDate = reader.IsDBNull(3) ? null : ReadLocalDateTime(reader.GetValue(3));
            var status = reader.IsDBNull(4) ? string.Empty : reader.GetString(4).Trim();
            DateTime? importDate = reader.IsDBNull(5) ? null : ReadLocalDateTime(reader.GetValue(5));
            DateTime? approveDate = reader.IsDBNull(6) ? null : ReadLocalDateTime(reader.GetValue(6));
            var source = reader.IsDBNull(7) ? string.Empty : reader.GetValue(7).ToString()?.Trim() ?? string.Empty;
            rows.Add(new UtilityBillGridDbRow(id, project, company, billDate, status, importDate, approveDate, source));
        }

        return rows;
    }

    public static async Task<UtilityBillViewHeaderRow> GetUtilityBillViewHeaderAsync(string companyName, DateTime billDate)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.CompanyName, i.Address, p.Name AS Project, i.InvoiceDate AS BillDate,
                   CONCAT_WS(' ', i.TotalCost, i.CurrencyCode) AS Total
            FROM [invoices].[Invoice] AS i
            LEFT JOIN [projects].[Project] AS p
                ON i.ProjectId = p.Id
            WHERE i.CompanyName = @companyName
              AND CAST(i.InvoiceDate AS date) = @billDate
            """,
            connection);
        command.Parameters.AddWithValue("@companyName", companyName);
        command.Parameters.AddWithValue("@billDate", billDate.Date);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException($"Utility bill for '{companyName}' on {billDate:yyyy-MM-dd} was not found.");

        var company = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
        var address = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
        var project = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
        DateTime? invoiceDate = reader.IsDBNull(3) ? null : ReadLocalDateTime(reader.GetValue(3));
        var total = reader.IsDBNull(4) ? string.Empty : reader.GetValue(4).ToString()?.Trim() ?? string.Empty;
        return new UtilityBillViewHeaderRow(company, address, project, invoiceDate, total);
    }

    public static async Task<IReadOnlyList<UtilityBillViewLineItemRow>> GetUtilityBillViewLineItemsAsync(string companyName, DateTime billDate)
    {
        var rows = new List<UtilityBillViewLineItemRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.LinePosition, i.LineDescription, i.Quantity, i.Cost, t.DisplayName, c.DisplayName,
                   CONCAT(u.DisplayName, ' (', u.Code, ')') AS Unit
            FROM [invoices].[InvoiceLineItem] AS i
            LEFT JOIN [emissions].[EmissionType] AS t
                ON i.EmissionTypeId = t.Id
            LEFT JOIN [emissions].[EmissionCategory] AS c
                ON i.CategoryId = c.Id
            LEFT JOIN [emissions].[UnitOfMeasure] AS u
                ON i.UnitOfMeasureId = u.Id
            WHERE i.InvoiceId = (
                SELECT Id
                FROM [invoices].[Invoice]
                WHERE CompanyName = @companyName
                  AND CAST(InvoiceDate AS date) = @billDate
            )
            ORDER BY i.LinePosition
            """,
            connection);
        command.Parameters.AddWithValue("@companyName", companyName);
        command.Parameters.AddWithValue("@billDate", billDate.Date);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var linePosition = Convert.ToInt32(reader.GetValue(0));
            var description = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            decimal? quantity = reader.IsDBNull(2) ? null : Convert.ToDecimal(reader.GetValue(2));
            decimal? cost = reader.IsDBNull(3) ? null : Convert.ToDecimal(reader.GetValue(3));
            var emissionType = reader.IsDBNull(4) ? string.Empty : reader.GetString(4).Trim();
            var category = reader.IsDBNull(5) ? string.Empty : reader.GetString(5).Trim();
            var unit = reader.IsDBNull(6) ? string.Empty : reader.GetString(6).Trim();
            rows.Add(new UtilityBillViewLineItemRow(linePosition, description, quantity, cost, emissionType, category, unit));
        }

        return rows;
    }

    public static async Task<InvoiceViewHeaderRow> GetInvoiceViewHeaderAsync(string companyName, string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.CompanyName, i.Address, p.Name AS Project, i.InvoiceDate,
                   c.DisplayName AS EmissionCategory, CONCAT_WS(' ', i.TotalCost, i.CurrencyCode) AS Total
            FROM [invoices].[Invoice] AS i
            LEFT JOIN [projects].[Project] AS p
                ON i.ProjectId = p.Id
            LEFT JOIN [emissions].[EmissionCategory] AS c
                ON i.EmissionCategoryId = c.Id
            WHERE i.CompanyName = @companyName
              AND i.InvoiceNumber = @invoiceNumber
            """,
            connection);
        command.Parameters.AddWithValue("@companyName", companyName);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException($"Invoice '{invoiceNumber}' for '{companyName}' was not found.");

        var company = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
        var address = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
        var project = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
        DateTime? invoiceDate = reader.IsDBNull(3) ? null : ReadLocalDateTime(reader.GetValue(3));
        var emissionCategory = reader.IsDBNull(4) ? string.Empty : reader.GetString(4).Trim();
        var total = reader.IsDBNull(5) ? string.Empty : reader.GetValue(5).ToString()?.Trim() ?? string.Empty;
        return new InvoiceViewHeaderRow(company, address, project, invoiceDate, emissionCategory, total);
    }

    public static async Task<InvoiceEditHeaderRow> GetInvoiceEditHeaderAsync(string companyName, string invoiceNumber)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.InvoiceNumber, i.CompanyName, i.Address, p.Name AS Project, i.InvoiceDate,
                   CONCAT(c.DisplayName, ' (Scope ', c.GhgScope, ')') AS EmissionCategory,
                   i.TotalCost, i.CurrencyCode
            FROM [invoices].[Invoice] AS i
            LEFT JOIN [projects].[Project] AS p
                ON i.ProjectId = p.Id
            LEFT JOIN [emissions].[EmissionCategory] AS c
                ON i.EmissionCategoryId = c.Id
            WHERE i.CompanyName = @companyName
              AND i.InvoiceNumber = @invoiceNumber
            """,
            connection);
        command.Parameters.AddWithValue("@companyName", companyName);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException($"Invoice '{invoiceNumber}' for '{companyName}' was not found.");

        var number = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
        var company = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
        var address = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
        var project = reader.IsDBNull(3) ? string.Empty : reader.GetString(3).Trim();
        DateTime? invoiceDate = reader.IsDBNull(4) ? null : ReadLocalDateTime(reader.GetValue(4));
        var emissionCategory = reader.IsDBNull(5) ? string.Empty : reader.GetString(5).Trim();
        decimal? totalCost = reader.IsDBNull(6) ? null : Convert.ToDecimal(reader.GetValue(6));
        var currencyCode = reader.IsDBNull(7) ? string.Empty : reader.GetString(7).Trim();
        return new InvoiceEditHeaderRow(number, company, address, project, invoiceDate, emissionCategory, totalCost, currencyCode);
    }

    public static async Task<IReadOnlyList<InvoiceViewLineItemRow>> GetInvoiceViewLineItemsAsync(string companyName, string invoiceNumber)
    {
        var rows = new List<InvoiceViewLineItemRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT i.LinePosition, i.LineDescription, i.Quantity, i.UnitPrice, i.Cost,
                   t.DisplayName AS EmissionType, CONCAT(u.DisplayName, ' (', u.Code, ')') AS Unit
            FROM [invoices].[InvoiceLineItem] AS i
            LEFT JOIN [emissions].[EmissionType] AS t
                ON i.EmissionTypeId = t.Id
            LEFT JOIN [emissions].[UnitOfMeasure] AS u
                ON i.UnitOfMeasureId = u.Id
            WHERE i.InvoiceId = (
                SELECT Id
                FROM [invoices].[Invoice]
                WHERE CompanyName = @companyName
                  AND InvoiceNumber = @invoiceNumber
            )
            ORDER BY i.LinePosition
            """,
            connection);
        command.Parameters.AddWithValue("@companyName", companyName);
        command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var linePosition = Convert.ToInt32(reader.GetValue(0));
            var description = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            decimal? quantity = reader.IsDBNull(2) ? null : Convert.ToDecimal(reader.GetValue(2));
            decimal? unitPrice = reader.IsDBNull(3) ? null : Convert.ToDecimal(reader.GetValue(3));
            decimal? cost = reader.IsDBNull(4) ? null : Convert.ToDecimal(reader.GetValue(4));
            var emissionType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5).Trim();
            var unit = reader.IsDBNull(6) ? string.Empty : reader.GetString(6).Trim();
            rows.Add(new InvoiceViewLineItemRow(linePosition, description, quantity, unitPrice, cost, emissionType, unit));
        }

        return rows;
    }

    private static string FormatFactorImportSource(object value) => value switch
    {
        string text => text.Trim(),
        _ => Convert.ToInt32(value) switch
        {
            0 => "EPA",
            1 => "DEFRA",
            2 => "CUSTOM",
            var other => other.ToString(),
        },
    };

    private static DateTime ReadLocalDateTime(object value) => value switch
    {
        DateTimeOffset offset => offset.LocalDateTime,
        DateTime dateTime when dateTime.Kind == DateTimeKind.Utc => dateTime.ToLocalTime(),
        DateTime dateTime => dateTime,
        _ => Convert.ToDateTime(value),
    };

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

    public static async Task<IReadOnlyList<UnitAliasGridRow>> GetUnitAliasGridRowsAsync()
    {
        var rows = new List<UnitAliasGridRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT a.AliasText, CONCAT_WS(' — ', u.Code, u.DisplayName) AS Resolves
            FROM [emissions].[ReferenceAlias] AS a
            LEFT JOIN [emissions].[UnitOfMeasure] AS u
                ON a.UnitOfMeasureId = u.Id
            WHERE a.IsDeleted = 0 AND a.TargetEntityType = 0
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var aliasText = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var resolvesTo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            rows.Add(new UnitAliasGridRow(aliasText, resolvesTo));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<EmissionTypeAliasGridRow>> GetEmissionTypeAliasGridRowsAsync()
    {
        var rows = new List<EmissionTypeAliasGridRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT a.Context, a.AliasText, a.FactorSource, CONCAT_WS(' — ', u.Code, u.DisplayName) AS Resolves
            FROM [emissions].[ReferenceAlias] AS a
            LEFT JOIN [emissions].[EmissionType] AS u
                ON a.EmissionTypeId = u.Id
            WHERE a.IsDeleted = 0 AND a.TargetEntityType = 1
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var context = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
            var aliasText = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            int? factorSource = reader.IsDBNull(2) ? null : Convert.ToInt32(reader.GetValue(2));
            var resolvesTo = reader.IsDBNull(3) ? string.Empty : reader.GetString(3).Trim();
            rows.Add(new EmissionTypeAliasGridRow(context, aliasText, factorSource, resolvesTo));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<UnitAliasGridRow>> GetProjectAliasGridRowsAsync()
    {
        var rows = new List<UnitAliasGridRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT a.AliasText, CONCAT_WS(' — ', u.Code, u.Name) AS Resolves
            FROM [emissions].[ReferenceAlias] AS a
            LEFT JOIN [projects].[Project] AS u
                ON a.ProjectId = u.Id
            WHERE a.IsDeleted = 0 AND a.TargetEntityType = 2
            ORDER BY a.AliasText
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var aliasText = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var resolvesTo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            rows.Add(new UnitAliasGridRow(aliasText, resolvesTo));
        }

        return rows;
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

    public static async Task EnsureSetupProjectAliasAsync()
    {
        var isDeleted = await TryGetProjectIsDeletedByNameAsync(Config.SetupProjectName1);
        if (isDeleted is null)
            await CreateSetupTestProjectAsync();
        else if (isDeleted == true)
            await RestoreProjectByNameAsync(Config.SetupProjectName1);

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[ReferenceAlias]
            SET IsDeleted = 0,
                AliasText = @aliasText,
                ProjectId = (
                    SELECT TOP (1) [Id]
                    FROM [projects].[Project]
                    WHERE [Name] = @projectName AND [IsDeleted] = 0)
            WHERE AliasText = @aliasText
               OR AliasText = @aliasText + N' UPDATED'
            """,
            connection);
        command.Parameters.AddWithValue("@aliasText", Config.SetupAliasProject1);
        command.Parameters.AddWithValue("@projectName", Config.SetupProjectName1);
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

    public static async Task<IReadOnlyList<ScopeEmissionCategoryRow>> GetEmissionCategoriesByScopeAsync(int ghgScope)
    {
        var rows = new List<ScopeEmissionCategoryRow>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT DisplayName, Code
            FROM [emissions].[EmissionCategory]
            WHERE GhgScope = @GhgScope
            """,
            connection);
        command.Parameters.AddWithValue("@GhgScope", ghgScope);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var displayName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var code = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            rows.Add(new ScopeEmissionCategoryRow(displayName, code));
        }

        return rows;
    }

    public static async Task<IReadOnlyList<string>> GetEmissionTypeDisplayNamesByCategoryCodeAsync(string categoryCode)
    {
        var displayNames = new List<string>();

        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            SELECT DisplayName
            FROM [emissions].[EmissionType]
            WHERE IsDeleted = 0
              AND DefaultCategoryId = (
                  SELECT Id
                  FROM [emissions].[EmissionCategory]
                  WHERE Code = @Code)
            ORDER BY DisplayName
            """,
            connection);
        command.Parameters.AddWithValue("@Code", categoryCode);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
                displayNames.Add(reader.GetString(0).Trim());
        }

        return displayNames;
    }

    public static async Task<IReadOnlyList<EmissionTypeEmissionsRow>> GetEmissionTypeEmissionsByCategoryCodeAsync(
        int year,
        string categoryCode)
    {
        var rows = new List<EmissionTypeEmissionsRow>();

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
                    a.TypeId,
                    COALESCE(SUM(a.Quantity * ef.Co2eTonnesPerActivityUnit), 0) AS Co2eTonnes
                FROM projects.Project AS p
                INNER JOIN emissions.Activity AS a
                    ON a.ProjectId = p.Id AND a.IsDeleted = 0
                    AND a.ActivityDate >= @PeriodStart
                    AND a.ActivityDate <= @PeriodEnd
                    AND a.CategoryId = (
                        SELECT Id
                        FROM emissions.EmissionCategory
                        WHERE Code = @Code)
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
                GROUP BY a.TypeId
            )
            SELECT
                et.DisplayName,
                COALESCE(totals.Co2eTonnes, 0) AS Co2eTonnes
            FROM emissions.EmissionType AS et
            LEFT JOIN ActivityTotals AS totals ON totals.TypeId = et.Id
            WHERE et.IsDeleted = 0
              AND et.DefaultCategoryId = (
                  SELECT Id
                  FROM emissions.EmissionCategory
                  WHERE Code = @Code)
            ORDER BY et.DisplayName;
            """,
            connection);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@Code", categoryCode);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var displayName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
            var tonnes = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
            rows.Add(new EmissionTypeEmissionsRow(displayName, tonnes));
        }

        return rows;
    }
}

public sealed record ActiveProjectEmissionsRow(string Name, decimal TotalCo2eTonnes);

public sealed record ChartMonthEmissionsRow(int Month, decimal Co2eTonnes);

public sealed record CategoryEmissionsRow(string CategoryName, decimal Co2eTonnes);

public sealed record ScopeEmissionCategoryRow(string DisplayName, string Code);

public sealed record EmissionCategoryGridRow(string DisplayName, int GhgScope);

public sealed record EmissionTypeGridRow(string Code, string DisplayName, string DefaultUnit);

public sealed record InvoiceEditHeaderRow(
    string InvoiceNumber,
    string CompanyName,
    string Address,
    string Project,
    DateTime? InvoiceDate,
    string EmissionCategory,
    decimal? TotalCost,
    string CurrencyCode);

public sealed record InvoiceViewHeaderRow(
    string CompanyName,
    string Address,
    string Project,
    DateTime? InvoiceDate,
    string EmissionCategory,
    string Total);

public sealed record InvoiceViewLineItemRow(
    int LinePosition,
    string LineDescription,
    decimal? Quantity,
    decimal? UnitPrice,
    decimal? Cost,
    string EmissionType,
    string Unit);

public sealed record UtilityBillViewHeaderRow(
    string CompanyName,
    string Address,
    string Project,
    DateTime? BillDate,
    string Total);

public sealed record UtilityBillViewLineItemRow(
    int LinePosition,
    string LineDescription,
    decimal? Quantity,
    decimal? Cost,
    string EmissionType,
    string Category,
    string Unit);

public sealed record UtilityBillGridDbRow(
    long Id,
    string Project,
    string Company,
    DateTime? BillDate,
    string Status,
    DateTime? ImportDate,
    DateTime? ApproveRejectDate,
    string Source);

public sealed record InvoiceGridDbRow(
    long Id,
    string Project,
    string InvoiceNumber,
    string Company,
    DateTime? InvoiceDate,
    string Status,
    DateTime? ImportDate,
    DateTime? ApproveRejectDate,
    string Source);

public sealed record FactorImportBatchGridDbRow(
    long Id,
    string Source,
    int? Year,
    string Library,
    int Lines,
    DateTime CreatedAt);

public sealed record EmissionTypeEmissionsRow(string DisplayName, decimal Co2eTonnes);

public sealed record ProjectGridRow(string Code, string Name, string Address, DateTime? StartDate, bool IsActive);

public sealed record UnitAliasGridRow(string AliasText, string ResolvesTo);

public sealed record EmissionTypeAliasGridRow(int Context, string AliasText, int? FactorSource, string ResolvesTo);
