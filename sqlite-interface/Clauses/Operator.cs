namespace Database.Clauses
{
    /// <summary>
    /// All operators that can be used in a query
    /// </summary>
    public struct Operator
    {
        /// <summary>
        /// The equals operator
        /// </summary>
        public new const string Equals = "=";

        /// <summary>
        /// The not equals operator
        /// </summary>
        public const string NotEquals = "!=";

        /// <summary>
        /// The greater than operator
        /// </summary>
        public const string GreaterThan = ">";

        /// <summary>
        /// The greater than or equals operator
        /// </summary>
        public const string GreaterThanOrEquals = ">=";

        /// <summary>
        /// The less than operator
        /// </summary>
        public const string LessThan = "<";

        /// <summary>
        /// The less than or equals operator
        /// </summary>
        public const string LessThanOrEquals = "<=";

        /// <summary>
        /// The like operator
        /// </summary>
        public const string Like = "LIKE";

        /// <summary>
        /// The not like operator
        /// </summary>
        public const string NotLike = "NOT LIKE";

        /// <summary>
        /// The in operator
        /// </summary>
        public const string In = "IN";

        /// <summary>
        /// The not in operator
        /// </summary>
        public const string NotIn = "NOT IN";

        /// <summary>
        /// The is operator
        /// </summary>
        public const string Is = "IS";

        /// <summary>
        /// The is not operator
        /// </summary>
        public const string IsNot = "IS NOT";

        /// <summary>
        /// The exists operator
        /// </summary>
        public const string Exists = "EXISTS";

        /// <summary>
        /// The not exists operator
        /// </summary>
        public const string NotExists = "NOT EXISTS";
    }
}
