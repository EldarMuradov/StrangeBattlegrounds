//#define USE_SharpZipLib
#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace SimpleJSON
{
    public enum JSONBinaryTag
    {
        Array = 1,
        Class = 2,
        Value = 3,
        IntValue = 4,
        DoubleValue = 5,
        BoolValue = 6,
        FloatValue = 7,
    }

    public class JSONNode
    {
        #region common interface
        public virtual void Add(string aKey, JSONNode aItem)
        {
        }
        public virtual JSONNode this[int aIndex]
        {
            get { return null; }
            set { }
        }
        public virtual JSONNode this[string aKey]
        {
            get { return null; }
            set { }
        }
        public virtual string Value
        {
            get { return ""; }
            set { }
        }
        public virtual int Count { get { return 0; } }

        public virtual void Add(JSONNode aItem)
        {
            Add("", aItem);
        }

        public virtual JSONNode Remove(string aKey)
        {
            return null;
        }
        public virtual JSONNode Remove(int aIndex)
        {
            return null;
        }
        public virtual JSONNode Remove(JSONNode aNode)
        {
            return aNode;
        }

        public virtual IEnumerable<JSONNode> Childs { get { yield break; } }
        public IEnumerable<JSONNode> DeepChilds
        {
            get
            {
                foreach (var C in Childs)
                    foreach (var D in C.DeepChilds)
                        yield return D;
            }
        }

        public override string ToString()
        {
            return "JSONNode";
        }
        public virtual string ToString(string aPrefix)
        {
            return "JSONNode";
        }

        #endregion common interface

        #region typecasting properties
        public virtual int AsInt
        {
            get
            {
                int v = 0;
                if (int.TryParse(Value, out v))
                    return v;
                return 0;
            }
            set
            {
                Value = value.ToString();
            }
        }
        public virtual float AsFloat
        {
            get
            {
                float v = 0.0f;
                if (float.TryParse(Value, out v))
                    return v;
                return 0.0f;
            }
            set
            {
                Value = value.ToString();
            }
        }
        public virtual double AsDouble
        {
            get
            {
                double v = 0.0;
                if (double.TryParse(Value, out v))
                    return v;
                return 0.0;
            }
            set
            {
                Value = value.ToString();
            }
        }
        public virtual bool AsBool
        {
            get
            {
                bool v = false;
                if (bool.TryParse(Value, out v))
                    return v;
                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = (value) ? "true" : "false";
            }
        }
        public virtual JSONArray AsArray
        {
            get
            {
                return this as JSONArray;
            }
        }

        public virtual JSONClass AsObject
        {
            get
            {
                return this as JSONClass;
            }
        }


        #endregion typecasting properties

        #region operators
        public static implicit operator JSONNode(string s)
        {
            return new JSONData(s);
        }
        public static implicit operator string(JSONNode d)
        {
            return (d == null) ? null : d.Value;
        }
        public static bool operator ==(JSONNode a, object b)
        {
            if (b == null && a is JSONLazyCreator)
                return true;
            return System.Object.ReferenceEquals(a, b);
        }

        public static bool operator !=(JSONNode a, object b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return System.Object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #endregion operators

        internal static string Escape(string aText)
        {
            string result = "";
            foreach (char c in aText)
            {
                switch (c)
                {
                    case '\\':
                        result += "\\\\";
                        break;
                    case '\"':
                        result += "\\\"";
                        break;
                    case '\n':
                        result += "\\n";
                        break;
                    case '\r':
                        result += "\\r";
                        break;
                    case '\t':
                        result += "\\t";
                        break;
                    case '\b':
                        result += "\\b";
                        break;
                    case '\f':
                        result += "\\f";
                        break;
                    default:
                        result += c;
                        break;
                }
            }
            return result;
        }

        public static JSONNode Parse(string aJSON)
        {
            Stack<JSONNode> stack = new Stack<JSONNode>();
            JSONNode ctx = null;
            int i = 0;
            string Token = "";
            string TokenName = "";
            bool QuoteMode = false;
            while (i < aJSON.Length)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        stack.Push(new JSONClass());
                        if (ctx != null)
                        {
                            TokenName = TokenName.Trim();
                            if (ctx is JSONArray)
                                ctx.Add(stack.Peek());
                            else if (TokenName != "")
                                ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = "";
                        Token = "";
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }

                        stack.Push(new JSONArray());
                        if (ctx != null)
                        {
                            TokenName = TokenName.Trim();
                            if (ctx is JSONArray)
                                ctx.Add(stack.Peek());
                            else if (TokenName != "")
                                ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = "";
                        Token = "";
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (Token != "")
                        {
                            TokenName = TokenName.Trim();
                            if (ctx is JSONArray)
                                ctx.Add(Token);
                            else if (TokenName != "")
                                ctx.Add(TokenName, Token);
                        }
                        TokenName = "";
                        Token = "";
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        TokenName = Token;
                        Token = "";
                        break;

                    case '"':
                        QuoteMode ^= true;
                        break;

                    case ',':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        if (Token != "")
                        {
                            if (ctx is JSONArray)
                                ctx.Add(Token);
                            else if (TokenName != "")
                                ctx.Add(TokenName, Token);
                        }
                        TokenName = "";
                        Token = "";
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (QuoteMode)
                            Token += aJSON[i];
                        break;

                    case '\\':
                        ++i;
                        if (QuoteMode)
                        {
                            char C = aJSON[i];
                            switch (C)
                            {
                                case 't':
                                    Token += '\t';
                                    break;
                                case 'r':
                                    Token += '\r';
                                    break;
                                case 'n':
                                    Token += '\n';
                                    break;
                                case 'b':
                                    Token += '\b';
                                    break;
                                case 'f':
                                    Token += '\f';
                                    break;
                                case 'u':
                                    {
                                        string s = aJSON.Substring(i + 1, 4);
                                        Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
                                        i += 4;
                                        break;
                                    }
                                default:
                                    Token += C;
                                    break;
                            }
                        }
                        break;

                    default:
                        Token += aJSON[i];
                        break;
                }
                ++i;
            }
            if (QuoteMode)
            {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }
            return ctx;
        }

        public virtual void Serialize(System.IO.BinaryWriter aWriter)
        {
        }

        public void SaveToStream(System.IO.Stream aData)
        {
            var W = new System.IO.BinaryWriter(aData);
            Serialize(W);
        }

#if USE_SharpZipLib
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
            {
                gzipOut.IsStreamOwner = false;
                SaveToStream(gzipOut);
                gzipOut.Close();
            }
        }
 
        public void SaveToCompressedFile(string aFileName)
        {
#if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            //using(var F = System.IO.File.OpenWrite(aFileName))
			//原因见备注：https://msdn.microsoft.com/zh-cn/library/system.io.file.openwrite.aspx
			using (var F = new FileStream(aFileName, FileMode.Create, FileAccess.Write))
            {
                SaveToCompressedStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public string SaveToCompressedBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToCompressedStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }
 
#else
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public void SaveToCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public string SaveToCompressedBase64()
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
#endif

        public void SaveToFile(string aFileName)
        {
#if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            //using (var F = System.IO.File.OpenWrite(aFileName)) {

            using (var F = new FileStream(aFileName, FileMode.Create, FileAccess.Write))
            {
                SaveToStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public void SaveToText(string aFileName)
        {
#if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            //using (var F = System.IO.File.OpenWrite(aFileName)) {

            using (var F = new FileStream(aFileName, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter streamwriter = new StreamWriter(F))
                {
                    streamwriter.Write(this.ToString());
                }
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public string SaveToBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }
        public static JSONNode Deserialize(System.IO.BinaryReader aReader)
        {
            JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
            switch (type)
            {
                case JSONBinaryTag.Array:
                    {
                        int count = aReader.ReadInt32();
                        JSONArray tmp = new JSONArray();
                        for (int i = 0; i < count; i++)
                            tmp.Add(Deserialize(aReader));
                        return tmp;
                    }
                case JSONBinaryTag.Class:
                    {
                        int count = aReader.ReadInt32();
                        JSONClass tmp = new JSONClass();
                        for (int i = 0; i < count; i++)
                        {
                            string key = aReader.ReadString();
                            var val = Deserialize(aReader);
                            tmp.Add(key, val);
                        }
                        return tmp;
                    }
                case JSONBinaryTag.Value:
                    {
                        return new JSONData(aReader.ReadString());
                    }
                case JSONBinaryTag.IntValue:
                    {
                        return new JSONData(aReader.ReadInt32());
                    }
                case JSONBinaryTag.DoubleValue:
                    {
                        return new JSONData(aReader.ReadDouble());
                    }
                case JSONBinaryTag.BoolValue:
                    {
                        return new JSONData(aReader.ReadBoolean());
                    }
                case JSONBinaryTag.FloatValue:
                    {
                        return new JSONData(aReader.ReadSingle());
                    }

                default:
                    {
                        throw new Exception("Error deserializing JSON. Unknown tag: " + type);
                    }
            }
        }

#if USE_SharpZipLib
        public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
            return LoadFromStream(zin);
        }
        public static JSONNode LoadFromCompressedFile(string aFileName)
        {
#if USE_FileIO
            using(var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromCompressedStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public static JSONNode LoadFromCompressedBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromCompressedStream(stream);
        }
#else
        public static JSONNode LoadFromCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public static JSONNode LoadFromCompressedBase64(string aBase64)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
#endif

        public static JSONNode LoadFromStream(System.IO.Stream aData)
        {
            using (var R = new System.IO.BinaryReader(aData))
            {
                return Deserialize(R);
            }
        }
        public static JSONNode LoadFromFile(string aFileName)
        {
#if USE_FileIO
            using (var F = System.IO.File.OpenRead(aFileName))
               
                return LoadFromStream(F);
        }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        public static JSONNode LoadFromText(string aFileName)
        {
#if USE_FileIO
            string readtext = File.ReadAllText(aFileName);
            return Parse(readtext);
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public static JSONNode LoadFromBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromStream(stream);
        }
    }
    
} // End of JSONNode

public class JSONArray : JSONNode, IEnumerable
{
    private List<JSONNode> m_List = new List<JSONNode>();
    public override JSONNode this[int aIndex]
    {
        get
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return new JSONLazyCreator(this);
            return m_List[aIndex];
        }
        set
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                m_List.Add(value);
            else
                m_List[aIndex] = value;
        }
    }
    public override JSONNode this[string aKey]
    {
        get { return new JSONLazyCreator(this); }
        set { m_List.Add(value); }
    }
    public override int Count
    {
        get { return m_List.Count; }
    }
    public override void Add(string aKey, JSONNode aItem)
    {
        m_List.Add(aItem);
    }
    public override JSONNode Remove(int aIndex)
    {
        if (aIndex < 0 || aIndex >= m_List.Count)
            return null;
        JSONNode tmp = m_List[aIndex];
        m_List.RemoveAt(aIndex);
        return tmp;
    }
    public override JSONNode Remove(JSONNode aNode)
    {
        m_List.Remove(aNode);
        return aNode;
    }
    public override IEnumerable<JSONNode> Childs
    {
        get
        {
            foreach (JSONNode N in m_List)
                yield return N;
        }
    }
    public IEnumerator GetEnumerator()
    {
        foreach (JSONNode N in m_List)
            yield return N;
    }
    public override string ToString()
    {
        string result = "[ ";
        foreach (JSONNode N in m_List)
        {
            if (result.Length > 2)
                result += ", ";
            result += N.ToString();
        }
        result += " ]";
        return result;
    }
    public override string ToString(string aPrefix)
    {
        string result = "[ ";
        foreach (JSONNode N in m_List)
        {
            if (result.Length > 3)
                result += ", ";
            result += "\n" + aPrefix + "   ";
            result += N.ToString(aPrefix + "   ");
        }
        result += "\n" + aPrefix + "]";
        return result;
    }
    public override void Serialize(System.IO.BinaryWriter aWriter)
    {
        aWriter.Write((byte)JSONBinaryTag.Array);
        aWriter.Write(m_List.Count);
        for (int i = 0; i < m_List.Count; i++)
        {
            m_List[i].Serialize(aWriter);
        }
    }
} // End of JSONArray

public class JSONClass : JSONNode, IEnumerable
{
    private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();
    public override JSONNode this[string aKey]
    {
        get
        {
            if (m_Dict.ContainsKey(aKey))
                return m_Dict[aKey];
            else
                return new JSONLazyCreator(this, aKey);
        }
        set
        {
            if (m_Dict.ContainsKey(aKey))
                m_Dict[aKey] = value;
            else
                m_Dict.Add(aKey, value);
        }
    }
    public override JSONNode this[int aIndex]
    {
        get
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return null;
            return m_Dict.ElementAt(aIndex).Value;
        }
        set
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return;
            string key = m_Dict.ElementAt(aIndex).Key;
            m_Dict[key] = value;
        }
    }
    public override int Count
    {
        get { return m_Dict.Count; }
    }


    public override void Add(string aKey, JSONNode aItem)
    {
        if (!string.IsNullOrEmpty(aKey))
        {
            if (m_Dict.ContainsKey(aKey))
                m_Dict[aKey] = aItem;
            else
                m_Dict.Add(aKey, aItem);
        }
        else
            m_Dict.Add(Guid.NewGuid().ToString(), aItem);
    }

    public override JSONNode Remove(string aKey)
    {
        if (!m_Dict.ContainsKey(aKey))
            return null;
        JSONNode tmp = m_Dict[aKey];
        m_Dict.Remove(aKey);
        return tmp;
    }
    public override JSONNode Remove(int aIndex)
    {
        if (aIndex < 0 || aIndex >= m_Dict.Count)
            return null;
        var item = m_Dict.ElementAt(aIndex);
        m_Dict.Remove(item.Key);
        return item.Value;
    }
    public override JSONNode Remove(JSONNode aNode)
    {
        try
        {
            var item = m_Dict.Where(k => k.Value == aNode).First();
            m_Dict.Remove(item.Key);
            return aNode;
        }
        catch
        {
            return null;
        }
    }

    public override IEnumerable<JSONNode> Childs
    {
        get
        {
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                yield return N.Value;
        }
    }

    public IEnumerator GetEnumerator()
    {
        foreach (KeyValuePair<string, JSONNode> N in m_Dict)
            yield return N;
    }
    public override string ToString()
    {
        string result = "{";
        foreach (KeyValuePair<string, JSONNode> N in m_Dict)
        {
            if (result.Length > 2)
                result += ", ";
            result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
        }
        result += "}";
        return result;
    }
    public override string ToString(string aPrefix)
    {
        string result = "{ ";
        foreach (KeyValuePair<string, JSONNode> N in m_Dict)
        {
            if (result.Length > 3)
                result += ", ";
            result += "\n" + aPrefix + "   ";
            result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix + "   ");
        }
        result += "\n" + aPrefix + "}";
        return result;
    }
    public override void Serialize(System.IO.BinaryWriter aWriter)
    {
        aWriter.Write((byte)JSONBinaryTag.Class);
        aWriter.Write(m_Dict.Count);
        foreach (string K in m_Dict.Keys)
        {
            aWriter.Write(K);
            m_Dict[K].Serialize(aWriter);
        }
    }
} // End of JSONClass

