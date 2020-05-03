using System;
using System.Collections.ObjectModel;
using System.Windows;
using VideoStateAxis;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentRendered += MainWindow_ContentRendered;
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            var VideoSource = new ObservableCollection<VideoStateItem>();

            for (int i = 0; i < 10; i++)
            {
                var item = new VideoStateItem() { CameraName=$"Demo {i.ToString("D2")}",CameraChecked=i%2==0};
                var charList = new char[1000];
                for (int j = 0; j < 1000; j++)
                {
                    charList[j] = '1';
                }
                item.AxisHistoryTimeList = charList;
                VideoSource.Add(item);
            }

            PART_TimeLine.HisVideoSources = VideoSource;
        }
    }
}
