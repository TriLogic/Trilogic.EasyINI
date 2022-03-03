using System;
using System.Collections.Generic;
using System.IO;

namespace Trilogic.EasyINI
{
    public class IniGroup
    {
        #region Static Members
        private static char[] argSplitter = { '=' };
        #endregion

        #region Class Members
        protected Dictionary<String, IniSetting> store;
        protected string name = string.Empty;

        protected List<string> comments = new List<string>();
        #endregion

        #region Constructors and Destructors
        public IniGroup()
        {
            store = new Dictionary<string, IniSetting>();
        }
        public IniGroup(string name)
        {
            this.name = name;
        }
        public IniGroup(Dictionary<string, IniSetting> storage)
        {
            store = storage;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return store.Count; }
        }

        public bool Exists(string key)
        {
            return store.ContainsKey(KeyOf(key));
        }

        public IniSetting this[string key]
        {
            get { return store[KeyOf(key)]; }
            set { store[KeyOf(key)] = value; }
        }

        public void Clear()
        {
            store.Clear();
        }

        public Dictionary<string, IniSetting> Storage
        {
            get { return store; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<string> Comments
        {
            get { return comments; }
        }
        #endregion

        #region Class Methods
        public IniSetting Set(IniSetting setting)
        {
            string key = KeyOf(setting.Value);
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("invalid key");
            if (store.ContainsKey(key))
                store.Remove(key);
            store[key] = setting;
            return setting;
        }
        public IniSetting Set(string key, string value)
        {
            return Set(new IniSetting(key, value));
        }

        public IniSetting Get(string key, string defValue)
        {
            IniSetting s = Get(key);
            if (s != null)
                return s;
            return Set(new IniSetting(key, defValue));
        }

        public IniSetting Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("invalid key");
            string lowkey = KeyOf(key);
            if (store.ContainsKey(lowkey))
                return store[lowkey];
            return null;
        }

        public IniSetting Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("invalid key");
            string lowkey = KeyOf(key);
            if (store.ContainsKey(lowkey))
            {
                IniSetting value = store[lowkey];
                store.Remove(lowkey);
                return value;
            }
            return null;
        }
        #endregion

        #region Assert Operations
        public void Assert(string key, string msg)
        {
            if (!Exists(key))
                throw new IniException(msg);
        }
        public void AssertAB(string keyA, string keyB, string msg)
        {
            Assert(keyA, msg);
            Assert(keyB, msg);
        }
        public void AssertAorB(string keyA, string keyB, string msg)
        {
            if (Exists(keyA) || Exists(keyB))
                return;
            throw new IniException(msg);
        }
        public void AssertAxorB(string keyA, string keyB, string msg)
        {
            if (Exists(keyA) ^ Exists(keyB))
                return;
            throw new IniException(msg);
        }
        #endregion

        #region Key Generation
        public virtual string KeyOf(string key)
        {
            return key.Trim().ToLower();
        }
        #endregion

        #region IO Routines
        internal int Write(StreamWriter writer)
        {
            int count = comments.Count;

            foreach (string comment in comments)
                writer.WriteLine(";{0}", comment);

            if (!string.IsNullOrEmpty(name))
            {
                writer.WriteLine("[{0}]", name);
                count++;
            }

            foreach (IniSetting s in store.Values)
            {
                count += s.WriteToFile(writer);
            }

            return count;
        }
        #endregion
    }
}
