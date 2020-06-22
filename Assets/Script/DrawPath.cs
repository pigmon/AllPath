using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public struct PathPos
{
    public double x;
    public double y;

    public PathPos(double _x, double _y)
    {
        x = _x;
        y = _y;
    }
}

public struct VehicleStates
{
    public double x;
    public double y;
    public double heading;
    public int current_path;
    public bool request;        // 是否有接管请求

    public VehicleStates(double _x, double _y, double _heading, int _current_path, bool _request)
    {
        x = _x;
        y = _y;
        heading = _heading;
        request = _request;
        current_path = _current_path;
    }
}

public class DrawPath : MonoBehaviour
{
    private object __LOCKOBJ__ = new object();
    private const int DATA_LEN = 4;

    double m_min_x = 327680;
    double m_min_y = 327680;
    double m_max_x = -327670;
    double m_max_y = -327670;

    /// 像素/米
    double m_pixel_per_meter = 0;
    /// 控件边长对应的原始坐标长度，单位米
    double m_logic_length = 0;
    /// 原点逻辑位置 (原始路径坐标）
    double ori_x, ori_y;
    /// 车辆按实际比例缩放因子
    double truck_zoom = 3;
    /// 动画帧计数器
    int m_anim_counter = 0;
    /// 卡车尺寸，单位米
    double TRUCK_WIDTH = 4;
    double TRUCK_HEIGHT = 9;

    /// 绘图区域宽度（像素）
    float screen_width = 0.0f;
    /// 地图正方形边长（像素）
    float map_size = 0.0f; 
    /// 地图中心在绘图区域的坐标（像素）
    float map_origin_x_pixel = 0.0f;
    float map_origin_y_pixel = 0.0f;

    /// 全部路径存储
    /// <VehicleID, <PathID, PathPointList>>
    Dictionary<int, Dictionary<int, List<PathPos>>> m_all_path = new Dictionary<int, Dictionary<int, List<PathPos>>>();
    /// 全部路径文件
    /// <VehicleID, FilePath>
    Dictionary<int, string> m_file_list = new Dictionary<int, string> {
            { 567, @"D:\path\path567.txt" }, { 12, @"D:\path\path12.txt" }
        };
    /// 所有车辆的当前路径ID
    /// <VehicleID, PathID>
    Dictionary<int, int> m_current_path = new Dictionary<int, int> { { 567, 512 }, { 14, 515 } };
    /// 所有车辆的当前位置和航向角
    /// <VehicleID, x, y, heading>
    Dictionary<int, VehicleStates> m_vehicle_states = new Dictionary<int, VehicleStates>();

    bool m_b_drew_map = false;
    int PATH_STEP = 1;
    float PATH_WIDTH = 3.0f;

    Vector2 pt_start = new Vector2(0, 0);
    Vector2 pt_end = new Vector2(100, 100);

    void Awake()
    {
    }

    void Start()
    {
        Debug.Log("----------------------------");

        RectTransform rt = GetComponent<RectTransform>();
        Rect rect = rt.rect;

        screen_width = rect.width;
        map_size = rect.height;
        map_origin_x_pixel = transform.position.x;
        map_origin_y_pixel = transform.position.y;

        Vector2 tmp = CoordTrans_MapPixel_GUI(new Vector2(0, 0));

        Debug.Log("Map Size is : " + map_size.ToString());
        Debug.Log("ScreenWidth is : " + screen_width.ToString());
        Debug.Log("Rect x,y: " + rect.x.ToString() + ", " + rect.y.ToString());
        Debug.Log("Map Image Local Pos: " + transform.position);
        Debug.Log("Map (0,0) in GUI Coord : " + tmp);

        ReadPathFiles();

        Debug.Log("----------------------------");
        Debug.Log("Map Size in Meter: " + m_logic_length.ToString());
        Debug.Log("Pixel per Meter: " + m_pixel_per_meter.ToString());
        Debug.Log("Origin Point in Meter: " + ori_x.ToString() + ", " + ori_y.ToString());
        Debug.Log("Truck Size in Pixel: " + TRUCK_WIDTH.ToString() + ", " + TRUCK_HEIGHT.ToString());

        pt_start = new Vector2((float)ori_x, (float)ori_y);
        pt_end = new Vector2((float)(ori_x + 100), (float)ori_y + 100);
    }

    //void Update()
    //{

    //}

