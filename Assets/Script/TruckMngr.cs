using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class TruckMngr : MonoBehaviour
{
    public GameObject TruckPrefab;
    public DrawLineToTexture MapBG;

    private object __LOCKOBJ__ = new object();
    Dictionary<int, GameObject> m_truck_list = new Dictionary<int, GameObject>();

    /// 所有车辆的当前路径ID
    /// <VehicleID, PathID>
    // Dictionary<int, int> m_current_path = new Dictionary<int, int> { { 567, 512 }, { 14, 515 } };
    /// 所有车辆的当前位置和航向角
    /// <VehicleID, x, y, heading>
    Dictionary<int, VehicleStates> m_vehicle_states = new Dictionary<int, VehicleStates>();

    // Temp Test
    int m_path_index = 0;

    void Start()
    {
        m_truck_list.Clear();
        m_vehicle_states.Clear();

        AddTruck(12);
    }

    private void Update()
    {
        TempTest_GetNextPathPoint();

        DrawTrucks();
    }


    void DrawTrucks()
    {
        lock (__LOCKOBJ__)
        {
            // Draw Truck
            foreach (KeyValuePair<int, VehicleStates> pair in m_vehicle_states)
            {
                int vid = pair.Key;
                float x = (float)pair.Value.x;
                float y = (float)pair.Value.y;
                float heading = (float)pair.Value.heading;
                bool requesting = pair.Value.request;

                // TODO: Update Truck Gameobject
                if (m_truck_list.ContainsKey(vid))
                {
                    GameObject truck = m_truck_list[vid];
                    if (truck != null)
                    {
                        truck.transform.position = new Vector3(x, y, -0.2f);
                        truck.transform.localRotation = Quaternion.Euler(0, 0, heading);
                    }
                }
            }
        }
    }

    public void AddTruck(int _vid)
    {
        if (m_truck_list.ContainsKey(_vid) || m_vehicle_states.ContainsKey(_vid))
            return;

        GameObject truck = Instantiate(TruckPrefab) as GameObject;
        if (truck != null)
        {
            truck.transform.parent = transform;
            TruckScript script = truck.GetComponent<TruckScript>();
            if (script != null)
            {
                script.SetTruckID(_vid.ToString());
            }    

            m_truck_list.Add(_vid, truck);
            m_vehicle_states.Add(_vid, new VehicleStates(0, 0, 0, 0, false));
        }
    }

    /// <summary>
    /// 当接收到车辆回传信息报文后，更新所有车辆的位置、航向角、当前路径ID及是否有接管请求。
    /// </summary>
    /// <param name="_dict">[VehicleID:VehicleStates]形式的字典，来源于车辆上行报文</param>
    public void UpdateVehicleStates(Dictionary<int, VehicleStates> _dict)
    {
        lock (__LOCKOBJ__)
        {
            foreach (KeyValuePair<int, VehicleStates> pair in _dict)
            {
                int vid = pair.Key;
                if (m_vehicle_states.ContainsKey(vid))
                {
                    Vector2 real_pos = new Vector2((float)pair.Value.x, (float)pair.Value.y);
                    Vector2 gui_pos = MapBG.CoordTrans_TruckPos_Texture2D(real_pos);
                    m_vehicle_states[vid] = new VehicleStates(gui_pos.x, gui_pos.y, pair.Value.heading + 90, pair.Value.current_path, pair.Value.request);
                }
            }
        }
    }


    // Temp Test
    public void TempTest_GetNextPathPoint()
    {
        if (m_path_index < 3000)
            m_path_index++;

        double x = MapBG.AllPath[12][512][m_path_index].x;
        double y = MapBG.AllPath[12][512][m_path_index].y;

        Dictionary<int, VehicleStates> dict = new Dictionary<int, VehicleStates>();
        dict.Add(12, new VehicleStates(x, y, 0, 0, false));
        UpdateVehicleStates(dict);
    }
}
