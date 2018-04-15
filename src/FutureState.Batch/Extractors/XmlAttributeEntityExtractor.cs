using System;
using System.Collections.Generic;
using NLog;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace FutureState.Batch
{

    public class XmlAttributeEntityExtractor<TDto>  : IExtractor<TDto>
            where TDto : class, new()
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<PropertyInfo, PropertySetterDelegate> _propertyMap;

        /// <summary>
        ///     Gets/sets the path to the file to read in.
        /// </summary>
        public string Uri { get; set; }

        static XmlAttributeEntityExtractor()
        {
            _propertyMap = typeof(TDto)
                .GetProperties()
                .Where(m => m.GetSetMethod() != null)
                .Where(m => m.GetGetMethod() != null)
                .ToDictionary(m => m, m => m.GetPropertySetterFn());
        }

        /// <summary>
        ///     Gets the name of the node set to read from.
        /// </summary>
        public string SelectionNode { get; set; }


        public IEnumerable<TDto> Read()
        {
            if(string.IsNullOrWhiteSpace(SelectionNode))
                throw new InvalidOperationException("SelectionNode has not been supplied.");

            if (string.IsNullOrWhiteSpace(Uri))
                throw new InvalidOperationException("Uri has not been supplied.");

            using (XmlReader reader = XmlReader.Create(this.Uri))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    // move to entity
                    if (reader.Name != SelectionNode)
                        continue;

                    XElement entityNode = XNode.ReadFrom(reader) as XElement;

                    XmlReader nodeReader = entityNode.CreateReader(ReaderOptions.OmitDuplicateNamespaces);

                    TDto entity = null;

                    try
                    {
                        // read entity
                        entity = new TDto();

                        ReadEntityNode(nodeReader, entity);
                    }
                    catch (Exception ex)
                    {
                        // don't fail reading
                        if (_logger.IsErrorEnabled)
                            _logger.Error(ex);

                        entity = null;
                    }

                    if(entity != null)
                        yield return entity;
                }
            }
        }

        protected virtual void ReadEntityNode(XmlReader nodeReader, TDto entity)
        {
            // loop through Item elements  
            while (nodeReader.Read())
            {
                if (nodeReader.NodeType == XmlNodeType.EndElement)
                    break;

                // try to auto map
                foreach (var dictKey in _propertyMap)
                {
                    if (!string.Equals(dictKey.Key.Name, nodeReader.Name, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var setter = dictKey.Value;
                    var propertyType = dictKey.Key.PropertyType;

                    var value = nodeReader.ReadInnerXml();

                    // auto bind propeties to common property types
                    if (propertyType == typeof(string))
                        setter.Invoke(entity, value);
                    else if (propertyType == typeof(DateTime))
                        setter.Invoke(entity, DateTime.Parse(value));
                    else if (propertyType == typeof(int))
                        setter.Invoke(entity, int.Parse(value));
                    else if (propertyType == typeof(long))
                        setter.Invoke(entity, long.Parse(value));
                    else if (propertyType == typeof(short))
                        setter.Invoke(entity, short.Parse(value));
                    else if (propertyType == typeof(double))
                        setter.Invoke(entity, double.Parse(value));
                    else if (propertyType == typeof(decimal))
                        setter.Invoke(entity, decimal.Parse(value));
                    else if (propertyType == typeof(bool))
                        setter.Invoke(entity, bool.Parse(value));
                    // nullables
                    else if (propertyType == typeof(DateTime?))
                        setter.Invoke(entity, value == null ? null : (DateTime?)DateTime.Parse(value));
                    else if (propertyType == typeof(int?))
                        setter.Invoke(entity, value == null ? null : (int?)int.Parse(value));
                    else if (propertyType == typeof(long?))
                        setter.Invoke(entity, value == null ? null : (long?)long.Parse(value));
                    else if (propertyType == typeof(short?))
                        setter.Invoke(entity, value == null ? null : (short?)short.Parse(value));
                    else if (propertyType == typeof(double?))
                        setter.Invoke(entity, value == null ? null : (double?)double.Parse(value));
                    else if (propertyType == typeof(decimal?))
                        setter.Invoke(entity, value == null ? null : (decimal?)decimal.Parse(value));
                    else if (propertyType == typeof(bool?))
                        setter.Invoke(entity, value == null ? null : (bool?)bool.Parse(value));
                }
            }
        }
    }
}
