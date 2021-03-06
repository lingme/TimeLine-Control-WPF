# TimeAxis Control WPF
---
The WPF-like control of the Adobe AE timeline is completely based on the binding of dependent attributes, which is very extensible and can be modified by itself

#### View视图代码：
```C {.line-numbers}
<axis:VideoStateAxisControl Grid.Row="0" SerStateTime="{Binding StartTime}" SerEndTime="{Binding EndTime}" HistoryVideoSources="{Binding VideoHistoryList}" Grid.RowSpan="2" ></axis:VideoStateAxisControl>
```

#### 已有依赖属性说明：
>**SerStateTime**
* Type：System.DateTime
* 说明：时间轴开始搜索时间

>**SerEndTime**
* Type：System.DateTime
* 说明：时间轴结束搜索时间

>**HistoryVideoSources**
* Type：List\<VideoStateItem\>
* 说明：时间轴时间
* 包含属性：

```java {.line-numbers}
public class VideoStateItem
    {
        /// <summary>
        /// 相机名称
        /// </summary>
        public string CameraName { get; set; }

        /// <summary>
        /// 相机是否选中
        /// </summary>
        public bool CameraChecked { get; set; }

        /// <summary>
        /// 相机历史视频视频时间集
        /// </summary>
        public char[] AxisHistoryTimeList { get; set; }
    }
```

![A](https://github.com/lingme/Picture_Bucket/raw/master/TimeAxis_Control_WPF_img/index_2.jpg)







