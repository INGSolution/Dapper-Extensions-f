﻿using DapperExtensions.Test.Data.Oracle;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Car = DapperExtensions.Test.Data.Common.Car;

namespace DapperExtensions.Test.IntegrationTests.Oracle
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class TimerFixture
    {
        private const int cnt = 1000;

        public class InsertTimes : OracleBaseFixture
        {
            [Test]
            public void IdentityKey_UsingEntity()
            {
                Person p = new Person
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateCreated = DateTime.Now,
                    Active = "Y"
                };
                Db.Insert(p);
                DateTime start = DateTime.Now;
                var ids = new List<long>();
                for (int i = 0; i < cnt; i++)
                {
                    Person p2 = new Person
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i,
                        DateCreated = DateTime.Now,
                        Active = "Y"
                    };
                    Db.Insert(p2);
                    ids.Add(p2.Id);
                }

                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                TestContext.WriteLine("Total Time:" + total);
                TestContext.WriteLine("Average Time:" + (total / cnt));
                Dispose();
            }

            [Test]
            public void IdentityKey_UsingReturnValue()
            {
                Person p = new Person
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateCreated = DateTime.Now,
                    Active = "Y"
                };
                Db.Insert(p);
                DateTime start = DateTime.Now;
                var ids = new List<long>();
                for (int i = 0; i < cnt; i++)
                {
                    Person p2 = new Person
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i,
                        DateCreated = DateTime.Now,
                        Active = "Y"
                    };
                    var id = Db.Insert(p2);
                    ids.Add(id);
                }

                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                TestContext.WriteLine("Total Time:" + total);
                TestContext.WriteLine("Average Time:" + (total / cnt));
                Dispose();
            }

            [Test]
            [Ignore("Oracle does not support GUID from the box")]
            public void GuidKey_UsingEntity()
            {
                /*
                Animal a = new Animal { Name = "Name" };
                Db.Insert(a);
                DateTime start = DateTime.Now;
                List<Guid> ids = new List<Guid>();
                for (int i = 0; i < cnt; i++)
                {
                    Animal a2 = new Animal { Name = "Name" + i };
                    Db.Insert(a2);
                    ids.Add(a2.Id);
                }

                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                TestContext.WriteLine("Total Time:" + total);
                TestContext.WriteLine("Average Time:" + total / cnt);
                Dispose();
                */
            }

            [Test]
            [Ignore("Oracle does not support GUID from the box")]
            public void GuidKey_UsingReturnValue()
            {
                Animal a = new Animal { Name = "Name" };
                Db.Insert(a);
                DateTime start = DateTime.Now;
                List<Guid> ids = new List<Guid>();
                for (int i = 0; i < cnt; i++)
                {
                    Animal a2 = new Animal { Name = "Name" + i };
                    var id = Db.Insert(a2);
                    ids.Add(id);
                }

                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                TestContext.WriteLine("Total Time:" + total);
                TestContext.WriteLine("Average Time:" + (total / cnt));
                Dispose();
            }

            [Test]
            public void AssignKey_UsingEntity()
            {
                Car ca = new Car { Id = string.Empty.PadLeft(15, '0'), Name = "Name" };
                Db.Insert(ca);
                DateTime start = DateTime.Now;
                List<string> ids = new List<string>();
                for (int i = 0; i < cnt; i++)
                {
                    var key = (i + 1).ToString().PadLeft(15, '0');
                    Car ca2 = new Car { Id = key, Name = "Name" + i };
                    Db.Insert(ca2);
                    ids.Add(ca2.Id);
                }

                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                TestContext.WriteLine("Total Time:" + total);
                TestContext.WriteLine("Average Time:" + (total / cnt));
                Dispose();
            }

            [Test]
            public void AssignKey_UsingReturnValue()
            {
                Car ca = new Car { Id = string.Empty.PadLeft(15, '0'), Name = "Name" };
                Db.Insert(ca);
                DateTime start = DateTime.Now;
                List<string> ids = new List<string>();
                for (int i = 0; i < cnt; i++)
                {
                    var key = (i + 1).ToString().PadLeft(15, '0');
                    Car ca2 = new Car { Id = key, Name = "Name" + i };
                    var id = Db.Insert(ca2);
                    ids.Add(id);
                }

                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                TestContext.WriteLine("Total Time:" + total);
                TestContext.WriteLine("Average Time:" + (total / cnt));
                Dispose();
            }
        }
    }
}