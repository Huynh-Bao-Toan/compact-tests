namespace Compat.Core
{
    /// <summary>
    /// Central catalog of GLOBAL migration-compatibility scenario keys shared by every
    /// controller/module. These represent recurring ASP.NET MVC5 → ASP.NET Core / .NET 8
    /// binding/serialization risks, not business rules of any one feature.
    ///
    /// Use a value here whenever a case is exercising one of these generic risks so the
    /// scenario name is centralized and cannot drift/typo across cases files. If a case is
    /// specific to one controller's business behavior (not a generic binding risk), define
    /// it in a feature-local "&lt;Feature&gt;Scenarios.cs" next to that feature's cases instead
    /// (see compat-tests/README.md, "Scenario taxonomy").
    ///
    /// Naming convention: snake_case, third-person-neutral, describes the INPUT SHAPE or
    /// CONDITION being tested (not the expected outcome) — e.g. "number_as_string", not
    /// "number_as_string_should_coerce".
    /// </summary>
    public static class CompatibilityScenarios
    {
        // ---- type coercion: value sent as a different JSON type than the target CLR type ----
        public const string NumberAsString = "number_as_string";
        public const string BoolAsString = "bool_as_string";
        public const string EnumAsString = "enum_as_string";

        // ---- empty / missing values ----
        public const string EmptyStringForNullable = "empty_string_for_nullable";
        public const string EmptyStringForNumber = "empty_string_for_number";
        public const string MissingOptionalField = "missing_optional_field";
        public const string ExtraUnknownField = "extra_unknown_field";

        // ---- request encoding ----
        public const string FormUrlEncoded = "form_urlencoded";
        public const string MultipartForm = "multipart_form";

        // ---- collections ----
        public const string NullCollection = "null_collection";
        public const string EmptyCollection = "empty_collection";

        // ---- formatting precision ----
        public const string NonIsoDate = "non_iso_date";
        public const string DecimalPrecision = "decimal_precision";

        // ---- property-name casing ----
        public const string CamelCase = "camel_case";
        public const string PascalCase = "pascal_case";
        public const string SnakeCase = "snake_case";

        // ---- query string edge cases ----
        public const string DuplicateQueryKey = "duplicate_query_key";
    }
}
