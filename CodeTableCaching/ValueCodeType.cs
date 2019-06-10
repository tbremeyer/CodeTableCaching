using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using CodeTableCaching.Core.Interfaces.Entities;

namespace CodeTableCaching
{
    [GeneratedCode("System.Xml", "2.0.50727.3074")]
    [XmlType(Namespace = "http://www.project.com/Project/complexTypes")]
    [XmlInclude(typeof(ValueCodeOptionalType))]
    public class ValueCodeType : INotifyPropertyChanged, IValueCode, IHasIsEmpty, IOverwritable, ICloneable, IMatchable
    {
        private string _id;
        private string _invalidValue;
        // ReSharper disable once InconsistentNaming
        protected bool _isReadOnly;
        private string _value;

        public ValueCodeType()
        {
            _isReadOnly = false;
            _id = null;
            _value = null;
        }

        public ValueCodeType(string id, string value)
          : this()
        {
            _id = id;
            _value = value;
        }

        public ValueCodeType(string id, string value, bool isReadOnly)
          : this(id, value)
        {
            _isReadOnly = isReadOnly;
        }

        [XmlAttribute]
        public virtual string Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;
                if (!_isReadOnly)
                {
                    _id = value;
                    RaisePropertyChanged(nameof(Id));
                }
                else
                    throw new NotSupportedException(
                        $"This instance of ValueCodeType is readonly, changing the Id property is not allowed.\n\nConsider using the GetCopy() Method. \n\nold value: {(_value == null ? (object)"null" : (object)_value)}\nnew value: {(_value == null ? (object)"null" : (object)value)}\n\nold ID: {(_id == null ? (object)"null" : (object)_id)}\nnew ID: {(_id == null ? (object)"null" : (object)value)}");
            }
        }

        [XmlText]
        public virtual string Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;
                if (!_isReadOnly)
                {
                    _value = value;
                    RaisePropertyChanged(nameof(Value));
                }
                else
                    throw new NotSupportedException(
                        $"This instance of ValueCodeType is readonly, changing the Value property is not allowed.\n\nConsider using the GetCopy() Method. \n\nold value: {(_value == null ? (object)"null" : (object)_value)}\nnew value: {(value == null ? (object)"null" : (object)value)}\n\nold ID: {(_id == null ? (object)"null" : (object)_id)}\nnew ID: {(_id == null ? (object)"null" : (object)_id)}");
            }
        }

        [XmlAttribute]
        public virtual string InvalidValue
        {
            get => _invalidValue;
            set
            {
                if (_invalidValue == value)
                    return;
                _invalidValue = value;
                RaisePropertyChanged(nameof(InvalidValue));
            }
        }

        [XmlIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(InvalidValue);

        [XmlIgnore]
        public bool IsReadOnly => _isReadOnly;

        [XmlIgnore]
        public bool HasId => !string.IsNullOrEmpty(Id);

        public virtual ValueCodeType GetCopy()
        {
            return Clone() as ValueCodeType;
        }

        public override bool Equals(object obj)
        {
            if (obj is ValueCodeType valueCodeType)
            {
                if (Id == null && valueCodeType.Id == null && (Value != null && valueCodeType.Value != null) && String.Compare(Value, valueCodeType.Value, StringComparison.OrdinalIgnoreCase) == 0 || Id != null && valueCodeType.Id != null && (Id.Equals(valueCodeType.Id) && !string.IsNullOrEmpty(Value)) && (!string.IsNullOrEmpty(valueCodeType.Value) && String.Compare(Value, valueCodeType.Value, StringComparison.OrdinalIgnoreCase) == 0))
                    return true;
                if (!string.IsNullOrWhiteSpace(Id) || !string.IsNullOrWhiteSpace(valueCodeType.Id))
                {
                    if (string.Compare(Id, valueCodeType.Id, StringComparison.OrdinalIgnoreCase) != 0)
                        goto label_11;
                }
                if (string.IsNullOrWhiteSpace(Value))
                {
                    if (string.IsNullOrWhiteSpace(valueCodeType.Value))
                    {
                        if (string.IsNullOrWhiteSpace(InvalidValue) &&
                            string.IsNullOrWhiteSpace(valueCodeType.InvalidValue)) return true;
                        if (String.Compare(InvalidValue, valueCodeType.InvalidValue, StringComparison.OrdinalIgnoreCase) != 0)
                            goto label_11;
                        return true;
                    }
                }
            }

            label_11:
            return false;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        public static bool IsMatching(ValueCodeType valueCodeA, ValueCodeType valueCodeB)
        {

            return valueCodeA == null && valueCodeB == null || valueCodeA != null && valueCodeB != null &&
                   (string.CompareOrdinal(valueCodeA.Id, valueCodeB.Id) == 0 && string.CompareOrdinal(valueCodeA.Value, valueCodeB.Value) == 0);
        }

        public void SetIsReadOnly(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
                return (HasId ? "Id: " + Id + ", " : string.Empty) + Value;
            return string.Empty;
        }

        public bool IsMatching(object other)
        {
            return IsMatching(this, other as ValueCodeType);
        }

        public void Overwrite(object source)
        {
            var valueCodeType = source as ValueCodeType;
            if (valueCodeType == null)
                return;
            Id = valueCodeType.Id;
            Value = valueCodeType.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual object Clone()
        {
            var valueCodeType = new ValueCodeType(Id, Value);
            if (!string.IsNullOrEmpty(InvalidValue))
                valueCodeType.InvalidValue = InvalidValue;
            return valueCodeType;
        }

        public static string GetId(ValueCodeType valueCode)
        {
            if (valueCode != null && !string.IsNullOrEmpty(valueCode.Id))
                return valueCode.Id;
            return string.Empty;
        }

        public static string GetValue(ValueCodeType valueCode)
        {
            if (valueCode != null && !valueCode.IsEmpty)
                return valueCode.Value;
            return string.Empty;
        }

        public static bool HasIdSet(ValueCodeType valueCode)
        {
            if (valueCode != null)
                return valueCode.HasId;
            return false;
        }
    }
}
