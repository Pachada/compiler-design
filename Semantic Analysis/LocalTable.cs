using System;
using System.Text;
using System.Collections.Generic;

namespace Falak{
    public class LocalTable: IEnumerable<KeyValuePair<string, bool>> {
        IDictionary<string, bool> data = new Dictionary<string, bool>();

        public bool this[string key] {
            get {
                return data[key];
            }
            set {
                data[key] = value;
            }    
        }

        public bool Contains(string key) {
            return data.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, bool>> GetEnumerator() {
            return data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        public override string ToString(){
            var sb = new StringBuilder();
            sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
            if (data.Count > 0){
                foreach (var entry in data) {
                    sb.Append($"\t{entry.Key}\n");
                    sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
                }
            }else{
                sb.Append("No local variables\n");
                sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
            }
            return sb.ToString();
        }


    }
    
}