using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace WPFLolHUDEditor
{

    [ExpandableObject]
    public class ComplexObject
    {
        public string Name { get; set; }

        [ExpandableObject]
        public ExpandableObservableCollection<CustomProperty> Properties { get; set; }

        public ComplexObject()
        {
            Properties = new ExpandableObservableCollection<CustomProperty>(); 
        }

        public void setVal(string name, object val)
        {
            if (exist(name))
            {
                CustomProperty ct = Properties.Where(x => x.Name.Trim().ToLower() == name.Trim().ToLower()).FirstOrDefault();
                ct.Value = val;
            }
        }

        public void Add(CustomProperty customProperty)
        {
            this.Properties.Add(customProperty);
        }

        public object getVal(string name)
        {
            if (exist(name))
            {
                CustomProperty ct = Properties.Where(x => x.Name.Trim().ToLower() == name.Trim().ToLower()).FirstOrDefault();
                return ct.Value;
            }
            return "";
        }

        public bool exist(string v)
        {
            if (Properties.Where(x => x.Name.Trim().ToLower() == v.Trim().ToLower()).Count() > 0)
                return true;
            else
                return false;
        }
    }

     
    public class ExpandableObservableCollection<T> : ObservableCollection<T>,
                                                     ICustomTypeDescriptor
    {
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            // Create a collection object to hold property descriptors
            PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

            for (int i = 0; i < Count; i++)
            {
                pds.Add(new ItemPropertyDescriptor<T>(this, i));
            }

            return pds;
        }

        #region Use default TypeDescriptor stuff

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, noCustomTypeDesc: true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, noCustomTypeDesc: true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(this, attributes, noCustomTypeDesc: true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }

    public class ItemPropertyDescriptor<T> : PropertyDescriptor
    {
        private readonly CustomProperty _owner;
        private readonly int _index;

        public ItemPropertyDescriptor(ObservableCollection<T> owner, int index)
          : base( (owner[index] as CustomProperty).Name, null)
        {
            T pr = owner[index];
            _owner = pr as CustomProperty;
            
            _index = index;
        }

        public override string Category
        {
            get
            {
                return _owner.Category;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return Value;
        }

        private object Value
          => _owner.Value;

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            _owner.Value = value;
            Statik.frm1.onPropertyChanged(_owner, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
          => _owner.GetType();

        public override bool IsReadOnly
          => false;

        public override Type PropertyType
          => Value?.GetType();
    }

    public class CustomProperty
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public object Value { get; set; }
        public bool ReadOnly { get; set; }
        public bool WProp { get; set; }
        public string Category { get; set; }
        Type type;

        public Type Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                if (type == typeof(string))
                    Value = "";
                else
                    Value = Activator.CreateInstance(value);
            }
        }
    }


}

