using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grid_EYE
{
    public delegate void MatrixChangedEvent();

    public class ObservableMatrixArray<T> : MatrixArray<T>
    {
        public MatrixChangedEvent OnMatrixAdd;

        public ObservableMatrixArray(int HistoryLength = 100) : base(HistoryLength)
        {

        }

        public ObservableMatrixArray(params T[][,] Params_Matrices) : base( Params_Matrices)
        {

        }

        public override void AddMatrix(T[,] Matrix)
        {
            base.AddMatrix(Matrix);
            OnMatrixAdd?.Invoke();
        }


    }
}