public class JSONData : JSONNode
{
    private string m_Data;
    public override string Value
    {
        get { return m_Data; }
        set { m_Data = value; }
    }
    public JSONData(string aData)
    {
        m_Data = aData;
    }
    public JSONData(float aData)
    {
        AsFloat = aData;
    }
    public JSONData(double aData)
    {
        AsDouble = aData;
    }
    public JSONData(bool aData)
    {
        AsBool = aData;
    }
    public JSONData(int aData)
    {
        AsInt = aData;
    }

    public override string ToString()
    {
        return "\"" + Escape(m_Data) + "\"";
    }
    public override string ToString(string aPrefix)
    {
        return "\"" + Escape(m_Data) + "\"";
    }
    public override void Serialize(System.IO.BinaryWriter aWriter)
    {
        var tmp = new JSONData("");

        tmp.AsInt = AsInt;
        if (tmp.m_Data == this.m_Data)
        {
            aWriter.Write((byte)JSONBinaryTag.IntValue);
            aWriter.Write(AsInt);
            return;
        }
        tmp.AsFloat = AsFloat;
        if (tmp.m_Data == this.m_Data)
        {
            aWriter.Write((byte)JSONBinaryTag.FloatValue);
            aWriter.Write(AsFloat);
            return;
        }
        tmp.AsDouble = AsDouble;
        if (tmp.m_Data == this.m_Data)
        {
            aWriter.Write((byte)JSONBinaryTag.DoubleValue);
            aWriter.Write(AsDouble);
            return;
        }

        tmp.AsBool = AsBool;
        if (tmp.m_Data == this.m_Data)
        {
            aWriter.Write((byte)JSONBinaryTag.BoolValue);
            aWriter.Write(AsBool);
            return;
        }
        aWriter.Write((byte)JSONBinaryTag.Value);
        aWriter.Write(m_Data);
    }
} // End of JSONData

