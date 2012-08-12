using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Compilify.Utilities;
using Roslyn.Compilers;

namespace Compilify.LanguageServices
{
    [DataContract]
    public struct TextPosition : IEquatable<TextPosition>, IComparable<TextPosition>
    {
        public static readonly TextPosition Zero = new TextPosition(0, 0);
        private readonly int line;
        private readonly int character;

        private TextPosition(int line, int character)
        {
            this.line = line;
            this.character = character;
        }
        
        [DataMember]
        public int Line
        {
            get { return line; }
        }
        
        [DataMember]
        public int Character
        {
            get { return character; }
        }
        
        internal static TextPosition Create(LinePosition position)
        {
            return new TextPosition(position.Line, position.Character);
        }
        
        public static TextPosition Create(int line, int character)
        {
            if (line < 0)
            {
                throw new ArgumentOutOfRangeException("line cannot be less than 0", line, "line");
            }
            
            if (character < 0)
            {
                throw new ArgumentOutOfRangeException("character cannot be less than 0", character, "character");
            }

            return new TextPosition(line, character);
        }
        
        public static bool operator ==(TextPosition left, TextPosition right)
        {
            return EqualityComparer<TextPosition>.Default.Equals(left, right);
        }

        public static bool operator !=(TextPosition left, TextPosition right)
        {
            return !(left == right);
        }

        public int CompareTo(TextPosition other)
        {
            int result;
            if ((result = line.CompareTo(other.line)) != 0 || (result = character.CompareTo(other.character)) != 0)
            {
                return result;
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is TextPosition))
            {
                return false;
            }

            return Equals((TextPosition)obj);
        }

        public bool Equals(TextPosition other)
        {
            return line == other.line && character == other.character;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(character, line);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}, {1})", line, character);
        }
    }
}
