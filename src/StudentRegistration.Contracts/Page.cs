using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

public sealed record Page<T>
{
    public Page(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount,
        string sort)
    {
        ArgumentNullException.ThrowIfNull(items);
        var snapshot = items.ToArray();

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Page size must be between 1 and 100.");
        }

        if (totalCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalCount),
                totalCount,
                "Total count cannot be negative.");
        }

        if (snapshot.Length > pageSize)
        {
            throw new ArgumentException(
                "The item count cannot exceed the declared page size.",
                nameof(items));
        }

        if (snapshot.Length > totalCount)
        {
            throw new ArgumentException(
                "The total count cannot be smaller than the returned item count.",
                nameof(totalCount));
        }

        Items = Array.AsReadOnly(snapshot);
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        Sort = Required(sort, nameof(sort));
    }

    public IReadOnlyList<T> Items { get; }

    [JsonPropertyName("page")]
    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public string Sort { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}