internal class JSONLazyCreator : JSONNode
{
    private JSONNode m_Node = null;
    private string m_Key = null;

    public JSONLazyCreator(JSONNode aNode)
    {
        m_Node = aNode;
        m_Key = null;
    }
    public JSONLazyCreator(JSONNode aNode, string aKey)
    {
        m_Node = aNode;
        m_Key = aKey;
    }

    private void Set(JSONNode aVal)
    {
        if (m_Key == null)
        {
            m_Node.Add(aVal);
        }
        else
        {
            m_Node.Add(m_Key, aVal);
        }
        m_Node = null; // Be GC friendly.
    }

    public override JSONNode this[int aIndex]
    {
        get
        {
            return new JSONLazyCreator(this);
        }
        set
        {
            var tmp = new JSONArray();
            tmp.Add(value);
            Set(tmp);
        }
    }

    public override JSONNode this[string aKey]
    {
        get
        {
            return new JSONLazyCreator(this, aKey);
        }
        set
        {
            var tmp = new JSONClass();
            tmp.Add(aKey, value);
            Set(tmp);
        }
    }
    public override void Add(JSONNode aItem)
    {
        var tmp = new JSONArray();
        tmp.Add(aItem);
        Set(tmp);
    }
    public override void Add(string aKey, JSONNode aItem)
    {
        var tmp = new JSONClass();
        tmp.Add(aKey, aItem);
        Set(tmp);
    }
    public static bool operator ==(JSONLazyCreator a, object b)
    {
        if (b == null)
            return true;
        return System.Object.ReferenceEquals(a, b);
    }

