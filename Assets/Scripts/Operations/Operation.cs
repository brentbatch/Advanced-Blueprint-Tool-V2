using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Operations
{
    public abstract class Operation
    {
        /// <summary>
        /// gets executed straight after construction by passing instance to OperationInvoker
        /// </summary>
        public abstract void Execute();

        public abstract void Undo();
    }
}
