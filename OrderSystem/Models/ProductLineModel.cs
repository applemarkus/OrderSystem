﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using OrderSystem.Data;
using OrderSystem.Database;
using OrderSystem.Enums;

namespace OrderSystem.Models
{
    /// <summary>
    /// The model for the product_line tables
    /// </summary>
    public class ProductLineModel : MainModel
    {
        public ProductLineModel() : base("product_line")
        {
        }

        /// <summary>
        /// Adds a set of product lines to the database.
        /// </summary>
        /// <param name="userId">The user ordering the products</param>
        /// <param name="orderId">The order where the products are in</param>
        /// <param name="elements">The products</param>
        /// <returns></returns>
        public bool Submit(int userId, int orderId, List<ProductLine> elements)
        {
            foreach (ProductLine p in elements)
            {
                InsertQueryBuilder ib = new InsertQueryBuilder(base.table);
                ib.Insert("id", "NULL");
                ib.Insert("user", userId);
                ib.Insert("food_order", orderId);
                ib.Insert("product", p.Product.Id);
                ib.Insert("quantity", p.Quantity);
                ib.Insert("added", "NOW()");
                ib.Insert("paid", 0);

                if (!Update(ib.Statement))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if the user has already a open order
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="orderId">The order to check</param>
        /// <returns></returns>
        public bool HasAlreadyOrdered(int userId, int orderId)
        {
            SelectQueryBuilder sb = new SelectQueryBuilder(base.table);
            sb.SelectAll()
                .Where("user", userId)
                .Where("food_order", orderId)
                .Where("status", QueryBuilder.ValueWrap("ok"));

            DataTable t = Run(sb.Statement);
            return t.Rows.Count >= 1;
        }

        /// <summary>
        /// Get the open orders the user.
        /// </summary>
        /// <param name="userId">The user</param>
        /// <returns>A list of open orders the user made.</returns>
        public List<OrderOverviewRow> GetOpenOrdersFromUser(int userId)
        {
            List<OrderOverviewRow> list = new List<OrderOverviewRow>();

            SelectQueryBuilder sb = new SelectQueryBuilder("food_orders f", false);
            sb.SelectAll()
                .Join(JoinType.Inner, "food_order o", "f.order = o.id")
                .Where("f.user", userId)
                .Where("o.closed", "0")
                .OrderBy("f.time", OrderType.Descending);

            DataTable d = Run(sb.Statement);
            foreach (DataRow row in d.Rows)
            {
                list.Add(OrderOverviewRow.Parse(row));
            }
            return list;
        }

        /// <summary>
        /// Get the statistic of orders from a specific user
        /// </summary>
        /// <param name="userId">The user</param>
        /// <returns>The statistic</returns>
        public OrderStatistic GetStatistic(int userId)
        {
            OrderStatistic statistic = new OrderStatistic();

            SelectQueryBuilder sb = new SelectQueryBuilder("food_orders");
            sb.Select("COALESCE(SUM(amount), 0)")
                .Select("COALESCE(SUM(sum), 0)")
                .Where("user", userId);

            DataTable d = Run(sb.Statement);
            statistic.BoughtProducts = 0;
            statistic.TotalPrice = 0;

            if (d.Rows.Count > 0)
            {
                DataRow row = d.Rows[0];
                ulong products = (ulong)row.Field<decimal>(0);
                decimal sum = row.Field<decimal>(1);
                statistic.BoughtProducts = products;
                statistic.TotalPrice = sum;
            }

            return statistic;
        }

        /// <summary>
        /// Cancels the order of a user
        /// </summary>
        /// <param name="id">The food order id</param>
        public bool CancelOrder(int id)
        {
            UpdateQueryBuilder ub = new UpdateQueryBuilder(base.table);
            ub.Update("status", QueryBuilder.ValueWrap("cancelled"));
            ub.Where("food_order", id);
            int ret = UpdateRows(ub.Statement);
            return ret > 0;
        }

        /// <summary>
        /// Get product lines from a order
        /// </summary>
        /// <param name="order">The order id</param>
        /// <returns>list of product lines</returns>
        public List<ProductLine> GetOrder(int order)
        {
            List<ProductLine> list = new List<ProductLine>();

            SelectQueryBuilder sb = new SelectQueryBuilder(base.table);
            sb.SelectAll()
                .Where("food_order", order)
                .Where("status", QueryBuilder.ValueWrap("ok"));

            DataTable dt = Run(sb.Statement);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(ProductLine.Parse(row));
            }

            return list;
        }
    }
}