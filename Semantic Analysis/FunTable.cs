using System;
using System.Text;
using System.Collections.Generic;

namespace Falak{
    public class FunTable: IEnumerable<KeyValuePair<string, List<object>>> {
        IDictionary<string, List<object>> data = new Dictionary<string, List<object>>();

        public List<object> this[string key] {
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

        public IEnumerator<KeyValuePair<string, List<object>>> GetEnumerator() {
            return data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Function Name | Primitive? | Arity | Ref              |\n");
            sb.Append("-*-*-*-*-*-*-*|-*-*-*-*-*-*|-*-*-*-|-*-*-*-*-*-*-*-*-*|\n");
            foreach (var entry in data) {
                sb.Append($"{entry.Key}\t\t");
                foreach(var obj in entry.Value){
                    sb.Append($"{obj}\t      ");
                }
                sb.Append("\n");
            }
            sb.Append("-*-*-*-*-*-*-*|-*-*-*-*-*-*|-*-*-*-|-*-*-*-*-*-*-*-*-*|\n");
            return sb.ToString();
        }


    }
    
}