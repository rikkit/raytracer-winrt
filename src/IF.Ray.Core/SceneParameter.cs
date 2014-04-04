using System;

namespace IF.Ray.Core
{
    /// <summary>
    /// Wrapper struct for scene parameters, only use with primitive types!
    /// </summary>
    /// <typeparam name="T">Primitive type</typeparam>
    public struct SceneParameter<T> : IEquatable<T> where T : struct
    {
        private T _value;

        public T Value
        {
            get
            {
                return _value.Equals(default(T))
                    ? Default
                    : _value;
            }
            set { _value = value; }
        }

        public T Default { get; set; }

        public SceneParameter(T start) : this()
        {
            Default = start;
        }

        public void Reset()
        {
            Value = Default;
        }

        #region Object overrides

        public bool Equals(SceneParameter<T> other)
        {
            return Value.Equals(other._value) && Default.Equals(other.Default);
        }

        public bool Equals(T other)
        {
            return Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SceneParameter<T> && Equals((SceneParameter<T>) obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ Default.GetHashCode();
            }
        }

        #endregion

        #region Operator overloads

        public static implicit operator SceneParameter<T>(T value)
        {
            return new SceneParameter<T>(value);
        }

        public static SceneParameter<T> operator +(SceneParameter<T> first, SceneParameter<T> second)
        {
            // this probably won't cause trouble
            dynamic a = first.Value;
            dynamic b = second.Value;
            return new SceneParameter<T>(a + b);
        }

        public static SceneParameter<T> operator -(SceneParameter<T> first, SceneParameter<T> second)
        {
            // this probably won't cause trouble
            dynamic a = first.Value;
            dynamic b = second.Value;
            return new SceneParameter<T>(a - b);
        }

        #endregion
    }
}