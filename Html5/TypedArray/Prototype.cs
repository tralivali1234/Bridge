// The documentation for this class (on <summary> tags) was extracted from:
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray/prototype

using System;
using System.Collections;
using System.Collections.Generic;

// This class has its usage limited drastically as it is not supposed to be used by users, unless when
// extending a new TypedArray object that might come into existence.
namespace Bridge.Html5.TypedArray
{
    /// <summary>
    /// Represents the prototype for TypedArray constructors.
    /// TypedArray instances inherit from TypedArray.prototype. You can use the constructor's prototype
    /// object to add properties or methods to all TypedArray instances, where TypedArray is one of the
    /// typed array types.
    /// </summary>
    [External]
    [Namespace(false)]
    public abstract class Prototype<TypedArray, TypedElement> : IList<TypedElement>
    {
        #region Properties

        //TODO: Identify how to represent the Func<x,TypedArray> ctor.
        // <summary>
        // Returns the function that created an instance's prototype. This is one the corresponding
        // typed array type functions by default.
        // </summary>
        //public Func<x, TypedArray> buffer;

        /// <summary>
        /// Returns the ArrayBuffer referenced by the typed array. Fixed at construction time and thus
        /// read only.
        /// </summary>
        public readonly ArrayBuffer Buffer;

        /// <summary>
        /// Returns the length (in bytes) of the typed array from the start of its ArrayBuffer.
        /// Fixed at construction time and thus read only
        /// </summary>
        public readonly int ByteLength;

        /// <summary>
        /// Returns the offset (in bytes) of the typed array from the start of its ArrayBuffer.
        /// Fixed at construction time and thus read only.
        /// </summary>
        public readonly int ByteOffset;

        /// <summary>
        /// Returns the number of elements hold in the typed array. Fixed at construction time and thus
        /// read only.
        /// </summary>
        public readonly int Length;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Copies a sequence of array elements within the array to the position starting at target.
        /// The copy is taken from the index positions of the second and third arguments start and end.
        /// The end argument is optional and defaults to the length of the array.
        /// </summary>
        /// <param name="target">Target start index position where to copy the elements to.</param>
        /// <param name="start">Source start index position where to start copying elements from.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void CopyWithin(Union<int, uint> target, Union<int, uint> start)
        {
            return;
        }

        /// <summary>
        /// Copies a sequence of array elements within the array to the position starting at target.
        /// The copy is taken from the index positions of the second and third arguments start and end.
        /// The end argument is optional and defaults to the length of the array.
        /// </summary>
        /// <param name="target">Target start index position where to copy the elements to.</param>
        /// <param name="start">Source start index position where to start copying elements from.</param>
        /// <param name="end">Optional. Source end index position where to end copying elements from.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void CopyWithin(Union<int, uint> target, Union<int, uint> start,
            Union<int, uint> end)
        {
            return;
        }

        // <summary>
        // Returns a new Array Iterator object that contains the key/value pairs for each index in the array.
        // </summary>
        // <returns>A new Array Iterator object that contains the key/value pairs for each index in the array.</returns>
        // <remarks>This is experimental ECMAScript 6 API that should not be used in production code.</remarks>
        //public TypedArray Entries();

