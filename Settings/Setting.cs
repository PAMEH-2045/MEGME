using BlackStartX.GestureManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MEGME.Settings
{
    public interface ISetting
    {
        public string Key { get; }

        public Type Type { get; }

        public event Action<ISetting> OnChange;

        public abstract object BoxedValue { get; set; }

        public abstract void Init();

        public abstract void SyncWithReference();

        public abstract void ApplyToReference();
    }

    public sealed class Setting<T> : ISetting
    {
        public string Key { get; }

        public Type Type { get; }

        T value;

        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                    return;

                InternalSet(value);

                Save();

                OnChange?.Invoke(this);
            }
        }

        public event Action<ISetting> OnChange;

        readonly ValueRef<T> valueRef;

        public object BoxedValue
        {
            get => Value;
            set => Value = (T)value;
        }

        public Setting(string key, T defaultValue, ValueRef<T> valueRef = null)
        {
            this.Key = key;
            this.Type = typeof(T);
            this.value = defaultValue;
            this.valueRef = valueRef;

            SettingsManager.Register(this);
        }
        public static Setting<T> From(ValueRef<T> valueRef, string key = null)
        {
            T defaultValue = default;
            //T defaultValue = valueRef.Value;
            key ??= GenerateKey(valueRef);
            return new(key, defaultValue, valueRef);
        }
        public static Setting<T> From(object instOrLookup, FieldInfo field, string key = null) => From(ValueRef<T>.From(instOrLookup, field), key);
        public static Setting<T> From(object instOrLookup, PropertyInfo property, string key = null) => From(ValueRef<T>.From(instOrLookup, property), key);
        public static Setting<T> Create(string key, T defaultValue = default)
        {
            return new(key, defaultValue);
        }

        public void InternalSet(T value)
        {
            this.value = value;

            valueRef?.SetValue(value);
        }
        public void SyncWithReference()
        {
            if (valueRef == null)
                return;

            Value = valueRef.Value;
        }
        public void ApplyToReference()
        {
            if (valueRef == null)
                return;

            valueRef.Value = Value;
        }
        public void Init()
        {
            if (SettingsCacheHandler.Cache.TryGetValue(Key, out var token))
            {
                Value = token.ToObject<T>();
            }
        }
        void Save()
        {
            Debug.Log($"SAVE {Key}");
            SettingsCacheHandler.Cache[Key] = JToken.FromObject(Value);
            SettingsCacheHandler.MarkDirty();
        }
        static string GenerateKey(ValueRef<T> valueRef)
        {
            return $"{valueRef.Info.ReflectedType.FullName}.{valueRef.Info.Name}";
        }
        public static implicit operator T(Setting<T> setting)
        {
            return setting.Value;
        }
    }
}
