using System;
using System.Collections.Generic;
using System.Text;
using Scheduler.Shared.Interfaces;

namespace Scheduler.Shared.Extensions
{
    public static class GetRowFrom2DArray
    {
        public static T[] GetRow<T>(this T[,] input2Darray, int rowByIndex) where T : IAllel
        {
            var height = input2Darray.GetLength(0);
            var length = input2Darray.GetLength(1);

            if (rowByIndex >= height)
            {
                throw new IndexOutOfRangeException("Row Index Out Of Range");
            }

            var returnRow = new T[length];
            for (int i = 0; i < length; i++)
            {
                returnRow[i] = input2Darray[rowByIndex, i];
            }

            return returnRow;
        }
    }
}
