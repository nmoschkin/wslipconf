using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WSLIPConf.Models
{
    public delegate void SimpleCommandHandler(object parameter);

    public delegate bool SimpleCanExecuteHandler(object parameter);

    public class SimpleCommand : ICommand
    {

        public event EventHandler CanExecuteChanged;

        private SimpleCommandHandler handler;

        private SimpleCanExecuteHandler canExec;

        private bool? canExecStatus;

        public void ChangeExecutionStatus(bool? canExecute)
        {
            if (canExecStatus != canExecute)
            {
                canExecStatus = canExecute;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return canExecStatus ?? canExec?.Invoke(parameter) ?? true;
        }

        public virtual void Execute(object parameter)
        {
            handler?.Invoke(parameter);
        }

        public SimpleCommand(SimpleCommandHandler handler, SimpleCanExecuteHandler canExecute = null)
        {
            this.handler = handler;
            this.canExec = canExecute;
        }
    }
}
