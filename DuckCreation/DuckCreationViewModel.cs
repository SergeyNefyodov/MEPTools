using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MEPTools
{
    public class DuckCreationViewModel : INotifyPropertyChanged
    {
        public DuckCreationViewModel(ExternalCommandData commandData)
        {
            this.commandData = commandData;
            CreateDuck = new RelayCommand(onCreateDuckExecute, canCreateDuckCommanExecuted);
        }
       // private SettingsManager settingsManager = new SettingsManager();
        private string offset = "300";
        public string Offset
        {
            get => offset;
            set
            {
                offset = value;
                OnPropertyChanged();
            }
        }
        private string angle = "90";
        public string Angle
        {
            get => angle;
            set
            {
                angle = value;
                OnPropertyChanged();
            }
        }
        private bool isUp;
        public bool IsUp
        {
            get => isUp;
            set
            {
                isUp = value;
                OnPropertyChanged();
            }
        }
        private bool hasSnap;
        public bool HasSnap
        {
            get => hasSnap;
            set
            {
                hasSnap = value;
                OnPropertyChanged();
            }
        }
        private bool isCyclic = true;
        public bool IsCyclic
        {
            get => isCyclic;
            set
            {
                isCyclic = value;
                OnPropertyChanged();
            }
        }
        public static ICommand CreateDuck { get; set; }
        public double usedOffset { get; set; }
        
        public double usedAngle {get; set; }
        private void onCreateDuckExecute(object p)
        {
            DuckCreationModel.viewModel = this;
            duckCreationModel = new DuckCreationModel();
            RaiseHideRequest();
            duckCreationModel.CreateDuck();
            RaiseShowRequest();
        }
        private bool canCreateDuckCommanExecuted(object p)
        {
            bool a1 = double.TryParse(Offset, NumberStyles.AllowDecimalPoint, null, out double b);
            bool a2 = double.TryParse(Angle, out double c);
            if (c > 90) return false;
            usedOffset = b;
            usedAngle = c;
            if (a1 && a2)
            {
                //settingsManager.CheckExisting();
                //settingsManager.Record(nameof(Offset), Offset);
                //settingsManager.Record(nameof(Angle), Angle);
                //settingsManager.Record(nameof(isCyclic), isCyclic.ToString());
                //settingsManager.Record(nameof(isUp), isUp.ToString());                
            }
            return a1 && a2;
        }
        internal ExternalCommandData commandData { get; set; }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        private DuckCreationModel  duckCreationModel { get; set;}
    }
}
