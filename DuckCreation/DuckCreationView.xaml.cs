using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static System.Math;

namespace MEPTools
{
    /// <summary>
    /// Логика взаимодействия для DuckCreationView.xaml
    /// </summary>
    /// 

    public partial class DuckCreationView : Window
    {
        public DuckCreationView(ExternalCommandData commandData)
        {            
            DuckCreationViewModel dcvm = new DuckCreationViewModel(commandData);            
            dcvm.CloseRequest += (s, e) => this.Close();
            dcvm.HideRequest += (s, e) => this.Hide();
            dcvm.ShowRequest += (s, e) => this.ShowDialog();
            DataContext = dcvm;
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
