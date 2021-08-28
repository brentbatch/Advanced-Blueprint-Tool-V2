using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Operations
{
    public abstract class MutableOperation : Operation
    {
        private bool isFinished;
        public virtual void FinishOperation() => this.isFinished = true;
        public virtual bool IsFinished() => this.isFinished;

        protected virtual void CheckFinished()
        {
            if (isFinished) // checkFinished (replace interface with abstract class)
                throw new InvalidOperationException("UpdateOffset not allowed after being marked 'finished'");
        }
    }
}
