﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscreteLogarithm
{
    class LinearDependentException : Exception
    {
        public int[] RowsToDelete
        {
            get;
            private set;
        }

        public LinearDependentException(int[] rowsToDelete)
        {
            this.RowsToDelete = rowsToDelete;
        }
    }
}
