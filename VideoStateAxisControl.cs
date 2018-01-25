using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoStateAxis
{
    #region TemplatePart 模板元素声明

    [TemplatePart(Name = Parid_axisCanvas)]
    [TemplatePart(Name = Parid_timePoint)]
    [TemplatePart(Name = Parid_currentTime)]
    [TemplatePart(Name = Parid_timePanel)]
    [TemplatePart(Name = Parid_timeLine)]
    [TemplatePart(Name = Parid_scrollViewer)]
    [TemplatePart(Name = Parid_videoHistoryPanel)]
    [TemplatePart(Name = Parid__axisCanvasTimeText)]
    [TemplatePart(Name = Parid_zoomSlider)]
    [TemplatePart(Name = Parid_clipCanvas)]
    [TemplatePart(Name = Parid_clipStartBorder)]
    [TemplatePart(Name = Parid_clipAreaBorder)]
    [TemplatePart(Name = Parid_clipEndBorder)]
    [TemplatePart(Name = Parid_clipStackPanel)]
    [TemplatePart(Name = Parid_clipOff)]
    [TemplatePart(Name = Parid_clipStateTimeTextBlock)]
    [TemplatePart(Name = Parid_clipEndTimeTextBlock)]
    [TemplatePart(Name = Parid_cameraListBox)]
    [TemplatePart(Name = Parid_downButtonListBox)]

    #endregion
    public class VideoStateAxisControl : Control
    {
        #region UIElement 模板元素

        private StackPanel _videoHistoryPanel;           //历史时间轴容器
        private ScrollViewer _scrollViewer;                 //滚动视图
        private Canvas _axisCanvas;                          //时间刻度尺容器
        private Canvas _axisCanvasTimeText;             //时间刻度时间文字容器
        private Slider _zoomSlider;                          //缩放时间轴滑块

        private TextBlock _currentTime;                    //进度指针时间
        private Canvas _timePanel;                           //进度容器
        private Canvas _timeLine;                             //进度指针容器
        private Grid _timePoint;                                //进度指针

        private Canvas _clipCanvas;                         //剪辑控制移动容器
        private Border _clipStartBorder;                  //剪辑左调解
        private Border _clipAreaBorder;                  //剪辑滑块
        private Border _clipEndBorder;                   //剪辑右调解
        private StackPanel _clipStackPanel;              //剪辑滑块容器
        private CheckBox _clipOff;                           //是否开启剪辑控制

        private TextBlock _clipStateTimeTextBlock;     //剪辑开始时间指示器
        private TextBlock _clipEndTimeTextBlock;      //剪辑结束时间指示器

        private ListBox _cameraListBox;                    //相机列表
        private ListBox _downButtonListBox;             //下载列表
        private ScrollViewer _cameraScrollViewer;     //相机列表ScrollViewer
        private ScrollViewer _downScrollViewer;       //下载列表ScrollViewer

        #endregion

        #region ConstString 模板元素名称，Geometry图像数据

        private const string Parid_axisCanvas = "Z_Parid_axisCanvas";
        private const string Parid__axisCanvasTimeText = "Z_Parid__axisCanvasTimeText";
        private const string Parid_zoomPanel = "Z_Parid_zoomPanel";
        private const string Parid_timePoint = "Z_Parid_timePoint";
        private const string Parid_currentTime = "Z_Parid_currentTime";
        private const string Parid_timePanel = "Z_Parid_timePanel";
        private const string Parid_timeLine = "Z_Parid_timeLine";
        private const string Parid_scrollViewer = "Z_Parid_scrollViewer";
        private const string Parid_videoHistoryPanel = "Z_videoHistoryPanel";
        private const string Parid_zoomSlider = "Z_Parid_zoomSlider";
        private const string Parid_clipCanvas = "Z_Parid_clipCanvas";
        private const string Parid_clipStartBorder = "Z_Parid_clipStartBorder";
        private const string Parid_clipAreaBorder = "Z_Parid_clipAreaBorder";
        private const string Parid_clipEndBorder = "Z_Parid_clipEndBorder";
        private const string Parid_clipStackPanel = "Z_Parid_clipStackPanel";
        private const string Parid_clipOff = "Z_Parid_clipOff";
        private const string Parid_clipStateTimeTextBlock = "Z_Parid_clipStateTimeTextBlock";
        private const string Parid_clipEndTimeTextBlock = "Z_Parid_clipEndTimeTextBlock";
        private const string Parid_cameraListBox = "Z_Parid_cameraListBox";
        private const string Parid_downButtonListBox = "Z_Parid_downButtonListBox";

        private const string GeometryDown = "M954.123536 509.086647 526.172791 932.999426c-8.074909 8.074909-20.185738 8.074909-28.260647 0L69.960375 509.086647c-12.110829-14.130835-4.427846-34.317597 14.130835-34.317597l215.994356 0L300.085566 107.149369c0-12.111852 10.093892-22.205745 22.204721-22.205745l379.50436 0c12.110829 0 22.204721 10.093892 22.204721 24.223704l0 365.601722 215.994356 0C958.159456 474.770074 966.234365 496.975818 954.123536 509.086647z";
        private const string GeometryFavorite = "M1024 378.88l-314.647273-37.236364L512 0 314.647273 341.643636 0 378.88l236.450909 266.24L191.767273 1024 512 872.261818 832.232727 1024l-44.683636-378.88L1024 378.88z";
        private const string GeometryOpen = "M512 68.191078c-245.204631 0-443.808922 198.60429-443.808922 443.808922s198.60429 443.808922 443.808922 443.808922 443.808922-198.60429 443.808922-443.808922S757.203608 68.191078 512 68.191078zM423.23842 711.713554 423.23842 312.285422l266.284739 199.713554L423.23842 711.713554z";

        #endregion

        #region DependencyProperty 依赖项属性
        public static readonly DependencyProperty HistoryVideoSourceProperty = DependencyProperty.Register(
            "HistoryVideoSources", 
            typeof(ObservableCollection<VideoStateItem>), 
            typeof(VideoStateAxisControl), 
            new PropertyMetadata(null, OnHistoryVideoSourcesChanged));

        public static readonly DependencyProperty StateTimeProperty = DependencyProperty.Register(
            "SerStateTime", 
            typeof(DateTime), 
            typeof(VideoStateAxisControl), 
            new PropertyMetadata(DateTime.Now.Date, OnTimeChanged));

        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register(
            "SerEndTime", 
            typeof(DateTime), 
            typeof(VideoStateAxisControl), 
            new PropertyMetadata(DateTime.Now.Date.AddDays(1), OnTimeChanged));

        public static readonly DependencyProperty AxisTimeProperty = DependencyProperty.Register(
            "AxisTime",
            typeof(DateTime),
            typeof(VideoStateAxisControl),
            new PropertyMetadata(OnAxisTimeChanged));

        public static readonly DependencyProperty ClipStartTimeProperty = DependencyProperty.Register(
            "ClipStartTime",
            typeof(DateTime),
            typeof(VideoStateAxisControl),
            new PropertyMetadata(OnClipTimeChanged));

        public static readonly DependencyProperty ClipEndTimeProperty = DependencyProperty.Register(
            "ClipEndTime", 
            typeof(DateTime), 
            typeof(VideoStateAxisControl),
            new PropertyMetadata(OnClipTimeChanged));

        public static readonly DependencyProperty ClipOffProperty = DependencyProperty.Register(
            "ClipOff", 
            typeof(bool), 
            typeof(VideoStateAxisControl),
            new PropertyMetadata(OnClipOffChanged));

        #endregion

        #region Property 属性关联字段

        /// <summary>
        /// 剪辑开启控制
        /// </summary>
        public bool ClipOff
        {
            get { return (bool)GetValue(ClipOffProperty); }
            set { SetValue(ClipOffProperty, value); }
        }

        /// <summary>
        /// 剪辑结束时间
        /// </summary>
        public DateTime ClipEndTime
        {
            get { return (DateTime)GetValue(ClipEndTimeProperty); }
            set { SetValue(ClipEndTimeProperty, value); }
        }

        /// <summary>
        /// 剪辑开始时间
        /// </summary>
        public DateTime ClipStartTime
        {
            get { return (DateTime)GetValue(ClipStartTimeProperty); }
            set { SetValue(ClipStartTimeProperty, value); }
        }

        /// <summary>
        /// 指针时间
        /// </summary>
        public DateTime AxisTime
        {
            get { return (DateTime)GetValue(AxisTimeProperty); }
            set { SetValue(AxisTimeProperty, value); }
        }

        /// <summary>
        /// 搜索历史视频开始时间
        /// </summary>
        public DateTime SerStateTime
        {
            get { return (DateTime)GetValue(StateTimeProperty); }
            set { SetValue(StateTimeProperty, value); }
        }

        /// <summary>
        /// 搜索历史视频结束时间
        /// </summary>
        public DateTime SerEndTime
        {
            get { return (DateTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        /// <summary>
        /// 历史视频来源列表
        /// </summary>
        public ObservableCollection<VideoStateItem> HistoryVideoSources
        {
            get { return (ObservableCollection<VideoStateItem>)GetValue(HistoryVideoSourceProperty); }
            set { SetValue(HistoryVideoSourceProperty, value); }
        }

        /// <summary>
        /// 每小时占用时间轴的的宽度
        /// </summary>
        private double Dial_Cell_H
        {
            get { return _scrollViewer == null ? 0 : ((_scrollViewer.ActualWidth - 10) * Slider_Magnification ) / 24; }
        }

        /// <summary>
        /// 每分钟占用时间轴的的宽度
        /// </summary>
        private double Dial_Cell_M
        {
            get { return Dial_Cell_H / 60; }
        }

        /// <summary>
        /// 每秒占用时间轴的的宽度
        /// </summary>
        private double Dial_Cell_S
        {
            get { return Dial_Cell_M / 60; }
        }

        /// <summary>
        /// 剪辑开始鼠标按下位置
        /// </summary>
        private double ClipStart_MouseDown_Offset = 0;

        /// <summary>
        /// 剪辑鼠标按下左坐标
        /// </summary>
        private double Start_MouseDown_ClipOffset = 0;

        /// <summary>
        /// 鼠标按下剪辑滑块宽度
        /// </summary>
        private double ClipStart_MouseDown_AreaWidth = 0;

        /// <summary>
        /// 时间轴缩放比例
        /// </summary>
        private double Slider_Magnification = 1;

        #endregion

        #region RouteEvent 路由事件

        public static readonly RoutedEvent AxisDownRoutedEvent = EventManager.RegisterRoutedEvent(
            "AxisDown",
            RoutingStrategy.Bubble,
            typeof(EventHandler<VideoStateAxisRoutedEventArgs>),
            typeof(VideoStateAxisControl));

        /// <summary>
        /// 下载路由事件
        /// </summary>
        public event RoutedEventHandler AxisDown
        {
            add { this.AddHandler(AxisDownRoutedEvent, value); }
            remove { this.RemoveHandler(AxisDownRoutedEvent, value); }
        }

        #endregion

        #region Method 方法

        /// <summary>
        /// 历史查询时间 - 改变
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnTimeChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = d as VideoStateAxisControl;
            if(AxisOb != null)
            {
                AxisOb.InitializeAxis();
            }
        }

        /// <summary>
        /// 历史视频来源 - 改变
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnHistoryVideoSourcesChanged(DependencyObject d ,DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = d as VideoStateAxisControl;
            if(AxisOb.HistoryVideoSources != null && AxisOb.HistoryVideoSources.Count() > 0)
            {
                AxisOb.HistoryVideoSources.CollectionChanged += (s , o)=> 
                {
                    AxisOb.AddHisPie();
                    AxisOb.InitiaListBox_ScrollChanged();
                };
                AxisOb.InitializeAxis();
            }
        }

        /// <summary>
        /// 指针时间刷新指针位置
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnAxisTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = d as VideoStateAxisControl;
            if (AxisOb != null && e.NewValue != e.OldValue)
            {
                AxisOb.RefreshTimeLineLeft((DateTime)e.NewValue);
            }
        }

        /// <summary>
        /// 剪辑时间变化，刷新剪辑控制条
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnClipTimeChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = d as VideoStateAxisControl;
            if(AxisOb != null && e.NewValue != e.OldValue)
            {
                if (e.Property.Name == nameof(AxisOb.ClipStartTime))
                {
                    AxisOb.ClipStartTimeChanged((DateTime)e.NewValue);
                }
                if(e.Property.Name == nameof(AxisOb.ClipEndTime))
                {
                    AxisOb.ClipEndTimeChanged((DateTime)e.NewValue);
                }
            }
        }

        /// <summary>
        /// 剪辑开启控制源改变事件
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnClipOffChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = d as VideoStateAxisControl;
            if(AxisOb != null && e.NewValue != e.OldValue)
            {
                AxisOb.ClipOff = (bool)e.NewValue;
                AxisOb._clipOff.IsChecked = ((bool)e.NewValue) ? true : false;
            }
        }

        /// <summary>
        /// 构造函数初始化一些属性和样式
        /// </summary>
        public VideoStateAxisControl()
        {
            Loaded += delegate 
            {
                InitializeAxis();
                SizeChanged += delegate 
                {
                    InitializeAxis();
                };
            };
        }

        /// <summary>
        /// 刷新指针位置
        /// </summary>
        /// <param name="dt"></param>
        private void RefreshTimeLineLeft(DateTime dt)
        {
            TimeSpan ts = dt - SerStateTime.Date;
            if (ts.Days <= 1 && ts.Seconds >= 0)
            {
                Canvas.SetLeft(_timeLine, 
                    Dial_Cell_H * (ts.Days == 1 ? 23 : dt.Hour) + 
                    Dial_Cell_M * (ts.Days == 1 ? 59 : dt.Minute)+ 
                    Dial_Cell_S * (ts.Days == 1 ? 59 : dt.Second));
                _currentTime.Text = dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 剪辑鼠标弹起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border bor = sender as Border;
            if (bor != null)
            {
                bor.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// 剪辑鼠标点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border bor = sender as Border;
            if (bor != null)
            {
                bor.CaptureMouse();
                ClipStart_MouseDown_Offset = e.GetPosition(_clipCanvas).X;
                Start_MouseDown_ClipOffset = _clipStackPanel.Margin.Left;
                ClipStart_MouseDown_AreaWidth = _clipStackPanel.Margin.Left + _clipStackPanel.ActualWidth;
            }
        }

        /// <summary>
        /// 剪辑鼠标移动，释放鼠标捕获
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clip_MouseMove(object sender, MouseEventArgs e)
        {
            Border bor = sender as Border;
            if (bor != null && e.LeftButton == MouseButtonState.Pressed)
            {
                switch(bor.Name)
                {
                    case Parid_clipStartBorder:
                        ClipStart(e.GetPosition(_clipCanvas));
                        break;

                    case Parid_clipEndBorder:
                        ClipEnd(e.GetPosition(_clipCanvas));
                        break;

                    case Parid_clipAreaBorder:
                        MoveClipArea(e.GetPosition(_clipCanvas));
                        break;
                }
                MathClipTime();
            }
        }

        /// <summary>
        /// 剪辑开始滑块增量
        /// </summary>
        /// <param name="pt"></param>
        private void ClipStart(Point pt)
        {
            if(pt.X >= 0)
            {
                double clipWidth = ClipStart_MouseDown_AreaWidth - (Start_MouseDown_ClipOffset + (pt.X - ClipStart_MouseDown_Offset) < 0 ? 0 :
                Start_MouseDown_ClipOffset + (pt.X - ClipStart_MouseDown_Offset) > _clipCanvas.ActualWidth - _clipAreaBorder.Width ?
                _axisCanvas.ActualWidth - _clipAreaBorder.Width :
                Start_MouseDown_ClipOffset + (pt.X - ClipStart_MouseDown_Offset)) - 10;
                _clipAreaBorder.Width = clipWidth <= 0 ? 0 : clipWidth;
                if(clipWidth >= 0)
                {
                    MoveClipArea(pt);
                }
            }
        }

        /// <summary>
        /// 剪辑结束滑块增量
        /// </summary>
        private void ClipEnd(Point pt)
        {
            double clipWidth = pt.X - _clipStackPanel.Margin.Left ;
            _clipAreaBorder.Width = clipWidth <= 0 ? 0 : 
                clipWidth > _axisCanvas.ActualWidth - _clipStackPanel.Margin.Left  ? 
                _axisCanvas.ActualWidth - _clipStackPanel.Margin.Left : clipWidth;
        }

        /// <summary>
        /// 剪辑滚动滑块
        /// </summary>
        /// <param name="pt"></param>
        private void MoveClipArea(Point pt)
        {
            double clipLeft = Start_MouseDown_ClipOffset + (pt.X - ClipStart_MouseDown_Offset) < 0 ? 0 : 
                Start_MouseDown_ClipOffset + (pt.X - ClipStart_MouseDown_Offset) > _clipCanvas.ActualWidth - _clipAreaBorder.Width ?
                _axisCanvas.ActualWidth - _clipAreaBorder.Width :
                Start_MouseDown_ClipOffset + (pt.X - ClipStart_MouseDown_Offset);
            _clipStackPanel.Margin = new Thickness(clipLeft, 0, 0, 0);
        }

        /// <summary>
        /// 时间缩放滑块事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider_Magnification = Math.Round(e.NewValue , 2);
            InitializeAxis();
        }

        /// <summary>
        /// 滚动重置时间刻度位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_Changed(object sender, ScrollChangedEventArgs e)
        {
            _timeLine.Margin = new Thickness(0, _scrollViewer.VerticalOffset, 0, 0);
            _axisCanvasTimeText.Margin = new Thickness(0, _scrollViewer.VerticalOffset, 0, 0);
            _axisCanvas.Margin = new Thickness(0, _scrollViewer.VerticalOffset, 0, 0);
            _clipCanvas.Margin = new Thickness(0, _scrollViewer.VerticalOffset, 0, 0);


            if (_cameraScrollViewer != null)
            {
                double offset = _scrollViewer.VerticalOffset / _scrollViewer.ScrollableHeight * _cameraScrollViewer.ScrollableHeight;
                _cameraScrollViewer.ScrollToVerticalOffset(double.IsNaN(offset) ? 0 : offset);
            }
            if (_downScrollViewer != null)
            {
                double offset = _scrollViewer.VerticalOffset / _scrollViewer.ScrollableHeight * _downScrollViewer.ScrollableHeight;
                _downScrollViewer.ScrollToVerticalOffset(double.IsNaN(offset) ? 0 : offset);
            }
        }

        /// <summary>
        /// 相机列表ListBox的ScrollerViewerChanged事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _cameraScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                double offset = _cameraScrollViewer.VerticalOffset / _cameraScrollViewer.ScrollableHeight * _scrollViewer.ScrollableHeight;
                _scrollViewer.ScrollToVerticalOffset(double.IsNaN(offset) ? 0 : offset);
            }
            if(_downScrollViewer != null)
            {
                _downScrollViewer.ScrollToVerticalOffset(_cameraScrollViewer.VerticalOffset);
            }
        }

        /// <summary>
        ///  下载列表ListBox的ScrollerViewerChanged事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _downScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                double offset = _downScrollViewer.VerticalOffset / _downScrollViewer.ScrollableHeight * _scrollViewer.ScrollableHeight;
                _scrollViewer.ScrollToVerticalOffset(double.IsNaN(offset) ? 0 : offset);
            }
            if (_cameraScrollViewer != null)
            {
                _cameraScrollViewer.ScrollToVerticalOffset(_downScrollViewer.VerticalOffset);
            }
        }

        /// <summary>
        ///   指针移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void timePoint_MouseMove(object sender, MouseEventArgs s)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
            {
                double delta = s.GetPosition(_timePanel).X;
                double timePointMaxLeft = _timePanel.ActualWidth - _timePoint.ActualWidth;
                Canvas.SetLeft(_timeLine, delta = delta < 0 ? 0 : (delta > timePointMaxLeft ? timePointMaxLeft : delta));
                TimeLine_Resver(delta);
            }
        }

        /// <summary>
        /// 刷新时间指示器坐标位置
        /// </summary>
        /// <param name="delta">鼠标位于Canvas坐标X</param>
        private void TimeLine_Resver(double delta)
        {
            double timePointMaxLeft = _timePanel.ActualWidth - _timePoint.ActualWidth ;
            double currentTimeMaxLeft = _timePanel.ActualWidth - _currentTime.ActualWidth;
            _currentTime.Text = (AxisTime = XToDateTime(delta < 0 ? 0 : (delta > timePointMaxLeft ? timePointMaxLeft : delta))).ToString("yyyy-MM-dd HH:mm:ss");
            _currentTime.Margin = delta < currentTimeMaxLeft ?
                new Thickness(delta < 0 ? 10 : delta + 10, 2, 0, 0) :
                new Thickness(delta > timePointMaxLeft ? timePointMaxLeft - _currentTime.ActualWidth : delta - _currentTime.ActualWidth, 2, 0, 0);
        }

        /// <summary>
        /// 剪辑控制开启Checked事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clip_UnChecked_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            if(check != null)
            {
                ClipOff = check.IsChecked == null || check.IsChecked == false ? false : true;
            }
        }

        /// <summary>
        /// 指针按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void timePoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs s)
        {
            _currentTime.Visibility = Visibility.Visible;
            _timePoint.CaptureMouse();
        }

        /// <summary>
        /// 指针弹起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void timePoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs s)
        {
            _currentTime.Visibility = Visibility.Collapsed;
            _timePoint.ReleaseMouseCapture();
        }

        /// <summary>
        /// 计算指针拖动的显示时间
        /// </summary>
        /// <param name="x">指针在Canvas容器中的Left坐标值</param>
        private DateTime XToDateTime(double point_x)
        {
            DateTime dt = SerStateTime.Date;
            double time;
            int H , M , S;
            time = point_x  / Dial_Cell_H  ;
            H = (int)(time);
            time = (time - H) * 60;
            M = (int)(time);
            S = (int)((time - M) * 60);
            return dt.AddHours(H).AddMinutes(M).AddSeconds(S);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitializeAxis()
        {
            AddTimeTextBlock();
            AddTimeLine();
            AddHisPie();
            InitiaClipTime();
            InitializationNewtTimeLine();
            ClipStartTimeChanged(ClipStartTime);
            ClipEndTimeChanged(ClipEndTime);
            InitiaListBox_ScrollChanged();
        }

        /// <summary>
        /// 初始化剪辑时间
        /// </summary>
        private void InitiaClipTime()
        {
            ClipStartTime = ClipStartTime == DateTime.Parse("0001/1/1 0:00:00") ? SerStateTime.Date : ClipStartTime;
            ClipEndTime = ClipEndTime == DateTime.Parse("0001/1/1 0:00:00") ? SerStateTime.Date.AddDays(1) : ClipEndTime;
        }

        /// <summary>
        /// 初始化相机列表ListBox的ScrollerViewerChanged事件
        /// </summary>
        private void InitiaListBox_ScrollChanged()
        {
            Decorator carmeraborder = VisualTreeHelper.GetChild(_cameraListBox, 0) as Decorator;
            if (carmeraborder != null)
            {
                _cameraScrollViewer = carmeraborder.Child as ScrollViewer;
                if (_cameraScrollViewer != null)
                {
                    _cameraScrollViewer.ScrollChanged += _cameraScrollViewer_ScrollChanged;
                }
            }

            Decorator downborder = VisualTreeHelper.GetChild(_downButtonListBox, 0) as Decorator;
            if(downborder != null)
            {
                _downScrollViewer = downborder.Child as ScrollViewer;
                if(_downScrollViewer != null)
                {
                    _downScrollViewer.ScrollChanged += _downScrollViewer_ScrollChanged;
                }
            }
        }

        /// <summary>
        /// 计算剪辑时间
        /// </summary>
        private void MathClipTime()
        {
            ClipStartTime = XToDateTime(_clipStackPanel.Margin.Left);
            ClipEndTime = XToDateTime(_clipStackPanel.Margin.Left + _clipAreaBorder.ActualWidth);
        }

        /// <summary>
        /// 重新计算剪辑时间为准的剪辑条左坐标
        /// </summary>
        private void ClipStartTimeChanged(DateTime dt)
        {
            TimeSpan ts = dt - SerStateTime.Date;
            if (ts.Days <= 1 && ts.Seconds >= 0 && _clipStackPanel != null)
            {
                double left = Dial_Cell_H * (ts.Days == 1 ? 23 : dt.Hour) + Dial_Cell_M * (ts.Days == 1 ? 59 : dt.Minute) + Dial_Cell_S * (ts.Days == 1 ? 59 : dt.Second);
                _clipStackPanel.Margin = new Thickness(left, 0 , 0 , 0);
            }
        }

        /// <summary>
        /// 重新计算剪辑时间为准的剪辑条宽度
        /// </summary>
        /// <param name="dt"></param>
        private void ClipEndTimeChanged(DateTime dt)
        {
            TimeSpan ts = dt - ClipStartTime;
            if (ts.Days <= 1 && ts.Seconds >= 0 && _clipAreaBorder != null)
            {
                double width = Dial_Cell_H * (ts.Days == 1 ? 23 : ts.Hours) + Dial_Cell_M * (ts.Days == 1 ? 59 : ts.Minutes) + Dial_Cell_S * (ts.Days == 1 ? 59 : ts.Seconds);
                _clipAreaBorder.Width = width;
            }
        }

        /// <summary>
        /// 初始化指针位置
        /// </summary>
        private void InitializationNewtTimeLine()
        {
            if (!double.IsNaN(Canvas.GetLeft(_timeLine)))
            {
                RefreshTimeLineLeft(AxisTime);
            }
        }

        /// <summary>
        /// 初始化时间刻度文字
        /// </summary>
        /// <param name="HaveMathTextBlock">需要填充的时间文字数量</param>
        private void AddTimeTextBlock()
        {
            if(_axisCanvasTimeText != null)
            {
                _axisCanvasTimeText.Width = (_scrollViewer.ActualWidth - 10) * Slider_Magnification ;
                _axisCanvasTimeText.Children.Clear();
                for (int i = 0; i < 24; i++)
                {
                    _axisCanvasTimeText.Children.Add((
                        new TextBlock()
                        {
                            Text = i.ToString().PadLeft(2, '0') + ":00",
                            Margin = new Thickness(Dial_Cell_H * i, 2, 0, 0)
                        }));
                }
            }
        }

        /// <summary>
        /// 初始化时间刻度
        /// </summary>
        /// <param name="HaveMathTextBlock">需要填充的时间刻度数量</param>
        private void AddTimeLine()
        {
            if(_axisCanvas != null)
            {
                _axisCanvas.Children.Clear();
                for (int i = 0; i < 24; i++)
                {
                    _axisCanvas.Children.Add(new Line()
                    {
                        X1 = Dial_Cell_H * i,
                        Y1 = 0,
                        X2 = Dial_Cell_H * i,
                        Y2 = 5,
                        StrokeThickness = 1
                    });
                }
            }
        }

        /// <summary>
        /// 初始化时间轴
        /// </summary>
        private void AddHisPie()
        {
            if(_videoHistoryPanel != null && HistoryVideoSources!=null && HistoryVideoSources.Count() > 0)
            {
                _videoHistoryPanel.Children.Clear();
                foreach (var item in HistoryVideoSources)
                {
                    Dictionary<KeyValuePair<int, int>,bool> dic = MathToTimeSp(item.AxisHistoryTimeList);
                    DisplayData(dic);
                }
            }
        }

        /// <summary>
        /// 计算填充时间轴查询结果
        /// </summary>
        private void DisplayData(Dictionary<KeyValuePair<int, int>,bool> dic)
        {
            DateTime serTime = SerStateTime;
            Canvas TimeCanvas = new Canvas(){ Width = (_scrollViewer.ActualWidth - 10) * Slider_Magnification };
            foreach (var item in dic)
            {
                TimeCanvas.Children.Add(new Rectangle()
                {
                    Width = item.Key.Value * Dial_Cell_M,
                    Height = item.Value ? 16 : 0,
                    Margin = new Thickness(serTime.Hour * Dial_Cell_H + serTime.Minute * Dial_Cell_M + serTime.Second * Dial_Cell_S, 0, 0, 0)
                });
                serTime = serTime.AddMinutes(item.Key.Value);
            }
            _videoHistoryPanel.Children.Add(TimeCanvas);
        }

        /// <summary>
        /// 计算断续时间轴
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private Dictionary<KeyValuePair<int, int>, bool> MathToTimeSp(char[] region)
        {
            Dictionary<KeyValuePair<int, int>, bool> dic = new Dictionary<KeyValuePair<int, int>, bool>();
            string regStr = new string(region.Select(x => x == '\0' ? x = '0' : '1').ToArray());
            foreach(Match item in Regex.Matches(regStr , "(.)\\1*"))
            {
                if(item.Success)
                {
                    dic.Add(new KeyValuePair<int, int>(dic.Count+1, item.Value.Length), item.Value.Contains('1') ? true : false);
                }
            }
            return dic;
        }

        /// <summary>
        /// 初始化下载数据模板
        /// </summary>
        private void Down_ListBox_Template()
        {
            DataTemplate dataTemplate = new DataTemplate();
            FrameworkElementFactory topElement = CreateTopElement();

            FrameworkElementFactory downPath = CreatePathElement(GeometryDown, "下载历史");
            FrameworkElementFactory downViewBox = CreateViewBoxElement(VideoAxisActionType.Dwon.ToString(), new Thickness(0, 0, 0, 2), downPath);
            topElement.AppendChild(downViewBox);

            FrameworkElementFactory favoritePath = CreatePathElement(GeometryFavorite, "收藏历史");
            FrameworkElementFactory favoriteViewBox = CreateViewBoxElement(VideoAxisActionType.Favorite.ToString(), new Thickness(10, 0, 0, 2), favoritePath);
            topElement.AppendChild(favoriteViewBox);

            FrameworkElementFactory openPath = CreatePathElement(GeometryOpen, "打开视频");
            FrameworkElementFactory openViewBox = CreateViewBoxElement(VideoAxisActionType.Open.ToString(), new Thickness(10, 0, 0, 2), openPath);
            topElement.AppendChild(openViewBox);

            dataTemplate.VisualTree = topElement;
            _downButtonListBox.ItemTemplate = dataTemplate;
        }

        /// <summary>
        /// 创建顶层数据模板容器
        /// </summary>
        /// <returns></returns>
        private FrameworkElementFactory CreateTopElement()
        {
            FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(StackPanel));
            frameworkElementFactory.SetValue(StackPanel.HeightProperty, 16.00);
            frameworkElementFactory.SetValue(StackPanel.MarginProperty, new Thickness(0, 3, 5, 1));
            frameworkElementFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            return frameworkElementFactory;
        }

        /// <summary>
        /// 创建Path元素模板
        /// </summary>
        /// <param name="GeometryPath"></param>
        /// <param name="ToolTipStr"></param>
        /// <returns></returns>
        private FrameworkElementFactory CreatePathElement(string GeometryPath ,string ToolTipStr)
        {
            FrameworkElementFactory path = new FrameworkElementFactory(typeof(Path));
            path.SetValue(CursorProperty, Cursors.Hand);
            path.SetValue(Path.DataProperty, Geometry.Parse(GeometryPath));
            path.SetValue(Path.FillProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c8c7c3")));
            path.SetValue(ToolTipProperty, ToolTipStr);
            return path;
        }

        /// <summary>
        /// 创建ViewBox元素模板
        /// </summary>
        /// <param name="NameStr"></param>
        /// <param name="thick"></param>
        /// <param name="pathChild"></param>
        /// <returns></returns>
        private FrameworkElementFactory CreateViewBoxElement(string NameStr , Thickness thick , FrameworkElementFactory pathChild)
        {
            FrameworkElementFactory viewBox = new FrameworkElementFactory(typeof(Viewbox));
            viewBox.SetValue(HeightProperty, 14.00);
            viewBox.SetValue(WidthProperty, 14.00);
            viewBox.SetValue(MarginProperty, thick);
            viewBox.SetValue(NameProperty, NameStr);
            viewBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(viewBox_LeftMouseButtonDown));
            viewBox.AppendChild(pathChild);
            return viewBox;
        }

        /// <summary>
        /// 模板事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewBox_LeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewBox = sender as Viewbox;
            VideoStateItem videoState = viewBox.DataContext as VideoStateItem;
            if(videoState != null && viewBox != null)
            {
                VideoStateAxisRoutedEventArgs args = new VideoStateAxisRoutedEventArgs(AxisDownRoutedEvent, this)
                {
                    CameraName = videoState.CameraName,
                    CameraChecked = videoState.CameraChecked
                };
                switch((VideoAxisActionType)Enum.Parse(typeof(VideoAxisActionType),viewBox.Name))
                {
                    case VideoAxisActionType.Dwon:
                        args.ActionType = VideoAxisActionType.Dwon;
                        break;
                    case VideoAxisActionType.Favorite:
                        args.ActionType = VideoAxisActionType.Favorite;
                        break;
                    case VideoAxisActionType.Open:
                        args.ActionType = VideoAxisActionType.Open;
                        break;
                }
                this.RaiseEvent(args);
            }
        }

        /// <summary>
        /// 获得实例项
        /// </summary>
        public override void OnApplyTemplate()
        {
            _timePanel = GetTemplateChild(Parid_timePanel) as Canvas;
            _timeLine = GetTemplateChild(Parid_timeLine) as Canvas;
            _axisCanvas = GetTemplateChild(Parid_axisCanvas) as Canvas;
            _videoHistoryPanel = GetTemplateChild(Parid_videoHistoryPanel) as StackPanel;
            _axisCanvasTimeText = GetTemplateChild(Parid__axisCanvasTimeText) as Canvas;
            _clipCanvas = GetTemplateChild(Parid_clipCanvas) as Canvas;
            _clipStackPanel = GetTemplateChild(Parid_clipStackPanel) as StackPanel;
            if ((_clipOff = GetTemplateChild(Parid_clipOff) as CheckBox) != null)
            {
                _clipOff.Checked += new RoutedEventHandler(Clip_UnChecked_Checked);
                _clipOff.Unchecked += new RoutedEventHandler(Clip_UnChecked_Checked);
            }
            if ((_zoomSlider = GetTemplateChild(Parid_zoomSlider) as Slider) != null)
            {
                _zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(_zoomSlider_ValueChanged);
            }
            if ((_timePoint = GetTemplateChild(Parid_timePoint) as Grid) != null)
            {
                _timePoint.MouseLeftButtonDown += new MouseButtonEventHandler(timePoint_MouseLeftButtonDown);
                _timePoint.MouseLeftButtonUp += new MouseButtonEventHandler(timePoint_MouseLeftButtonUp);
                _timePoint.MouseMove += new MouseEventHandler(timePoint_MouseMove);
            }
            if ((_scrollViewer = GetTemplateChild(Parid_scrollViewer) as ScrollViewer) != null)
            {
                _scrollViewer.ScrollChanged += new ScrollChangedEventHandler(scrollViewer_Changed);
            }
            if ((_currentTime = GetTemplateChild(Parid_currentTime) as TextBlock) != null)
            {
                _currentTime.Text = SerStateTime.ToString("yyyy-MM-dd 00:00:00");
            }
            if ((_clipEndBorder = GetTemplateChild(Parid_clipEndBorder) as Border) != null)
            {
                _clipEndBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Clip_MouseLeftButtonDown);
                _clipEndBorder.MouseMove += new MouseEventHandler(Clip_MouseMove);
                _clipEndBorder.MouseLeftButtonUp += new MouseButtonEventHandler(Clip_MouseLeftButtonUp);
            }
            if ((_clipAreaBorder = GetTemplateChild(Parid_clipAreaBorder) as Border) != null)
            {
                _clipAreaBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Clip_MouseLeftButtonDown);
                _clipAreaBorder.MouseMove += new MouseEventHandler(Clip_MouseMove);
                _clipAreaBorder.MouseLeftButtonUp += new MouseButtonEventHandler(Clip_MouseLeftButtonUp);
            }
            if ((_clipStartBorder = GetTemplateChild(Parid_clipStartBorder) as Border) != null)
            {
                _clipStartBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Clip_MouseLeftButtonDown);
                _clipStartBorder.MouseMove += new MouseEventHandler(Clip_MouseMove);
                _clipStartBorder.MouseLeftButtonUp += new MouseButtonEventHandler(Clip_MouseLeftButtonUp);
            }
            if ((_clipStateTimeTextBlock = GetTemplateChild(Parid_clipStateTimeTextBlock) as TextBlock) != null)
            {
                Binding binding = new Binding("ClipStartTime") { Source = this, StringFormat = " yyyy-MM-dd HH:mm:ss " };
                _clipStateTimeTextBlock.SetBinding(TextBlock.TextProperty, binding);
            }
            if ((_clipEndTimeTextBlock = GetTemplateChild(Parid_clipEndTimeTextBlock) as TextBlock) != null)
            {
                Binding binding = new Binding("ClipEndTime") { Source = this, StringFormat = " yyyy-MM-dd HH:mm:ss " };
                _clipEndTimeTextBlock.SetBinding(TextBlock.TextProperty, binding);
            }
            if ((_cameraListBox = GetTemplateChild(Parid_cameraListBox) as ListBox) != null)
            {
                Binding binding = new Binding("HistoryVideoSources") { Source = this };
                _cameraListBox.SetBinding(ListBox.ItemsSourceProperty, binding);
            }
            if ((_downButtonListBox = GetTemplateChild(Parid_downButtonListBox) as ListBox) != null)
            {
                Binding binding = new Binding("HistoryVideoSources") { Source = this };
                _downButtonListBox.SetBinding(ListBox.ItemsSourceProperty, binding);
                Down_ListBox_Template();
            }
        }

        /// <summary>
        /// 将原始控件的样式覆盖
        /// </summary>
        static VideoStateAxisControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoStateAxisControl), new FrameworkPropertyMetadata(typeof(VideoStateAxisControl)));
        }

        #endregion
    }

    /// <summary>
    /// 时间轴事件参数类
    /// </summary>
    public class VideoStateAxisRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 基类构造函数
        /// </summary>
        /// <param name="routedEvent"></param>
        /// <param name="source"></param>
        public VideoStateAxisRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }

        /// <summary>
        /// 事件类型
        /// </summary>
        public VideoAxisActionType ActionType { get; set; }

        /// <summary>
        /// 相机名称
        /// </summary>
        public string CameraName { get; set; }

        /// <summary>
        /// 相机是否选中
        /// </summary>
        public bool CameraChecked { get; set; }
    }

    /// <summary>
    /// 时间轴控件事件类型
    /// </summary>
    public enum VideoAxisActionType
    {
        [Description("下载")]
        Dwon,

        [Description("收藏")]
        Favorite,

        [Description("打开")]
        Open
    }

    /// <summary>
    /// 时间轴对象
    /// </summary>
    public class VideoStateItem : INotifyPropertyChanged
    {
        private string _cameraName;
        /// <summary>
        /// 相机名称
        /// </summary>
        public string CameraName
        {
            get => _cameraName;
            set { _cameraName = value; OnPropertyChanged("CameraName"); }
        }

        private bool _cameraChedcked;
        /// <summary>
        /// 相机是否选中
        /// </summary>
        public bool CameraChecked
        {
            get => _cameraChedcked;
            set { _cameraChedcked = value; OnPropertyChanged("CameraChecked"); }
        }

        private char[] _axisHistoryTimeList;
        /// <summary>
        /// 相机历史视频视频时间集
        /// </summary>
        public char[] AxisHistoryTimeList
        {
            get => _axisHistoryTimeList;
            set { _axisHistoryTimeList = value; OnPropertyChanged("AxisHistoryTimeList"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
