using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;

namespace CodeTableCaching
{
    public abstract class CodeTableSet : IDisposable
    {
        #region Singleton instance
        // ReSharper disable once InconsistentNaming
        protected static CodeTableSet _instance;

        public static CodeTableSet Instance => _instance;

        protected static void InitInstance(CodeTableSet instance)
        {
            _instance = instance;
        }

        #endregion

        public abstract List<ICodeTable> LoadedTables
        {
            get;
        }

        public abstract DataContext CacheDatabase
        {
            get;
        }

        public abstract void ReopenCacheDatabase();

        public abstract void LoadAllTables(bool forceDownload);

        public abstract void LoadAllTables(bool forceDownload, BackgroundWorker backgroundWorker);

        public abstract void DeleteLocalCache();

        public abstract ICodeTable GetCodeTable(Type type);

        public abstract ICodeTable GetCodeTable(string typeName);

        #region general tables
        //abstract public IUserTable IUserTable
        //{
        //    get;
        //}
        //abstract public IOrgUnitTable IOrgUnitTable
        //{
        //    get;
        //}
        //abstract public IPersonQualificationFilterTable IPersonQualificationFilterTable
        //{
        //    get;
        //}

        #endregion
        #region general ripol tables
        //abstract public ICityTable ICityTable
        //{
        //    get;
        //}

        //#endregion

        //#region Program tables (enum wrapping classes)
        //abstract public ColorSchemaTable ColorSchemaTable
        //{
        //    get;
        //}
        //abstract public WeekdayTable WeekdayTable
        //{
        //    get;
        //}
        #endregion

        #region IDisposable Members
        public abstract void Dispose();
        #endregion
    }
}
