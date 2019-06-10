using System.Xml.Serialization;
using CodeTableCaching.Core.Codes;

namespace CodeTableCaching
{
    [XmlInclude(typeof(Product))]
    public abstract class ProjectCode : Code
    {
        protected ProjectCode()
        {
        }
        //public ProjectCode(ICodeTable<Code> table)
        //    : base(table)
        //{
        //}
        protected ProjectCode(string id, bool isValid, int version)
            : base(id, isValid, version)
        {
        }

        protected ProjectCode(string id, bool isValid, string comment, int version)
            : base(id, isValid, comment, version)
        {
        }

        protected ProjectCode(string id, bool isValid, double? sortKey, int version)
            : base(id, isValid, sortKey, version)
        {
        }

        protected ProjectCode(string id, bool isValid, string comment, double? sortKey, int version)
            : base(id, isValid, comment, sortKey, version)
        {
        }

        protected ProjectCode(string id, bool isValid, string comment, string description, int version)
            : base(id, isValid, comment, description, version)
        {
        }

        protected ProjectCode(string id, bool isValid, string comment, string description, string shortDescription, double? sortKey, int version)
            : base(id, isValid, comment, description, shortDescription, sortKey, version)
        {
        }

        protected ProjectCode(string id, bool isValid, string comment, string description, double? sortKey, int version)
            : base(id, isValid, comment, description, sortKey, version)
        {
        }
    }
}
