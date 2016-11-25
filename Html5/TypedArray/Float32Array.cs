// The documentation for this class (on <summary> tags) was extracted from:
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Float32Array

namespace Bridge.Html5
{
    /// <summary>
    /// Represents an array of 32-bit floating point numbers (corresponding to the C 'float' data type)
    /// in the platform byte order. If control over byte order is needed, use DataView instead.
    /// The contents are initialized to 0. Once established, you can reference elements in the array
    /// using the object's methods, or using standard array index syntax (that is, using bracket
    /// notation).
    /// </summary>
    [External]
    [Namespace(false)]
    public class Float32Array : TypedArray.Prototype<Float32Array, float>
    {
        /// <summary>
        /// Creates a new Float32Array of the specified length.
        /// </summary>
        /// <param name="length">Length of array to create</param>
        public Float32Array(Union<int, uint> length)
        {
        }

        /// <summary>
        /// Creates a new Float32Array out of the specified Float32Array.
        /// </summary>
        /// <param name="typedArray">Float32Array to use as initial contents to the new array.</param>
        public Float32Array(Float32Array typedArray)
        {
        }

        /// <summary>
        /// Creates a new Float32Array out of the specified object.
        /// </summary>
        /// <param name="objectInstance">Object to be converted into the new array.</param>
        public Float32Array(object objectInstance)
        {
        }

        /// <summary>
        /// Creates a new Float32Array out of the specified buffer and byteOffset with the specified length.
        /// </summary>
        /// <param name="buffer">Arraybuffer to use as base for contents.</param>
        /// <param name="byteOffset">Optional. Position in the buffer to start reading data from.</param>
        /// <param name="length">Optional. Number of elements to consider while creating the array.</param>
        public Float32Array(ArrayBuffer buffer, uint byteOffset = 0, uint length = 0)
        {
        }

        /// <summary>
        /// Allows index operations on the array.
        /// </summary>
        /// <param name="i">Index position in the array.</param>
        /// <returns>The element in the specified position.</returns>
        public float this[Union<int, uint> i]
        {
            get
            {
                return default(float);
            }
            set
            {
            }
        }

        #region Properties

        /// <summary>
        /// Returns a number value of the element size. 1 in the case of an Float32Array.
        /// </summary>
        public const short BYTES_PER_ELEMENT = 1;

        /// <summary>
        /// Length property whose value is 3.
        /// </summary>
        public const short length = 3;

        /// <summary>
        /// Returns the string value of the constructor name. In the case of the Float32Array type: "Float32Array".
        /// </summary>
        public const string name = "Float32Array";

        #endregion Properties

        #region Methods

        // Float32Array.from() and Float32Array.of() are work in progress for ES6 and, at the time of the writing,
        // were not ready for production environment. Thus, their definitions are not present here.

        #endregion Methods
    }
}