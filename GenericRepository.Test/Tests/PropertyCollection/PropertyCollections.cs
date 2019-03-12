using GenericRepository.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.PropertyCollection
{
    public class PropClass
    {
        public int Prop1 { get; set; }
        public int Prop2 { get; set; }
        public int Prop3 { get; set; }
    }

    [TestClass]
    public class PropertyCollections
    {
        [TestMethod]
        public void AllProps()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>();

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");
        }

        [TestMethod]
        public void AllAndRemovedProps()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>();
            col.RemoveProperty<PropClass>(p => p.Prop2);

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");

            var props = col.GetProperties<PropClass>();

            Assert.IsTrue(props.Count() == 2, "Incorrect number of properties.");
        }

        [TestMethod]
        public void AllAndRemovedAllProps()
        {
            GRPropertyCollection col = new GRPropertyCollection();
            col.AddType<PropClass>();
            col.RemoveProperty<PropClass>(p => p.Prop2);
            col.RemoveProperty<PropClass>(p => p.Prop1);
            col.RemoveProperty<PropClass>(p => p.Prop3);

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");

            var props = col.GetProperties<PropClass>();

            Assert.IsTrue(props.Count() == 0, "Incorrect number of properties.");
        }

        [TestMethod]
        public void CollectionWith()
        {
            GRPropertyCollection col = GRPropertyCollection.With<PropClass>(p => p.Prop2);

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");

            var props = col.GetProperties<PropClass>();

            Assert.IsTrue(props.Count() == 1, "Incorrect number of properties.");
        }

        [TestMethod]
        public void CollectionWithout()
        {
            GRPropertyCollection col = GRPropertyCollection.Without<PropClass>(p => p.Prop2);

            Assert.IsTrue(col.Count() == 1, "Incorrect number of types.");

            var props = col.GetProperties<PropClass>();

            Assert.IsTrue(props.Count() == 2, "Incorrect number of properties.");
        }
    }
}
