﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderSystem.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderSystem.Enums;
using OrderSystem.Exceptions;

namespace OrderSystem.Database.Tests
{
    [TestClass()]
    public class QueryBuilderTests
    {
        [TestMethod()]
        public void SelectFromTest()
        {
            SelectQueryBuilder test = new SelectQueryBuilder("user");
            test.SelectDistinct();
            test.SelectColumn("test");

            Assert.AreEqual("SELECT DISTINCT `test` FROM `user` ", test.Statement);
        }

        [TestMethod()]
        [ExpectedException(typeof(QueryBuilderException))]
        public void SelectFroMTestInvalid()
        {
            // This should fail
            SelectQueryBuilder test = new SelectQueryBuilder("yolo");
            test.SelectAll();
            test.SelectDistinct();
            test.SelectColumn("test");
            test.SelectAll();

            Console.WriteLine(test.Statement);
        }

        [TestMethod]
        public void SelectFromWhereTest()
        {
            SelectQueryBuilder test = new SelectQueryBuilder("user");
            test.SelectColumn("username");
            test.SelectColumn("password");
            test.Where("name", QueryBuilder.ValueWrap("Markus"), CompareType.Equal);
            test.Where("tries", 5, CompareType.LessThanOrEqual);

            Assert.AreEqual("SELECT `username`, `password` FROM `user` WHERE name = 'Markus' AND tries <= 5 ", test.Statement);
        }
    }
}