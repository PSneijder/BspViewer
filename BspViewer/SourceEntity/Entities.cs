using System.Collections.Generic;
using System.Linq;

namespace BspViewer
{
    public sealed class Entities
        : List<Entity>
    {
        public Entities(IEnumerable<Entity> entities)
            : base(entities) { }

        public Dictionary<string, string> this[string key]
        {
            get
            {
                int index = FindIndex(dict => dict.Values.Any(val => val == key));

                if(index != -1)
                {
                    return this[index];
                }

                return new Dictionary<string, string>();
            }
            set {  }
        }
    }
}