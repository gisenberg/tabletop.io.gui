//     Linq.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tabletop.io.Linq {
    public static class Enumerable {
        public static bool All<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            foreach (T o in src)
                if (!pred(o))
                    return false;
            return true;
        }

        public static bool Any<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            foreach (T o in src)
                if (pred(o))
                    return true;
            return false;
        }

        public static bool Any<T> (this IEnumerable<T> src) {
            foreach (T o in src)
                return true;
            return false;
        }

        public static IEnumerable<T> Cast<T> (this IEnumerable src) {
            foreach (var o in src)
                yield return (T)o;
        }

        public static IEnumerable<T> Concat<T> (this IEnumerable<T> src1, IEnumerable<T> src2) {
            foreach (T o in src1)
                yield return o;
            foreach (T o in src2)
                yield return o;
        }

        public static bool Contains<T> (this IEnumerable<T> src, T val) {
            foreach (T o in src)
                if (EqualityComparer<T>.Default.Equals(o, val))
                    return true;
            return false;
        }

        public static bool Contains<T> (this IEnumerable<T> src, T val, IEqualityComparer<T> cmp) {
            foreach (T o in src)
                if (cmp.Equals(o, val))
                    return true;
            return false;
        }

        public static int Count<T> (this IEnumerable<T> src) {
            var count = 0;
            foreach (T o in src)
                count++;
            return count;
        }

        public static int Count<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            var count = 0;
            foreach (T o in src)
                if (pred(o))
                    count++;
            return count;
        }

        public static IEnumerable<T> DefaultIfEmpty<T>(this IEnumerable<T> src, T def) {
            int values = 0;

            foreach (T o in src) {
                yield return o;
                values++;
            }

            if (values == 0)
                yield return def;
        }

        public static IEnumerable<T> Empty<T> () {
            yield break;
        }

        public static T FirstOrDefault<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            foreach (T o in src)
                if (pred(o))
                    return o;
            return default(T);
        }

        public static T FirstOrDefault<T> (this IEnumerable<T> src) {
            foreach (T o in src)
                return o;
            return default(T);
        }

        public static T First<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            foreach (T o in src)
                if (pred(o))
                    return o;
            throw new InvalidOperationException("Element not found.");
        }

        public static T First<T> (this IEnumerable<T> src) {
            foreach (T o in src)
                return o;
            throw new InvalidOperationException("Element not found.");
        }

        public static T Last<T> (this IEnumerable<T> src) {
            var ie = src.GetEnumerator();
            if (ie.MoveNext()) {
                T val = ie.Current;
                while (ie.MoveNext())
                    val = ie.Current;
                return val;
            } else
                throw new InvalidOperationException("Element not found.");
        }

        public static T Last<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            var ie = src.GetEnumerator();
            while (ie.MoveNext()) {
                if (pred(ie.Current)) {
                    T val = ie.Current;
                    while (ie.MoveNext()) {
                        if (pred(ie.Current))
                            val = ie.Current;
                    }
                    return val;
                }
            }
            throw new InvalidOperationException("Element not found.");
        }

        public static T LastOrDefault<T> (this IEnumerable<T> src) {
            T val = default(T);
            foreach (T o in src)
                val = o;
            return val;
        }

        public static T LastOrDefault<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            T val = default(T);
            foreach (T o in src)
                if (pred(o))
                    val = o;
            return val;
        }

        public static float Max (this IEnumerable<float> src) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            float acc = iee.Current;
            while (iee.MoveNext())
                acc = Math.Max(iee.Current, acc);
            return acc;
        }

        public static int Max (this IEnumerable<int> src) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            int acc = iee.Current;
            while (iee.MoveNext())
                acc = Math.Max(iee.Current, acc);
            return acc;
        }

        public static int Max<T> (this IEnumerable<T> src, Func<T, int> sel) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            int acc = sel(iee.Current);
            while (iee.MoveNext())
                acc = Math.Max(sel(iee.Current), acc);
            return acc;
        }

        public static float Max<T> (this IEnumerable<T> src, Func<T, float> sel) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            float acc = sel(iee.Current);
            while (iee.MoveNext())
                acc = Math.Max(sel(iee.Current), acc);
            return acc;
        }

        public static float Min (this IEnumerable<float> src) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            float acc = iee.Current;
            while (iee.MoveNext())
                acc = Math.Min(iee.Current, acc);
            return acc;
        }

        public static int Min (this IEnumerable<int> src) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            int acc = iee.Current;
            while (iee.MoveNext())
                acc = Math.Min(iee.Current, acc);
            return acc;
        }

        public static int Min<T> (this IEnumerable<T> src, Func<T, int> sel) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            int acc = sel(iee.Current);
            while (iee.MoveNext())
                acc = Math.Min(sel(iee.Current), acc);
            return acc;
        }

        public static float Min<T> (this IEnumerable<T> src, Func<T, float> sel) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                throw new InvalidOperationException("Empty enumeration");
            float acc = sel(iee.Current);
            while (iee.MoveNext())
                acc = Math.Min(sel(iee.Current), acc);
            return acc;
        }

        public static IEnumerable<T> OrderBy<T, K>(this IEnumerable<T> src, Func<T, K> keySelector) {
            var vals = new List<T>(src);

            vals.Sort((a, b) => Comparer.Default.Compare(keySelector(a), keySelector(b)));

            return vals;
        }

        public static IEnumerable<T> OfType<T> (this IEnumerable src) {
            foreach (object o in src)
                if (o is T)
                    yield return (T)o;
        }

        public static IEnumerable<int> Range (int start, int count) {
            while (count-- > 0)
                yield return start++;
        }

        public static IEnumerable<T> Repeat<T> (T elem, int count) {
            while (count-- > 0)
                yield return elem;
        }

        public static IEnumerable<T> Reverse<T> (this IEnumerable<T> src) {
            var stack = new Stack<T>();
            foreach (T o in src)
                stack.Push(o);
            while (stack.Count > 0)
                yield return stack.Pop();
        }

        public static IEnumerable<U> Select<T, U> (this IEnumerable<T> src, Func<T, U> pred) {
            foreach (T o in src)
                yield return pred(o);
        }

        public static IEnumerable<U> Select<T, U> (this IEnumerable<T> src, Func<T, int, U> pred) {
            var idx = 0;
            foreach (T o in src)
                yield return pred(o, idx++);
        }

        public static IEnumerable<U> SelectMany<T, U> (this IEnumerable<T> src, Func<T, IEnumerable<U>> sel) {
            foreach (T o in src)
                foreach (U o2 in sel(o))
                    yield return o2;
        }

        public static IEnumerable<U> SelectMany<T, U> (this IEnumerable<T> src, Func<T, int, IEnumerable<U>> sel) {
            var idx = 0;
            foreach (T o in src)
                foreach (U o2 in sel(o, idx++))
                    yield return o2;
        }

        public static bool SequenceEqual<T> (this IEnumerable<T> src1, IEnumerable<T> src2) {
            return src1.SequenceEqual(src2, EqualityComparer<T>.Default);
        }

        public static bool SequenceEqual<T> (this IEnumerable<T> src1, IEnumerable<T> src2, IEqualityComparer<T> cmp) {
            var ie1 = src1.GetEnumerator();
            var ie2 = src2.GetEnumerator();
            bool s1, s2 = false;
            while ((s1 = ie1.MoveNext()) && (s2 = ie2.MoveNext())) {
                if (!cmp.Equals(ie1.Current, ie2.Current))
                    return false;
            }
            return !s1 && !s2;
        }

        public static T Single<T> (this IEnumerable<T> src) {
            var ie = src.GetEnumerator();
            if (ie.MoveNext()) {
                T val = ie.Current;
                if (ie.MoveNext())
                    throw new InvalidOperationException("More than one element.");
                return val;
            }
            throw new InvalidOperationException("Element not found.");
        }

        public static T Single<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            var ie = src.GetEnumerator();
            while (ie.MoveNext()) {
                if (pred(ie.Current)) {
                    T val = ie.Current;
                    while(ie.MoveNext())
                        if(pred(ie.Current))
                            throw new InvalidOperationException("More than one element.");
                    return val;
                }
            }
            throw new InvalidOperationException("Element not found.");
        }

        public static IEnumerable<T> Skip<T> (this IEnumerable<T> src, int num) {
            foreach (T o in src) {
                if (num > 0)
                    num--;
                else
                    yield return o;
            }
        }

        public static IEnumerable<T> SkipWhile<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            var ie = src.GetEnumerator();
            while (ie.MoveNext()) {
                if (!pred(ie.Current)) {
                    do {
                        yield return ie.Current;
                    } while (ie.MoveNext());
                    yield break;
                }
            }
        }

        public static IEnumerable<T> SkipWhile<T> (this IEnumerable<T> src, Func<T, int, bool> pred) {
            var ie = src.GetEnumerator();
            var idx = 0;
            while (ie.MoveNext()) {
                if (!pred(ie.Current, idx++)) {
                    do {
                        yield return ie.Current;
                    } while (ie.MoveNext());
                    yield break;
                }
            }
        }

        public static float Sum (this IEnumerable<float> src) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                return 0.0f;
            float acc = iee.Current;
            while (iee.MoveNext())
                acc += iee.Current;
            return acc;
        }

        public static int Sum (this IEnumerable<int> src) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                return 0;
            int acc = iee.Current;
            while (iee.MoveNext())
                acc += iee.Current;
            return acc;
        }

        public static int Sum<T> (this IEnumerable<T> src, Func<T, int> sel) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                return 0;
            int acc = sel(iee.Current);
            while (iee.MoveNext())
                acc += sel(iee.Current);
            return acc;
        }

        public static float Sum<T> (this IEnumerable<T> src, Func<T, float> sel) {
            var iee = src.GetEnumerator();
            if (!iee.MoveNext())
                return 0.0f;
            float acc = sel(iee.Current);
            while (iee.MoveNext())
                acc += sel(iee.Current);
            return acc;
        }

        public static IEnumerable<T> Take<T> (this IEnumerable<T> src, int num) {
            foreach (T o in src) {
                if (num > 0) {
                    num--;
                    yield return o;
                } else
                    yield break;
            }
        }

        public static IEnumerable<T> TakeWhile<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            foreach (T o in src) {
                if (!pred(o))
                    yield break;
                yield return o;
            }
        }

        public static IEnumerable<T> TakeWhile<T> (this IEnumerable<T> src, Func<T, int, bool> pred) {
            var idx = 0;
            foreach (T o in src) {
                if (!pred(o, idx++))
                    yield break;
                yield return o;
            }
        }

        public static T[] ToArray<T> (this IEnumerable<T> src) {
            var coll = src as ICollection<T>;
            if (coll != null) {
                var arr = new T[coll.Count];
                coll.CopyTo(arr, 0);
                return arr;
            } else {
                return src.ToList().ToArray();
            }
        }

        public static Dictionary<U, T> ToDictionary<T, U> (this IEnumerable<T> src, Func<T, U> selKey, IEqualityComparer<U> cmp) {
            var dict = new Dictionary<U, T>(cmp);
            foreach (T o in src)
                dict.Add(selKey(o), o);
            return dict;
        }

        public static Dictionary<U, V> ToDictionary<T, U, V> (this IEnumerable<T> src, Func<T, U> selKey, Func<T, V> selElem, IEqualityComparer<U> cmp) {
            var dict = new Dictionary<U, V>(cmp);
            foreach (T o in src)
                dict.Add(selKey(o), selElem(o));
            return dict;
        }

        public static Dictionary<U, T> ToDictionary<T, U> (this IEnumerable<T> src, Func<T, U> selKey) {
            return src.ToDictionary(selKey, EqualityComparer<U>.Default);
        }

        public static Dictionary<U, V> ToDictionary<T, U, V> (this IEnumerable<T> src, Func<T, U> selKey, Func<T, V> selElem) {
            return src.ToDictionary(selKey, selElem, EqualityComparer<U>.Default);
        }

        public static List<T> ToList<T> (this IEnumerable<T> src) {
            var list = new List<T>();
            list.AddRange(src);
            return list;
        }

        public static IEnumerable<T> Where<T> (this IEnumerable<T> src, Func<T, bool> pred) {
            foreach (T o in src)
                if (pred(o))
                    yield return o;
        }

        public static IEnumerable<T> Where<T> (this IEnumerable<T> src, Func<T, int, bool> pred) {
            var idx = 0;
            foreach (T o in src)
                if (pred(o, idx++))
                    yield return o;
        }
    }
}
