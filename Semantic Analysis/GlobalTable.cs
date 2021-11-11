using System;
using System.Text;
using System.Collections.Generic;

namespace Falak{
    public class GlobalTable: IEnumerable<KeyValuePair<string, string>> {
        IDictionary<string, string> data = new Dictionary<string, string>();

        public string this[string key] {
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

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        public override string ToString(){
            var sb = new StringBuilder();
            sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
            sb.Append("  Global Variables\n");
            sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
            if (data.Count > 0){
                foreach (var entry in data) {
                    sb.Append($"\t{entry.Key}\n");
                    sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
                }
            }else{
                sb.Append("No global variables\n");
                sb.Append("-*-*-*-*-*-*-*-*-*-*\n");
            }
            return sb.ToString();
        }


    }
    
}