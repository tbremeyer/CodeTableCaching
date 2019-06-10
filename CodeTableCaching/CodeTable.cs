using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using CodeTableCaching.Core.Codes;

namespace CodeTableCaching
{
    [XmlType]
    [XmlRoot("ProjectCodeTable", IsNullable = false)]
    public abstract class CodeTable<TCode> : ICodeTable, INotifyPropertyChanged where TCode : Code
    {
        #region static ctor

        static CodeTable()
        {
        }

        #endregion

        #region ctors

        protected CodeTable()
        {
            _instances.Add(this);
            PreviousStructureVersion = null;
            CodesForSerialization = new List<Code>();
        }

        #endregion

        #region static fields

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, Type> ContainedTypeOfTableType = new Dictionary<Type, Type>();
        private static List<CodeTable<TCode>> _instances = new List<CodeTable<TCode>>();

        #endregion

        #region fields

        private DataContext _cacheDatabase;
        private ITable _cacheTable;
        private ConcurrentDictionary<string, Code> _codesInMemory;
        protected List<Code> CodesForSerialization;
        private bool? _isCacheDataBaseTable;
        protected bool IsDisposed;

        #region memory caching

        private IQueryable<Code> _codes;
        private IQueryable<TCode> _codesCasted;
        private List<Code> _codesEnumerable;
        private List<TCode> _codesCastedEnumerable;
        private List<Code> _codesActiveOnly;
        private List<TCode> _codesCastedActiveOnly;
        private List<Code> _codesActiveOnlySortedBySortKey;
        private List<TCode> _codesCastedActiveOnlySortedBySortKey;
        private List<TCode> _codesSortedBySortKey;
        private readonly ConcurrentDictionary<string, Code> _codeOfId = new ConcurrentDictionary<string, Code>();

        private readonly ConcurrentDictionary<string, Code> _codeOfExactDescription =
            new ConcurrentDictionary<string, Code>();

        private readonly ConcurrentDictionary<string, Code> _codeOfExactShortDescription =
            new ConcurrentDictionary<string, Code>();

        private readonly ConcurrentDictionary<string, Code> _codeOfExactValueCode =
            new ConcurrentDictionary<string, Code>();

        private readonly ConcurrentDictionary<string, Code> _codeOfContainingDescription =
            new ConcurrentDictionary<string, Code>();

        private readonly ConcurrentDictionary<string, Code> _codeOfStartsWithDescription =
            new ConcurrentDictionary<string, Code>();

        #endregion

        #endregion

        #region properties

        #region public

        #region Xml serialized

        //[XmlElement("Code")]
        [XmlIgnore]
        public List<Code> CodesSerialization => CodesForSerialization ?? new List<Code>(CodeDictionary.Values);

        [XmlElement("StructureVersion")]
        public int? StructureVersionSerialization
        {
            get => CurrentStructureVersion;
            set => PreviousStructureVersion = value;
        }

        [XmlElement("TableVersion")] public long TableVersion { get; set; }

        #endregion

        [XmlIgnore] public virtual int CurrentStructureVersion => 1;

        [XmlIgnore] public int? PreviousStructureVersion { get; private set; }

        #region not serialized

        public DataContext CacheDatabase
        {
            set
            {
                _cacheDatabase = value;

                if (_cacheDatabase != null)
                {
                    if (CodesForSerialization != null)
                        // do not dispose objects as changes would reflect in cache database
                        //foreach (Code c in _codesForSerialization)
                        //{
                        //	c.Dispose();
                        //}
                        CodesForSerialization.Clear();
                    CodesForSerialization = null;

                    if (_codesInMemory != null)
                    {
                        // do not dispose objects as changes would reflect in cache database
                        //foreach (Code c in _codesInMemory.Values)
                        //{
                        //	c.Dispose();
                        //}
                        _codesInMemory.Clear();
                        _codesInMemory = null;
                    }

                    foreach (var otherInstance in _instances)
                        if (otherInstance != this)
                        {
                            otherInstance._cacheDatabase = _cacheDatabase;
                            if (otherInstance.CodesForSerialization != null)
                            {
                                otherInstance.CodesForSerialization.Clear();
                                otherInstance.CodesForSerialization = null;
                            }

                            if (otherInstance._codesInMemory != null)
                            {
                                otherInstance._codesInMemory.Clear();
                                otherInstance._codesInMemory = null;
                            }
                        }
                }
            }
        }

