using System.Windows;
using System.Windows.Controls;

namespace Schneedetektion.ImagePlayground
{
    public partial class PatchView : UserControl
    {
        private static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(PatchViewModel), typeof(PatchView));

        public PatchView()
        {
            InitializeComponent();
        }

        public PatchViewModel Model
        {
            get { return (PatchViewModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
    }
}
