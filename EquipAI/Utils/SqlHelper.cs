using Microsoft.Data.SqlClient;

namespace EquipAI.Utils;

public static class SqlHelper
{
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

    public static async Task RestoreEmissionTypeAsync(object id, string code, string displayName, string defaultUnitCode)
    {
        await using var connection = new SqlConnection(Config.SqlConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            UPDATE [emissions].[EmissionType]
            SET Code = @code,
                DisplayName = @displayName,
                DefaultUnitOfMeasureId = (SELECT [Id] FROM [emissions].[UnitOfMeasure] WHERE Code = @defaultUnitCode)
            WHERE Id = @id
            """,
            connection);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@displayName", displayName);
        command.Parameters.AddWithValue("@defaultUnitCode", defaultUnitCode);
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
}