    public void OnGUI()
    {
        //if (!m_b_drew_map)
            DrawAllPath();
        //Drawing.DrawLine(CoordTrans_MapPixel_GUI(CoordTrans_MapMeter_MapPixel(pt_start)), CoordTrans_MapPixel_GUI(CoordTrans_MapMeter_MapPixel(pt_end)), Color.green, 4.0f);
    }

    /// <summary>
    /// 坐标变换：地图像素坐标系（地图中心为圆心） - GUI坐标系（左上角为00）
    /// </summary>
    /// <param name="_pt">地图像素坐标点</param>
    /// <returns>GUI坐标点，绘图用</returns>
    public Vector2 CoordTrans_MapPixel_GUI(Vector2 _pt)
    {
        float x = _pt.x + map_origin_x_pixel;
        float y = _pt.y + map_origin_y_pixel;

        return new Vector2(x, y);
    }

    public Vector2 CoordTrans_MapMeter_MapPixel(Vector2 _pt)
    {
        float x = (float)(-1 * (_pt.x - ori_x) * m_pixel_per_meter);
        float y = (float)((_pt.y - ori_y) * m_pixel_per_meter);

        return new Vector2(x, y);
    }

    /// <summary>
    /// 根据 m_file_list 填充 m_all_path
    /// </summary>
    public void ReadPathFiles()
    {
        m_all_path.Clear();
        m_vehicle_states.Clear();
        TRUCK_WIDTH = 4;
        TRUCK_HEIGHT = 9;

        foreach (KeyValuePair<int, string> path_file in m_file_list)
        {
            int vid = path_file.Key;
            string file_name = path_file.Value;

            if (!File.Exists(file_name))
                continue;

            char[] spliter = { ' ' };
            StreamReader sr = new StreamReader(file_name);

            Dictionary<int, List<PathPos>> dict_path = new Dictionary<int, List<PathPos>>();
            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();
                string[] part = line.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                if (part.Length != DATA_LEN)
                    continue;

                int path_id = -1;
                int.TryParse(part[3], out path_id);

                double x, y;
                double.TryParse(part[0], out x);
                double.TryParse(part[1], out y);

                PathPos pos = new PathPos(x, y);

                if (!dict_path.ContainsKey(path_id))
                {
                    dict_path.Add(path_id, new List<PathPos> { pos });
                }
                else
                {
                    dict_path[path_id].Add(pos);
                }

                if (x < m_min_x) m_min_x = x;
                else if (x > m_max_x) m_max_x = x;

                if (y < m_min_y) m_min_y = y;
                else if (y > m_max_y) m_max_y = y;
            }

            sr.Close();

            m_all_path.Add(vid, dict_path);

            m_vehicle_states.Add(vid, new VehicleStates(0, 0, 0, 0, false));

            //break;
        }

        // 计算地图基本数据
        m_logic_length = Math.Max((m_max_x - m_min_x), (m_max_y - m_min_y));
        m_pixel_per_meter = (double)map_size / m_logic_length;
        ori_x = 0.5 * (m_max_x - m_min_x) + m_min_x;
        ori_y = 0.5 * (m_max_y - m_min_y) + m_min_y;

        // 车辆基本数据
        TRUCK_WIDTH *= m_pixel_per_meter * truck_zoom;
        TRUCK_HEIGHT *= m_pixel_per_meter * truck_zoom;
    }

    void DrawAllPath()
    {
        foreach (KeyValuePair<int, Dictionary<int, List<PathPos>>> vehicle_pair in m_all_path)
        {
            int vid = vehicle_pair.Key;

            foreach (KeyValuePair<int, List<PathPos>> path_pair in vehicle_pair.Value)
            {
                int path_id = path_pair.Key;
                List<PathPos> pos_list = path_pair.Value;

                for (int i = 0; i + PATH_STEP < pos_list.Count - 1; i += PATH_STEP)
                {
                    PathPos pt1 = pos_list[i];
                    PathPos pt2 = pos_list[i + PATH_STEP];

                    Vector2 start_pt = new Vector2((float)pt1.x, (float)pt1.y);
                    Vector2 end_pt = new Vector2((float)pt2.x, (float)pt2.y);

                    Drawing.DrawLine(CoordTrans_MapPixel_GUI(CoordTrans_MapMeter_MapPixel(start_pt)), CoordTrans_MapPixel_GUI(CoordTrans_MapMeter_MapPixel(end_pt)), Color.green, PATH_WIDTH);
                }
            }

            break;
        }

        m_b_drew_map = true;
    }
}
