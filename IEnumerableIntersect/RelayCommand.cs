using System;
using System.Windows.Input;

namespace IEnumerableIntersect
{
    public class RelayCommand : RelayCommand<object>, ICommand
    {
        public RelayCommand(Action action) : base(action) { }
        public RelayCommand(Action action, Func<bool> canExec) : base(action, canExec) { }
        public RelayCommand(Action action, Func<object, bool> canExec) : base(action, canExec) { }
    }
    public class RelayCommand<T> : ICommand
    {
        private Action mbrAction;
        private Func<bool> mbrExecuteAdjudgement;
        private Func<T, bool> mbrParameterAdjudgement;
        private static readonly EventArgs mbrExecEvent = new EventArgs();
        private bool mbrCanExec;

        protected RelayCommand() { }

        public RelayCommand(Action action)
        {
            mbrAction = action;
        }

        public RelayCommand(Action action, Func<bool> canExec) : this(action)
        {
            mbrExecuteAdjudgement = canExec;
        }

        public RelayCommand(Action action, Func<T, bool> canExec) : this(action)
        {
            mbrParameterAdjudgement = canExec;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (mbrExecuteAdjudgement == null & mbrParameterAdjudgement == null)
            {
                return true;
            }
            else
            {
                if ((parameter == null | !(parameter is T) ? (mbrExecuteAdjudgement == null ? false : mbrExecuteAdjudgement()) : (mbrParameterAdjudgement == null ? false : mbrParameterAdjudgement(((T)(parameter))))) ^ mbrCanExec)
                {
                    mbrCanExec = !mbrCanExec;
                    CanExecuteChanged?.Invoke(this, mbrExecEvent);
                }
                return mbrCanExec;
            }
        }

        public void Execute(object parameter)
        {
            mbrAction?.Invoke();
        }
    }
}