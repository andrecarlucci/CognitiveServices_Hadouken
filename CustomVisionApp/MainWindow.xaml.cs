using CustomVisionApp.Models;
using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CustomVisionApp
{
    public partial class MainWindow : System.Windows.Window {

        private MyCamera _camera;
        private LocalCustomVisionClient _localCustomVisionClient;
        private BitmapImage _currentImage;
        private string _projectName = "hadouken";
        
        public MainWindow() {
            InitializeComponent();
            _localCustomVisionClient = new LocalCustomVisionClient();
        }

        private void BStart_Click(object sender, RoutedEventArgs e) {
            if(bStart.Content.Equals("Start")) {
                _camera = new MyCamera();
                _camera.NewImage = NewImage;
                bStart.Content = "Stop";
                Task.Factory.StartNew(() => _camera.StartCamera(0));
            }
            else {
                _camera.Dispose();
                bStart.Content = "Start";
            }
        }

        private void BLoad_Click(object sender, RoutedEventArgs e) {
            var op = new OpenFileDialog {
                Title = "Select a picture",
                Filter = @"All supported graphics|*.jpg;*.jpeg;*.png|
                         JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|
                         Portable Network Graphic (*.png)|*.png"
            };
            if (op.ShowDialog() == true) {
                _currentImage = new BitmapImage(new Uri(op.FileName));
                TheImage.Source = _currentImage;
            }
        }

        private async void BDescribe_Click(object sender, RoutedEventArgs e) {
            if(_currentImage == null) {
                return;
            }
            var image = ImageHelper.ToImage(_currentImage);

            string description = "Analyze...";
            Description.Text = description;
            description = await _localCustomVisionClient.Analyze(image, _projectName);
            Description.Text = description;
        }

        private void NewImage(Mat frame) {
            var bytes = frame.ToBytes(".png");
            var result = _localCustomVisionClient.Analyze(bytes, _projectName).Result;
            ChangeUI(() => {
                _currentImage = ImageHelper.ToImage(bytes);
                TheImage.Source = _currentImage;
                TheWindow.Title = "Azure Vision: " + _camera.FramesPerSecond.ToString();
                Description.Text = result;
            });
            Console.WriteLine("--> " + result);
            //SpecialAtacks.Execute(result);
        }

        private void ChangeUI(Action action) {
            Dispatcher.BeginInvoke(action);
        }

        private void BSave_Click(object sender, RoutedEventArgs e) {
            var bytes = ImageHelper.ToImage(_currentImage);
            File.WriteAllBytes("image.jpg", bytes);
        }
    }
}
