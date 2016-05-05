﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderSystem.Enums;

namespace OrderSystem.Database
{
    /// <summary>
    /// The compiler for queries. Used by query builders.
    /// </summary>
    public class QueryCompiler
    {
        private string table;

        public QueryCompiler(string table)
        {
            this.table = table;
        }

        /// <summary>
        /// Creates SELECT ... and FROM
        /// </summary>
        /// <param name="selects">The selects</param>
        /// <returns>The compiled statement</returns>
        public string Select(List<string> selects)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            for (int i = 0; i < selects.Count; i++)
            {
                if (i == selects.Count - 1 || selects[i].Equals("DISTINCT"))
                {
                    sb.Append(selects[i]).Append(" ");
                }
                else
                {
                    sb.Append(selects[i]).Append(", ");
                }
            }
            sb.Append("FROM ").Append(table).Append(" ");
            return sb.ToString();
        }

        /// <summary>
        /// Creates WHERE and AND
        /// </summary>
        /// <param name="cols">The where conditions</param>
        /// <returns>The compiled statement</returns>
        public string Where(List<Tuple<string, string, CompareType>> cols)
        {
            if (cols.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            bool usedWhere = false;

            foreach (Tuple<string, string, CompareType> col in cols)
            {
                if (!usedWhere)
                {
                    sb.Append("WHERE ");
                    usedWhere = true;
                }
                else
                {
                    sb.Append("AND ");
                }

                sb.Append(col.Item1)
                    .Append(" ")
                    .Append(CompareToString(col.Item3))
                    .Append(" ")
                    .Append(col.Item2)
                    .Append(" ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns string value of the compare type
        /// </summary>
        /// <param name="type">The compare type</param>
        /// <returns>The string value of the compare type</returns>
        public string CompareToString(CompareType type)
        {
            switch (type)
            {
                case CompareType.Equal:
                    return "=";
                case CompareType.GreaterThan:
                    return ">";
                case CompareType.GreaterThanOrEqual:
                    return ">=";
                case CompareType.Is:
                    return "IS";
                case CompareType.IsNot:
                    return "IS NOT";
                case CompareType.LessThan:
                    return "<";
                case CompareType.LessThanOrEqual:
                    return "<=";
                case CompareType.NotEqual:
                    return "<>";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Creates ORDER BY
        /// </summary>
        /// <param name="list">column names and order type</param>
        /// <returns>The compiled statement</returns>
        public string OrderBy(List<Tuple<string, OrderType>> list)
        {
            if (list.Count == 0) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("ORDER BY ");

            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i].Item1)
                    .Append(" ")
                    .Append(OrderToString(list[i].Item2))
                    .Append(i == list.Count - 1 ? " " : ", ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns string value of the order type
        /// </summary>
        /// <param name="type">The order type</param>
        /// <returns>The string value of the order type</returns>
        public string OrderToString(OrderType type)
        {
            switch (type)
            {
                case OrderType.Ascending:
                    return "ASC";
                case OrderType.Descending:
                    return "DESC";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Creates LIMIT and OFFSET (if needed)
        /// </summary>
        /// <param name="tuple">The limit and offset values</param>
        /// <returns>The compiled statement</returns>
        public string Limit(Tuple<long, long> tuple)
        {
            if (tuple == null) return "";
            if (tuple.Item1 == 0) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("LIMIT ").Append(tuple.Item1).Append(" ");

            if (tuple.Item2 > 0)
            {
                sb.Append("OFFSET ").Append(tuple.Item2).Append(" ");
            }

            return sb.ToString();
        }
    }
}