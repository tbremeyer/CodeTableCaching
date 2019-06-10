using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using CodeTableCaching.Core.Codes;

namespace CodeTableCaching
{
    #region XmlIncludes
    [XmlInclude(typeof(ProductTable))]
    #endregion

    [XmlType]
    [XmlRoot("ProjectCodeTable", IsNullable = false)]
    public abstract class ProjectCodeTable<TCode> : CodeTable<TCode> where TCode : Code
    {
        private ObservableCollection<ProjectCode> _projectcodesForSerialization;
        // ReSharper disable once StaticMemberInGenericType
        private static Dictionary<Type,Type> _containedTypeOfTableType = new Dictionary<Type, Type>();

        static ProjectCodeTable()
        {
            ProjectCodeTable<Code>._containedTypeOfTableType[typeof(ProductTable)] = typeof(Product);
        }

        public ProjectCodeTable()
        {
            _projectcodesForSerialization = new ObservableCollection<ProjectCode>();
        }
    }
}
