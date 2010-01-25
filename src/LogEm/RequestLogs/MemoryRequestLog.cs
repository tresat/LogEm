using System;
using ReaderWriterLock = System.Threading.ReaderWriterLock;
using Timeout = System.Threading.Timeout;
using NameObjectCollectionBase = System.Collections.Specialized.NameObjectCollectionBase;
using IList = System.Collections.IList;
using IDictionary = System.Collections.IDictionary;
using CultureInfo = System.Globalization.CultureInfo;

namespace LogEm
{
    /// <summary>
    /// An <see cref="ErrorLog"/> implementation that uses memory as its 
    /// backing store. 
    /// </summary>
    /// <remarks>
    /// All <see cref="MemoryRequestLog"/> instances will share the same memory 
    /// store that is bound to the application (not an instance of this class).
    /// </remarks>

    public sealed class MemoryRequestLog : RequestLog
    {
        //
        // The collection that provides the actual storage for this log
        // implementation and a lock to guarantee concurrency correctness.
        //

        private static EntryCollection _entries;
        private readonly static ReaderWriterLock _lock = new ReaderWriterLock();

        //
        // IMPORTANT! The size must be the same for all instances
        // for the entires collection to be intialized correctly.
        //

        private readonly int _size;

        /// <summary>
        /// The maximum number of errors that will ever be allowed to be stored
        /// in memory.
        /// </summary>

        public static readonly int MaximumSize = 500;

        /// <summary>
        /// The maximum number of errors that will be held in memory by default 
        /// if no size is specified.
        /// </summary>

        public static readonly int DefaultSize = 15;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRequestLog"/> class
        /// with a default size for maximum recordable entries.
        /// </summary>

        public MemoryRequestLog() : this(DefaultSize) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRequestLog"/> class
        /// with a specific size for maximum recordable entries.
        /// </summary>

        public MemoryRequestLog(int size)
        {
            if (size < 0 || size > MaximumSize)
                throw new ArgumentOutOfRangeException("size", size, string.Format("Size must be between 0 and {0}.", MaximumSize));

            _size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRequestLog"/> class
        /// using a dictionary of configured settings.
        /// </summary>

        public MemoryRequestLog(IDictionary config)
        {
            if (config == null)
            {
                _size = DefaultSize;
            }
            else
            {
                string sizeString = Mask.NullString((string)config["size"]);

                if (sizeString.Length == 0)
                {
                    _size = DefaultSize;
                }
                else
                {
                    _size = Convert.ToInt32(sizeString, CultureInfo.InvariantCulture);
                    _size = Math.Max(0, Math.Min(MaximumSize, _size));
                }
            }
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>

        public override string Name
        {
            get { return "In-Memory Error Log"; }
        }

        /// <summary>
        /// Logs an request to the application memory.
        /// </summary>
        /// <remarks>
        /// If the log is full then the oldest request entry is removed.
        /// </remarks>

        public override string Log(UserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //
            // Make a copy of the error to log since the source is mutable.
            // Assign a new GUID and create an entry for the error.
            //

            request = (UserRequest)((ICloneable)request).Clone();
            request.Application = this.ApplicationName;
            Guid newId = Guid.NewGuid();
            RequestLogEntry entry = new RequestLogEntry(this, newId.ToString(), request);

            _lock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (_entries == null)
                {
                    _entries = new EntryCollection(_size);
                }

                _entries.Add(entry);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }

            return newId.ToString();
        }

        /// <summary>
        /// Returns the specified request from application memory, or null 
        /// if it does not exist.
        /// </summary>

        public override RequestLogEntry GetRequest(string id)
        {
            _lock.AcquireReaderLock(Timeout.Infinite);

            RequestLogEntry entry;

            try
            {
                if (_entries == null)
                    return null;

                entry = _entries[id];
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            if (entry == null)
                return null;

            //
            // Return a copy that the caller can party on.
            //

            UserRequest request = (UserRequest)((ICloneable)entry.Request).Clone();
            return new RequestLogEntry(this, entry.Id, request);
        }

        /// <summary>
        /// Returns a page of errors from the application memory in
        /// descending order of logged time.
        /// </summary>

        public override int GetRequests(int pageIndex, int pageSize, IList errorEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            //
            // To minimize the time for which we hold the lock, we'll first
            // grab just references to the entries we need to return. Later,
            // we'll make copies and return those to the caller. Since Error 
            // is mutable, we don't want to return direct references to our 
            // internal versions since someone could change their state.
            //

            RequestLogEntry[] selectedEntries;
            int totalCount;

            _lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (_entries == null)
                    return 0;

                int lastIndex = Math.Max(0, _entries.Count - (pageIndex * pageSize)) - 1;
                selectedEntries = new RequestLogEntry[lastIndex + 1];

                int sourceIndex = lastIndex;
                int targetIndex = 0;

                while (sourceIndex >= 0)
                {
                    selectedEntries[targetIndex++] = _entries[sourceIndex--];
                }

                totalCount = _entries.Count;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            if (errorEntryList != null)
            {
                //
                // Return copies of fetched entries. If the Request class would 
                // be immutable then this step wouldn't be necessary.
                //

                foreach (RequestLogEntry entry in selectedEntries)
                {
                    UserRequest request = (UserRequest)((ICloneable)entry.Request).Clone();
                    errorEntryList.Add(new RequestLogEntry(this, entry.Id, request));
                }
            }
            return totalCount;
        }

        private class EntryCollection : NameObjectCollectionBase
        {
            private readonly int _size;

            public EntryCollection(int size)
                : base(size)
            {
                _size = size;
            }

            public RequestLogEntry this[int index]
            {
                get { return (RequestLogEntry)BaseGet(index); }
            }

            public RequestLogEntry this[Guid id]
            {
                get { return (RequestLogEntry)BaseGet(id.ToString()); }
            }

            public RequestLogEntry this[string id]
            {
                get { return this[new Guid(id)]; }
            }

            public void Add(RequestLogEntry entry)
            {
                Debug.Assert(entry != null);
                Debug.AssertStringNotEmpty(entry.Id);

                Debug.Assert(this.Count <= _size);

                if (this.Count == _size)
                {
                    BaseRemoveAt(0);
                }

                BaseAdd(entry.Id, entry);
            }
        }
    }
}
