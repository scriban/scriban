// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriban.Helpers;
using System.Reflection;
using System.Text;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Base runtime object used to store properties.
    /// </summary>
    /// <seealso cref="System.Collections.IEnumerable" />
    public class ScriptObject : IDictionary<string, object>, IEnumerable, IScriptObject, IDictionary
    {
        internal Dictionary<string, InternalValue> Store { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptObject"/> class.
        /// </summary>
        public ScriptObject() : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptObject"/> class.
        /// </summary>
        public ScriptObject(int capacity) : this(capacity, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptObject"/> class.
        /// </summary>
        /// <param name="capacity">Initial capacity of the dictionary</param>
        /// <param name="autoImportStaticsFromThisType">if set to <c>true</c> it is automatically importing statics members from the derived type.</param>
        public ScriptObject(int capacity, bool? autoImportStaticsFromThisType)
        {
            Store = new Dictionary<string, InternalValue>(capacity);

            // Only import if we are asked for and we have a derived type
            if (this.GetType() != typeof(ScriptObject) && autoImportStaticsFromThisType.GetValueOrDefault())
            {
                this.Import(this.GetType());
            }
        }

        void IDictionary.Add(object key, object value)
        {
            ((IDictionary) Store).Add(key, value);
        }

        /// <summary>
        /// Clears all members stored in this object.
        /// </summary>
        public void Clear()
        {
            Store.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary) Store).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary) Store).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            ((IDictionary) Store).Remove(key);
        }

        bool IDictionary.IsFixedSize
        {
            get { return ((IDictionary) Store).IsFixedSize; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) Store).CopyTo(array, index);
        }

        /// <summary>
        /// Gets the number of members.
        /// </summary>
        public int Count => Store.Count;

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection) Store).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection) Store).SyncRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public virtual bool IsReadOnly { get; set; }

        object IDictionary.this[object key]
        {
            get { return ((IDictionary) Store)[key]; }
            set { ((IDictionary) Store)[key] = value; }
        }

        public IEnumerable<string> GetMembers()
        {
            return Store.Keys;
        }

        /// <summary>
        /// Determines whether this object contains the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if this object contains the specified member; <c>false</c> otherwise</returns>
        /// <exception cref="System.ArgumentNullException">If member is null</exception>
        public virtual bool Contains(string member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return Store.ContainsKey(member);
        }

        /// <summary>
        /// Tries the get the value of the specified member.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="span"></param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was retrieved</returns>
        public virtual bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            InternalValue internalValue;
            var result = Store.TryGetValue(member, out internalValue);
            value = internalValue.Value;
            return result;
        }

        /// <summary>
        /// Gets the value for the specified member and type.
        /// </summary>
        /// <typeparam name="T">Type of the expected member</typeparam>
        /// <param name="name">The name of the member.</param>
        /// <returns>The value or default{T} is the value is different. Note that this method will override the value in this instance if the value doesn't match the type {T} </returns>
        public T GetSafeValue<T>(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var obj = this[name];
            // If value is null, the property does no exist,
            // so we can safely return immediately with the default value
            if (obj == null)
            {
                return default(T);
            }
            if (!(obj is T))
            {
                obj = default(T);
                this[name] = obj;
            }
            return (T)obj;
        }

        bool IDictionary<String,Object>.TryGetValue(string key, out object value)
        {
            return TryGetValue(null, new SourceSpan(), key, out value);
        }

        public virtual object this[string key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                object value;
                TryGetValue(null, new SourceSpan(), key, out value);
                return value;
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                this.AssertNotReadOnly();
                SetValue(null, new SourceSpan(), key, value, false);
            }
        }

        public ICollection<string> Keys => Store.Keys;
        ICollection IDictionary.Values
        {
            get { return ((IDictionary) Store).Values; }
        }

        ICollection IDictionary.Keys
        {
            get { return ((IDictionary) Store).Keys; }
        }

        public ICollection<object> Values
        {
            get { return Store.Values.Select(val => val.Value).ToList(); }
        }

        /// <summary>
        /// Determines whether the specified member is read-only.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if the specified member is read-only</returns>
        public virtual bool CanWrite(string member)
        {
            InternalValue internalValue;
            Store.TryGetValue(member, out internalValue);
            return !internalValue.IsReadOnly;
        }

        /// <summary>
        /// Sets the value and readonly state of the specified member. This method overrides previous readonly state.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="span"></param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        public virtual void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            this.AssertNotReadOnly();
            Store[member] = new InternalValue(value, readOnly);
        }

        public void SetValue(string member, object value, bool readOnly)
        {
            SetValue(null, new SourceSpan(), member, value, readOnly);
        }
        public void Add(string key, object value)
        {
            SetValue(null, new SourceSpan(), key, value, false);
        }

        public bool ContainsKey(string key)
        {
            return Contains(key);
        }

        /// <summary>
        /// Removes the specified member from this object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if it was removed</returns>
        public virtual bool Remove(string member)
        {
            this.AssertNotReadOnly();
            return Store.Remove(member);
        }

        /// <summary>
        /// Sets to read only the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        public void SetReadOnly(string member, bool readOnly)
        {
            this.AssertNotReadOnly();
            InternalValue internalValue;
            if (Store.TryGetValue(member, out internalValue))
            {
                internalValue.IsReadOnly = readOnly;
                Store[member] = internalValue;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="context">The template context requesting this evaluation</param>
        /// <param name="span">The span.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public virtual string ToString(TemplateContext context, SourceSpan span)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var result = new StringBuilder();
            result.Append("{");
            bool isFirst = true;
            foreach (var item in this)
            {
                if (!isFirst)
                {
                    result.Append(", ");
                }
                var keyPair = (KeyValuePair<string, object>)item;
                result.Append(keyPair.Key);
                result.Append(": ");
                result.Append(context.ToString(span, keyPair.Value));
                isFirst = false;
            }
            result.Append("}");
            return result.ToString();
        }

        public virtual void CopyTo(ScriptObject dest)
        {
            if (dest == null) throw new ArgumentNullException(nameof(dest));
            foreach (var keyPair in Store)
            {
                dest.Store[keyPair.Key] = keyPair.Value;
            }
        }

        /// <summary>
        /// Clones the content of this object.
        /// </summary>
        /// <param name="deep">If set to <c>true</c> all <see cref="ScriptObject"/> and <see cref="ScriptArray"/> will be cloned and copied recursively</param>
        public virtual IScriptObject Clone(bool deep)
        {
            var toObject = (ScriptObject)MemberwiseClone();
            toObject.Store = new Dictionary<string, InternalValue>(Store.Count);
            if (deep)
            {
                foreach (var keyPair in Store)
                {
                    var value = keyPair.Value.Value;
                    if (value is ScriptObject)
                    {
                        var fromObject = (ScriptObject) value;
                        value = fromObject.Clone(true);
                    }
                    else if (value is ScriptArray)
                    {
                        var fromArray = (ScriptArray)value;
                        value = fromArray.Clone(true);
                    }
                    toObject.Store[keyPair.Key] = new InternalValue(value, keyPair.Value.IsReadOnly);
                }

            }
            else
            {
                foreach (var keyPair in Store)
                {
                    toObject.Store[keyPair.Key] = keyPair.Value;
                }
            }
            return toObject;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var list = Store.Select(item => new KeyValuePair<string, object>(item.Key, item.Value.Value))
                    .ToList();
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates a <see cref="ScriptObject"/> by importing from the specified object. See remarks.
        /// </summary>
        /// <param name="obj">The object or a type.</param>
        /// <returns>A script object</returns>
        /// <remarks>
        /// <ul>
        /// <li>If <paramref name="obj"/> is a <see cref="System.Type"/>, this method will import only the static field/properties of the specified object.</li>
        /// <li>If <paramref name="obj"/> is a <see cref="ScriptObject"/>, this method will import the members of the specified object into the new object.</li>
        /// <li>If <paramref name="obj"/> is a plain object, this method will import the public fields/properties of the specified object into the <see cref="ScriptObject"/>.</li>
        /// </ul>
        /// </remarks>
        [ScriptMemberIgnore]
        public static ScriptObject From(object obj)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(obj);
            return scriptObject;
        }

        /// <summary>
        /// Determines whether the specified object is importable by the method the various Import methods.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if the object is importable; <c>false</c> otherwise</returns>
        [ScriptMemberIgnore]
        public static bool IsImportable(object obj)
        {
            if (obj == null)
            {
                return true;
            }

            var typeInfo = (obj as Type ?? obj.GetType()).GetTypeInfo();
            return !(obj is string || typeInfo.IsPrimitive || typeInfo == typeof(decimal).GetTypeInfo() || typeInfo.IsEnum || typeInfo.IsArray);
        }

        // Methods for ICollection<KeyValuePair<string, object>> that we don't care to implement

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        internal struct InternalValue
        {
            public InternalValue(object value, bool isReadOnly)
            {
                Value = value;
                IsReadOnly = isReadOnly;
            }

            public InternalValue(object value) : this()
            {
                Value = value;
            }

            public object Value { get; }

            public bool IsReadOnly { get; set; }
        }
    }
}