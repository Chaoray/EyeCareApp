using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace EyeCareApp {
    internal class RegistryManager {
        RegistryKey workingKey;
        Dictionary<string, string> nv = new Dictionary<string, string>() { };

        public RegistryManager(RegistryKey root, string rpath, bool createIfNExist) {
            setWorkingSubKey(root, rpath, createIfNExist);
            storeAllKeys();
        }

        ~RegistryManager() {
            if (workingKey != null) workingKey.Close();
        }

        public void setWorkingSubKey(RegistryKey root, string rpath, bool createIfNExist) {
            if (workingKey != null) workingKey.Close();
            if (createIfNExist) {
                workingKey = root.CreateSubKey(rpath, true);
            } else {
                workingKey = root.OpenSubKey(rpath, true);
            }
        }

        public void storeAllKeys() {
            if (workingKey == null) {
                throw new Exception("Specific SubKey Not Found, Try to pass true on parameter \"createIfNExist\"");
            }

            nv.Clear();
            string[] names = workingKey.GetValueNames();
            foreach (string name in names) {
                object value = workingKey.GetValue(name);
                nv.Add(name, value.ToString());
            }
        }

        public void set(string name, string value) {
            writeToDict(name, value);
            workingKey.SetValue(name, value);
        }

        private void writeToDict(string name, string value) {
            nv[name] = value;
        }

        public void delete(string name) {
            workingKey.DeleteValue(name, false);
        }

        public bool has(string name) {
            return nv.ContainsKey(name);
        }

        public string this[string name] {
            get { return nv[name]; }
            private set { }
        }
    }
}
