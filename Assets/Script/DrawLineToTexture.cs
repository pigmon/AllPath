using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;

public class DrawLineToTexture : MonoBehaviour
{
    public Image MapBackground;

    public Vector2[] m_point;//特征点位置
    public Color m_clear_color;
    public Color m_lineColor;
   
    public int m_width;
    public int m_height;

    private static Texture2D m_texure;

    private float m_drawing_x;
    private float m_drawing_y;

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

    int PATH_STEP = 1;

    public void InitCanvas()
    {
        MapBackground = GetComponent<Image>();

        RectTransform rt = MapBackground.GetComponent<RectTransform>();
        Rect rect = rt.rect;
        map_origin_x_pixel = transform.position.x;
        map_origin_y_pixel = transform.position.y;
        map_size = m_width = m_height = (int)rect.height;
        Vector3 pos = MapBackground.transform.position;

        screen_width = Screen.width;
        m_drawing_x = pos.x - m_width / 2;
        m_drawing_y = 0;


        m_texure = new Texture2D(m_width, m_height);
    }

    public void InitTexture()
    {
        //清空纹理对象
        for (int col = 0; col < m_width; col++)
        {
            for (int row = 0; row < m_height; row++)
            {
                m_texure.SetPixel(row, col, m_clear_color);
            }
        }

        foreach (KeyValuePair<int, Dictionary<int, List<PathPos>>> vehicle_pair in m_all_path)
        {
            int vid = vehicle_pair.Key;

            foreach (KeyValuePair<int, List<PathPos>> path_pair in vehicle_pair.Value)
            {
                List<PathPos> pos_list = path_pair.Value;

                for (int i = 0; i + PATH_STEP < pos_list.Count - 1; i += PATH_STEP)
                {
                    PathPos pt1 = pos_list[i];
                    PathPos pt2 = pos_list[i + PATH_STEP];

                    Vector2 start_pt = CoordTrans_MapMeter_Texture2D(new Vector2((float)pt1.x, (float)pt1.y));
                    Vector2 end_pt = CoordTrans_MapMeter_Texture2D(new Vector2((float)pt2.x, (float)pt2.y));

                    for (float j = 0; j < 1; j = j + 0.05f)
                    {
                        Vector2 temp = Vector2.Lerp(start_pt, end_pt, j);
                        m_texure.SetPixel(Convert.ToInt32(temp.x), Convert.ToInt32(temp.y), m_lineColor);
                    }
                }
            }
        }

        m_texure.Apply();

        MapBackground.material.mainTexture = m_texure;
    }


    // 贴图坐标，左下角00，x向右正，y向上正
    public IEnumerator Draw()
    {
        ////清空纹理对象
        //for (int col = 0; col < m_width; col++)
        //{
        //    for (int row = 0; row < m_height; row++)
        //    {
        //        m_texure.SetPixel(row, col, m_clear_color);
        //    }
        //}

        //foreach (KeyValuePair<int, Dictionary<int, List<PathPos>>> vehicle_pair in m_all_path)
        //{
        //    int vid = vehicle_pair.Key;

        //    foreach (KeyValuePair<int, List<PathPos>> path_pair in vehicle_pair.Value)
        //    {
        //        List<PathPos> pos_list = path_pair.Value;

        //        for (int i = 0; i + PATH_STEP < pos_list.Count - 1; i += PATH_STEP)
        //        {
        //            PathPos pt1 = pos_list[i];
        //            PathPos pt2 = pos_list[i + PATH_STEP];

        //            Vector2 start_pt = CoordTrans_MapMeter_Texture2D(new Vector2((float)pt1.x, (float)pt1.y));
        //            Vector2 end_pt = CoordTrans_MapMeter_Texture2D(new Vector2((float)pt2.x, (float)pt2.y));

        //            for (float j = 0; j < 1; j = j + 0.01f)
        //            {
        //                Vector2 temp = Vector2.Lerp(start_pt, end_pt, j);
        //                m_texure.SetPixel(Convert.ToInt32(temp.x), Convert.ToInt32(temp.y), m_lineColor);
        //            }
        //        }
        //    }
        //}

        //m_texure.Apply();
        yield return m_texure;
    }

    void OnPostRender()
    {
        //StartCoroutine(Draw());
    }

    void Start()
    {
        if (MapBackground == null)
        {
            Debug.LogError("Drag the Map background image to this script !");
        }    

        InitCanvas();

        ReadPathFiles();

        InitTexture();
    }

    //void OnGUI()
    //{
    //    GUI.DrawTexture(new Rect(m_drawing_x, m_drawing_y, m_width, m_height), m_texure);
    //}

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

    /// <summary>
    /// 坐标变换：地图像素坐标系（地图中心为圆心） - GUI坐标系（左上角为00）
    /// </summary>
    /// <param name="_pt">GUI坐标点</param>
    /// <returns>GUI坐标点，绘图用</returns>
    public Vector2 CoordTrans_MapPixel_GUI(Vector2 _pt)
    {
        float x = _pt.x + map_origin_x_pixel;
        float y = _pt.y + map_origin_y_pixel;

        return new Vector2(x, y);
    }

    /// <summary>
    /// 坐标变换：地图实际坐标系（地图中心为圆心，单位米） - 地图像素坐标系（地图中心为圆心，单位像素）
    /// </summary>
    /// <param name="_pt">地图像素坐标点</param>
    /// <returns></returns>
    public Vector2 CoordTrans_MapMeter_MapPixel(Vector2 _pt)
    {
        float x = (float)(-1 * (_pt.x - ori_x) * m_pixel_per_meter);
        float y = (float)((_pt.y - ori_y) * m_pixel_per_meter);

        return new Vector2(x, y);
    }

    /// <summary>
    /// 坐标变换: GUI 坐标（屏幕左上角00，x向右正，y向下正） - Texture2D坐标（左下角00，x向右正，y向上正）
    /// </summary>
    /// <param name="_pt">Texture2D 坐标点</param>
    /// <returns></returns>
    public Vector2 CoordTrans_GUI_Texture2D(Vector2 _pt)
    {
        float x = (float)(_pt.x - (map_origin_x_pixel - m_width / 2.0f));
        float y = -1 * (float)(_pt.y - m_height);

        return new Vector2(x, y);
    }

    public Vector2 CoordTrans_MapMeter_Texture2D(Vector2 _pt)
    {
        return CoordTrans_GUI_Texture2D(CoordTrans_MapPixel_GUI(CoordTrans_MapMeter_MapPixel(_pt)));
        //return CoordTrans_MapPixel_GUI(CoordTrans_MapMeter_MapPixel(_pt));
    }
}