        /// <summary>
        ///     Returns the whole queryable collection of codes.
        /// </summary>
        [XmlIgnore]
        public IQueryable<Code> Codes
        {
            get
            {
                if (_codes != null) return _codes;
                var cacheTable = CacheDatabaseTable;
                if (cacheTable != null && IsCacheDataBaseTable)
                    try
                    {
                        _codes = cacheTable.AsQueryable().Cast<Code>();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                else
                    _codes = CodeDictionary.Values.AsQueryable();

                return _codes;
            }
        }

        /// <summary>
        ///     Returns the whole queryable collection (including inactive ones) of type casted codes.
        /// </summary>
        [XmlIgnore]
        public IQueryable<TCode> CodesCasted
        {
            get
            {
                if (_codesCasted == null)
                    _codesCasted = Codes.Cast<TCode>();
                return _codesCasted;
            }
        }

        /// <summary>
        ///     Returns the whole collection (including inactive ones) of codes as List.
        /// </summary>
        [XmlIgnore]
        public virtual List<Code> CodesEnumerable =>
            _codesEnumerable ?? (_codesEnumerable = new List<Code>(Codes.AsEnumerable()));

        /// <summary>
        ///     Returns a new List of all codes (including inactive ones) of type casted codes as IEnumerable.
        /// </summary>
        [XmlIgnore]
        public virtual List<TCode> CodesCastedEnumerable
        {
            get
            {
                if (_codesCastedEnumerable == null)
                    _codesCastedEnumerable = new List<TCode>(CodesEnumerable.Cast<TCode>());
                return new List<TCode>(_codesCastedEnumerable);
            }
        }

        /// <summary>
        ///     Returns a new List that contains only active codes, sorted by ValueCodeText
        /// </summary>
        [XmlIgnore]
        public virtual List<Code> CodesActiveOnly
        {
            get
            {
                if (_codesActiveOnly == null)
                    _codesActiveOnly = new List<Code>(from Code c in CodesEnumerable
                                                      where c.IsActive
                                                      orderby c.ValueCodeText
                                                      select c);
                return new List<Code>(_codesActiveOnly);
            }
        }

        /// <summary>
        ///     Returns a new List that contains only active type casted codes, sorted by ValueCodeText
        /// </summary>
        [XmlIgnore]
        public virtual List<TCode> CodesCastedActiveOnly
        {
            get
            {
                if (_codesCastedActiveOnly == null)
                    _codesCastedActiveOnly = new List<TCode>(CodesActiveOnly.Cast<TCode>());
                return new List<TCode>(_codesCastedActiveOnly);
            }
        }

        /// <summary>
        ///     Returns a new List that contains only active codes, sorted by SortKey, ValueCodeText
        /// </summary>
        [XmlIgnore]
        public virtual List<Code> CodesActiveOnlySortedBySortKey
        {
            get
            {
                if (_codesActiveOnlySortedBySortKey == null)
                    _codesActiveOnlySortedBySortKey = new List<Code>(from Code c in CodesActiveOnly
                                                                     where c.IsActive
                                                                     orderby c.SortKey, c.ValueCodeText
                                                                     select c);
                return new List<Code>(_codesActiveOnlySortedBySortKey);
            }
        }

        /// <summary>
        ///     Returns a new List that contains only active type casted codes, sorted by SortKey, ValueCodeText
        /// </summary>
        [XmlIgnore]
        public virtual List<TCode> CodesCastedActiveOnlySortedBySortKey
        {
            get
            {
                if (_codesCastedActiveOnlySortedBySortKey == null)
                    _codesCastedActiveOnlySortedBySortKey =
                        new List<TCode>(CodesActiveOnlySortedBySortKey.Cast<TCode>());
                return new List<TCode>(_codesCastedActiveOnlySortedBySortKey);
            }
        }

        /// <summary>
        ///     Returns a new List that contains all codes (incl. inactives ones), sorted by SortKey, ValueCodeText
        /// </summary>
        [XmlIgnore]
        public virtual List<TCode> CodesSortedBySortKey
        {
            get
            {
                if (_codesSortedBySortKey == null)
                    _codesSortedBySortKey = new List<TCode>(from TCode c in CodesEnumerable
                                                            orderby c.SortKey, c.ValueCodeText
                                                            select c);
                return new List<TCode>(_codesSortedBySortKey);
            }
        }

        /// <summary>
        ///     Gets the Type of the Code objects that are contained in this table.
        /// </summary>
        [XmlIgnore]
        public virtual Type ContainedType => typeof(TCode);

        [XmlIgnore]
        public bool IsCacheDataBaseTable
        {
            get
            {
                var elementType = ContainedType;
                if (_isCacheDataBaseTable == null && elementType != null)
                {
                    var ca = elementType.GetCustomAttributes(typeof(TableAttribute), true);
                    _isCacheDataBaseTable = ca.Length > 0;
                }

                return _isCacheDataBaseTable ?? false;
            }
        }

        /// <summary>
        ///     Tells whether this table was newly downloaded from server.
        /// </summary>
        [XmlIgnore]
        public bool IsDownloaded { get; private set; }

        [XmlIgnore] public abstract string TableName { get; }

        #region indexer

        public TCode this[string codeId]
        {
            get
            {
                if (!string.IsNullOrEmpty(codeId)) return GetCode(codeId) as TCode;
                return null;
            }
        }

        public TCode this[ValueCodeType valueCode] => GetCode(valueCode) as TCode;

        #endregion

        #endregion

        #endregion

        #region private

        private ITable CacheDatabaseTable
        {
            get
            {
                if (_cacheDatabase != null)
                {
                    try
                    {
                        if (_cacheDatabase.Connection.State != ConnectionState.Open)
                        {
                            _cacheDatabase.Connection.Open();
                            _cacheTable = null;
                        }
                    }
                    catch //(Exception ex)
                    {
                        _cacheTable = null;
                        ReopenCacheDatabase();
                    }

                    if (_cacheTable == null)
                        _cacheTable = _cacheDatabase.GetTable(ContainedType);
                }

                return _cacheTable;
            }
        }

        public virtual void ReopenCacheDatabase()
        {
            if (IsCacheDataBaseTable)
            {
                CodeTableSet.Instance.ReopenCacheDatabase();
                UpdateLists();
                _cacheDatabase = CodeTableSet.Instance.CacheDatabase;
            }
        }

        ///// <summary>
        ///// Gets the whole collection of codes, including inactive ones
        ///// </summary>		
        protected virtual ConcurrentDictionary<string, Code> CodeDictionary
        {
            get
            {
                if (_codesInMemory == null && CodesForSerialization != null)
                {
                    _codesInMemory = new ConcurrentDictionary<string, Code>((from Code c in CodesForSerialization
                                                                             orderby c.ValueCodeText
                                                                             select c).ToDictionary(n => n.Id, n => n));
                    CodesForSerialization = null;

                    var ictThis = this as ICodeTable;
                    foreach (var c in _codesInMemory.Values)
                        c.SetParentTable(ictThis);
                }

                return _codesInMemory;
            }
        }

        #endregion

        #endregion

        #region methods

        #region Code retrieval methods (GetCode/ValueCodeType)

        #region Methods to retrieve codes by ID or ValueCodeType

        public static bool TryGetCode(IQueryable<Code> codes, string id, out Code outCode)
        {
            outCode = null;
            if (codes == null) return false;
            // do not change to FirstOrDefault method because not compatible with SqLite
            IEnumerable<Code> ieCodes = from Code c in codes
                                        where c.Id == id
                                        select c;
            outCode = ieCodes.FirstOrDefault();
            return outCode != null;
        }

        public static bool TryGetCode(IQueryable<Code> codes, ValueCodeType valueCode, out Code outCode)
        {
            outCode = null;
            if (codes != null && valueCode != null && valueCode.HasId)
                return TryGetCode(codes, valueCode.Id, out outCode);
            return false;
        }

        public bool TryGetCodeBase(string id, out Code outCode)
        {
            outCode = null;
            Code result = null;
            if (!string.IsNullOrEmpty(id) && (!_codeOfId.TryGetValue(id, out result) || result == null))
            {
                if (IsCacheDataBaseTable)
                    //IEnumerable<Code> ieCodes = from Code c in this.Codes
                    //							where c.ID == ID
                    //							select c;
                    //if (ieCodes != null && ieCodes.Count() > 0)
                    //	result = ieCodes.First();
                    TryGetCode(Codes, id, out result);
                else
                    CodeDictionary.TryGetValue(id, out result);
                _codeOfId[id] = result;
            }

            if (result != null)
            {
                outCode = result;
                return true;
            }

            return false;
        }

        public bool TryGetCode(string id, out TCode outCode)
        {
            outCode = null;
            Code code;
            if (TryGetCodeBase(id, out code))
            {
                outCode = code as TCode;
                return true;
            }

            return false;
        }

        public Code GetCode(string id)
        {
            TryGetCodeBase(id, out var result);
            return result;
        }

        public Code GetCodeByProperty(string propertyName, object value)
        {
            throw new NotImplementedException();
        }

        public Code GetCode(ValueCodeType valueCode)
        {
            if (valueCode != null && valueCode.HasId)
                return GetCode(valueCode.Id);
            return null;
        }

        public bool TryGetValueCode(string id, out ValueCodeType outValueCode)
        {
            outValueCode = null;
            Code code;
            if (TryGetCodeBase(id, out code))
            {
                outValueCode = code.GetValueCodeType();
                return true;
            }

            return false;
        }

        public ValueCodeType GetValueCode(string id)
        {
            TryGetValueCode(id, out var valueCode);
            return valueCode;
        }

        #endregion

        #region Methods to retrieve codes by description and other properties

        #region static base functions

        public static Code GetCodeByDescriptionExact(IQueryable<Code> codes, string description)
        {
            Code result = null;
            if (!string.IsNullOrEmpty(description))
            {
                IEnumerable<Code> ieCodes = from Code c in codes
                                            where String.Compare(c.Description, description, StringComparison.OrdinalIgnoreCase) == 0
                                            select c;
                if (ieCodes.Any())
                    result = ieCodes.First();
            }

            return result;
        }

        public static Code GetCodeByDescriptionContains(IQueryable<Code> codes, string description)
        {
            Code result = null;
            if (!string.IsNullOrEmpty(description))
            {
                var searchString = description.ToLower();
                IEnumerable<Code> ieCodes = from Code c in codes
                                            where c.Description != null && c.Description.ToLower().Contains(searchString)
                                            select c;
                //if (ieCodes != null && ieCodes.Count() > 0)
                //    result = ieCodes.First();
                result = ieCodes.FirstOrDefault();
            }

            return result;
        }

        public static Code GetCodeByDescriptionStartsWith(IQueryable<Code> codes, string description)
        {
            Code result = null;
            if (!string.IsNullOrEmpty(description))
            {
                var searchString = description.ToLower();
                IEnumerable<Code> ieCodes = from Code c in codes
                                            where c.Description != null && c.Description.ToLower().StartsWith(searchString)
                                            select c;
                //if (ieCodes != null && ieCodes.Count() > 0)
                //    result = ieCodes.First();
                result = ieCodes.FirstOrDefault();
            }

            return result;
        }

        #endregion

        /// <summary>
        ///     Searches the table by description text and returns the first occurrence of Code whose description matches the
        ///     search string exactly.
        ///     Searches case insensitive.
        /// </summary>
        /// <param name="description">The description string to be found in Code.Description</param>
        /// <returns>The first code object that matches the search string</returns>
        public virtual Code GetCodeByDescriptionExact(string description)
        {
            if (!(string.IsNullOrEmpty(description) && !_codeOfExactDescription.TryGetValue(description ?? throw new ArgumentNullException(nameof(description)), out var result)))
                _codeOfExactDescription[description] = result = GetCodeByDescriptionExact(Codes, description);
            return result;
        }

        /// <summary>
        ///     Searches the table by description text and returns a ValueCodeType of the first occurrence of Code whose
        ///     description matches the search string exactly.
        ///     Searches case insensitive.
        /// </summary>
        /// <param name="description">The description string to be found in Code.Description</param>
        /// <returns>The first code object that matches the search string</returns>
        public virtual ValueCodeType GetValueCodeByDescriptionExact(string description)
        {
            var code = GetCodeByDescriptionExact(description);
            if (code != null)
                return code.GetValueCodeType();
            return null;
        }

        /// <summary>
        ///     Searches the table by description string and returns the first occurrence whose description contains the search
        ///     string.
        ///     Search is case insensitive.
        /// </summary>
        /// <param name="description">The description string to be found in Code.Description</param>
        /// <returns>The first code object that matches the search string</returns>
        public virtual Code GetCodeByDescriptionContains(string description)
        {
            Code result = null;
            if (!string.IsNullOrEmpty(description))
            {
                var searchString = description.ToLower();
                if (!_codeOfContainingDescription.TryGetValue(searchString, out result))
                    _codeOfContainingDescription[searchString] =
                        result = GetCodeByDescriptionContains(Codes, searchString);
            }

            return result;
        }

        /// <summary>
        ///     Searches the table by description string and returns the first occurrence whose description starts with the search
        ///     string.
        ///     Search is case insensitive.
        /// </summary>
        /// <param name="description">The description string to be found in Code.Description</param>
        /// <returns>The first code object that matches the search string</returns>
        public virtual Code GetCodeByDescriptionStartsWith(string description)
        {
            Code result = null;
            if (!string.IsNullOrEmpty(description))
            {
                var searchString = description.ToLower();
                if (!_codeOfStartsWithDescription.TryGetValue(searchString, out result))
                    _codeOfStartsWithDescription[searchString] =
                        result = GetCodeByDescriptionStartsWith(Codes, searchString);
            }

            return result;
        }

        /// <summary>
        ///     Searches the table by description text and returns the first occurrence whose short description matches the search
        ///     string exactly.
        ///     Searches case insensitive.
        /// </summary>
        /// <returns>The first code object that matches the search string</returns>
        public virtual Code GetCodeByShortDescriptionExact(string shortDescription)
        {
            Code result = null;
            if (string.IsNullOrEmpty(shortDescription) ||
                _codeOfExactShortDescription.TryGetValue(shortDescription, out result)) return result;
            IEnumerable<Code> ieCodes = from Code c in Codes
                where String.Compare(c.ShortDescription, shortDescription, StringComparison.OrdinalIgnoreCase) == 0
                select c;

            if (ieCodes.Any())
                _codeOfStartsWithDescription[shortDescription] = result = ieCodes.First();

            return result;
        }

        /// <summary>
        ///     Searches the table by description text and returns the first occurrence whose ValueCodeText matches the search
        ///     string exactly.
        ///     Searches case insensitive.
        /// </summary>
        /// <returns>The first code object that matches the search string</returns>
        public virtual Code GetCodeByValueCodeTextExact(string searchString)
        {
            Code result = null;
            try
            {
                if (!string.IsNullOrEmpty(searchString) && !_codeOfExactValueCode.TryGetValue(searchString, out result))
                {
                    //IEnumerable<Code> ieCodes = from Code c in this.Codes
                    //							where String.Compare(c.ValueCodeText, searchString, true) == 0
                    //							select c;

                    //result = ieCodes.FirstOrDefault();
                    result = GetCodeByValueCodeTextExact(Codes, searchString);
                    if (result != null)
                        _codeOfExactValueCode[searchString] = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ProjectCodeTable.GetCodeByValueCodeTextExact: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        ///     Searches the table by description text and returns the first occurrence whose ValueCodeText matches the search
        ///     string exactly.
        ///     Searches case insensitive.
        /// </summary>
        /// <returns>The first code object that matches the search string</returns>
        public static Code GetCodeByValueCodeTextExact(IQueryable<Code> codesToQuery, string searchString)
        {
            Code result = null;
            try
            {
                if (
                    !string.IsNullOrEmpty(
                        searchString) /*&& !this._codeOfExactValueCode.TryGetValue(searchString, out result)*/)
                {
                    IEnumerable<Code> ieCodes = from Code c in codesToQuery
                                                where string.Compare(c.ValueCodeText, searchString, StringComparison.OrdinalIgnoreCase) == 0
                                                select c;

                    result = ieCodes.FirstOrDefault();
                    //if (result != null)
                    //	this._codeOfExactValueCode[searchString] = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ProjectCodeTable.GetCodeByValueCodeTextExact: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        ///     Gets the first code whose speficied property value equals the given value (By Object.Equals(object obj)).
        /// </summary>
        /// <param name="propertyName">Name of the property to be compared with the value</param>
        /// <param name="value">Value to be compared (By Object.Equals(object obj))</param>
        /// <returns>The first occurrence whose property value equals to the value.</returns>
        //public virtual Code GetCodeByProperty(string propertyName, object value)
        //{
        //    return GetCodeByProperty(CodesEnumerable, propertyName, value);
        //    //if (value is string)
        //    //{
        //    //	return this.GetCodeByProperty(propertyName, value as string);
        //    //}
        //    //Code result = null;
        //    //object propertyValueOut;
        //    //if (value != null)
        //    //{
        //    //	IEnumerable<Code> q = from Code code in this.CodesEnumerable
        //    //						  where code.TryGetPropertyValue(propertyName, out propertyValueOut) && propertyValueOut.Equals(value)
        //    //						  select code;
        //    //	if (q != null && q.Count() > 0)
        //    //		result = q.First();
        //    //}
        //    //return result;
        //}
        public virtual Code GetCodeByProperty(string propertyName, string value)
        {
            return GetCodeByProperty(propertyName, value, true);
        }

        public virtual Code GetCodeByProperty(string propertyName, string value, bool ignoreCase)
        {
            return GetCodeByProperty(CodesEnumerable, propertyName, value, ignoreCase);
            //if (value != null)
            //{
            //	IEnumerable<Code> q = from Code code in this.CodesEnumerable
            //						  let propertyValueOut = code.GetPropertyValue(propertyName) as string
            //						  where !string.IsNullOrEmpty(propertyValueOut) && string.Compare(value, propertyValueOut, ignoreCase) == 0
            //						  select code;

            //	if (q != null && q.Count() > 0)
            //		result = q.First();
            //}
            //return result;
        }

        private Code GetCodeByProperty(List<Code> codesEnumerable, string propertyName, string value, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        #region Same methods returning casted type specific objects

        public TCode GetByDescriptionContains(string description)
        {
            return GetCodeByDescriptionContains(description) as TCode;
        }

        public TCode GetByDescriptionExact(string description)
        {
            return GetCodeByDescriptionExact(description) as TCode;
        }

        public TCode GetByDescriptionStartsWith(string description)
        {
            return GetCodeByDescriptionStartsWith(description) as TCode;
        }

        public TCode GetByShortDescriptionExact(string shortDescription)
        {
            return GetCodeByShortDescriptionExact(shortDescription) as TCode;
        }

        public TCode GetByValueCodeTextExact(string searchString)
        {
            return GetCodeByValueCodeTextExact(searchString) as TCode;
        }

        public TCode GetProperty(string propertyName, object value)
        {
            return GetCodeByProperty(propertyName, value) as TCode;
        }

        #endregion

        #endregion

        #endregion

        #region Add/Remove/Save

        public void Add(Code newCode)
        {
            Add(newCode, true);
        }

        public void AddRange(IEnumerable<Code> newCodes)
        {
            if (newCodes != null)
            {
                foreach (var code in newCodes)
                    Add(code, false);
                UpdateLists();
            }
        }

        private void Add(Code newCode, bool updateLists)
        {
            var cacheTable = CacheDatabaseTable;
            if (newCode != null)
            {
                if (cacheTable != null && IsCacheDataBaseTable)
                {
                    // insert into cache db
                    cacheTable.InsertOnSubmit(newCode);
                    cacheTable.Context.SubmitChanges();
                }
                else
                {
                    // insert into dictionary
                    var codeDict = CodeDictionary;
                    if (codeDict != null)
                    {
                        // first remove existing code with same ID
                        codeDict.TryRemove(newCode.Id, out _);
                        // add the new or changed code to the dictionary
                        codeDict.TryAdd(newCode.Id, newCode);
                    }
                }

                newCode.SetParentTable(this as ICodeTable);
                if (updateLists)
                    UpdateLists();
            }
        }

        public void Remove(Code codeToBeRemoved)
        {
            if (codeToBeRemoved != null)
            {
                var cacheTable = CacheDatabaseTable;
                if (cacheTable != null && IsCacheDataBaseTable)
                {
                    // remove from cache db
                    cacheTable.DeleteOnSubmit(codeToBeRemoved);
                    cacheTable.Context.SubmitChanges();
                }
                else
                {
                    // remove from dictionary
                    if (CodeDictionary != null && CodeDictionary.ContainsKey(codeToBeRemoved.Id))
                        CodeDictionary.TryRemove(codeToBeRemoved.Id, out _);
                }

                UpdateLists();
            }
        }

        /// <summary>
        ///     Saves pending changes
        /// </summary>
        public void Save()
        {
            var cacheTable = CacheDatabaseTable;
            if (cacheTable != null && IsCacheDataBaseTable)
                cacheTable.Context.SubmitChanges();
        }

        #endregion

        #region Overwrite Methods

        /// <summary>
        ///     Overwrites the Value of the provided ValueCodeType with the corresponding value, compared by ID.
        /// </summary>
        /// <param name="valueCode">The ValueCodeType to be overwritten</param>
        public void OverwriteValueCode(ValueCodeType valueCode)
        {
            OverwriteValueCodeStatic(Codes, valueCode, this);
            //if (valueCode != null)
            //{
            //	if (valueCode.IsEmpty && valueCode.HasID)
            //	{
            //		// if an ID is set but no Value, try to find the corresponding value
            //		Code code = this.GetCode(valueCode);
            //		if (code != null)
            //			valueCode.Overwrite(code.GetValueCodeType());
            //		else
            //			valueCode.Id = null;
            //	}
            //	else if (!valueCode.IsEmpty && !valueCode.HasID)
            //	{
            //		// if a value is set but no ID, try to find the corresponding ID
            //		Code code = this.GetCodeByValueCodeTextExact(valueCode.Value);
            //		if (code != null)
            //			valueCode.Overwrite(code.GetValueCodeType());
            //	}
            //}
        }

        public void OverwriteValueCode(IQueryable<TCode> codesToQuery, ValueCodeType valueCode)
        {
            OverwriteValueCodeStatic(codesToQuery.Cast<Code>(), valueCode, null);
        }

        public static void OverwriteValueCodeStatic(IQueryable<Code> codesToQuery, ValueCodeType valueCode,
            CodeTable<TCode> table)
        {
            Code code;
            if (valueCode != null && codesToQuery != null)
            {
                if (valueCode.IsEmpty && TryGetCode(codesToQuery, valueCode, out code))
                {
                    // if an ID is set but no Value, try to find the corresponding value
                    valueCode.Overwrite(code.GetValueCodeType());
                }
                else if (!valueCode.IsEmpty && !valueCode.HasId)
                {
                    // if a value is set but no ID, try to find the corresponding ID
                    if (table != null)
                        code = table.GetCodeByValueCodeTextExact(valueCode.Value);
                    else
                        code = GetCodeByValueCodeTextExact(codesToQuery, valueCode.Value);
                    if (code != null)
                        valueCode.Overwrite(code.GetValueCodeType());
                }
            }
        }

        /// <summary>
        ///     Overwrites the Values of the provided ValueCodeTypes with the corresponding value, compared by ID.
        /// </summary>
        /// <param name="valueCodes">The collection of ValueCodeTypes to be overwritten</param>
        public void OverwriteValueCode(IList<ValueCodeType> valueCodes)
        {
            if (valueCodes == null) return;
            foreach (var vc in new List<ValueCodeType>(valueCodes))
            {
                OverwriteValueCode(vc);
                if (!vc.HasId && vc.IsEmpty)
                    valueCodes.Remove(vc);
            }
        }

        /// <summary>
        ///     Overwrites the Values of the provided ValueCodeOptionalType with the corresponding value, compared by ID.
        /// </summary>
        /// <param name="valueCodes">The collection of ValueCodeOptionalType to be overwritten</param>
        public void OverwriteValueCode(IList<ValueCodeOptionalType> valueCodes)
        {
            if (valueCodes != null)
                //List<ValueCodeOptionalType> codesToRemove = new List<ValueCodeOptionalType>();
                foreach (var vc in new List<ValueCodeOptionalType>(valueCodes))
                {
                    OverwriteValueCode(vc);
                    if (!vc.HasId && vc.IsEmpty)
                        valueCodes.Remove(vc);
                }
            //foreach (ValueCodeOptionalType vc in codesToRemove)
            //	valueCodes.Remove(vc);
        }

        #endregion

        public void SetIsDownloaded(bool isDownloaded)
        {
            IsDownloaded = isDownloaded;
        }

        /// <summary>
        ///     Destroys all cached code views to force recreation when accessed the next time.
        /// </summary>
        public void UpdateLists()
        {
            _codes = null;
            _codesCasted = null;
            _codesEnumerable = null;
            _codesCastedEnumerable = null;
            _codesActiveOnly = null;
            _codesCastedActiveOnly = null;
            _codesActiveOnlySortedBySortKey = null;
            _codesCastedActiveOnlySortedBySortKey = null;
            _codeOfId.Clear();
            _codeOfExactDescription.Clear();
            _codeOfExactShortDescription.Clear();
            _codeOfExactValueCode.Clear();
            _codeOfContainingDescription.Clear();
            _codeOfStartsWithDescription.Clear();
            RaisePropertyChanged($"Codes");
            updateLists();
        }

        // ReSharper disable once InconsistentNaming
        protected virtual void updateLists()
        {
        }

        /// <summary>
        ///     Gets the class type of the contained elements from the specified table type.
        /// </summary>
        /// <param name="tableType">The Type of the table</param>
        /// <returns>The Type of the contained elements</returns>
        public static Type GetContainedType(Type tableType)
        {
            Type result;
            CodeTable<Code>.ContainedTypeOfTableType.TryGetValue(tableType, out result);
            if (Debugger.IsAttached && ContainedTypeOfTableType.Count > 0 && result == null)
                throw new Exception(string.Format(
                    "Table Type {0} is not listed in dictionary _containedTypeOfTableType in class CodeTable<TCode>",
                    tableType.Name));
            return result;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            _instances.Remove(this);
            if ( /*!ModuleControllerBase.IsExitingApplication &&*/ !IsDisposed)
            {
                if (_codesInMemory != null)
                {
                    //_codesInMemory.Values.ToList().ForEach(c => c.DisposeDirectly());
                    _codesInMemory.Clear();
                    _codesInMemory = null;
                }

                if (CodesForSerialization != null)
                {
                }

                UpdateLists();
                IsDisposed = true;
            }
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}