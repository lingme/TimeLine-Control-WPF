﻿using System;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoStateAxis
{
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

    public class VideoStateAxisControl : Control
    {
        private StackPanel _videoHistoryPanel;           //历史时间轴容器
        private ScrollViewer _scrollViewer;                 //滚动视图
        private Canvas _axisCanvas;                          //时间刻度尺容器
        private Canvas _axisCanvasTimeText;             //时间刻度时间文字容器
        private Grid _timePoint;                                //进度指针
        private TextBlock _currentTime;                    //进度指针时间
        private Canvas _timePanel;                           //进度容器
        private Canvas _timeLine;                             //进度指针容器
        private Slider _zoomSlider;                          //缩放时间轴滑块
        private Canvas _clipCanvas;                         //剪辑控制移动容器
        private Border _clipStartBorder;                  //剪辑左调解
        private Border _clipAreaBorder;                  //剪辑滑块
        private Border _clipEndBorder;                   //剪辑右调解
        private StackPanel _clipStackPanel;              //剪辑滑块容器

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

        public static readonly DependencyProperty HistoryVideoSourceProperty = DependencyProperty.Register(
            "HistoryVideoSources", 
            typeof(ObservableCollection<VideoStateItem>), 
            typeof(VideoStateAxisControl), 
            new PropertyMetadata(null, OnHistoryVideoSourcesChanged, CoerceHistoryVideoSrouces));

        public static readonly DependencyProperty StateTimeProperty = DependencyProperty.Register(
            "SerStateTime", 
            typeof(DateTime), 
            typeof(VideoStateAxisControl), 
            new PropertyMetadata(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")), OnTimeChanged, CoerceTime));

        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register(
            "SerEndTime", 
            typeof(DateTime), 
            typeof(VideoStateAxisControl), 
            new PropertyMetadata(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")), OnTimeChanged, CoerceTime));

        public static readonly DependencyProperty AxisTimeProperty = DependencyProperty.Register(
            "AxisTime",
            typeof(DateTime),
            typeof(VideoStateAxisControl),
            new PropertyMetadata(OnAxisTimeChanged));

        public static readonly DependencyProperty ClipStartTimeProperty = DependencyProperty.Register(
            "ClipStartTime",
            typeof(DateTime),
            typeof(VideoStateAxisControl));

        public static readonly DependencyProperty ClipEndTimeProperty = DependencyProperty.Register(
            "ClipEndTime", 
            typeof(DateTime), 
            typeof(VideoStateAxisControl));

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
            get { return _scrollViewer == null ? 0 : ((_scrollViewer.ActualWidth - 10) * Magnification ) / 24; }
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
        private double StartClipOffset = 0;

        /// <summary>
        /// 剪辑左坐标
        /// </summary>
        private double StartPanelOffset = 0;

        /// <summary>
        /// 时间轴缩放比例
        /// </summary>
        private double Magnification = 1;

        /// <summary>
        /// 历史查询时间 - 改变
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnTimeChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = (VideoStateAxisControl)d;
            if(AxisOb != null)
            {
                AxisOb.InitializeAxis();
            }
        }

        /// <summary>
        /// 历史查询时间 - 强制转换
        /// </summary>
        /// <param name="d"></param>
        /// <param name="basevalue"></param>
        /// <returns></returns>
        private static object CoerceTime(DependencyObject d , object basevalue)
        {
            var _eDataTime = (DateTime)basevalue;
            return _eDataTime;
        }

        /// <summary>
        /// 历史视频来源 - 改变
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnHistoryVideoSourcesChanged(DependencyObject d ,DependencyPropertyChangedEventArgs e)
        {
            VideoStateAxisControl AxisOb = (VideoStateAxisControl)d;
            if(AxisOb.HistoryVideoSources != null && AxisOb.HistoryVideoSources.Count() > 0)
            {
                AxisOb.HistoryVideoSources.CollectionChanged += (s , o)=> 
                {
                    AxisOb.InitializeAxis();
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
        /// 历史视频来源 - 强制转换
        /// </summary>
        /// <param name="d"></param>
        /// <param name="basevalue"></param>
        /// <returns></returns>
        private static object CoerceHistoryVideoSrouces(DependencyObject d ,object basevalue)
        {
            return basevalue;
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
        /// 通过事件刷新指针位置
        /// </summary>
        /// <param name="dt"></param>
        private void RefreshTimeLineLeft(DateTime dt)
        {
            TimeSpan ts = dt - DateTime.Parse(SerStateTime.ToString("yyyy-MM-dd 00:00:00"));
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
                StartClipOffset = e.GetPosition(_clipCanvas).X;
                StartPanelOffset = Canvas.GetLeft(_clipStackPanel);
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
                    case Parid_clipAreaBorder:
                        ClipArea(e.GetPosition(_clipCanvas));
                        break;
                    case Parid_clipEndBorder:
                        ClipEnd(e.GetPosition(_clipCanvas));
                        break;
                }
            }
        }

        /// <summary>
        /// 剪辑结束滑块
        /// </summary>
        private void ClipEnd(Point pt)
        {
            double clipWidth = pt.X - Canvas.GetLeft(_clipStackPanel) ;
            _clipAreaBorder.Width = clipWidth <= 0 ? 0 : 
                clipWidth > _axisCanvas.ActualWidth - Canvas.GetLeft(_clipStackPanel)  ? 
                _axisCanvas.ActualWidth - Canvas.GetLeft(_clipStackPanel) : clipWidth;
        }
        
        /// <summary>
        /// 剪辑开始滑块
        /// </summary>
        /// <param name="pt"></param>
        private void ClipStart(Point pt)
        {
            
        }

        /// <summary>
        /// 剪辑滚动滑块
        /// </summary>
        /// <param name="pt"></param>
        private void ClipArea(Point pt)
        {
            double clipLeft = StartPanelOffset + (pt.X - StartClipOffset) < 0 ? 0 : 
                StartPanelOffset + (pt.X - StartClipOffset) > _clipCanvas.ActualWidth - _clipAreaBorder.Width ?
                _axisCanvas.ActualWidth - _clipAreaBorder.Width :
                StartPanelOffset + (pt.X - StartClipOffset);
            Canvas.SetLeft(_clipStackPanel, clipLeft);
        }

        /// <summary>
        /// 时间缩放滑块事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Magnification = Math.Round(e.NewValue , 2);
            if(!double.IsNaN(Canvas.GetLeft(_timeLine)))
            {
                Canvas.SetLeft(_timeLine , AxisTime.Hour * Dial_Cell_H + AxisTime.Minute * Dial_Cell_M + AxisTime.Second * Dial_Cell_S);
            }
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
            double timePointMaxLeft = _timePanel.ActualWidth - _timePoint.ActualWidth;
            double currentTimeMaxLeft = _timePanel.ActualWidth - _currentTime.ActualWidth;
            _currentTime.Text = XToDateTime(delta < 0 ? 0 : (delta > timePointMaxLeft ? timePointMaxLeft : delta));
            _currentTime.Margin = delta < currentTimeMaxLeft ?
                new Thickness(delta < 0 ? 10 : delta + 10, 0, 0, 0) :
                new Thickness(delta > timePointMaxLeft ? timePointMaxLeft - _currentTime.ActualWidth : delta - _currentTime.ActualWidth, 0, 0, 0);
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
            XToDateTime(Canvas.GetLeft(_timeLine));
        }

        /// <summary>
        /// 计算指针拖动的显示时间
        /// </summary>
        /// <param name="x">指针在Canvas容器中的Left坐标值</param>
        private string XToDateTime(double point_x)
        {
            DateTime dt = DateTime.Parse(SerStateTime.ToString("yyyy-MM-dd 00:00:00"));
            double time;
            int H , M , S;
            time = point_x  / Dial_Cell_H  ;
            H = (int)(time);
            time = (time - H) * 60;
            M = (int)(time);
            S = (int)((time - M) * 60);
            return ((AxisTime = dt.AddHours(H).AddMinutes(M).AddSeconds(S))).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitializeAxis()
        {
            AddTimeTextBlock();
            AddTimeLine();
            AddHisPie();
        }

        /// <summary>
        /// 添加时间刻度文字
        /// </summary>
        /// <param name="HaveMathTextBlock">需要填充的时间文字数量</param>
        private void AddTimeTextBlock()
        {
            if(_axisCanvasTimeText != null)
            {
                _axisCanvasTimeText.Width = _scrollViewer.ActualWidth * Magnification + 0.1;
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
        /// 添加时间刻度
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
        /// 填充时间轴
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
        /// 计算 Rectangle
        /// </summary>
        private void DisplayData(Dictionary<KeyValuePair<int, int>,bool> dic)
        {
            DateTime serTime = SerStateTime;
            Canvas TimeCanvas = new Canvas(){ Width = _scrollViewer.ActualWidth - 10 };
            foreach (var item in dic)
            {
                TimeCanvas.Children.Add(new Rectangle()
                {
                    Width = item.Key.Value * Dial_Cell_M,
                    Height = item.Value ? 15 : 0,
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
                    int ins = dic.Count;
                    dic.Add(new KeyValuePair<int, int>(ins++, item.Value.Length), item.Value.Contains('1') ? true : false);
                }
            }
            return dic;
        }

        /// <summary>
        /// 将原始控件的样式覆盖
        /// </summary>
        static VideoStateAxisControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoStateAxisControl), new FrameworkPropertyMetadata(typeof(VideoStateAxisControl)));
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
        }
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
