using System;
using System.Collections.Generic;

namespace Grid_EYE
{

    public class MatrixArray<T>
    {
        public string MatrixName { get; set; } = "";

        public int HistoryLength { get; private set; } = 100;

        public T[][,] Matrices { get; private set; }

        public int CurrentIndex = 0;
        private int RelativeIndex => CurrentIndex % HistoryLength;

        public MatrixArray(int HistoryLength = 100)
        {
            this.HistoryLength = HistoryLength;
            Matrices = new T[HistoryLength][,];
          

        }

        public MatrixArray(params T[][,] Params_Matrices)
        {
            this.HistoryLength = Params_Matrices.Length;
            Matrices = new T[HistoryLength][,];

            foreach (var matrix in Params_Matrices)
                AddMatrix(matrix);

        }

        public virtual void AddMatrix(T[,] Matrix)
        {
            Matrices[RelativeIndex] = Matrix;
            ++CurrentIndex;
        }

        public T[,] getLastInsertedMatrix() => Matrices[goBack(RelativeIndex, 1)];

        public IEnumerable<T[,]> getLastMatrixes(int quantity = int.MaxValue)
        {
            if (quantity > HistoryLength)
                quantity = HistoryLength;

            if (quantity > CurrentIndex)
                quantity = CurrentIndex;



            for (int i = 1; i <= quantity; i++)
            {
                yield return Matrices[goBack(RelativeIndex, i)];
            }
        }

        private int goBack(int currIndex, int steps)
        {
            return (currIndex - (steps % HistoryLength) + HistoryLength) % HistoryLength;
        }



    }
}