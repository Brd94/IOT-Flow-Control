using System;
using System.Collections.Generic;

namespace Grid_EYE_Visualizer
{

    public class MatrixArray<T>
    {
        public int HistoryLength { get; private set; } = 100;

        public T[][,] Matrixes { get; private set; }

        public int CurrentIndex = 0;
        private int RelativeIndex => CurrentIndex % HistoryLength;

        public MatrixArray(int HistoryLength = 100)
        {
            this.HistoryLength = HistoryLength;
            Matrixes = new T[HistoryLength][,];
        }

        public void AddMatrix(T[,] Matrix)
        {
            Matrixes[RelativeIndex] = Matrix;
            ++CurrentIndex;
        }

        public T[,] getLastInsertedMatrix() => Matrixes[goBack(RelativeIndex, 1)];

        public IEnumerable<T[,]> getLastMatrixes(int quantity = int.MaxValue)
        {
            if (quantity > HistoryLength)
                quantity = HistoryLength;

            if(quantity > CurrentIndex)
                quantity = CurrentIndex;

            

            for (int i = 1; i <= quantity; i++)
            {
                yield return Matrixes[goBack(RelativeIndex, i)];
            }
        }

        private int goBack(int currIndex, int steps)
        {
            return (currIndex - (steps % HistoryLength) + HistoryLength) % HistoryLength;
        }

        public static TResult[,] ApplyOperation<TResult>(Func<T[], TResult> op, IEnumerable<T[,]> input)
        {
            int inputLenght = System.Linq.Enumerable.Count(input);

            TResult[,] result = new TResult[8, 8];


            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int iCount = 0;
                    T[] operations = new T[inputLenght];

                    foreach (var matrix in input)
                        operations[iCount++] = matrix[i,j];
                   
                        result[i, j] = op.Invoke(operations);

                }
            }

            return result;
        }

    }
}