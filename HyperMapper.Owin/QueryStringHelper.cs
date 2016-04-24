﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HyperMapper.Owin
{

    // Written by Peter O. in 2013.
    // Any copyright is dedicated to the Public Domain.
    // http://creativecommons.org/publicdomain/zero/1.0/
    //
    // If you like this, you should donate to Peter O.
    // at: http://upokecenter.com/d/
    //
    
    public sealed class QueryStringHelper
    {
        private QueryStringHelper()
        {
        }

        private static string[] SplitAt(string s, string delimiter)
        {
            if (delimiter == null ||
                delimiter.Length == 0) throw new ArgumentException();
            if (s == null || s.Length == 0) return new string[] {""};
            int index = 0;
            bool first = true;
            List<string> strings = null;
            int delimLength = delimiter.Length;
            while (true)
            {
                int index2 = s.IndexOf(delimiter, index, StringComparison.Ordinal);
                if (index2 < 0)
                {
                    if (first) return new string[] {s};
                    strings.Add(s.Substring(index));
                    break;
                }
                else
                {
                    if (first)
                    {
                        strings = new List<string>();
                        first = false;
                    }
                    string newstr = s.Substring(index, (index2) - (index));
                    strings.Add(newstr);
                    index = index2 + delimLength;
                }
            }
            return strings.ToArray();
        }

        private static int ToHexNumber(int c)
        {
            if (c >= 'A' && c <= 'Z')
                return 10 + c - 'A';
            else if (c >= 'a' && c <= 'z')
                return 10 + c - 'a';
            else if (c >= '0' && c <= '9')
                return c - '0';
            return -1;
        }

        private static string PercentDecodeUTF8(string str)
        {
            int len = str.Length;
            bool percent = false;
            for (int i = 0; i < len; i++)
            {
                char c = str[i];
                if (c == '%')
                {
                    percent = true;
                }
                else if (c >= 0x80) // Non-ASCII characters not allowed
                    return null;
            }
            if (!percent) return str; // return early if there are no percent decodings
            int cp = 0;
            int bytesSeen = 0;
            int bytesNeeded = 0;
            int lower = 0x80;
            int upper = 0xBF;
            int markedPos = -1;
            StringBuilder retString = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                int c = str[i];
                if (c == '%')
                {
                    if (i + 2 < len)
                    {
                        int a = ToHexNumber(str[i + 1]);
                        int b = ToHexNumber(str[i + 2]);
                        if (a >= 0 && b >= 0)
                        {
                            b = (byte) (a*16 + b);
                            i += 2;
                            // b now contains the byte read
                            if (bytesNeeded == 0)
                            {
                                // this is the lead byte
                                if (b < 0x80)
                                {
                                    retString.Append((char) b);
                                    continue;
                                }
                                else if (b >= 0xc2 && b <= 0xdf)
                                {
                                    markedPos = i;
                                    bytesNeeded = 1;
                                    cp = b - 0xc0;
                                }
                                else if (b >= 0xe0 && b <= 0xef)
                                {
                                    markedPos = i;
                                    lower = (b == 0xe0) ? 0xa0 : 0x80;
                                    upper = (b == 0xed) ? 0x9f : 0xbf;
                                    bytesNeeded = 2;
                                    cp = b - 0xe0;
                                }
                                else if (b >= 0xf0 && b <= 0xf4)
                                {
                                    markedPos = i;
                                    lower = (b == 0xf0) ? 0x90 : 0x80;
                                    upper = (b == 0xf4) ? 0x8f : 0xbf;
                                    bytesNeeded = 3;
                                    cp = b - 0xf0;
                                }
                                else
                                {
                                    // illegal byte in UTF-8
                                    retString.Append('\uFFFD');
                                    continue;
                                }
                                cp <<= (6*bytesNeeded);
                                continue;
                            }
                            else
                            {
                                // this is a second or further byte
                                if (b < lower || b > upper)
                                {
                                    // illegal trailing byte
                                    cp = bytesNeeded = bytesSeen = 0;
                                    lower = 0x80;
                                    upper = 0xbf;
                                    i = markedPos; // reset to the last marked position
                                    retString.Append('\uFFFD');
                                    continue;
                                }
                                // reset lower and upper for the third
                                // and further bytes
                                lower = 0x80;
                                upper = 0xbf;
                                bytesSeen++;
                                cp += (b - 0x80) << (6*(bytesNeeded - bytesSeen));
                                markedPos = i;
                                if (bytesSeen != bytesNeeded)
                                {
                                    // continue if not all bytes needed
                                    // were read yet
                                    continue;
                                }
                                int ret = cp;
                                cp = 0;
                                bytesSeen = 0;
                                bytesNeeded = 0;
                                // append the Unicode character
                                if (ret <= 0xFFFF)
                                {
                                    retString.Append((char) (ret));
                                }
                                else
                                {
                                    retString.Append((char) ((((ret - 0x10000) >> 10) & 0x3FF) + 0xD800));
                                    retString.Append((char) ((((ret - 0x10000)) & 0x3FF) + 0xDC00));
                                }
                                continue;
                            }
                        }
                    }
                }
                if (bytesNeeded > 0)
                {
                    // we expected further bytes here,
                    // so emit a replacement character instead
                    bytesNeeded = 0;
                    retString.Append('\uFFFD');
                }
                // append the code point as is (we already
                // checked for ASCII characters so this will
                // be simple
                retString.Append((char) (c & 0xFF));
            }
            if (bytesNeeded > 0)
            {
                // we expected further bytes here,
                // so emit a replacement character instead
                bytesNeeded = 0;
                retString.Append('\uFFFD');
            }
            return retString.ToString();
        }

        public static IList<string[]> ParseQueryString(
            string input
            )
        {
            return ParseQueryString(input, null);
        }

        public static IList<string[]> ParseQueryString(
            string input, string delimiter
            )
        {
            if ((input) == null) throw new ArgumentNullException("input");
            if (delimiter == null)
            {
                // set default delimiter to ampersand
                delimiter = "&";
            }
            // Check input for non-ASCII characters
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > 0x7F)
                    throw new ArgumentException("input contains a non-ASCII character");
            }
            // split on delimiter
            string[] strings = SplitAt(input, delimiter);
            List<string[]> pairs = new List<string[]>();
            foreach (var str in strings)
            {
                if (str.Length == 0)
                {
                    continue;
                }
                // split on key
                int index = str.IndexOf('=');
                string name = str;
                string value = ""; // value is empty if there is no key
                if (index >= 0)
                {
                    name = str.Substring(0, (index) - (0));
                    value = str.Substring(index + 1);
                }
                name = name.Replace('+', ' ');
                value = value.Replace('+', ' ');
                string[] pair = new string[] {name, value};
                pairs.Add(pair);
            }
            foreach (var pair in pairs)
            {
                // percent decode the key and value if necessary
                pair[0] = PercentDecodeUTF8(pair[0]);
                pair[1] = PercentDecodeUTF8(pair[1]);
            }
            return pairs;
        }

        private static string[] GetKeyPath(string s)
        {
            int index = s.IndexOf('[');
            if (index < 0)
            {
// start bracket not found
                return new string[] {s};
            }
            List<string> path = new List<string>();
            path.Add(s.Substring(0, (index) - (0)));
            index++; // move to after the bracket
            while (true)
            {
                int endBracket = s.IndexOf(']', index);
                if (endBracket < 0)
                {
                    // end bracket not found
                    path.Add(s.Substring(index));
                    break;
                }
                path.Add(s.Substring(index, (endBracket) - (index)));
                index = endBracket + 1; // move to after the end bracket
                index = s.IndexOf('[', index);
                if (index < 0)
                {
// start bracket not found
                    break;
                }
                index++; // move to after the start bracket
            }
            return path.ToArray();
        }

        private static bool IsList(IDictionary<string, Object> dict)
        {
            if (dict == null) return false;
            int index = 0;
            int count = dict.Count;
            if (count == 0) return false;
            while (true)
            {
                if (index == count)
                    return true;
                string indexString = Convert.ToString(index, CultureInfo.InvariantCulture);
                if (!dict.ContainsKey(indexString))
                {
                    return false;
                }
                index++;
            }
        }

        private static IList<Object> ConvertToList(IDictionary<string, Object> dict)
        {
            List<Object> ret = new List<Object>();
            int index = 0;
            int count = dict.Count;
            while (index < count)
            {
                string indexString = Convert.ToString(index, CultureInfo.InvariantCulture);
                ret.Add(dict[indexString]);
                index++;
            }
            return ret;
        }

        private static void ConvertLists(IList<Object> dict)
        {
            for (int i = 0; i < dict.Count; i++)
            {

                IDictionary<string, Object> value = ((dict[i] is IDictionary<string, Object>)
                    ? (IDictionary<string, Object>) dict[i]
                    : null);
                // A list contains only indexes 0, 1, 2, and so on,
                // with no gaps.
                if (IsList(value))
                {
                    IList<Object> newList = ConvertToList(value);
                    dict[i] = newList;
                    ConvertLists(newList);
                }
                else if (value != null)
                {
                    // Convert the list's descendents
                    // if they are lists
                    ConvertLists(value);
                }
            }
        }

        private static void ConvertLists(IDictionary<string, Object> dict)
        {
            foreach (var key in new List<string>(dict.Keys))
            {

                IDictionary<string, Object> value = ((dict[key] is IDictionary<string, Object>)
                    ? (IDictionary<string, Object>) dict[key]
                    : null);
                // A list contains only indexes 0, 1, 2, and so on,
                // with no gaps.
                if (IsList(value))
                {
                    IList<Object> newList = ConvertToList(value);
                    dict.Add(key, newList);
                    ConvertLists(newList);
                }
                else if (value != null)
                {
                    // Convert the dictionary's descendents
                    // if they are lists
                    ConvertLists(value);
                }
            }
        }

        public static IDictionary<string, Object> QueryStringToDict(string query)
        {
            return QueryStringToDict(query, "&");
        }

        public static IDictionary<string, Object> QueryStringToDict(string query, string delimiter)
        {
            IDictionary<string, Object> root = new Dictionary<string, Object>();
            foreach (var keyvalue in ParseQueryString(query, delimiter))
            {
                string[] path = GetKeyPath(keyvalue[0]);
                IDictionary<string, Object> leaf = root;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (!leaf.ContainsKey(path[i]))
                    {
                        // node doesn't exist so add it
                        IDictionary<string, Object> newLeaf = new Dictionary<string, Object>();
                        leaf.Add(path[i], newLeaf);
                        leaf = newLeaf;
                    }
                    else
                    {

                        IDictionary<string, Object> o = ((leaf[path[i]] is IDictionary<string, Object>)
                            ? (IDictionary<string, Object>) leaf[path[i]]
                            : null);
                        if (o != null)
                        {
                            leaf = o;
                        }
                        else
                        {
                            // error, not a dictionary
                            leaf = null;
                            break;
                        }
                    }
                }
                if (leaf != null)
                {
                    leaf.Add(path[path.Length - 1], keyvalue[1]);
                }
            }
            // Convert array-like dictionaries to ILists
            ConvertLists(root);
            return root;
        }
    }

}