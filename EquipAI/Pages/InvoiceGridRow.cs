namespace EquipAI.Pages;

public sealed class InvoiceGridRow
{
    public required string Project { get; init; }
    public required string InvoiceNumber { get; init; }
    public required string Company { get; init; }
    public required string Date { get; init; }
    public required string Status { get; init; }
    public required string ImportDate { get; init; }
    public required string ApproveRejectDate { get; init; }
    public required string Source { get; init; }
}
