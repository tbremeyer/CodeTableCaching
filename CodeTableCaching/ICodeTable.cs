using System;
using System.Collections.Generic;
using System.Linq;
using CodeTableCaching.Core.Codes;

namespace CodeTableCaching
{
    public interface ICodeTable : IDisposable
    {
        int? PreviousStructureVersion
        {
            get;
        }

        int CurrentStructureVersion
        {
            get;
        }

        Type ContainedType
        {
            get;
        }

        List<Code> CodesSerialization
        {
            get;
        }

        IQueryable<Code> Codes
        {
            get;
        }

        List<Code> CodesEnumerable
        {
            get;
        }

        List<Code> CodesActiveOnly
        {
            get;
        }

        List<Code> CodesActiveOnlySortedBySortKey
        {
            get;
        }

        bool TryGetCodeBase(string id, out Code outCode);

        /// <summary>
        /// Gets the Code object for the given ID, null if not existant
        /// </summary>
        /// <param name="id">ID of the the Code to be found</param>
        /// <returns>Code instance, null if not found</returns>
        Code GetCode(string id);

        Code GetCodeByProperty(string propertyName, object value);

        void Add(Code newCode);

        void Remove(Code codeToBeRemoved);

        void Save();

        void UpdateLists();

        bool IsDownloaded
        {
            get;
        }

        void SetIsDownloaded(bool isDownloaded);

        bool IsCacheDataBaseTable
        {
            get;
        }

        string TableName
        {
            get;
        }

        long TableVersion
        {
            get;
            set;
        }
    }
}
