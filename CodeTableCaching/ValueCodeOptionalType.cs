using System.Xml.Serialization;

namespace CodeTableCaching
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3074")]
    [XmlType(Namespace = "http://www.project.com/Project/complexTypes")]
    public class ValueCodeOptionalType : ValueCodeType
    {
        #region fields
        private string _id;
        private string _value;
        #endregion

        #region ctors
        public ValueCodeOptionalType()
        {
            _id = null;
            _value = null;
        }

        public ValueCodeOptionalType(string value)
            : this()
        {
            _value = value;
        }

        public ValueCodeOptionalType(string id, string value)
            : this(value)
        {
            _id = id;
        }
        #endregion

        #region properties
        [XmlAttribute]
        public override string Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        [XmlText]
        public override string Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region methods
        public static string GetId(ValueCodeOptionalType valueOptCode)
        {
            return ValueCodeType.GetId(valueOptCode);
        }

        public override object Clone()
        {
            ValueCodeOptionalType clone = new ValueCodeOptionalType(Id, Value);
            if (!string.IsNullOrEmpty(InvalidValue))
                clone.InvalidValue = InvalidValue;
            return clone;
        }

        public new ValueCodeOptionalType GetCopy()
        {
            return Clone() as ValueCodeOptionalType;
        }
        #endregion
    }

}
