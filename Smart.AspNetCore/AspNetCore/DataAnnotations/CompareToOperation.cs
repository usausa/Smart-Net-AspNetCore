namespace Smart.AspNetCore.DataAnnotations;

public enum CompareToOperation
{
    LessThan,
    LessEqualThan,
    GreaterThan,
    GreaterEqualThan
}

public static class CompareToOperationExtensions
{
    public static bool IsValidCompare(this CompareToOperation operation, int compare) =>
        operation switch
        {
            CompareToOperation.LessThan => compare < 0,
            CompareToOperation.LessEqualThan => compare is < 0 or 0,
            CompareToOperation.GreaterThan => compare > 0,
            CompareToOperation.GreaterEqualThan => compare is > 0 or 0,
            _ => false
        };
}
