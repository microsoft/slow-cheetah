namespace Wpf.Transform {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        private TransformModel Model { get; set; }

        private void WindowOnLoad(object sender, RoutedEventArgs e) {
            this.ApplyTransformModel(TransformModel.BuildFromCurrent());
        }

        private void ApplyTransformModel(TransformModel transformModel) {
            if (transformModel == null) { throw new ArgumentNullException("transformModel"); }

            this.Model = transformModel;
            this.TextAppSettings.Text = this.BuildStringFrom(this.Model.AppSettings);
            this.TextConnectionStrings.Text = this.BuildStringFrom(this.Model.ConnectionStrings);
            this.TextConfigContents.Text = this.Model.ConfigContents;
            this.InvalidateVisual();
        }

        private string BuildStringFrom(IDictionary<string, string> dictionary) {
            if (dictionary == null) { throw new ArgumentNullException("dictionary"); }

            StringBuilder sb = new StringBuilder();
            foreach (string key in dictionary.Keys) {
                sb.AppendFormat("{0} : {1}{2}",key,dictionary[key],Environment.NewLine);
            }

            return sb.ToString();
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
