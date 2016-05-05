﻿using OrderSystem.Data;
using OrderSystem.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderSystem.Enums;

namespace OrderSystem.Models
{
    /// <summary>
    /// The model for the food_order table
    /// </summary>
    public class OrderModel : MainModel
    {
        public OrderModel() : base("food_order")
        {
        }

        /// <summary>
        /// Get the available orders
        /// </summary>
        /// <returns></returns>
        public List<Order> GetOrders()
        {
            List<Order> list = new List<Order>();

            SelectQueryBuilder sb = new SelectQueryBuilder(base.table);
            sb.SelectAll().Where("closed", 0);

            DataTable table = Run(sb.Statement);

            foreach (DataRow row in table.Rows)
            {
                list.Add(Order.Parse(row));
            }

            return list;
        }
    }
}