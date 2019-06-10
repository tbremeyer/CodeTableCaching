namespace CodeTableCaching
{
    using System.ComponentModel;
    using System.Xml.Serialization;
    using System.Runtime.CompilerServices;

    namespace Core.Codes
    {
        [XmlType]
        public abstract class Code : INotifyPropertyChanged
        {
            #region Fields
            private string _id;
            private string _comment;
            private bool _isActive;
            private int _version;
            // ReSharper disable once InconsistentNaming
            protected string _description;
            // ReSharper disable once InconsistentNaming
            protected string _shortDescription;
            private double? _sortKey;
            #endregion

            protected Code()
            {
                _sortKey = null;
            }

            protected Code(string id, bool isActive, int version)
                : this()
            {
                _id = id;
                _isActive = isActive;
                _version = version;
            }

            protected Code(string id, bool isActive, string comment, int version)
                : this(id, isActive, version)
            {
                _comment = comment;
            }

            protected Code(string id, bool isActive, string comment, string description, int version)
                : this(id, isActive, comment, version)
            {
                _description = description;
            }

            protected Code(string id, bool isActive, string comment, string description, double? sortKey, int version)
                : this(id, isActive, comment, sortKey, version)
            {
                _description = description;
            }

            protected Code(string id, bool isActive, string comment, string description, string shortDescription, double? sortKey, int version)
                : this(id, isActive, comment, description, sortKey, version)
            {
                _shortDescription = shortDescription;
            }

            protected Code(string id, bool isActive, double? sortKey, int version)
                : this(id, isActive, version)
            {
                _sortKey = sortKey;
            }

            protected Code(string id, bool isActive, string comment, double? sortKey, int version)
                : this(id, isActive, comment, version)
            {
                _sortKey = sortKey;
            }

            public virtual string Id
            {
                get => _id;
                set
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }

            public virtual string Comment
            {
                get => _comment;
                set
                {
                    _comment = value;
                    RaisePropertyChanged();
                }
            }

            /// <summary>
            /// Gets or sets whether this Code in active or inactive (to be treated as deleted).
            /// </summary>
            public virtual bool IsActive
            {
                get
                {
                    return _isActive;
                }
                set
                {
                    _isActive = value;
                    RaisePropertyChanged();
                }
            }

            public virtual double? SortKey
            {
                get
                {
                    return _sortKey;
                }
                set
                {
                    _sortKey = value;
                    RaisePropertyChanged();
                }
            }

            [XmlIgnore]
            public bool SortKeySpecified => _sortKey != null;

            public virtual int Version
            {
                get => _version;
                set
                {
                    _version = value;
                    RaisePropertyChanged();
                }
            }

            [XmlIgnore]
            public virtual string Description
            {
                get
                {
                    return _description;
                }
                set
                {
                    _description = value;
                }
            }

            [XmlIgnore]
            public virtual string ShortDescription
            {
                get
                {
                    return _shortDescription;
                }
                set
                {
                    _shortDescription = value;
                }
            }

            [XmlIgnore]
            public virtual string ValueCodeText => Description;

            #region INotifyPropertyChanged Members
            public event PropertyChangedEventHandler PropertyChanged;
            #endregion

            protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
            }

            #region IOverwritable Members
            // ReSharper disable once UnusedMember.Global
            public virtual void Overwrite(object source)
            {
                var other = source as Code;
                if (other == null) return;
                _comment = other._comment;
                _id = other._id;
                _isActive = other._isActive;
                _sortKey = other._sortKey;
            }
            #endregion

            public override string ToString()
            {
                if (!string.IsNullOrEmpty(ValueCodeText))
                {
                    return (!string.IsNullOrEmpty(Id) ? "Id: " + Id + ", " : string.Empty) + ValueCodeText;
                }
                return string.Empty;
            }

            /// <summary>
            /// Destroys all cached code views to force recreation when accessed the next time.
            /// </summary>
            public void UpdateLists()
            {
                updateLists();
            }

            // ReSharper disable once InconsistentNaming
            protected virtual void updateLists()
            {
            }

            public void SetParentTable(ICodeTable ictThis)
            {
                
            }

            public ValueCodeType GetValueCodeType()
            {
               return new ValueCodeType(Id, ValueCodeText);
            }
        }
    }


}
