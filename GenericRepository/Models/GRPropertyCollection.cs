using GenericRepository.Helpers;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using GenericRepository.Exceptions;

namespace GenericRepository.Models
{
    /// <summary>
    /// GRPropertyCollection is not thread-safe for getting better performance! Keep it mind while defining list of properties.
    /// </summary>
    public class GRPropertyCollection : IEnumerable<GRPropertyCollectionItem>
    {
        private Dictionary<GRPropertyCollectionKey, GRPropertyCollectionItem> collection = new Dictionary<GRPropertyCollectionKey, GRPropertyCollectionItem>();

        public void AddType(Type type)
        {
            AddType(string.Empty, type);
        }

        public void AddType(string prefix, Type type)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(type);

            structure.Properties.ForEach(prop =>
            {
                AddProperty(prefix, new GRDBQueryProperty(prop));
            });
        }

        public void AddType<T>()
        {
            AddType<T>(string.Empty);
        }

        public void AddType<T>(string prefix)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));

            structure.Properties.ForEach(prop =>
            {
                AddProperty(prefix, new GRDBQueryProperty(prop));
            });
        }

        public void AddProperty<T>(params Expression<Func<T, object>>[] propExps)
        {
            AddProperty(string.Empty, propExps);
        }

        public void AddProperty<T>(string prefix, params Expression<Func<T, object>>[] propExps)
        {
            foreach (Expression<Func<T, object>> propExp in propExps)
            {
                GRDBProperty property = GRDataTypeHelper.GetDBProperty(propExp);
                AddProperty(prefix, new GRDBQueryProperty(property));
            }
        }

        void AddProperty(string prefix, GRDBQueryProperty property)
        {
            Type propertyType = property.Property.PropertyInfo.DeclaringType;

            GRPropertyCollectionKey key = new GRPropertyCollectionKey(prefix, property.Property.PropertyInfo.DeclaringType);

            if (!collection.ContainsKey(key))
            {
                collection.Add(key, new GRPropertyCollectionItem(key.Prefix, key.Type));
            }

            GRPropertyCollectionItem item = collection[key];

            item.Properties.Add(property.Property, property);
        }

        public void RemoveType(Type type)
        {
            RemoveType(string.Empty, type);
        }

        public void RemoveType(string prefix, Type type)
        {
            GRPropertyCollectionKey key = new GRPropertyCollectionKey(prefix, type);

            if (!collection.ContainsKey(key))
            {
                return;
            }

            collection.Remove(key);
        }

        public void RemoveProperty<T>(params Expression<Func<T, object>>[] propsExps)
        {
            RemoveProperty(null, propsExps);
        }

        /// <summary>
        /// Removes properties from collection. If no concrete properties are specified, all properties of the generic type will be removed.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="propsExps">Property expressions</param>
        public void RemoveProperty<T>(string prefix, params Expression<Func<T, object>>[] propsExps)
        {
            GRPropertyCollectionKey key = new GRPropertyCollectionKey(prefix, typeof(T));

            // nothing to remove
            if (!collection.ContainsKey(key))
            {
                return;
            }

            GRDBStructure structure = GRDataTypeHelper.GetDBStructure<T>();

            // removing only specified properties
            foreach (var propExp in propsExps)
            {
                GRDBProperty property = GRDataTypeHelper.GetDBProperty(propExp);

                if (collection[key].Properties.ContainsKey(property))
                {
                    collection[key].Properties.Remove(property);
                }
            }

        }

        public override string ToString()
        {
            if (collection == null)
            {
                return "Empty collection";
            }

            return $"{this.collection.Count} item(s)";
        }

        /// <summary>
        /// Returns properties for provided data type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<GRDBQueryProperty> GetProperties<T>()
        {
            Type type = typeof(T);
            return GetProperties(null, type);
        }

        public List<GRDBQueryProperty> GetProperties(Type type)
        {
            return GetProperties(null, type);
        }

        public List<GRDBQueryProperty> GetProperties<T>(string prefix)
        {
            Type type = typeof(T);
            return GetProperties(prefix, type);
        }

        public List<GRDBQueryProperty> GetProperties(string prefix, Type type)
        {
            GRPropertyCollectionKey key = new GRPropertyCollectionKey(prefix, type);
            return collection[key].Properties.Values.ToList(); ;
        }

        public IEnumerator<GRPropertyCollectionItem> GetEnumerator()
        {
            if (collection == null || !collection.Any())
            {
                yield break;
            }

            foreach (KeyValuePair<GRPropertyCollectionKey, GRPropertyCollectionItem> pair in collection)
            {
                yield return pair.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public GRPropertyCollection(Dictionary<string, Type> dic)
        {
            foreach (KeyValuePair<string, Type> item in dic)
            {
                AddType(item.Key, item.Value);
            }
        }

        public GRPropertyCollection()
        {

        }
    }
}
