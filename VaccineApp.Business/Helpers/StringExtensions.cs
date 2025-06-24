using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions; 

namespace System
{
    /// <summary>
    /// Extension methods for String class.
    /// </summary>
    public static class DmsStringExtensions
    {
        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            //Check.NotNull(str, nameof(str));

            if (str.EndsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            //Check.NotNull(str, nameof(str));

            if (str.StartsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return c + str;
        }

        public static string EnsureEndsWith(this string str, string exStr, StringComparison comparisonType = StringComparison.Ordinal)
        {
            //Check.NotNull(str, nameof(str));

            if (str.EndsWith(exStr, comparisonType))
            {
                return str;
            }

            return str + exStr;
        }

        public static string ToLowerOrNull(this string str)
        {
            if (str == null)
            {
                return str;
            }

            return str.ToLower();
        }

        public static string ToUpperOrNull(this string str)
        {
            if (str == null)
            {
                return str;
            }

            return str.ToUpper();
        }


        public static string TrimOrNull(this string str)
        {
            if (str == null)
            {
                return str;
            }

            return str.Trim();
        }


        public static string TrimOrNull(this string str, char trimChar)
        {
            if (str == null)
            {
                return str;
            }

            return str.Trim(trimChar);
        }

        public static string RemoveMultipleSpacesOrNull(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            return Regex.Replace(str, " +", " ");
        }
        /// <summary>
        /// Indicates whether this string is null or an System.String.Empty string.
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Indicates whether this string is null or an System.String.Empty string.
        /// </summary>
        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }


        /// <summary>
        /// indicates whether this string is null, empty, or consists only of white-space characters.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// indicates whether this string is null, empty, or consists only of white-space characters.
        /// </summary>
        public static bool IsNotNullOrWhiteSpace(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Left(this string str, int len)
        {
            //Check.NotNull(str, nameof(str));

            if (str.Length <= len)
            {
                return str;
            }

            return str.Substring(0, len);
        }

        /// <summary>
        /// Converts line endings in the string to <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Gets index of nth occurence of a char in a string.
        /// </summary>
        /// <param name="str">source string to be searched</param>
        /// <param name="c">Char to search in <see cref="str"/></param>
        /// <param name="n">Count of the occurence</param>
        public static int NthIndexOf(this string str, char c, int n)
        {
            //Check.NotNull(str, nameof(str));

            var count = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != c)
                {
                    continue;
                }

                if ((++count) == n)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes first occurrence of the given postfixes from end of the given string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="postFixes">one or more postfix.</param>
        /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            return str.RemovePostFix(StringComparison.Ordinal, postFixes);
        }

