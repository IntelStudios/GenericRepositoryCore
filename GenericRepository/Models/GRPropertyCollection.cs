using GenericRepository.Helpers;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRPropertyCollection
    {
        // lock sync object
        object sync = new object();

        // list of properties grouped by entity type
        Dictionary<Type, List<GRDBProperty>> collection = new Dictionary<Type, List<GRDBProperty>>();

        /// <summary>
        /// Adds properties to collection. If no concrete properties are specified, all properties of the generic type will be added.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="propExps">Property expressions</param>
        public void Add<T>(params Expression<Func<T, object>>[] propExps)
        {
            List<GRDBProperty> propsToAdd = new List<GRDBProperty>();
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure<T>();

            if (propExps.Any())
            {
                foreach (var propExp in propExps)
                {
                    string propertyName = GRDataTypeHelper.GetExpressionPropertyName(propExp);
                    GRDBProperty property = structure[propertyName];
                    propsToAdd.Add(property);
                }
            }
            else
            {
                propsToAdd.AddRange(structure.Properties);
            }

            propsToAdd.ForEach(prop =>
            {
                AddProperty(prop);
            });
        }

        /// <summary>
        /// Removes properties from collection. If no concrete properties are specified, all properties of the generic type will be removed.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="propsExps">Property expressions</param>
        public void Remove<T>(params Expression<Func<T, object>>[] propsExps)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure<T>();
            Type type = typeof(T);

            lock (sync)
            {
                if (!propsExps.Any())
                {
                    if (!collection.ContainsKey(type))
                    {
                        return;
                    }

                    collection.Remove(type);
                }

                if (!collection.ContainsKey(type))
                {
                    return;
                }

                foreach (var propExp in propsExps)
                {
                    string propertyName = GRDataTypeHelper.GetExpressionPropertyName(propExp);
                    GRDBProperty property = structure[propertyName];

                    if (collection[type].Contains(property))
                    {
                        collection[type].Remove(property);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the property into collection.
        /// </summary>
        /// <param name="property"></param>
        void AddProperty(GRDBProperty property)
        {
            Type type = property.PropertyInfo.DeclaringType;

            lock (sync)
            {
                if (!collection.ContainsKey(type))
                {
                    collection.Add(type, new List<GRDBProperty>());
                }

                if (!collection[type].Contains(property))
                {
                    collection[type].Add(property);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in collection)
            {
                sb.AppendLine(string.Format("Type {0}:", pair.Key));

                foreach (GRDBProperty property in pair.Value)
                {
                    sb.AppendLine(string.Format(" - {0}: {1}", property.PropertyInfo.Name, property.PropertyInfo.PropertyType));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns properties for provided data type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<GRDBProperty> GetProperties<T>()
        {
            Type type = typeof(T);
            return GetProperties(type);
        }

        /// <summary>
        /// Returns properties for provided data type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<GRDBProperty> GetProperties(Type type)
        {
            if (!collection.ContainsKey(type))
            {
                return new List<GRDBProperty>();
            }

            return collection[type];
        }
    }
}
