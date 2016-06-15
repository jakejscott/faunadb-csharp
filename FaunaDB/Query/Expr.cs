﻿using FaunaDB.Errors;
using FaunaDB.Values;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;

namespace FaunaDB.Query
{
    [JsonConverter(typeof(ValueJsonConverter))]
    public abstract class Expr : IEquatable<Expr>
    {
        internal abstract void WriteJson(JsonWriter writer);

        /// <summary>
        /// Convert to a JSON string.
        /// </summary>
        /// <param name="pretty">If true, output with helpful whitespace.</param>
        public string ToJson(bool pretty = false) =>
            JsonConvert.SerializeObject(this, pretty ? Formatting.Indented : Formatting.None);

        /// <summary>
        /// Read a Value from JSON.
        /// </summary>
        /// <exception cref="Errors.InvalidResponseException"/>
        //todo: Should we convert invalid Value downcasts and missing field exceptions to InvalidResponseException?
        public static Expr FromJson(string json)
        {
            // We handle dates ourselves. Don't want them automatically parsed.
            var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
            try
            {
                return JsonConvert.DeserializeObject<Expr>(json, settings);
            }
            catch (JsonReaderException j)
            {
                throw new InvalidResponseException($"Bad JSON: {j}");
            }
        }


        #region implicit conversions
        public static implicit operator Expr(ImmutableArray<Expr> values) =>
            new ArrayV(values);

        public static implicit operator Expr(ImmutableDictionary<string, Expr> d) =>
            new ObjectV(d);

        public static implicit operator Expr(bool b) =>
            BoolV.Of(b);

        public static implicit operator Expr(double d) =>
            new DoubleV(d);

        public static implicit operator Expr(long l) =>
            new LongV(l);

        public static implicit operator Expr(int i) =>
            new LongV(i);

        public static implicit operator Expr(string s) =>
            // todo: null Value is bad...
            s == null ? null : new StringV(s);
        #endregion

        #region explicit (downcasting) conversions
        public static explicit operator ImmutableDictionary<string, Expr>(Expr v) =>
            ((ObjectV) v).Value;

        public static explicit operator ImmutableArray<Expr>(Expr v) =>
            ((ArrayV) v).Value;

        public static explicit operator bool(Expr v) =>
            ((BoolV)v).Value;

        public static explicit operator double(Expr v) =>
            ((DoubleV)v).Value;

        public static explicit operator long(Expr v) =>
            ((LongV)v).Value;

        public static explicit operator string(Expr v) =>
            ((StringV)v).Value;
        #endregion

        #region boilerplate
        public override bool Equals(object obj)
        {
            var v = obj as Expr;
            return v != null && Equals(v);
        }

        public abstract bool Equals(Expr v);

        public static bool operator ==(Expr a, Expr b) =>
            object.Equals(a, b);

        public static bool operator !=(Expr a, Expr b) =>
            !object.Equals(a, b);

        public override int GetHashCode() =>
            HashCode();

        // Force subclasses to implement hash code.
        protected abstract int HashCode();
        #endregion

    }
}
