#region

using System;
using System.ComponentModel;

#endregion

// ReSharper disable once CheckNamespace

namespace Hyper.ComponentModel
{
    public abstract class ChainingPropertyDescriptor : PropertyDescriptor
    {
        protected ChainingPropertyDescriptor(PropertyDescriptor root)
            : base(root)
        {
            Root = root;
        }

        public override AttributeCollection Attributes => Root.Attributes;

        public override string Category => Root.Category;

        public override Type ComponentType => Root.ComponentType;

        public override TypeConverter Converter => Root.Converter;

        public override string Description => Root.Description;

        public override bool DesignTimeOnly => Root.DesignTimeOnly;

        public override string DisplayName => Root.DisplayName;

        public override bool IsBrowsable => Root.IsBrowsable;

        public override bool IsLocalizable => Root.IsLocalizable;

        public override bool IsReadOnly => Root.IsReadOnly;

        public override string Name => Root.Name;

        public override Type PropertyType => Root.PropertyType;

        protected PropertyDescriptor Root { get; }

        public override bool SupportsChangeEvents => Root.SupportsChangeEvents;

        public override void AddValueChanged(object component, EventHandler handler)
        {
            Root.AddValueChanged(component, handler);
        }

        public override bool CanResetValue(object component)
        {
            return Root.CanResetValue(component);
        }

        public override bool Equals(object obj)
        {
            return Root.Equals(obj);
        }

        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            return Root.GetChildProperties(instance, filter);
        }

        public override object GetEditor(Type editorBaseType)
        {
            return Root.GetEditor(editorBaseType);
        }

        public override int GetHashCode()
        {
            return Root.GetHashCode();
        }

        public override object GetValue(object component)
        {
            return Root.GetValue(component);
        }

        public override void RemoveValueChanged(object component, EventHandler handler)
        {
            Root.RemoveValueChanged(component, handler);
        }

        public override void ResetValue(object component)
        {
            Root.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            Root.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return Root.ShouldSerializeValue(component);
        }

        public override string ToString()
        {
            return Root.ToString();
        }
    }
}