    public static bool operator !=(JSONLazyCreator a, object b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return true;
        return System.Object.ReferenceEquals(this, obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return "";
    }
    public override string ToString(string aPrefix)
    {
        return "";
    }

    public override int AsInt
    {
        get
        {
            JSONData tmp = new JSONData(0);
            Set(tmp);
            return 0;
        }
        set
        {
            JSONData tmp = new JSONData(value);
            Set(tmp);
        }
    }
    public override float AsFloat
    {
        get
        {
            JSONData tmp = new JSONData(0.0f);
            Set(tmp);
            return 0.0f;
        }
        set
        {
            JSONData tmp = new JSONData(value);
            Set(tmp);
        }
    }
    public override double AsDouble
    {
        get
        {
            JSONData tmp = new JSONData(0.0);
            Set(tmp);
            return 0.0;
        }
        set
        {
            JSONData tmp = new JSONData(value);
            Set(tmp);
        }
    }
    public override bool AsBool
    {
        get
        {
            JSONData tmp = new JSONData(false);
            Set(tmp);
            return false;
        }
        set
        {
            JSONData tmp = new JSONData(value);
            Set(tmp);
        }
    }
    public override JSONArray AsArray
    {
        get
        {
            JSONArray tmp = new JSONArray();
            Set(tmp);
            return tmp;
        }
    }
    public override JSONClass AsObject
    {
        get
        {
            JSONClass tmp = new JSONClass();
            Set(tmp);
            return tmp;
        }
    }
} // End of JSONLazyCreator

public static class JSON
{
    public static JSONNode Parse(string aJSON)
    {
        return JSONNode.Parse(aJSON);
    }
}
public class JSONObject
{
    const int MAX_DEPTH = 1000;
    const string INFINITY = "\"INFINITY\"";
    const string NEGINFINITY = "\"NEGINFINITY\"";
    const string NaN = "\"NaN\"";
    public static char[] WHITESPACE = new char[] { ' ', '\r', '\n', '\t' };
    public enum Type { NULL, STRING, NUMBER, OBJECT, ARRAY, BOOL }
    public bool isContainer { get { return (type == Type.ARRAY || type == Type.OBJECT); } }
    public JSONObject parent;
    public Type type = Type.NULL;
    public int Count
    {
        get
        {
            if (list == null)
                return -1;
            return list.Count;
        }
    }
    //TODO: Switch to list
    public List<JSONObject> list;
    public List<string> keys;
    public string str;
#if USEFLOAT
	public float n;
	public float f {
		get {
			return n;
		}
	}
#else
    public double n;
    public float f
    {
        get
        {
            return (float)n;
        }
    }
#endif
    public bool b;
    public delegate void AddJSONConents(JSONObject self);

