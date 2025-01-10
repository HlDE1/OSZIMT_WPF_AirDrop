using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class ImageZoomWindow : Window
    {
        public ImageZoomWindow(BitmapImage image)
        {
            InitializeComponent();
            ZoomImage.Source = image;
        }
        private Point _startPoint;
        private bool _isDragging;
        private void ZoomImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _startPoint = e.GetPosition(ZoomImage);
                _isDragging = true;
                ZoomImage.CaptureMouse();
            }
        }

        private void ZoomImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var currentPoint = e.GetPosition(ZoomImage);
                var transformGroup = (TransformGroup)ZoomImage.RenderTransform;
                var translateTransform = (TranslateTransform)transformGroup.Children.First(t => t is TranslateTransform);
                translateTransform.X += currentPoint.X - _startPoint.X;
                translateTransform.Y += currentPoint.Y - _startPoint.Y;
                _startPoint = currentPoint;
            }
        }

        private void ZoomImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = false;
                ZoomImage.ReleaseMouseCapture();
            }
        }

        private void ZoomImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double zoomFactor = 0.1;
            var transformGroup = (TransformGroup)ZoomImage.RenderTransform;
            var scaleTransform = (ScaleTransform)transformGroup.Children.First(t => t is ScaleTransform);
            double zoom = e.Delta > 0 ? zoomFactor : -zoomFactor;

            double newScaleX = scaleTransform.ScaleX + zoom;
            double newScaleY = scaleTransform.ScaleY + zoom;

            if (newScaleX < 0.1 || newScaleY < 0.1)
                return;
            
            scaleTransform.ScaleX = newScaleX;
            scaleTransform.ScaleY = newScaleY;

            var position = e.GetPosition(ZoomImage);
            ZoomImage.RenderTransformOrigin =
                new Point(position.X / ZoomImage.ActualWidth, position.Y / ZoomImage.ActualHeight);
        }
    }
}