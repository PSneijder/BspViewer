using System;
using System.Collections.Generic;

namespace BspViewer
{
    sealed class EntityParser
        : List<Entity>
    {
        private byte[] _data;

        public EntityParser(byte[] data)
        {
            _data = data;
        }

        public Entities Parse()
        {
            // Keep track of whether or not we're currently in a set of quotation marks.
            // I came across a map where the idiot map maker used { and } within a value. This broke the code before.
            bool inQuotes = false;
            int braceCount = 0;

            // The current character being read in the file. This is necessary because
            // we need to know exactly when the { and } characters occur and capture
            // all text between them.
            char currentChar;
            // This will be the resulting entity, fed into Entity.FromString
            System.Text.StringBuilder current = new System.Text.StringBuilder();

            for (int offset = 0; offset < _data.Length; ++offset)
            {
                currentChar = (char) _data[offset];

                // Allow for escape-sequenced quotes to not affect the state machine.
                if (currentChar == '\"' && (offset == 0 || (char) _data[offset - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }

                if (!inQuotes)
                {
                    if (currentChar == '{')
                    {
                        // Occasionally, texture paths have been known to contain { or }. Since these aren't always contained
                        // in quotes, we must be a little more precise about how we want to select our delimiters.
                        // As a general rule, though, making sure we're not in quotes is still very effective at error prevention.
                        if (offset == 0 || (char) _data[offset - 1] == '\n' || (char) _data[offset - 1] == '\t' || (char) _data[offset - 1] == ' ' || (char) _data[offset - 1] == '\r')
                        {
                            ++braceCount;
                        }
                    }
                }

                if (braceCount > 0)
                {
                    current.Append(currentChar);
                }

                if (!inQuotes)
                {
                    if (currentChar == '}')
                    {
                        if (offset == 0 || (char) _data[offset - 1] == '\n' || (char) _data[offset - 1] == '\t' || (char) _data[offset - 1] == ' ' || (char) _data[offset - 1] == '\r')
                        {
                            --braceCount;
                            if (braceCount == 0)
                            {
                                Add(Entity.FromString(current.ToString()));

                                // Reset StringBuilder
                                current.Length = 0;
                            }
                        }
                    }
                }
            }

            if (braceCount != 0)
            {
                throw new ArgumentException(string.Format("Brace mismatch when parsing entities! Brace level: {0}", braceCount));
            }
            
            return new Entities(this);
        }
    }
}