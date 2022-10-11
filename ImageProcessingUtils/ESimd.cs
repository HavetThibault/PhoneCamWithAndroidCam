using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils
{
    public enum ESimdOperationBinary8uType
    {
        /// <summary>
        /// Computes the average value for every channel of every point of two images.
        /// Average(a, b) = (a + b + 1)/2.
        /// </summary>
        SimdOperationBinary8uAverage = 0,

        /// <summary>
        /// Computes the bitwise AND between two images.
        /// </summary>
        SimdOperationBinary8uAnd = 1,

        /// <summary>
        /// Computes the bitwise OR between two images.
        /// </summary>
        SimdOperationBinary8uOr = 2,

        /// <summary>
        /// Computes maximal value for every channel of every point of two images.
        /// </summary>
        SimdOperationBinary8uMaximum = 3,

        /// <summary>
        /// Computes minimal value for every channel of every point of two images.
        /// </summary>
        SimdOperationBinary8uMinimum = 4,

        /// <summary>
        /// Subtracts unsigned 8-bit integer b from unsigned 8-bit integer a and saturates (for every channel of every point of the images).
        /// </summary>
        SimdOperationBinary8uSaturatedSubtraction = 5,

        /// <summary>
        /// Adds unsigned 8-bit integer b from unsigned 8-bit integer a and saturates (for every channel of every point of the images).
        /// </summary>
        SimdOperationBinary8uSaturatedAddition = 6
    }

    public enum ESimdCompareType
    {
        /// <summary>
        /// equal to: a == b
        /// </summary>
        SimdCompareEqual = 0,

        /// <summary>
        /// equal to: a != b
        /// </summary>
        SimdCompareNotEqual = 1,

        /// <summary>
        /// equal to: a > b
        /// </summary>
        SimdCompareGreater = 2,

        /// <summary>
        /// equal to: a >= b
        /// </summary>
        SimdCompareGreaterOrEqual = 3,

        /// <summary>
        /// equal to: a<b
        /// </summary>
        SimdCompareLesser = 4,

        /// <summary>
        /// equal to: a <= b
        /// </summary>
        SimdCompareLesserOrEqual = 5
    }
}
