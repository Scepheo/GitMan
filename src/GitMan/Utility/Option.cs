using System;
using System.Collections.Generic;
using System.Linq;

namespace GitMan.Utility
{
    internal static class Option
    {
        public static Option<T> None<T>()
        {
            return new Option<T>.None();
        }

        public static Option<T> Some<T>(T value)
        {
            return new Option<T>.Some(value);
        }

        public static Option<T> Make<T>(T? value)
            where T : class
        {
            return value == null ? None<T>() : Some<T>(value);
        }

        public static Option<T> Make<T>(T? value)
            where T : struct
        {
            return value.HasValue ? None<T>() : Some<T>(value.GetValueOrDefault());
        }

        public static Option<TValue> OptionGet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            var inDictionary = dictionary.TryGetValue(key, out var value);
            return inDictionary ? Some(value) : None<TValue>();
        }

        public static Option<T> OptionSingle<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
            where T : class
        {
            T? single = enumerable.SingleOrDefault(predicate);
            return Make(single);
        }
    }

    internal abstract class Option<T>
    {
        private Option() {}

        public abstract Option<TResult> Map<TResult>(Func<T, TResult> func);
        public abstract Option<T> If(Func<T, bool> predicate);

        public sealed class None : Option<T>
        {
            public override Option<T> If(Func<T, bool> predicate)
            {
                return this;
            }

            public override Option<TResult> Map<TResult>(Func<T, TResult> func)
            {
                return Option.None<TResult>();
            }

            public void Deconstruct() { }
        }

        public sealed class Some : Option<T>
        {
            public T Value { get; }
            
            public Some(T value)
            {
                Value = value;
            }

            public override Option<TResult> Map<TResult>(Func<T, TResult> func)
            {
                var newValue = func(Value);
                return Option.Some(newValue);
            }

            public override Option<T> If(Func<T, bool> predicate)
            {
                var condition = predicate(Value);
                return condition ? this : Option.None<T>();
            }

            public void Deconstruct(out T value)
            {
                value = Value;
            }
        }
    }
}
