using System;
using System.Reflection;

namespace MEGME.Settings
{
    public class ValueRef<T>
    {
        readonly Func<T> Get;
        readonly Action<T> Set;

        public readonly MemberInfo Info;

        public T Value
        {
            get => Get();
            set => Set(value);
        }

        ValueRef(MemberInfo info, Func<T> get, Action<T> set)
        {
            Info = info;

            Get = get;
            Set = set;
        }

        public static ValueRef<T> From(object instOrLookup, FieldInfo field)
        {
            if (typeof(T) != field.FieldType)
                throw new ArgumentException();

            var getTarget = GetLookup(instOrLookup, field.IsStatic);
            return new ValueRef<T>(
                field,
                () => (T)field.GetValue(getTarget()),
                v => field.SetValue(getTarget(), v)
            );
        }
        public static ValueRef<T> From(object instOrLookup, PropertyInfo property)
        {
            if (typeof(T) != property.PropertyType)
                throw new ArgumentException();

            var getTarget = GetLookup(instOrLookup, property.GetMethod.IsStatic);
            return new ValueRef<T>(
                property,
                () => (T)property.GetValue(getTarget()),
                v => property.SetValue(getTarget(), v)
            );
        }

        static Func<object> GetLookup(object inst, bool isStatic)
        {
            return inst switch
            {
                Func<object> getInst => getInst,
                not null => () => inst,
                null when isStatic => () => null,
                _ => throw new TargetException()
            };
        }

        public void SetValue(T value)
        {
            Value = value;
        }
    }
}
