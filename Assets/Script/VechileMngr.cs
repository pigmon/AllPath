using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VechileMngr : MonoBehaviour
{
    public TruckMngr truck_mngr;

    NetAgent<G.UplinkDGram> net_agent = new NetAgent<G.UplinkDGram>();
    Dictionary<int, G.UplinkDGram> dict_updgram = new Dictionary<int, G.UplinkDGram>();

    void Start()
    {
        dict_updgram.Clear();
        dict_updgram.Add(12, new G.UplinkDGram());
        dict_updgram.Add(13, new G.UplinkDGram());

        net_agent.AddRcver(12, 9110);
        net_agent.AddRcver(13, 9113);
    }

    void Update()
    {
        FetchData();
    }

    public void FetchData()
    {
        Dictionary<int, VehicleStates> all_path_param = new Dictionary<int, VehicleStates>();

        List<int> vid_list = new List<int>(dict_updgram.Keys);
        for (int i = 0; i < vid_list.Count; i++)
        {
            int vid = vid_list[i];
            G.UplinkDGram dgram = new G.UplinkDGram();
            net_agent.FetchData(vid, ref dgram);
            dict_updgram[vid] = dgram;

            // for AllPathCtrl
            double x = dgram.m_path_x * 0.0001;
            double y = dgram.m_path_y * 0.0001;
            double heading = dgram.m_path_heading * 0.01;
            int path_id = dgram.m_path_id;
            bool b_request = dgram.m_ctrl_info > 0;

            VehicleStates stats = new VehicleStates(x, y, heading, path_id, b_request);
            all_path_param.Add(vid, stats);
        }

        truck_mngr.UpdateVehicleStates(all_path_param);
    }

    private void OnApplicationQuit()
    {
        net_agent.Dispose();
    }

    public void OnButtonClick()
    {
        net_agent.Dispose();
    }
}
