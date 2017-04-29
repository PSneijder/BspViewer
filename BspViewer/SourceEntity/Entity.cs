using System.Collections.Generic;

namespace BspViewer
{
    public sealed class Entity
        : Dictionary<string, string>
    {
        public const char ConnectionMemberSeparater = (char)0x1B;

        public Entity(string[] lines)
        {
            int braceCount = 0;

            bool inConnections = false;
            bool inBrush = false;

            List<string> child = new List<string>();

            foreach (string line in lines)
            {
                string current = line.Trim(' ', '\t', '\r');

                // Cull everything after a //
                bool inQuotes = false;

                for (int i = 0; i < current.Length; ++i)
                {
                    if (current[i] == '\"' && (i == 0 || current[i - 1] != '\\'))
                    {
                        inQuotes = !inQuotes;
                    }

                    if (!inQuotes && current[i] == '/' && i != 0 && current[i - 1] == '/')
                    {
                        current = current.Substring(0, i - 1);
                    }
                }

                if (string.IsNullOrEmpty(current))
                {
                    continue;
                }

                // Perhaps I should not assume these will always be the first thing on the line
                if (current[0] == '{')
                {
                    // If we're only one brace deep, and we have no prior information, assume a brush
                    if (braceCount == 1 && !inBrush && !inConnections)
                    {
                        inBrush = true;
                    }
                    ++braceCount;
                }
                else if (current[0] == '}')
                {
                    --braceCount;
                    // If this is the end of an entity substructure
                    if (braceCount == 1)
                    {
                        // If we determined we were inside a brush substructure
                        if (inBrush)
                        {
                            child.Add(current);
                            //brushes.Add(new MAPBrush(child.ToArray())); // TODO
                            child = new List<string>();
                        }
                        inBrush = false;
                        inConnections = false;
                    }
                    else
                    {
                        child.Add(current);
                    }
                    continue;
                }
                else if (current.Length >= 5 && current.Substring(0, 5) == "solid")
                {
                    inBrush = true;
                    continue;
                }
                else if (current.Length >= 11 && current.Substring(0, 11) == "connections")
                {
                    inConnections = true;
                    continue;
                }

                if (inBrush)
                {
                    child.Add(current);
                    continue;
                }

                Add(current);
            }

            void Add(string st)
            {
                string key = "";
                string val = "";
                bool inQuotes = false;
                bool isVal = false;
                int numCommas = 0;
                for (int i = 0; i < st.Length; ++i)
                {
                    // Some entity values in Source can use escape sequenced quotes. Need to make sure not to parse those.
                    if (st[i] == '\"' && (i == 0 || st[i - 1] != '\\'))
                    {
                        if (inQuotes)
                        {
                            if (isVal)
                            {
                                break;
                            }
                            isVal = true;
                        }
                        inQuotes = !inQuotes;
                    }
                    else
                    {
                        if (inQuotes)
                        {
                            if (!isVal)
                            {
                                key += st[i];
                            }
                            else
                            {
                                val += st[i];
                                if (st[i] == ',' || st[i] == ConnectionMemberSeparater) { ++numCommas; }
                            }
                        }
                    }
                }
                val.Replace("\\\"", "\"");
                if (key != null && key != "")
                {
                    if (numCommas == 4 || numCommas == 6)
                    {
                        st = st.Replace(',', ConnectionMemberSeparater);
                        string[] connection = val.Split(',');
                        if (connection.Length < 5)
                        {
                            connection = val.Split((char)0x1B);
                        }
                        if (connection.Length == 5 || connection.Length == 7)
                        {
                            // TODO
                            //connections.Add(new EntityConnection
                            //{
                            //    name = key,
                            //    target = connection[0],
                            //    action = connection[1],
                            //    param = connection[2],
                            //    delay = Double.Parse(connection[3], _format),
                            //    fireOnce = Int32.Parse(connection[4]),
                            //    unknown0 = connection.Length > 5 ? connection[5] : "",
                            //    unknown1 = connection.Length > 6 ? connection[6] : "",
                            //});
                        }
                    }
                    else
                    {
                        if (!ContainsKey(key))
                        {
                            this[key] = val;
                        }
                    }
                }
            }
        }

        public static Entity FromString(string value)
        {
            return new Entity(value.Split('\n'));
        }

        public new string this[string key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                else
                {
                    return "";
                }
            }
            set { base[key] = value; }
        }
    }
}
