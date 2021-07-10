using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Operations
{
    public class GenericOperation : Operation
    {
        private Action ExecuteAction { get; }
        private Action UndoAction { get; }

        public GenericOperation(Action executeAction, Action undoAction)
        {
            ExecuteAction = executeAction;
            UndoAction = undoAction;
        }

        public override void Execute() => ExecuteAction();

        public override void Undo() => UndoAction();
    }
}
