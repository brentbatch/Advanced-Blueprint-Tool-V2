using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Operations
{
    public class ComboOperation : MutableOperation
    {
        readonly Stack<Operation> operations = new Stack<Operation>();

        public ComboOperation(Operation operation)
        {
            operations = new Stack<Operation>();
            operations.Push(operation);
        }
        public ComboOperation(IEnumerable<Operation> operations)
        {
            this.operations = new Stack<Operation>(operations);
        }

        public override void Execute()
        {
            Stack<Operation>.Enumerator operationsEnumerator = operations.GetEnumerator();
            while(operationsEnumerator.MoveNext())
            {
                operationsEnumerator.Current.Execute();
            }
        }

        public override void Undo()
        {
            Stack<Operation>.Enumerator operationsEnumerator = operations.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                operationsEnumerator.Current.Undo();
            }
        }

        public void AddOperation(Operation operation)
        {
            operations.Push(operation);
            operation.Execute();
        }
    }
}