        /// <summary>
        /// Tests whether all elements in the array pass the test provided by a function.
        /// </summary>
        /// <param name="callback">
        /// Function to test for each element, taking three arguments:
        /// currentValue: The current element being processed in the typed array.
        /// index: The index of the current element being processed in the typed array.
        /// array: The typed array every was called upon.
        /// </param>
        /// <param name="thisArg">Optional. Value to use as this when executing callback.</param>
        /// <returns>True if callback returns true for all elements on array, false otherwise.</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public bool Every(Func<TypedElement, Union<int, uint>, TypedArray, bool> callback,
            TypedArray thisArg = default(TypedArray))
        {
            return default(bool);
        }

        /// <summary>
        /// Fills all the elements of an array from a start index to an end index with a static value.
        /// </summary>
        /// <param name="value">Value to fill the typed array with.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void Fill(TypedElement value)
        {
            return;
        }

        /// <summary>
        /// Fills all the elements of an array from a start index to an end index with a static value.
        /// </summary>
        /// <param name="value">Value to fill the typed array with.</param>
        /// <param name="start">Optional. Start index. Defaults to 0.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void Fill(TypedElement value, Union<int, uint> start)
        {
            return;
        }

        /// <summary>
        /// Fills all the elements of an array from a start index to an end index with a static value.
        /// </summary>
        /// <param name="value">Value to fill the typed array with.</param>
        /// <param name="start">Optional. Start index. Defaults to 0.</param>
        /// <param name="end">Optional. End index. Defaults to 0.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void Fill(TypedElement value,
            Union<int, uint> start, Union<int, uint> end)
        {
            return;
        }

        /// <summary>
        /// Creates a new array with all of the elements of this array for which the provided filtering
        /// function returns true.
        /// </summary>
        /// <param name="callback">
        /// Function to test each element of the typed array. Invoked with arguments
        /// (element, index, typedarray). Return true to keep the element, false otherwise.
        /// </param>
        /// <param name="thisArg">Value to use as this when executing callback.</param>
        /// <returns></returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedArray Filter(Func<TypedElement, Union<int, uint>, TypedArray, bool> callback,
            TypedArray thisArg = default(TypedArray))
        {
            return default(TypedArray);
        }

        // find() pertains to ECMAScript 6 proposal and should not be used in production environment.
        // findIndex() pertains to ECMAScript 6 proposal and should not be used in production environment.

        /// <summary>
        /// Calls a function for each element in the array.
        /// </summary>
        /// <param name="callback">
        /// Function that produces an element of the new typed array, taking three arguments:
        /// currentValue: The current element being processed in the typed array.
        /// index: The index of the current element being processed in the typed array.
        /// array: The array forEach() was called upon.
        /// </param>
        /// <param name="thisArg">Optional. Value to use as 'this' when executing callback.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void ForEach(Func<TypedElement, Union<int, uint>, TypedArray, TypedElement> callback,
            TypedArray thisArg = default(TypedArray))
        {
            return;
        }

        /// <summary>
        /// Returns the first (least) index of an element within the array equal to the specified
        /// value, or -1 if none is found.
        /// </summary>
        /// <param name="searchElement">Element to locate in the typed array.</param>
        /// <param name="fromIndex">
        /// The index to start the search at. If the index is greater than or equal to the typed array's
        /// length, -1 is returned, which means the typed array will not be searched. If the provided
        /// index value is a negative number, it is taken as the offset from the end of the typed array.
        /// Note: if the provided index is negative, the typed array is still searched from front to back.
        /// If the calculated index is less than 0, then the whole typed array will be searched.
        /// Default: 0 (entire typed array is searched).
        /// </param>
        /// <returns>
        /// The first index at which a given element can be found in the typed array, or -1 if it is
        /// not present.
        /// </returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public int IndexOf(TypedElement searchElement, Union<int, uint> fromIndex)
        {
            return default(int);
        }

        /// <summary>
        /// Joins all elements of an array into a string.
        /// </summary>
        /// <param name="separator">
        /// Optional. Specifies a string to separate each element. The separator is converted to a string
        /// if necessary. If omitted, the typed array elements are separated with a comma (",").
        /// </param>
        /// <returns>A string containing the elements, separated by "," or the specified separator.</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public string Join(char separator = ',')
        {
            return default(string);
        }

        /// <summary>
        /// Joins all elements of an array into a string.
        /// </summary>
        /// <param name="separator">
        /// Optional. Specifies a string to separate each element. The separator is converted to a string
        /// if necessary. If omitted, the typed array elements are separated with a comma (",").
        /// </param>
        /// <returns>A string containing the elements, separated by "," or the specified separator.</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public string Join(string separator)
        {
            return default(string);
        }

        // keys() pertains to ECMAScript 6 proposal and should not be used in production environment.

        /// <summary>
        /// Returns the last (greatest) index of an element within the array equal to the specified
        /// value, or -1 if none is found.
        /// </summary>
        /// <param name="searchElement">Element to locate in the typed array.</param>
        /// <returns>
        /// The last index at which a given element can be found in the typed array, or -1 if it is
        /// not present.
        /// </returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public int LastIndexOf(TypedElement searchElement)
        {
            return default(int);
        }

        /// <summary>
        /// Returns the last (greatest) index of an element within the array equal to the specified
        /// value, or -1 if none is found.
        /// </summary>
        /// <param name="searchElement">Element to locate in the typed array.</param>
        /// <param name="fromIndex">
        /// The index at which to start searching backwards. Defaults to the typed array's length, i.e.
        /// the whole typed array will be searched. If the index is greater than or equal to the length
        /// of the typed array, the whole typed array will be searched. If negative, it is taken as the
        /// offset from the end of the typed array. Note that even when the index is negative, the typed
        /// array is still searched from back to front. If the calculated index is less than 0, -1 is
        /// returned, i.e. the typed array will not be searched.
        /// </param>
        /// <returns>
        /// The last index at which a given element can be found in the typed array, or -1 if it is
        /// not present.
        /// </returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public int LastIndexOf(TypedElement searchElement, Union<int, uint> fromIndex)
        {
            return default(int);
        }

        /// <summary>
        /// Creates a new array with the results of calling a provided function on every element in this array.
        /// </summary>
        /// <param name="callback">
        /// Function that produces an element of the new typed array, taking three arguments:
        /// currentValue: The current element being processed in the typed array.
        /// index: The index of the current element being processed in the typed array.
        /// array: The array forEach() was called upon.
        /// </param>
        /// <param name="thisArg">Optional. Value to use as 'this' when executing callback.</param>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedArray Map(Func<TypedElement, Union<int, uint>, TypedArray, TypedElement> callback,
            TypedArray thisArg = default(TypedArray))
        {
            return default(TypedArray);
        }

        // move() is marked as obsolete since Gecko 34. It is now copyWithin()

        /// <summary>
        /// Apply a function against an accumulator and each value of the array (from left-to-right)
        /// as to reduce it to a single value.
        /// </summary>
        /// <param name="callback">
        /// Function to execute on each value in the typed array, taking four arguments:
        /// previousValue: The value previously returned in the last invocation of the callback, or initialValue,
        /// if supplied (see below).
        /// currentValue: The current element being processed in the typed array.
        /// index: The index of the current element being processed in the typed array.
        /// array: The typed array reduce was called upon.
        /// </param>
        /// <param name="initialValue">
        /// Optional. Object to use as the first argument to the first call of the callback.
        /// </param>
        /// <returns></returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedElement Reduce(
            Func<TypedElement, TypedElement, Union<int, uint>, TypedArray, TypedElement> callback,
            TypedElement initialValue = default(TypedElement))
        {
            return default(TypedElement);
        }

        /// <summary>
        /// Apply a function against an accumulator and each value of the array (from right-to-left)
        /// as to reduce it to a single value.
        /// </summary>
        /// <param name="callback">
        /// Function to execute on each value in the typed array, taking four arguments:
        /// previousValue: The value previously returned in the last invocation of the callback, or initialValue,
        /// if supplied (see below).
        /// currentValue: The current element being processed in the typed array.
        /// index: The index of the current element being processed in the typed array.
        /// array: The typed array reduce was called upon.
        /// </param>
        /// <param name="initialValue">
        /// Optional. Object to use as the first argument to the first call of the callback.
        /// </param>
        /// <returns></returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedElement ReduceRight(
            Func<TypedElement, TypedElement, Union<int, uint>, TypedArray, TypedElement> callback,
            TypedElement initialValue = default(TypedElement))
        {
            return default(TypedElement);
        }

        /// <summary>
        /// Reverses the order of the elements of an array — the first becomes the last,
        /// and the last becomes the first.
        /// </summary>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public void Reverse()
        {
            return;
        }

        /// <summary>
        /// Stores multiple values in the typed array, reading input values from a specified array.
        /// </summary>
        /// <param name="array">
        /// The array from which to copy values. All values from the source array are copied into the
        /// target array, unless the length of the source array plus the offset exceeds the length of
        /// the target array, in which case an exception is thrown.
        /// </param>
        public void Set(Array array)
        {
            return;
        }

        /// <summary>
        /// Stores multiple values in the typed array, reading input values from a specified array.
        /// </summary>
        /// <param name="array">
        /// The array from which to copy values. All values from the source array are copied into the
        /// target array, unless the length of the source array plus the offset exceeds the length of
        /// the target array, in which case an exception is thrown.
        /// </param>
        /// <param name="offset">
        /// The offset into the target array at which to begin writing values from the source array.
        /// If you omit this value, 0 is assumed (that is, the source array will overwrite values in
        /// the target array starting at index 0).
        /// </param>
        public void Set(Array array, Union<int, uint> offset)
        {
            return;
        }

        public void Set(TypedArray typedArray)
        {
            return;
        }

        /// <summary>
        /// Stores multiple values in the typed array, reading input values from a specified array.
        /// </summary>
        /// <param name="typedArray">
        /// If the source array is a typed array, the two arrays may share the same underlying
        /// ArrayBuffer; the browser will intelligently copy the source range of the buffer to the
        /// destination range.
        /// </param>
        /// <param name="offset">
        /// The offset into the target array at which to begin writing values from the source array.
        /// If you omit this value, 0 is assumed (that is, the source array will overwrite values in
        /// the target array starting at index 0).
        /// </param>
        public void Set(TypedArray typedArray, Union<int, uint> offset)
        {
            return;
        }

        /// <summary>
        /// Extracts a section of an array and returns a new array.
        /// </summary>
        /// <returns>A shallow copy of elements from the original typed array</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedArray Slice()
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Extracts a section of an array and returns a new array.
        /// </summary>
        /// <param name="begin">
        /// Zero-based index at which to begin extraction.
        /// As a negative index, begin indicates an offset from the end of the sequence.
        /// slice(-2) extracts the last two elements in the sequence.
        /// If begin is omitted, slice begins from index 0.
        /// </param>
        /// <returns>A shallow copy of elements from the original typed array</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedArray Slice(Union<int, uint> begin)
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Extracts a section of an array and returns a new array.
        /// </summary>
        /// <param name="begin">
        /// Zero-based index at which to begin extraction.
        /// As a negative index, begin indicates an offset from the end of the sequence.
        /// slice(-2) extracts the last two elements in the sequence.
        /// If begin is omitted, slice begins from index 0.
        /// </param>
        /// <param name="end">
        /// Zero-based index at which to end extraction.
        /// slice() extracts up to but not including end.slice(1,4) extracts the second element up to
        /// the fourth element (elements indexed 1, 2, and 3).
        /// As a negative index, end indicates an offset from the end of the sequence.
        /// slice(2,-1) extracts the third element through the second-to-last element in the sequence.
        /// If end is omitted, slice extracts to the end of the sequence (arr.length).
        /// </param>
        /// <returns>A shallow copy of elements from the original typed array</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedArray Slice(Union<int, uint> begin, Union<int, uint> end)
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Returns true if at least one element in this array satisfies the provided testing function.
        /// </summary>
        /// <param name="callback">
        /// Function to test for each element, taking three arguments:
        /// currentValue: The current element being processed in the typed array.
        /// index: The index of the current element being processed in the typed array.
        /// array: The array some() was called upon.
        /// </param>
        /// <param name="thisArg">Optional. Value to use as 'this' when executing callback.</param>
        /// <returns>
        /// True if at least one element in this array satisfies the provided testing function.
        /// False otherwise.
        /// </returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public bool Some(Func<TypedElement, Union<int, uint>, TypedArray, TypedElement> callback,
            TypedArray thisArg = default(TypedArray))
        {
            return default(bool);
        }

        /// <summary>
        /// Sorts the elements of an array in place and returns the array.
        /// </summary>
        /// <returns>Sorted TypedArray.</returns>
        /// <remarks>Most browsers do not support this yet.</remarks>
        public TypedArray Sort()
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Sorts the elements of an array in place and returns the array.
        /// </summary>
        /// <param name="compareFunction">
        /// Specifies a function that defines the sort order. If omitted, the array is sorted according to
        /// each character's Unicode code point value, according to the string conversion of each element.
        /// Example: int compareFunction(a, b) { ... };
        /// </param>
        /// <remarks>
        /// Most browsers do not support this yet.
        ///
        /// If compareFunction is supplied, the array elements are sorted according to the return value of the
        /// compare function. If a and b are two elements being compared, then:
        /// • If compareFunction(a, b) is less than 0, sort a to a lower index than b, i.e. a comes first.
        /// • If compareFunction(a, b) returns 0, leave a and b unchanged with respect to each other, but sorted
        ///   with respect to all different elements. Note: the ECMAscript standard does not guarantee this
        ///   behaviour, and thus not all browsers (e.g. Mozilla versions dating back to at least 2003) respect
        ///   this.
        /// • If compareFunction(a, b) is greater than 0, sort b to a lower index than a.
        /// • compareFunction(a, b) must always return the same value when given a specific pair of elements a
        ///   and b as its two arguments. If inconsistent results are returned then the sort order is undefined.
        /// </remarks>
        /// <returns>Sorted TypedArray.</returns>
        public TypedArray Sort(Func<TypedElement, TypedElement, int> compareFunction)
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Returns a new TypedArray from the given start and end element index.
        /// </summary>
        /// <returns>
        /// A new TypedArray on the same ArrayBuffer store and with the same element types
        /// as for this TypedArray object.
        /// </returns>
        /// <remarks>
        /// Some browsers do not support this yet (may require begin and end parameters).
        /// </remarks>
        [Name("subarray")]
        public TypedArray SubArray()
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Returns a new TypedArray from the given start and end element index.
        /// </summary>
        /// <param name="begin">Element to begin at. The offset is inclusive.</param>
        /// <returns>
        /// A new TypedArray on the same ArrayBuffer store and with the same element types
        /// as for this TypedArray object.
        /// </returns>
        [Name("subarray")]
        public TypedArray SubArray(Union<int, uint> begin)
        {
            return default(TypedArray);
        }

        /// <summary>
        /// Returns a new TypedArray from the given start and end element index.
        /// </summary>
        /// <param name="begin">Element to begin at. The offset is inclusive.</param>
        /// <param name="end">
        /// Element to end at. The offset is exclusive. If not specified, all elements from the one
        /// specified by begin to the end of the array are included in the new view.
        /// </param>
        /// <returns>
        /// A new TypedArray on the same ArrayBuffer store and with the same element types
        /// as for this TypedArray object.
        /// </returns>
        [Name("subarray")]
        public TypedArray SubArray(Union<int, uint> begin, Union<int, uint> end)
        {
            return default(TypedArray);
        }

        // values() pertains to ECMAScript 6 proposal and should not be used in production environment.

        /// <summary>
        /// The elements are converted to Strings using their toLocaleString methods and these Strings
        /// are separated by a locale-specific String (such as a comma “,”).
        /// </summary>
        /// <returns>A localized string representing the array and its elements.</returns>
        public override string ToLocaleString()
        {
            return default(string);
        }

        /// <summary>
        /// Joins the array and returns one string containing each array element separated by commas.
        /// </summary>
        /// <returns>A string representing the array and its elements.</returns>
        public override string ToString()
        {
            return default(string);
        }

        // TypedArray.prototype[@@iterator]() pertains to ECMAScript 6 proposal and should not be used in
        // production environment.

        #endregion Methods

        #region IList
        extern TypedElement IList<TypedElement>.this[int index]
        {
            get;
            set;
        }

        extern int ICollection<TypedElement>.Count
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the TypedArray is read-only.
        /// </summary>
        extern bool ICollection<TypedElement>.IsReadOnly
        {
            get;
        }

        extern void ICollection<TypedElement>.Add(TypedElement item);

        extern void ICollection<TypedElement>.Clear();

        [Template("System.Array.contains({this}, {item})")]
        public extern bool Contains(TypedElement item);

        extern bool ICollection<TypedElement>.Contains(TypedElement item);

        extern void ICollection<TypedElement>.CopyTo(TypedElement[] array, int arrayIndex);

        [Template("Bridge.getEnumerator({this})")]
        public extern IEnumerator<TypedElement> GetEnumerator();

        [Template("System.Array.indexOf({this}, {item})")]
        public extern int IndexOf(TypedElement item);

        extern void IList<TypedElement>.Insert(int index, TypedElement item);

        extern bool ICollection<TypedElement>.Remove(TypedElement item);

        extern void IList<TypedElement>.RemoveAt(int index);

        extern IEnumerator IEnumerable.GetEnumerator();
        #endregion IList
    }
}