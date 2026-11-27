using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace QueryMind.Api.Analytics;

public sealed record ValidatedSql(string Sql);

/// <summary>AST gate for model-proposed queries. Only one SELECT over the curated analytics schema is accepted.</summary>
public sealed class SafeSqlValidator
{
    private static readonly HashSet<string> AllowedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "orders", "order_items", "products", "categories", "customers", "regions"
    };
    private static readonly HashSet<string> AllowedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "SUM", "COUNT", "AVG", "MIN", "MAX", "CONVERT", "CAST", "COALESCE", "ISNULL", "DATEADD", "DATEDIFF", "DATEPART", "YEAR", "MONTH", "DAY", "LOWER", "UPPER", "ROUND"
    };

    public ValidatedSql Validate(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) throw new SqlSafetyException("The model did not provide a query.");
        using var reader = new StringReader(sql);
        var parser = new TSql160Parser(initialQuotedIdentifiers: true);
        var fragment = parser.Parse(reader, out var errors);
        if (errors.Count > 0) throw new SqlSafetyException("Generated SQL could not be parsed safely.");
        if (fragment is not TSqlScript script || script.Batches.Count != 1 || script.Batches[0].Statements.Count != 1 || script.Batches[0].Statements[0] is not SelectStatement)
            throw new SqlSafetyException("Only one SELECT statement is permitted.");

        var visitor = new AnalyticsSchemaVisitor();
        fragment.Accept(visitor);
        if (visitor.HasUnsafeReference) throw new SqlSafetyException(visitor.FailureReason ?? "The query contains a disallowed SQL construct.");
        if (visitor.Tables.Count == 0) throw new SqlSafetyException("The query must read from the curated analytics schema.");
        return new ValidatedSql(sql.Trim());
    }

    private sealed class AnalyticsSchemaVisitor : TSqlFragmentVisitor
    {
        public List<string> Tables { get; } = [];
        public bool HasUnsafeReference { get; private set; }
        public string? FailureReason { get; private set; }

        public override void ExplicitVisit(NamedTableReference node)
        {
            var parts = node.SchemaObject.Identifiers.Select(identifier => identifier.Value).ToArray();
            if (parts.Length != 2 || !string.Equals(parts[0], "analytics", StringComparison.OrdinalIgnoreCase) || !AllowedTables.Contains(parts[1]))
                Reject("Only approved tables in the analytics schema are permitted.");
            else Tables.Add(parts[1]);
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(SelectStatement node)
        {
            if (node.Into is not null) Reject("SELECT INTO is not permitted.");
            base.ExplicitVisit(node);
        }
        public override void ExplicitVisit(OpenRowsetTableReference node) => Reject("External rowsets are not permitted.");
        public override void ExplicitVisit(OpenQueryTableReference node) => Reject("External queries are not permitted.");

        public override void ExplicitVisit(FunctionCall node)
        {
            if (!AllowedFunctions.Contains(node.FunctionName.Value))
                Reject($"Function '{node.FunctionName.Value}' is not approved for analytics queries.");
            base.ExplicitVisit(node);
        }

        private void Reject(string reason)
        {
            HasUnsafeReference = true;
            FailureReason ??= reason;
        }
    }
}

public sealed class SqlSafetyException(string message) : Exception(message);
