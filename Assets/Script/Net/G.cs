

namespace G
{
    // 车辆上行信息，接收到就发给UI和PanoSim
    public struct UplinkDGram
    {
        public short m_gram_header;      // 报文头 0xAAA
        public short m_gram_id;          // 报文标识 0xAF1
        public short m_vehicle_id;       // 车辆ID 0x00A ~ 0x00C
        public short m_speed;            // 速度
        public int m_steering;           // 方向盘转角
        public short m_gear;              // 挡位
        public short m_acc_pedal;
        public short m_brake_pedal;
        public short m_rpm;              // RPM 
        public short m_bucket_pos;       // 斗位置，0-底，1-中间，2-顶，3-故障
        public short m_bucket_angle;
        public short m_vehicle_state;    // 0:正常; 1:故障
        public short m_ctrl_sign;        // 1:自动; 2:遥控
        public short m_ctrl_info;        // 控制信息 1:车辆主动请求接管; 2:车辆故障 ...
        public short m_release_feedback;          // 
        public int m_lat_error;
        public int m_lon_error;
        public int m_angle_error;
        public int m_path_x;
        public int m_path_y;
        public short m_path_heading;
        public short m_path_id;
        short m_basket_filled;
        short unused0;
        short unused1;
        short unused2;
    };

    public enum GEAR
    {
        GEAR_R = -1,
        GEAR_N = 0,
        GEAR_D = 1
    };

    public enum BasketState
    {
        BasketDown = 0,
        BasketUp = 1,
        BasketFree = 17
    };

    public enum CtrlState
    {
        STATE_NOTHING = 0,
        STATE_AUTO = 1,
        STATE_TAKING_OVER = 2,
        STATE_RELEASING = 3,
        STATE_RELEASED = 4
    };
}