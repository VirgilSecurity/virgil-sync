namespace Virgil.Sync.ViewModels
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;
    using Infrastructure;
    using Mvvm.Common;

    public abstract class ViewModel : ViewModelBase, IViewModel, INotifyDataErrorInfo
    {
        private readonly ValidationErrors validationErrors = new ValidationErrors();

        private string errorMessage;
        private bool isBusy;

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                this.isBusy = value;
                this.RaisePropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                if (value == this.errorMessage) return;
                this.errorMessage = value;
                this.RaisePropertyChanged();
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return this.validationErrors.GetErrors(propertyName);
        }

        public bool HasErrors => this.validationErrors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected void ClearErrors()
        {
            this.ErrorMessage = "";
            var props = this.validationErrors.Keys.ToArray();
            this.validationErrors.Clear();
            foreach (var prop in props)
            {
                this.RaiseErrorsChanged(prop);
            }
        }

        protected void AddErrors(ValidationErrors errors)
        {
            foreach (var kvp in errors)
            {
                foreach (var error in kvp.Value)
                {
                    this.AddErrorFor(kvp.Key, error);
                }
            }
        }

        protected void AddErrorFor(string property, string error)
        {
            this.validationErrors.AddErrorFor(property, error);
            this.RaiseErrorsChanged(property);
        }

        protected void RaiseErrorsChanged(string name = null)
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(name));
        }

        protected void RaiseErrorMessage(string error)
        {
            this.ErrorMessage = error;
        }

        public virtual void CleanupState()
        {
            
        }
    }
}