        /// <summary>
        /// Removes first occurrence of the given postfixes from end of the given string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="comparisonType">String comparison type</param>
        /// <param name="postFixes">one or more postfix.</param>
        /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
        public static string RemovePostFix(this string str, StringComparison comparisonType, params string[] postFixes)
        {
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (postFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix, comparisonType))
                {
                    return str.Left(str.Length - postFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Removes first occurrence of the given prefixes from beginning of the given string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="preFixes">one or more prefix.</param>
        /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
        public static string RemovePreFix(this string str, params string[] preFixes)
        {
            return str.RemovePreFix(StringComparison.Ordinal, preFixes);
        }

        /// <summary>
        /// Removes first occurrence of the given prefixes from beginning of the given string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="comparisonType">String comparison type</param>
        /// <param name="preFixes">one or more prefix.</param>
        /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
        public static string RemovePreFix(this string str, StringComparison comparisonType, params string[] preFixes)
        {
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (preFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix, comparisonType))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        public static string ReplaceFirst(this string str, string search, string replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            //Check.NotNull(str, nameof(str));

            var pos = str.IndexOf(search, comparisonType);
            if (pos < 0)
            {
                return str;
            }

            return str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string str, string search, string replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            //Check.NotNull(str, nameof(str));

            var pos = str.LastIndexOf(search, comparisonType);
            if (pos < 0)
            {
                return str;
            }

            return str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
        }

        /// <summary>
        /// Gets a substring of a string from end of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Right(this string str, int len)
        {
            //Check.NotNull(str, nameof(str));

            if (str.Length < len)
            {
                return str;
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// Uses string.Split method to split given string by given separator.
        /// </summary>
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Uses string.Split method to split given string by given separator.
        /// </summary>
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new[] { separator }, options);
        }

        /// <summary>
        /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string[] SplitToLines(this string str)
        {
            return str.Split(Environment.NewLine);
        }

        /// <summary>
        /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string[] SplitToLines(this string str, StringSplitOptions options)
        {
            return str.Split(Environment.NewLine, options);
        }

        /// <summary>
        /// Converts PascalCase string to camelCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="useCurrentCulture">set true to use current culture. Otherwise, invariant culture will be used.</param>
        /// <returns>camelCase of the string</returns>
        public static string ToCamelCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return useCurrentCulture ? str.ToLower() : str.ToLowerInvariant();
            }

            return (useCurrentCulture ? char.ToLower(str[0]) : char.ToLowerInvariant(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
        /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="useCurrentCulture">set true to use current culture. Otherwise, invariant culture will be used.</param>
        public static string ToSentenceCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return useCurrentCulture
                ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]))
                : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLowerInvariant(m.Value[1]));
        }

        public static string ToMd5(this string str)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(str);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts camelCase string to PascalCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="useCurrentCulture">set true to use current culture. Otherwise, invariant culture will be used.</param>
        /// <returns>PascalCase of the string</returns>
        public static string ToPascalCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return useCurrentCulture ? str.ToUpper() : str.ToUpperInvariant();
            }

            return (useCurrentCulture ? char.ToUpper(str[0]) : char.ToUpperInvariant(str[0])) + str.Substring(1);
        }

        public static string ToKebabCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            str = str.ToCamelCase();

            return useCurrentCulture
                ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + "-" + char.ToLower(m.Value[1]))
                : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + "-" + char.ToLowerInvariant(m.Value[1]));
        }

        /// <summary>
        /// Converts camelCase string to PascalCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="useCurrentCulture">set true to use current culture. Otherwise, invariant culture will be used.</param>
        /// <returns>PascalCase of the string</returns>
        public static string ToCapitalizeEachWord(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return useCurrentCulture ? str.ToUpper() : str.ToUpperInvariant();
            }

            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string result = "";

            foreach (var w in words)
            {

                result += (useCurrentCulture ? char.ToUpper(w[0]) : char.ToUpperInvariant(w[0])) + w.Substring(1) + " ";
            }

            return result.Trim();
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string Truncate(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Left(maxLength);
        }

        /// <summary>
        /// Gets a substring of a string from Ending of the string if it exceeds maximum length.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateFromBeginning(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Right(maxLength);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// It adds a "..." postfix to end of the string if it's truncated.
        /// Returning string can not be longer than maxLength.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateWithPostfix(this string str, int maxLength)
        {
            return TruncateWithPostfix(str, maxLength, "...");
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// It adds given <paramref name="postfix"/> to end of the string if it's truncated.
        /// Returning string can not be longer than maxLength.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
        {
            if (str == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(str) || maxLength == 0)
            {
                return string.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            if (maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            return str.Left(maxLength - postfix.Length) + postfix;
        }

        /// <summary>
        /// Converts given string to a byte array using iso-8859-9 encoding.
        /// </summary>
        public static byte[] GetBytes(this string str)
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return str.GetBytes(Encoding.UTF8);
        }

        /// <summary>
        /// Converts given string to a byte array using iso-8859-9 encoding.
        /// </summary>
        public static byte[] GetIso8859Bytes(this string str)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return str.GetBytes(Encoding.GetEncoding("iso-8859-9"));
        }

        /// <summary>
        /// Converts given string to a byte array using the given <paramref name="encoding"/>
        /// </summary>
        public static byte[] GetBytes( this string str,  Encoding encoding)
        {
            //Check.NotNull(str, nameof(str));
            //Check.NotNull(encoding, nameof(encoding));
            return encoding.GetBytes(str);
        }

        public static bool FromBase64(this string str, out byte[] array)
        {
            array = null;
            try
            {
                array = Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ToUrlEncodeBase64(this string base64Str)
        {
            if (base64Str == null)
            {
                return base64Str;
            }
            return base64Str.Replace('+', '.').Replace('/', '_').Replace('=', '-');
        }

        public static string ToUrlDecodeBase64(this string base64Str)
        {
            if (base64Str == null)
            {
                return base64Str;
            }
            return base64Str.Replace('.', '+').Replace('_', '/').Replace('-', '=');
        }

        public static (string Name, string MiddleName, string Surname) ParseFullName(this string fullname)
        {
            string name = "";
            string surName = "";
            string middleName = "";
            if (!string.IsNullOrEmpty(fullname))
            {
                var words = fullname.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                name = words.FirstOrDefault() ?? "";

                surName = string.Join(" ", words.Skip(1).Where(p => p.All(q => Char.IsUpper(q))));
                if (string.IsNullOrEmpty(surName) && words.Count() > 0)
                {
                    surName = words.Skip(1).LastOrDefault();
                }

                if (!string.IsNullOrEmpty(name))
                {
                    middleName = fullname.Replace(name, "");
                }
                if (!string.IsNullOrEmpty(surName))
                {
                    middleName = middleName.Replace(surName, "");
                }
                middleName = string.Join(" ", middleName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            }


            return (name, middleName, surName);
        }

        public static string NormalizeTurkishChars(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            var normalizedStr = str;
            char[] turkishChars = new char[] { 'ı', 'İ', 'ü', 'Ü', 'ö', 'Ö', 'ş', 'Ş', 'ç', 'Ç', 'ğ', 'Ğ' };
            char[] englishChars = new char[] { 'i', 'I', 'u', 'U', 'o', 'O', 's', 'S', 'c', 'C', 'g', 'G' };
            for (int i = 0; i < turkishChars.Length; i++)
            {
                normalizedStr = normalizedStr.Replace(turkishChars[i], englishChars[i]);
            }
            return normalizedStr;
        }

        public static string ReplaceHexadecimalSymbols(this string str, string replacement = "")
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            var regex = "[\x00-\x08\x0B\x0C\x0E-\x1F]";
            return Regex.Replace(str, regex, replacement, RegexOptions.Compiled);
        }

    }
}