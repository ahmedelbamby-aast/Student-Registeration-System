using System.Globalization;
using System.Text.Json;

namespace StudentRegistration.Academics.Domain;

public enum PolicyValueType
{
    Number = 1,
    Boolean = 2,
    String = 3,
    StringList = 4,
}

public sealed class PolicyRule
{
    private PolicyRule()
    {
    }

    public PolicyRule(
        Guid id,
        Guid policySetId,
        string code,
        string reasonCode,
        PolicyValueType valueType,
        string value,
        string sourceReference,
        CatalogueSourceKind sourceKind)
    {
        DomainValue.Identifier(id, nameof(id));
        DomainValue.Identifier(policySetId, nameof(policySetId));
        if (!Enum.IsDefined(valueType))
        {
            throw new ArgumentOutOfRangeException(nameof(valueType));
        }

        if (!Enum.IsDefined(sourceKind))
        {
            throw new ArgumentOutOfRangeException(nameof(sourceKind));
        }

        Id = id;
        PolicySetId = policySetId;
        Code = DomainValue.Code(code, nameof(code));
        ReasonCode = DomainValue.Code(reasonCode, nameof(reasonCode));
        ValueType = valueType;
        Value = ValidateValue(valueType, value);
        SourceReference = DomainValue.Required(sourceReference, nameof(sourceReference));
        SourceKind = sourceKind;
    }

    public Guid Id { get; private set; }

    public Guid PolicySetId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string ReasonCode { get; private set; } = string.Empty;

    public PolicyValueType ValueType { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public string SourceReference { get; private set; } = string.Empty;

    public CatalogueSourceKind SourceKind { get; private set; }

    private static string ValidateValue(PolicyValueType valueType, string value)
    {
        var normalized = DomainValue.Required(value, nameof(value));
        var valid = valueType switch
        {
            PolicyValueType.Number => decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _),
            PolicyValueType.Boolean => bool.TryParse(normalized, out _),
            PolicyValueType.String => true,
            PolicyValueType.StringList => IsStringList(normalized),
            _ => false,
        };

        if (!valid)
        {
            throw new ArgumentException(
                "The policy value does not match its declared type.",
                nameof(value));
        }

        return normalized;
    }

    private static bool IsStringList(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind is JsonValueKind.Array
                && document.RootElement.EnumerateArray().All(
                    item => item.ValueKind is JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(item.GetString()));
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
