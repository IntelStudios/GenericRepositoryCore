using GenericRepository.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.PropertyCollection
{
    [TestClass]
    public class PrefixedPropertyCollections
    {
        [TestMethod]
        public void AllProps()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>("pr");

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");

            try
            {
                var props1 = col.GetProperties<PropClass>();
                Assert.Fail("Type should not get.");
            }
            catch { }

            var props2 = col.GetProperties<PropClass>("pr");

            Assert.IsTrue(props2.Count == 3, "Incorrect number of properties.");
        }

        [TestMethod]
        public void AllAndRemovedProps()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>("pr");
            col.RemoveProperty<PropClass>(p => p.Prop2);

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");

            try
            {
                var props = col.GetProperties<PropClass>();
                Assert.Fail("Type should not get.");
            }
            catch { }

            col.RemoveProperty<PropClass>("pr", p => p.Prop2);

            var props2 = col.GetProperties<PropClass>("pr");
            Assert.IsTrue(props2.Count() == 2, "Incorrect number of properties.");
        }

        [TestMethod]
        public void VariousPrefixes()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>("p1");
            col.AddType<PropClass>("p2");

            Assert.IsTrue(col.Count() == 2, "Incorrect number of types.");

            var props = col.GetProperties<PropClass>("p1");

            Assert.IsTrue(props.Count() == 3, "Incorrect number of properties.");
        }

        [TestMethod]
        public void VariousPrefixesRemovedProperty()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>("p1");
            col.AddType<PropClass>("p2");

            col.RemoveProperty<PropClass>("p2", p => p.Prop1);

            Assert.IsTrue(col.Count() == 2, "Incorrect number of types.");

            var props1 = col.GetProperties<PropClass>("p1");
            var props2 = col.GetProperties<PropClass>("p2");

            Assert.IsTrue(props1.Count() == 3, "Incorrect number of properties.");
            Assert.IsTrue(props2.Count() == 2, "Incorrect number of properties.");
        }
    }
}
