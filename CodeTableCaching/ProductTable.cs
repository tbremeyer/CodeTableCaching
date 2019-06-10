using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CodeTableCaching
{
    [XmlType]
    public class ProductTable : ProjectCodeTable<Product>
    {
        private List<Product> _sortedList;

        [XmlIgnore]
        public List<Product> Product
        {
            get
            {
                if (_sortedList == null)
                {
                    _sortedList = new List<Product>(from c in base.CodesCastedActiveOnly
                        orderby c.Description
                        select c);
                }
                return new List<Product>(_sortedList);
            }
        }
        public override string TableName { get; }
    }
}