    public static JSONObject nullJO { get { return new JSONObject(JSONObject.Type.NULL); } }    //an empty, null object
    public static JSONObject obj { get { return new JSONObject(JSONObject.Type.OBJECT); } }     //an empty object
    public static JSONObject arr { get { return new JSONObject(JSONObject.Type.ARRAY); } }      //an empty array

    public JSONObject(JSONObject.Type t)
    {
        type = t;
        switch (t)
        {
            case Type.ARRAY:
                list = new List<JSONObject>();
                break;
            case Type.OBJECT:
                list = new List<JSONObject>();
                keys = new List<string>();
                break;
        }
    }
    public JSONObject(bool b)
    {
        type = Type.BOOL;
        this.b = b;
    }
    public JSONObject(float f)
    {
        type = Type.NUMBER;
        this.n = f;
    }
    public JSONObject(Dictionary<string, string> dic)
    {
        type = Type.OBJECT;
        keys = new List<string>();
        list = new List<JSONObject>();
        foreach (KeyValuePair<string, string> kvp in dic)
        {
            keys.Add(kvp.Key);
            list.Add(new JSONObject { type = Type.STRING, str = kvp.Value });
        }
    }
    public JSONObject(Dictionary<string, JSONObject> dic)
    {
        type = Type.OBJECT;
        keys = new List<string>();
        list = new List<JSONObject>();
        foreach (KeyValuePair<string, JSONObject> kvp in dic)
        {
            keys.Add(kvp.Key);
            list.Add(kvp.Value);
        }
    }
    public JSONObject(AddJSONConents content)
    {
        content.Invoke(this);
    }
    public JSONObject(JSONObject[] objs)
    {
        type = Type.ARRAY;
        list = new List<JSONObject>(objs);
    }
    //Convenience function for creating a JSONObject containing a string.  This is not part of the constructor so that malformed JSON data doesn't just turn into a string object
    public static JSONObject StringObject(string val) { return new JSONObject { type = JSONObject.Type.STRING, str = val }; }
    public void Absorb(JSONObject obj)
    {
        list.AddRange(obj.list);
        keys.AddRange(obj.keys);
        str = obj.str;
        n = obj.n;
        b = obj.b;
        type = obj.type;
    }
    public JSONObject() { }
    #region PARSE
    public JSONObject(string str, bool strict = false)
    {   //create a new JSONObject from a string (this will also create any children, and parse the whole string)
        if (str != null)
        {
            str = str.Trim(WHITESPACE);
            if (strict)
            {
                if (str[0] != '[' && str[0] != '{')
                {
                    type = Type.NULL;
                    Debug.LogWarning("Improper (strict) JSON formatting.  First character must be [ or {");
                    return;
                }
            }
            if (str.Length > 0)
            {
                if (string.Compare(str, "true", true) == 0)
                {
                    type = Type.BOOL;
                    b = true;
                }
                else if (string.Compare(str, "false", true) == 0)
                {
                    type = Type.BOOL;
                    b = false;
                }
                else if (string.Compare(str, "null", true) == 0)
                {
                    type = Type.NULL;
#if USEFLOAT
				} else if(str == INFINITY) {
					type = Type.NUMBER;
					n = float.PositiveInfinity;
				} else if(str == NEGINFINITY) {
					type = Type.NUMBER;
					n = float.NegativeInfinity;
				} else if(str == NaN) {
					type = Type.NUMBER;
					n = float.NaN;
#else
                }
                else if (str == INFINITY)
                {
                    type = Type.NUMBER;
                    n = double.PositiveInfinity;
                }
                else if (str == NEGINFINITY)
                {
                    type = Type.NUMBER;
                    n = double.NegativeInfinity;
                }
                else if (str == NaN)
                {
                    type = Type.NUMBER;
                    n = double.NaN;
#endif
                }
                else if (str[0] == '"')
                {
                    type = Type.STRING;
                    this.str = str.Substring(1, str.Length - 2);
                }
                else
                {
                    try
                    {
#if USEFLOAT
						n = System.Convert.ToSingle(str);
#else
                        n = System.Convert.ToDouble(str);
#endif
                        type = Type.NUMBER;
                    }
                    catch (System.FormatException)
                    {
                        int token_tmp = 1;
                        /*
						 * Checking for the following formatting (www.json.org)
						 * object - {"field1":value,"field2":value}
						 * array - [value,value,value]
						 * value - string	- "string"
						 *		 - number	- 0.0
						 *		 - bool		- true -or- false
						 *		 - null		- null
						 */
                        int offset = 0;
                        switch (str[offset])
                        {
                            case '{':
                                type = Type.OBJECT;
                                keys = new List<string>();
                                list = new List<JSONObject>();
                                break;
                            case '[':
                                type = JSONObject.Type.ARRAY;
                                list = new List<JSONObject>();
                                break;
                            default:
                                type = Type.NULL;
                                Debug.LogWarning("improper JSON formatting:" + str);
                                return;
                        }
                        string propName = "";
                        bool openQuote = false;
                        bool inProp = false;
                        int depth = 0;
                        while (++offset < str.Length)
                        {
                            if (System.Array.IndexOf<char>(WHITESPACE, str[offset]) > -1)
                                continue;
                            if (str[offset] == '\"')
                            {
                                if (openQuote)
                                {
                                    if (!inProp && depth == 0 && type == Type.OBJECT)
                                        propName = str.Substring(token_tmp + 1, offset - token_tmp - 1);
                                    openQuote = false;
                                }
                                else
                                {
                                    if (depth == 0 && type == Type.OBJECT)
                                        token_tmp = offset;
                                    openQuote = true;
                                }
                            }
                            if (openQuote)
                                continue;
                            if (type == Type.OBJECT && depth == 0)
                            {
                                if (str[offset] == ':')
                                {
                                    token_tmp = offset + 1;
                                    inProp = true;
                                }
                            }

                            if (str[offset] == '[' || str[offset] == '{')
                            {
                                depth++;
                            }
                            else if (str[offset] == ']' || str[offset] == '}')
                            {
                                depth--;
                            }
                            //if  (encounter a ',' at top level)  || a closing ]/}
                            if ((str[offset] == ',' && depth == 0) || depth < 0)
                            {
                                inProp = false;
                                string inner = str.Substring(token_tmp, offset - token_tmp).Trim(WHITESPACE);
                                if (inner.Length > 0)
                                {
                                    if (type == Type.OBJECT)
                                        keys.Add(propName);
                                    list.Add(new JSONObject(inner));
                                }
                                token_tmp = offset + 1;
                            }
                        }
                    }
                }
            }
            else type = Type.NULL;
        }
        else type = Type.NULL;  //If the string is missing, this is a null
    }
    #endregion
    public bool IsNumber { get { return type == Type.NUMBER; } }
    public bool IsNull { get { return type == Type.NULL; } }
    public bool IsString { get { return type == Type.STRING; } }
    public bool IsBool { get { return type == Type.BOOL; } }
    public bool IsArray { get { return type == Type.ARRAY; } }
    public bool IsObject { get { return type == Type.OBJECT; } }
    public void Add(bool val) { Add(new JSONObject(val)); }
    public void Add(float val) { Add(new JSONObject(val)); }
    public void Add(int val) { Add(new JSONObject(val)); }
    public void Add(string str) { Add(StringObject(str)); }
    public void Add(AddJSONConents content) { Add(new JSONObject(content)); }
    public void Add(JSONObject obj)
    {
        if (obj)
        {       //Don't do anything if the object is null
            if (type != JSONObject.Type.ARRAY)
            {
                type = JSONObject.Type.ARRAY;       //Congratulations, son, you're an ARRAY now
                if (list == null)
                    list = new List<JSONObject>();
            }
            list.Add(obj);
        }
    }
    public void AddField(string name, bool val) { AddField(name, new JSONObject(val)); }
    public void AddField(string name, float val) { AddField(name, new JSONObject(val)); }
    public void AddField(string name, int val) { AddField(name, new JSONObject(val)); }
    public void AddField(string name, AddJSONConents content) { AddField(name, new JSONObject(content)); }
    public void AddField(string name, string val) { AddField(name, StringObject(val)); }
    public void AddField(string name, JSONObject obj)
    {
        if (obj)
        {       //Don't do anything if the object is null
            if (type != JSONObject.Type.OBJECT)
            {
                keys = new List<string>();
                if (type == Type.ARRAY)
                {
                    for (int i = 0; i < list.Count; i++)
                        keys.Add(i + "");
                }
                else if (list == null)
                    list = new List<JSONObject>();
                type = JSONObject.Type.OBJECT;      //Congratulations, son, you're an OBJECT now
            }
            keys.Add(name);
            list.Add(obj);
        }
    }
    public void SetField(string name, bool val) { SetField(name, new JSONObject(val)); }
    public void SetField(string name, float val) { SetField(name, new JSONObject(val)); }
    public void SetField(string name, int val) { SetField(name, new JSONObject(val)); }
    public void SetField(string name, JSONObject obj)
    {
        if (HasField(name))
        {
            list.Remove(this[name]);
            keys.Remove(name);
        }
        AddField(name, obj);
    }
    public void RemoveField(string name)
    {
        if (keys.IndexOf(name) > -1)
        {
            list.RemoveAt(keys.IndexOf(name));
            keys.Remove(name);
        }
    }
    public delegate void FieldNotFound(string name);
    public delegate void GetFieldResponse(JSONObject obj);
    public void GetField(ref bool field, string name, FieldNotFound fail = null)
    {
        if (type == JSONObject.Type.OBJECT)
        {
            int index = keys.IndexOf(name);
            if (index >= 0)
            {
                field = list[index].b;
                return;
            }
        }
        if (fail != null) fail.Invoke(name);
    }
#if USEFLOAT
	public void GetField(ref float field, string name, FieldNotFound fail = null) {
#else
    public void GetField(ref double field, string name, FieldNotFound fail = null)
    {
#endif
        if (type == JSONObject.Type.OBJECT)
        {
            int index = keys.IndexOf(name);
            if (index >= 0)
            {
                field = list[index].n;
                return;
            }
        }
        if (fail != null) fail.Invoke(name);
    }
    public void GetField(ref int field, string name, FieldNotFound fail = null)
    {
        if (type == JSONObject.Type.OBJECT)
        {
            int index = keys.IndexOf(name);
            if (index >= 0)
            {
                field = (int)list[index].n;
                return;
            }
        }
        if (fail != null) fail.Invoke(name);
    }
    public void GetField(ref uint field, string name, FieldNotFound fail = null)
    {
        if (type == JSONObject.Type.OBJECT)
        {
            int index = keys.IndexOf(name);
            if (index >= 0)
            {
                field = (uint)list[index].n;
                return;
            }
        }
        if (fail != null) fail.Invoke(name);
    }
    public void GetField(ref string field, string name, FieldNotFound fail = null)
    {
        if (type == JSONObject.Type.OBJECT)
        {
            int index = keys.IndexOf(name);
            if (index >= 0)
            {
                field = list[index].str;
                return;
            }
        }
        if (fail != null) fail.Invoke(name);
    }
    public void GetField(string name, GetFieldResponse response, FieldNotFound fail = null)
    {
        if (response != null && type == Type.OBJECT)
        {
            int index = keys.IndexOf(name);
            if (index >= 0)
            {
                response.Invoke(list[index]);
                return;
            }
        }
        if (fail != null) fail.Invoke(name);
    }
    public JSONObject GetField(string name)
    {
        if (type == JSONObject.Type.OBJECT)
            for (int i = 0; i < keys.Count; i++)
                if ((string)keys[i] == name)
                    return (JSONObject)list[i];
        return null;
    }
    public bool HasFields(string[] names)
    {
        foreach (string name in names)
            if (!keys.Contains(name))
                return false;
        return true;
    }
    public bool HasField(string name)
    {
        if (type == JSONObject.Type.OBJECT)
            for (int i = 0; i < keys.Count; i++)
                if ((string)keys[i] == name)
                    return true;
        return false;
    }
    public void Clear()
    {
        type = JSONObject.Type.NULL;
        if (list != null)
            list.Clear();
        if (keys != null)
            keys.Clear();
        str = "";
        n = 0;
        b = false;
    }
    public JSONObject Copy()
    {
        return new JSONObject(print());
    }
    /*
	 * The Merge function is experimental. Use at your own risk.
	 */
    public void Merge(JSONObject obj)
    {
        MergeRecur(this, obj);
    }
    /// <summary>
    /// Merge object right into left recursively
    /// </summary>
    /// <param name="left">The left (base) object</param>
    /// <param name="right">The right (new) object</param>
    static void MergeRecur(JSONObject left, JSONObject right)
    {
        if (left.type == JSONObject.Type.NULL)
            left.Absorb(right);
        else if (left.type == Type.OBJECT && right.type == Type.OBJECT)
        {
            for (int i = 0; i < right.list.Count; i++)
            {
                string key = (string)right.keys[i];
                if (right[i].isContainer)
                {
                    if (left.HasField(key))
                        MergeRecur(left[key], right[i]);
                    else
                        left.AddField(key, right[i]);
                }
                else
                {
                    if (left.HasField(key))
                        left.SetField(key, right[i]);
                    else
                        left.AddField(key, right[i]);
                }
            }
        }
        else if (left.type == Type.ARRAY && right.type == Type.ARRAY)
        {
            if (right.Count > left.Count)
            {
                Debug.LogError("Cannot merge arrays when right object has more elements");
                return;
            }
            for (int i = 0; i < right.list.Count; i++)
            {
                if (left[i].type == right[i].type)
                {           //Only overwrite with the same type
                    if (left[i].isContainer)
                        MergeRecur(left[i], right[i]);
                    else
                    {
                        left[i] = right[i];
                    }
                }
            }
        }
    }
    public string print(bool pretty = false)
    {
        return print(0, pretty);
    }
    #region STRINGIFY
    public string print(int depth, bool pretty = false)
    {   //Convert the JSONObject into a string
        if (depth++ > MAX_DEPTH)
        {
            Debug.Log("reached max depth!");
            return "";
        }
        string str = "";
        switch (type)
        {
            case Type.STRING:
                str = "\"" + this.str + "\"";
                break;
            case Type.NUMBER:
#if USEFLOAT
				if(float.IsInfinity(n))
					str = INFINITY;
				else if(float.IsNegativeInfinity(n))
					str = NEGINFINITY;
				else if(float.IsNaN(n))
					str = NaN;
#else
                if (double.IsInfinity(n))
                    str = INFINITY;
                else if (double.IsNegativeInfinity(n))
                    str = NEGINFINITY;
                else if (double.IsNaN(n))
                    str = NaN;
#endif
                else
                    str += n;
                break;

            case JSONObject.Type.OBJECT:
                str = "{";
                if (list.Count > 0)
                {
#if (PRETTY)    //for a bit more readability, comment the define above to disable system-wide
					if(pretty)
						str += "\n";
#endif
                    for (int i = 0; i < list.Count; i++)
                    {
                        string key = (string)keys[i];
                        JSONObject obj = (JSONObject)list[i];
                        if (obj)
                        {
#if (PRETTY)
							if(pretty)
								for(int j = 0; j < depth; j++)
									str += "\t"; //for a bit more readability
#endif
                            str += "\"" + key + "\":";
                            str += obj.print(depth, pretty) + ",";
#if (PRETTY)
							if(pretty)
								str += "\n";
#endif
                        }
                    }
#if (PRETTY)
					if(pretty)
						str = str.Substring(0, str.Length - 1);		//BOP: This line shows up twice on purpose: once to remove the \n if readable is true and once to remove the comma
#endif
                    str = str.Substring(0, str.Length - 1);
                }
#if (PRETTY)
				if(pretty && list.Count > 0) {
					str += "\n";
					for(int j = 0; j < depth - 1; j++)
						str += "\t"; //for a bit more readability
				}
#endif
                str += "}";
                break;
            case JSONObject.Type.ARRAY:
                str = "[";
                if (list.Count > 0)
                {
#if (PRETTY)
					if(pretty)
						str += "\n"; //for a bit more readability
#endif
                    foreach (JSONObject obj in list)
                    {
                        if (obj)
                        {
#if (PRETTY)
							if(pretty)
								for(int j = 0; j < depth; j++)
									str += "\t"; //for a bit more readability
#endif
                            str += obj.print(depth, pretty) + ",";
#if (PRETTY)
							if(pretty)
								str += "\n"; //for a bit more readability
#endif
                        }
                    }
#if (PRETTY)
					if(pretty)
						str = str.Substring(0, str.Length - 1);	//BOP: This line shows up twice on purpose: once to remove the \n if readable is true and once to remove the comma
#endif
                    str = str.Substring(0, str.Length - 1);
                }
#if (PRETTY)
				if(pretty && list.Count > 0) {
					str += "\n";
					for(int j = 0; j < depth - 1; j++)
						str += "\t"; //for a bit more readability
				}
#endif
                str += "]";
                break;
            case Type.BOOL:
                if (b)
                    str = "true";
                else
                    str = "false";
                break;
            case Type.NULL:
                str = "null";
                break;
        }
        return str;
    }
    #endregion
    public static implicit operator WWWForm(JSONObject obj)
    {
        WWWForm form = new WWWForm();
        for (int i = 0; i < obj.list.Count; i++)
        {
            string key = i + "";
            if (obj.type == Type.OBJECT)
                key = obj.keys[i];
            string val = obj.list[i].ToString();
            if (obj.list[i].type == Type.STRING)
                val = val.Replace("\"", "");
            form.AddField(key, val);
        }
        return form;
    }
    public JSONObject this[int index]
    {
        get
        {
            if (list.Count > index) return (JSONObject)list[index];
            else return null;
        }
        set
        {
            if (list.Count > index)
                list[index] = value;
        }
    }
    public JSONObject this[string index]
    {
        get
        {
            return GetField(index);
        }
        set
        {
            SetField(index, value);
        }
    }
    public override string ToString()
    {
        return print();
    }
    public string ToString(bool pretty)
    {
        return print(pretty);
    }
    public Dictionary<string, string> ToDictionary()
    {
        if (type == Type.OBJECT)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < list.Count; i++)
            {
                JSONObject val = (JSONObject)list[i];
                switch (val.type)
                {
                    case Type.STRING: result.Add((string)keys[i], val.str); break;
                    case Type.NUMBER: result.Add((string)keys[i], val.n + ""); break;
                    case Type.BOOL: result.Add((string)keys[i], val.b + ""); break;
                    default: Debug.LogWarning("Omitting object: " + (string)keys[i] + " in dictionary conversion"); break;
                }
            }
            return result;
        }
        else Debug.LogWarning("Tried to turn non-Object JSONObject into a dictionary");
        return null;
    }
    public static implicit operator bool(JSONObject o)
    {
        return (object)o != null;
    }
}