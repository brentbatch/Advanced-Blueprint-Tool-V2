using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Operations
{
    public static class OperationInvoker
    {
        private static readonly Stack<Operation> availableUndos = new Stack<Operation>();
        private static readonly Stack<Operation> availableRedos = new Stack<Operation>();


        public static MutableOperation DoOperation(Operation operation)
        {
            availableRedos.Clear();
            TryCloseOperationRecordOnStack();
            availableUndos.Push(operation);
            operation.Execute();
            return operation is MutableOperation? operation as MutableOperation : null;
        }

        public static void ConfirmRecordOperation()
        {
            TryCloseOperationRecordOnStack();
        }

        public static void CancelRecordOperation()
        {
            if (availableUndos.Count == 0) return;
            var operation = availableUndos.Peek();
            if (operation is MutableOperation)
            {
                availableUndos.Pop();
                operation.Undo();
            }
        }

        public static void UndoOperation()
        {
            if (availableUndos.Count == 0) return;
            Operation operation = availableUndos.Pop();
            operation.Undo();
            availableRedos.Push(operation);
        }
        public static void RedoOperation()
        {
            if (availableRedos.Count == 0) return;
            Operation operation = availableRedos.Pop();
            operation.Execute();
            availableUndos.Push(operation);
        }
        public static void ClearOperations()
        {
            availableRedos.Clear();
            availableUndos.Clear();
        }

        private static void TryCloseOperationRecordOnStack()
        {
            if (availableUndos.Count == 0) return;
            var operation = availableUndos.Peek();
            if (operation is MutableOperation)
            {
                (operation as MutableOperation).FinishOperation();
            }
        }
    }
}
