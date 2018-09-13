using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using USB2XXX;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static APA.Ultrasonic;
using static APA.RTK;
using System.Collections.Generic;

#region ZLG CAN Struct
//1.ZLGCAN系列接口卡信息的数据类型。
public struct VCI_BOARD_INFO
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Reserved;
}


/////////////////////////////////////////////////////
//2.定义CAN信息帧的数据类型。
unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;
    public byte TimeFlag;
    public byte SendType;
    public byte RemoteFlag;//是否是远程帧
    public byte ExternFlag;//是否是扩展帧
    public byte DataLen;

    public fixed byte Data[8];

    public fixed byte Reserved[3];

}
////2.定义CAN信息帧的数据类型。
//public struct VCI_CAN_OBJ 
//{
//    public UInt32 ID;
//    public UInt32 TimeStamp;
//    public byte TimeFlag;
//    public byte SendType;
//    public byte RemoteFlag;//是否是远程帧
//    public byte ExternFlag;//是否是扩展帧
//    public byte DataLen;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
//    public byte[] Data;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
//    public byte[] Reserved;

//    public void Init()
//    {
//        Data = new byte[8];
//        Reserved = new byte[3];
//    }
//}

//3.定义CAN控制器状态的数据类型。
public struct VCI_CAN_STATUS
{
    public byte ErrInterrupt;
    public byte regMode;
    public byte regStatus;
    public byte regALCapture;
    public byte regECCapture;
    public byte regEWLimit;
    public byte regRECounter;
    public byte regTECounter;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}

//4.定义错误信息的数据类型。
public struct VCI_ERR_INFO
{
    public UInt32 ErrCode;
    public byte Passive_ErrData1;
    public byte Passive_ErrData2;
    public byte Passive_ErrData3;
    public byte ArLost_ErrData;
}

//5.定义初始化CAN的数据类型
public struct VCI_INIT_CONFIG
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;
    public byte Timing0;
    public byte Timing1;
    public byte Mode;
}

public struct CHGDESIPANDPORT
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
        szpwd = new byte[10];
        szdesip = new byte[20];
    }
}

///////// new add struct for filter /////////
//typedef struct _VCI_FILTER_RECORD{
//    DWORD ExtFrame;	//是否为扩展帧
//    DWORD Start;
//    DWORD End;
//}VCI_FILTER_RECORD,*PVCI_FILTER_RECORD;
public struct VCI_FILTER_RECORD
{
    public UInt32 ExtFrame;
    public UInt32 Start;
    public UInt32 End;
}
#endregion

#region User Define Struct

public struct VehicleImformation
{
    public double Speed;
    public double Speed_ms;
    public double Displacement;
    public Int16 TargetSteeringWheelAngle;
    public Int16 ActualSteeringWheelAngle;
    public UInt16 SteeringWheelAgularVelocity;
    public double SteeringWheelTorque;
    public byte ECU_status;
    public byte CommunicationStatus;
    public double Yaw;
    public double Last_Yaw;
    public double X;
    public double Y;
    public double R;
}

public struct TimeStruct
{
    public UInt64 SystemTime;
    public UInt64 LastSystemTime;
    public UInt64 TimeErr;
}
#endregion


namespace APA
{
    public partial class Form1 : Form
    {
        #region ZLG CAN Variable define and the interface
        // 设备型号
        const int VCI_PCI5121 = 1;
        const int VCI_PCI9810 = 2;
        const int VCI_USBCAN1 = 3;
        const int VCI_USBCAN2 = 4;
        const int VCI_USBCAN2A = 4;
        const int VCI_PCI9820 = 5;
        const int VCI_CAN232 = 6;
        const int VCI_PCI5110 = 7;
        const int VCI_CANLITE = 8;
        const int VCI_ISA9620 = 9;
        const int VCI_ISA5420 = 10;
        const int VCI_PC104CAN = 11;
        const int VCI_CANETUDP = 12;
        const int VCI_CANETE = 12;
        const int VCI_DNP9810 = 13;
        const int VCI_PCI9840 = 14;
        const int VCI_PC104CAN2 = 15;
        const int VCI_PCI9820I = 16;
        const int VCI_CANETTCP = 17;
        const int VCI_PEC9920 = 18;
        const int VCI_PCI5010U = 19;
        const int VCI_USBCAN_E_U = 20;
        const int VCI_USBCAN_2E_U = 21;
        const int VCI_PCI5020U = 22;
        const int VCI_EG20T_CAN = 23;
        const int VCI_USBCAN_4E_U = 31;

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        //static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        unsafe static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, byte* pData);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        //static UInt32 m_devtype = 4;//USBCAN2
        static UInt32 m_devtype = 31;//USBCAN-2e-u
        //usb-e-u 波特率
        static UInt32[] GCanBrTab = new UInt32[10]{
                        1000000,800000,500000,
                        250000, 125000, 100000,
                        50000, 20000, 10000,
                        5000
                };


        const UInt32 STATUS_OK = 1;

        UInt32 m_bOpen = 0;
        UInt32 m_devind = 0;
        UInt32 m_canind = 0;

        VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[50];

        UInt32[] m_arrdevtype = new UInt32[20];

        string[] DeviceType = new string[2] { "USBCAN_4E_U", "USBCAN_2E_U" };
        string[] BaudRate = new string[10] { "1000kbps", "800kbps", "500kbps", "250kbps", "125kbps", "100kbps", "50kbps", "20kbps", "10kbps", "5kbps" };
        string[] ECU_Status = new string[8] { "待机模式", "自动驾驶模式", "未知", "未知", "手动模式", "手动介入恢复模式", "警告模式", "错误模式" };
        string[] ComunicationStatus = new string[2] { "通信正常", "通信异常" };
        #endregion

        #region LIN Device Configure relation varibale

        bool LinDeviceStatus = false;
        bool LinTreadStatus = false;
        Ultrasonic m_UltrasonicObj = new Ultrasonic();
        #endregion

        #region Sensing Relation Control Status Variable
        string[] SamplingModle = new string[2] { "两侧4组采集", "12组轮询采集" };

        //LRU STP313 传感器 控件显示
        TextBox[][] SensingControl_LRU = new TextBox[4][];
        //SRU STP318 传感器 控件显示
        TextBox[] SensingControl_SRU_TextBox = new TextBox[8];
        Label[] SensingControl_SRU_Label = new Label[8];

        public LIN_STP313_ReadData[] m_LIN_STP313_ReadData = new LIN_STP313_ReadData[4];
        public LIN_STP318_ReadData[] m_LIN_STP318_ReadData = new LIN_STP318_ReadData[8];

        #endregion

        #region Time Relation Function Interface and Variable
        [DllImport("winmm")]
        static extern uint timeGetTime();
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        UInt64 SystemTime, LastSystemTime, TimeErr;
        TimeStruct CanReceiveTime = new TimeStruct();
        TimeStruct UltrasonicSamplingTime = new TimeStruct();
        TimeStruct FileSaveTime = new TimeStruct();
        #endregion

        #region File Operation Relation Variable
        StreamWriter DataSave;
        string FilePath, localFilePath, newFileName, fileNameExt;
        bool DataSaveStatus = false;
        #endregion

        #region Vehicle Relation Variable
        VehicleImformation m_VehicleImformation = new VehicleImformation();
        #endregion

        ChartShowForm cf = new ChartShowForm();

        #region 串口操作相关变量
        RTK m_rtk = new RTK();
        private List<byte> buffer = new List<byte>(4096);//默认分配1页内存，并始终限制不允许超过
        private byte[] binary_data = new byte[820];
        RTK_Imformation m_RTK_Imformation = new RTK_Imformation();

        private string [] RTK_SystemStatus = new string[] {"初始化", "粗对准", "精对准", "GPS 定位", "GPS 定向", "RTK", "DMI 组合", "DMI 标定", "纯惯性", "零速校正", "VG 模式" };
        private string[] RTK_AntannaType = new string[] { "GPS","北斗", "双模" };
        #endregion

        #region 函数方法

        #region CAN设备相关方法
        /// <summary>
        /// CAN0 Receive Function
        /// </summary>
        unsafe public void CAN0_ReceiveFunction(ref VehicleImformation m_vehicle)
        {
            UInt32 res = new UInt32();
            res = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);
            if (res == 0)
                return;
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)res);

            res = VCI_Receive(m_devtype, m_devind, m_canind, pt, res, 100);

            for (UInt32 i = 0; i < res; i++)
            {
                VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                if (obj.ID == 0x0C1)
                {
                    m_vehicle.ActualSteeringWheelAngle = (Int16)((UInt16)(obj.Data[2] << 8 | obj.Data[3]) * 0.1 - 780);//实际角度
                    m_vehicle.TargetSteeringWheelAngle = (Int16)((UInt16)(obj.Data[6] << 8 | obj.Data[7]) * 0.1 - 780);//目标角度
                    m_vehicle.SteeringWheelAgularVelocity = (UInt16)(obj.Data[1] * 25);//转角速度
                    m_vehicle.SteeringWheelTorque = (obj.Data[4] - 128) * 0.07;//扭矩
                    m_vehicle.ECU_status = obj.Data[0];//ECU状态
                    m_vehicle.CommunicationStatus = obj.Data[5];//通信状态
                }
            }
            Marshal.FreeHGlobal(pt);
        }

        /// <summary>
        /// CAN1 Receive Function
        /// </summary>
        unsafe void CAN1_ReceiveFunction(ref VehicleImformation m_vehicle)
        {
            UInt32 res = new UInt32();
            res = VCI_GetReceiveNum(m_devtype, m_devind, 1);
            if (res == 0)
                return;
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)res);

            res = VCI_Receive(m_devtype, m_devind, 1, pt, res, 100);

            for (UInt32 i = 0; i < res; i++)
            {
                VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                if (obj.ID == 0x300)
                {
                    CanReceiveTime.SystemTime = timeGetTime();
                    if(CanReceiveTime.LastSystemTime == 0)
                    {
                        CanReceiveTime.TimeErr = 0;
                    }
                    else
                    {
                        CanReceiveTime.TimeErr = CanReceiveTime.SystemTime - CanReceiveTime.LastSystemTime;     
                    }
                    CanReceiveTime.LastSystemTime = CanReceiveTime.SystemTime;

                    m_vehicle.Speed = (UInt16)(obj.Data[0] << 8 | obj.Data[1]) / 15.4583;//车速
                    VehiclePositioning(ref m_vehicle);
                }
            }
            Marshal.FreeHGlobal(pt);
        }

        /// <summary>
        /// 地盘CAN发送函数
        /// </summary>
        unsafe void SendData()
        {
            if (m_bOpen == 0)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.i
            //sendobj.Init();
            sendobj.SendType = 0;//0 -> 正常发送 ;2 -> 自发自收(byte)comboBox_SendType.SelectedIndex;
            sendobj.RemoteFlag = 0;//标准帧 (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = 0;// 标准帧数(byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = 0x215;// System.Convert.ToUInt32("0x" + textBox_ID.Text, 16);
            sendobj.DataLen = 8;

            byte[] b_temp = BitConverter.GetBytes((Convert.ToInt16(textBox_Angle.Text) + 780) * 10);

            sendobj.Data[0] = b_temp[1];
            sendobj.Data[1] = b_temp[0];

            sendobj.Data[2] = 0x31;

            sendobj.Data[3] = 0x00;

            sendobj.Data[4] = (byte)(Convert.ToUInt16(textBox_AngularVelocity.Text) / 25);

            sendobj.Data[5] = 0x80;

            sendobj.Data[6] = 0x80;

            sendobj.Data[7] = 0x00;
            int nTimeOut = 3000;
            VCI_SetReference(m_devtype, m_devind, m_canind, 4, (byte*)&nTimeOut);
            if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
            {
                MessageBox.Show("发送失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 地盘CAN发送函数,用于控制模块
        /// </summary>
        unsafe void CanSendData(Int16 WheelAngle, UInt16 AngularVelocity)
        {
            if (m_bOpen == 0)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.SendType = 0;//0 -> 正常发送 ;2 -> 自发自收(byte)comboBox_SendType.SelectedIndex;
            sendobj.RemoteFlag = 0;//标准帧 (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = 0;// 标准帧数(byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = 0x215;// System.Convert.ToUInt32("0x" + textBox_ID.Text, 16);
            sendobj.DataLen = 8;

            byte[] b_temp = BitConverter.GetBytes((WheelAngle + 780) * 10);

            sendobj.Data[0] = b_temp[1];
            sendobj.Data[1] = b_temp[0];

            sendobj.Data[2] = 0x31;

            sendobj.Data[3] = 0x00;

            sendobj.Data[4] = (byte)(AngularVelocity / 25);

            sendobj.Data[5] = 0x80;

            sendobj.Data[6] = 0x80;

            sendobj.Data[7] = 0x00;
            int nTimeOut = 3000;
            VCI_SetReference(m_devtype, m_devind, m_canind, 4, (byte*)&nTimeOut);
            if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
            {
                MessageBox.Show("发送失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 测试地盘CAN接收的函数
        /// </summary>
        unsafe void RecTestSendData()
        {
            if (m_bOpen == 0)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.SendType = 2;//0 -> 正常发送 (byte)comboBox_SendType.SelectedIndex;
            sendobj.RemoteFlag = 0;//标准帧 (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = 0;// 标准帧数(byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = 0x0C1;// System.Convert.ToUInt32("0x" + textBox_ID.Text, 16);
            sendobj.DataLen = 8;

            byte[] b_temp = BitConverter.GetBytes((Convert.ToInt16(textBox_Angle.Text) + 780) * 10);

            sendobj.Data[0] = 0;
            sendobj.Data[1] = 2;

            sendobj.Data[2] = 0x25;

            sendobj.Data[3] = 0x80;

            sendobj.Data[4] = 132;

            sendobj.Data[5] = 1;

            sendobj.Data[6] = 0x25;

            sendobj.Data[7] = 0x80;
            int nTimeOut = 3000;
            VCI_SetReference(m_devtype, m_devind, m_canind, 4, (byte*)&nTimeOut);
            if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
            {
                MessageBox.Show("发送失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 测试车身CAN接收函数
        /// </summary>
        unsafe void RecTestSpeedSendData()
        {
            if (m_bOpen == 0)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.SendType = 2;//0 -> 正常发送 (byte)comboBox_SendType.SelectedIndex;
            sendobj.RemoteFlag = 0;//标准帧 (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = 0;// 标准帧数(byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = 0x300;// System.Convert.ToUInt32("0x" + textBox_ID.Text, 16);
            sendobj.DataLen = 8;

            sendobj.Data[0] = 0x0;
            sendobj.Data[1] = 0x9A;

            sendobj.Data[2] = 0x25;

            sendobj.Data[3] = 0x80;

            sendobj.Data[4] = 132;

            sendobj.Data[5] = 1;

            sendobj.Data[6] = 0x25;

            sendobj.Data[7] = 0x80;
            int nTimeOut = 3000;
            VCI_SetReference(m_devtype, m_devind, 1, 4, (byte*)&nTimeOut);
            if (VCI_Transmit(m_devtype, m_devind, 1, ref sendobj, 1) == 0)
            {
                MessageBox.Show("发送失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        /// <summary>
        /// 车辆位置定位
        /// </summary>
        /// <param name="m_Vehicle"></param>
        private void VehiclePositioning(ref VehicleImformation m_Vehicle)
        {
            m_Vehicle.Speed_ms = m_Vehicle.Speed / 3.6;
            m_Vehicle.Displacement += m_Vehicle.Speed_ms * CanReceiveTime.TimeErr * 0.001;
            //m_Vehicle.Displacement += m_Vehicle.Speed_ms * CanReceiveTime.TimeErr / 3600;
            if (m_Vehicle.ActualSteeringWheelAngle != 0)
            {
                m_Vehicle.Yaw = m_Vehicle.Last_Yaw - Math.Sign(m_Vehicle.ActualSteeringWheelAngle) * m_Vehicle.Displacement / m_Vehicle.R;
                m_Vehicle.X -= Math.Sign(m_Vehicle.ActualSteeringWheelAngle) * m_Vehicle.R * (Math.Cos(m_Vehicle.Last_Yaw) - Math.Cos(m_Vehicle.Yaw));
                m_Vehicle.Y -= Math.Sign(m_Vehicle.ActualSteeringWheelAngle) * m_Vehicle.R * (Math.Sin(m_Vehicle.Yaw) - Math.Sin(m_Vehicle.Last_Yaw));
                m_Vehicle.Last_Yaw = m_Vehicle.Yaw;
            }
            else
            {
                m_Vehicle.X += m_Vehicle.Displacement * Math.Sin(m_Vehicle.Yaw);
                m_Vehicle.Y += m_Vehicle.Displacement * Math.Cos(m_Vehicle.Yaw);
            }
        }

        /// <summary>
        /// 超声波数据的坐标变换
        /// </summary>
        /// <param name="m_318"></param>
        /// <param name="m_313"></param>
        private void UltrasonicCoordinateChange(LIN_STP318_ReadData m_318, LIN_STP313_ReadData m_313)
        {

        }

        /// <summary>
        /// 车辆信息显示
        /// </summary>
        /// <param name="m_Vehicle"></param>
        private void VehicleInformationShow(VehicleImformation m_Vehicle)
        {
            label11.Text = ECU_Status[m_Vehicle.ECU_status];//ECU状态
            label12.Text = ComunicationStatus[m_Vehicle.CommunicationStatus];
            textBox2.Text = Convert.ToString(m_Vehicle.SteeringWheelAgularVelocity) + "°/s";//转角速度
            textBox3.Text = Convert.ToString(m_Vehicle.SteeringWheelTorque) + " Nm";//扭矩

            textBox4.Text = Convert.ToString(m_Vehicle.TargetSteeringWheelAngle) + "°";//目标角度
            textBox5.Text = Convert.ToString(m_Vehicle.ActualSteeringWheelAngle) + "°";//实际角度
            textBox33.Text = Convert.ToString(m_Vehicle.Speed) + " km/h";//实际速度
            textBox35.Text = Convert.ToString(m_Vehicle.Displacement) + " m";//实际位移

            label88.Text = Convert.ToString(m_Vehicle.X) + " m";//X轴位移
            label89.Text = Convert.ToString(m_Vehicle.Y) + " m";//Y轴位移

            label91.Text = Convert.ToString(m_Vehicle.Yaw*57.6) + " 度";//偏航角度
        }

        #endregion

        #region 控件事件
        #region 窗体事件
        public Form1()
        {
            InitializeComponent();
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            UInt16 i;
            for(i=0;i<2;i++)
            {
                comboBox_DevType.Items.Add(DeviceType[i]);
            }
            m_arrdevtype[0] = VCI_USBCAN_4E_U;
            m_arrdevtype[1] = VCI_USBCAN_2E_U;

            comboBox_DevType.SelectedIndex = 0;
            for (i = 0; i < 8; i++)
            {
                comboBox_DevIndex.Items.Add(i);
            }
            comboBox_DevIndex.SelectedIndex = 0;
            for (i = 0; i < 2; i++)
            {
                comboBox_CANIndex.Items.Add(i);
            }
            //comboBox_CANIndex.Items.Add("all");
            comboBox_CANIndex.SelectedIndex = 0;
            for (i = 0; i < 10; i++)
            {
                comboBox_BaudRate.Items.Add(BaudRate[i]);
            }
            comboBox_BaudRate.SelectedIndex = 2;
            button_Connect.BackColor = Color.Red;
            //长距离传感器的控件显示
            SensingControl_LRU = new TextBox[4][] {
                new TextBox[5]{ textBox13, textBox14, textBox15, textBox16, textBox17 },
                new TextBox[5]{ textBox18, textBox19, textBox20, textBox21, textBox22 },
                new TextBox[5]{ textBox23, textBox24, textBox25, textBox26, textBox27 },
                new TextBox[5]{ textBox28, textBox29, textBox30, textBox31, textBox32 }
            };
            //短距离传感器的控件初始化
            SensingControl_SRU_TextBox = new TextBox[8] { textBox1,textBox6,textBox7,textBox8,textBox9,textBox10,textBox11,textBox12};
            SensingControl_SRU_Label = new Label[8] { label26 , label27 , label28 , label29 , label30 , label31 , label41 , label42 };


            for (i=0;i<2;i++)
            {
                comboBox2.Items.Add(SamplingModle[i]);
            }
            comboBox2.SelectedIndex = 0;

            m_VehicleImformation.R = 3.85;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (m_bOpen == 1)
            {
                VCI_ResetCAN(m_devtype, m_devind, m_canind);
                VCI_ResetCAN(m_devtype, m_devind, 1);
            }

            if (LinDeviceStatus)
            {
                m_UltrasonicObj.CloseUltrasonicDevice();
            }
        }
        #endregion

        #region CAN设备相关操作
        /// <summary>
        /// CAN设备连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        unsafe private void button_Connect_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 1)
            {
                VCI_CloseDevice(m_devtype, m_devind);
                m_bOpen = 0;
            }
            else
            {
                m_devtype = m_arrdevtype[comboBox_DevType.SelectedIndex];

                m_devind = (UInt32)comboBox_DevIndex.SelectedIndex;
                m_canind = (UInt32)comboBox_CANIndex.SelectedIndex;
                if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
                {
                    MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                //USB-E-U 代码
                UInt32 baud;
                baud = GCanBrTab[comboBox_BaudRate.SelectedIndex];
                
                if (VCI_SetReference(m_devtype, m_devind, m_canind, 0, (byte*)&baud) != STATUS_OK)
                {

                    MessageBox.Show("设置波特率错误，打开设备0失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    VCI_CloseDevice(m_devtype, m_devind);
                    return;
                }
                if (VCI_SetReference(m_devtype, m_devind, 1, 0, (byte*)&baud) != STATUS_OK)
                {

                    MessageBox.Show("设置波特率错误，打开设备1失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    VCI_CloseDevice(m_devtype, m_devind);
                    return;
                }
                //滤波设置
                //////////////////////////////////////////////////////////////////////////
                m_bOpen = 1;
                VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
                config.AccCode = 00000000;// System.Convert.ToUInt32("0x" + textBox_AccCode.Text, 16);
                config.AccMask = 0xFFFFFFFF;// System.Convert.ToUInt32("0x" + textBox_AccMask.Text, 16);
                config.Timing0 = 0;// System.Convert.ToByte("0x" + textBox_Time0.Text, 16);
                config.Timing1 = 14;// System.Convert.ToByte("0x" + textBox_Time1.Text, 16);
                config.Filter = 1;// 单滤波 (Byte)comboBox_Filter.SelectedIndex;
                config.Mode = 0;//正常模式 (Byte)comboBox_Mode.SelectedIndex;
                VCI_InitCAN(m_devtype, m_devind, m_canind, ref config);
                VCI_InitCAN(m_devtype, m_devind, 1, ref config);
                //////////////////////////////////////////////////////////////////////////
                Int32 filterMode = 2;// comboBox_e_u_Filter.SelectedIndex;
                if (2 != filterMode)//不是禁用
                {
                    VCI_FILTER_RECORD filterRecord = new VCI_FILTER_RECORD();
                    filterRecord.ExtFrame = (UInt32)filterMode;
                    filterRecord.Start = 1;// System.Convert.ToUInt32("0x" + textBox_e_u_startid.Text, 16);
                    filterRecord.End = 0xff;// System.Convert.ToUInt32("0x" + textBox_e_u_endid.Text, 16);
                    //填充滤波表格

                    VCI_SetReference(m_devtype, m_devind, m_canind, 1, (byte*)&filterRecord);
                    VCI_SetReference(m_devtype, m_devind, 1, 1, (byte*)&filterRecord);
                    //使滤波表格生效
                    byte tm = 0;
                    if (VCI_SetReference(m_devtype, m_devind, m_canind, 2, &tm) != STATUS_OK)
                    {
                        MessageBox.Show("设置滤波失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        VCI_CloseDevice(m_devtype, m_devind);
                        return;
                    }
                    if (VCI_SetReference(m_devtype, m_devind, 1, 2, &tm) != STATUS_OK)
                    {
                        MessageBox.Show("设置滤波失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        VCI_CloseDevice(m_devtype, m_devind);
                        return;
                    }
                }
                //////////////////////////////////////////////////////////////////////////
            }
            button_Connect.Text = m_bOpen == 1 ? "断开" : "连接";
            button_Connect.BackColor = m_bOpen == 1 ? Color.Green : Color.Red;
        }

        /// <summary>
        /// 启动CAN的端口号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_StartCan_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 0)
                return;
            VCI_StartCAN(m_devtype, m_devind, 0);
            VCI_StartCAN(m_devtype, m_devind, 1);
            ThreadStart CANTreadChild = new ThreadStart(CallToCANReceiveThread);
            Thread m_CanReceiveChildThread = new Thread(CANTreadChild);
            m_CanReceiveChildThread.IsBackground = true;
            m_CanReceiveChildThread.Start();
        }

        /// <summary>
        /// 关闭CAN设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Reset_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 0)
                return;
            VCI_ResetCAN(m_devtype, m_devind, 0);
            VCI_ResetCAN(m_devtype, m_devind, 1);
        }

        //方向盘角度
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            textBox_Angle.Text = trackBar_Angle.Value.ToString();
        }
        private void trackBar_Angle_MouseUp(object sender, MouseEventArgs e)
        {
            SendData();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SendData();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            textBox_Angle.Text = "0";
            SendData();
        }
        //角速度
        private void trackBar_AngularVelocity_Scroll(object sender, EventArgs e)
        {
            textBox_AngularVelocity.Text = trackBar_AngularVelocity.Value.ToString();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            SendData();
        }
        private void trackBar_AngularVelocity_MouseUp(object sender, MouseEventArgs e)
        {
            SendData();
        }

        /// <summary>
        /// CAN接收测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            RecTestSendData();
            RecTestSpeedSendData();
        }

        #endregion

        #region Lin设备相关事件
        /// <summary>
        /// Lin设备扫描
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            //扫描查找设备
            m_UltrasonicObj.ScanningDevice();
            if (m_UltrasonicObj.DevNum <= 0)
            {
                MessageBox.Show("No device connected!", "错误",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                for(int i=0;i< m_UltrasonicObj.DevNum; i++)
                {
                    if (!comboBox1.Items.Contains(m_UltrasonicObj.DevHandles[i]))
                    {
                        comboBox1.Items.Add(m_UltrasonicObj.DevHandles[i]);
                    }
                }
                comboBox1.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Lin 设备连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            if (LinDeviceStatus)
            {
                m_UltrasonicObj.CloseUltrasonicDevice();
                LinDeviceStatus = false;
            }
            else
            {
                bool DevOpenStatus = m_UltrasonicObj.OpenUltrasonicDevice();
                if(DevOpenStatus)
                {
                    ThreadStart SamplingThread = new ThreadStart(CallToUltrasonicSamplingThread);
                    Thread UltrasonicSamplingThread = new Thread(SamplingThread);
                    UltrasonicSamplingThread.IsBackground = true;
                    UltrasonicSamplingThread.Start();
                }
                LinDeviceStatus = true;
            }
            button8.BackColor = LinDeviceStatus ? Color.Green : Color.Red;
            button8.Text = LinDeviceStatus ? "Lin连接断开" : "Lin设备连接";
        }

        /// <summary>
        /// 单次测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            m_UltrasonicObj.ScheduleOneTime(ref m_LIN_STP318_ReadData, ref m_LIN_STP313_ReadData);
            for (int k = 0; k < 4; k++) { m_UltrasonicObj.DataMapping2Control_STP313(m_LIN_STP313_ReadData[k], ref SensingControl_LRU[k]); }
            for (int k = 0; k < 8; k++) { m_UltrasonicObj.DataMapping2Control_STP318(m_LIN_STP318_ReadData[k], ref SensingControl_SRU_TextBox[k], ref SensingControl_SRU_Label[k]); }
        }
        #endregion

        #region 串口事件相关

        /// <summary>
        /// 串口端口号搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label92_Click(object sender, EventArgs e)
        {
            m_rtk.SearchAndAddSerialToComboBox(serialPort1, comboBox3);
        }
        /// <summary>
        /// 串口打开关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button16_Click(object sender, EventArgs e)
        {
            m_rtk.SerialPortName = comboBox3.Text;
            m_rtk.SerialBaudRate = 115200;
            m_rtk.SerialOperation(serialPort1, button16);
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (m_rtk.IsClosing)
                return;
            try
            {
                m_rtk.IsListning = true;
                int n = serialPort1.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致  
                byte[] data = new byte[n]; 
                serialPort1.Read(data, 0, n);//读取缓冲数据 

                //<协议解析>  
                bool data_catched = false;//缓存记录数据是否捕获到  
                                          //1.缓存数据  
                buffer.AddRange(data);
                //2.完整性判断，进行数据的校验
                while (buffer.Count >= 53)//至少要包含头（2字节）+命令（1字节）+ 长度（1字节）+ 数据（4字节）+ 校验（1字节）
                {
                    //2.1 查找数据头
                    if (buffer[0] == 0xAA && buffer[1] == 0x55)
                    {
                        //2.2 探测缓存数据是否有一条数据的字节，如果不够，就不用费劲的做其他验证了  
                        //前面已经限定了剩余长度>=4，那我们这里一定能访问到buffer[2]这个长度  
                        int Frame_ID = buffer[2];//数据帧号
 
                        if (Frame_ID != 1) break;

                        byte checksum = 0;
                        for (int i = 0; i < 53; i++)//len+3表示校验之前的位置  
                        {
                            checksum += buffer[i];
                        }
                        if (checksum != buffer[53]) //如果数据校验失败，丢弃这一包数据  
                        {
                            buffer.RemoveRange(0, 53);//从缓存中删除错误数据  
                            continue;//继续下一次循环  
                        }
                        //至此，已经被找到了一条完整数据。我们将数据直接分析，或是缓存起来一起分析  
                        //我们这里采用的办法是缓存一次，好处就是如果你某种原因，数据堆积在缓存buffer中  
                        //已经很多了，那你需要循环的找到最后一组，只分析最新数据，过往数据你已经处理不及时  
                        //了，就不要浪费更多时间了，这也是考虑到系统负载能够降低。  
                        buffer.CopyTo(3, binary_data, 0, 49);//复制一条完整数据到具体的数据缓存  
                        data_catched = true;
                        buffer.RemoveRange(0, 53);//正确分析一条数据，从缓存中移除数据。  
                    }
                    else
                    {
                        //这里是很重要的，如果数据开始不是头，则删除数据  
                        buffer.RemoveAt(0);
                    }
                }//while结束
                //分析数据 
                if (data_catched)
                {
                    //更新界面  
                    this.Invoke((EventHandler)(delegate
                    {
                        m_RTK_Imformation.Week = BitConverter.ToUInt16(binary_data,0);//0-1
                        m_RTK_Imformation.Second = BitConverter.ToUInt16(binary_data, 2);//2-5
                        m_RTK_Imformation.Yaw = BitConverter.ToSingle(binary_data, 6);//6-9
                        m_RTK_Imformation.Pitch = BitConverter.ToSingle(binary_data, 10);//10-13
                        m_RTK_Imformation.Roll = BitConverter.ToSingle(binary_data, 14);//14-17

                        m_RTK_Imformation.Latitude = BitConverter.ToUInt32(binary_data, 18);//18-21
                        m_RTK_Imformation.Longitude = BitConverter.ToUInt32(binary_data, 22);//22-25
                        m_RTK_Imformation.Height = BitConverter.ToUInt32(binary_data, 26);//26-29

                        m_RTK_Imformation.EastVelocity = BitConverter.ToSingle(binary_data, 30);//30-33
                        m_RTK_Imformation.WestVelocity = BitConverter.ToSingle(binary_data, 34);//34-37
                        m_RTK_Imformation.SkyVelocity = BitConverter.ToSingle(binary_data, 38);//38-41

                        m_RTK_Imformation.BaseLineLenght = BitConverter.ToSingle(binary_data, 42);//42-45

                        m_RTK_Imformation.AntennaNumber1 = binary_data[46];
                        m_RTK_Imformation.AntennaNumber2 = binary_data[47];
                        m_RTK_Imformation.Status = binary_data[48];

                        textBox36.Text = Math.Sqrt(Math.Pow(m_RTK_Imformation.EastVelocity, 2) + Math.Pow(m_RTK_Imformation.WestVelocity, 2)).ToString() + "m/s";

                        label96.Text = RTK_SystemStatus[m_RTK_Imformation.Status & 0x0f];
                        label97.Text = RTK_AntannaType[(m_RTK_Imformation.Status >> 4) & 0x0f];

                        textBox37.Text = m_RTK_Imformation.Yaw.ToString();
                    }));
                }
            }
            finally
            {
                m_rtk.IsListning = false;
            }
        }
        #endregion


        #region 数据保存相关事件

        /// <summary>
        /// 保存路径选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.InitialDirectory = "D:\\APA\\DataSet";
            saveFileDialog1.Title = "请选择要保存的文件路径";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.FileName = "Data.txt";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.AddExtension = true;
            DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr == DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                //获得文件路径
                localFilePath = saveFileDialog1.FileName.ToString();
                //获取文件路径，不带文件名
                FilePath = localFilePath.Substring(0, localFilePath.LastIndexOf("\\"));
                //获取文件名，不带路径
                fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);
            }
        }

        /// <summary>
        /// 开始保存数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            if (!DataSaveStatus)
            {
                //给文件名前加上时间
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + textBox34.Text + "_" + fileNameExt;
                DataSave = new StreamWriter(FilePath + "\\" + newFileName, true, Encoding.ASCII);
                m_VehicleImformation.Displacement = 0.0;
                m_VehicleImformation.X = 0.0;
                m_VehicleImformation.Y = 0.0;
                m_VehicleImformation.Yaw = 0.0;
                m_VehicleImformation.Last_Yaw = 0.0;
                //timer1_DataSave.Enabled = true;
                timeBeginPeriod(1);
                DataSaveStatus = true;
            }
            else
            {
                //timer1_DataSave.Enabled = false;
                timeEndPeriod(1);
                DataSave.Close();
                DataSaveStatus = false;
            }
            button12.Text = DataSaveStatus ? "取消保存" : "开始保存";
            button12.BackColor = DataSaveStatus ? Color.Green : Color.Red;
        }
        #endregion

        #region 图形化显示事件
        /// <summary>
        /// 波形图显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            if (!cf.Visible)
            {
                cf.Show();
            }
        }

        #endregion

        /// <summary>
        /// 定时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_DataSave_Tick(object sender, EventArgs e)
        {
            if (m_VehicleImformation.Displacement < 3.345)
            {
                m_VehicleImformation.R = 3.856;
                CanSendData(-540, 20);
            }
            else if(m_VehicleImformation.Displacement >= 3.345 && m_VehicleImformation.Displacement < 6.434)
            {
                m_VehicleImformation.R = 3.5609;
                CanSendData(540, 20);
            }
            else
            {
                CanSendData(540, 20);
                MessageBox.Show("请立即停车！！!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //停车
            }
            //FileSaveTime.SystemTime = timeGetTime();
            //if(FileSaveTime.LastSystemTime == 0)
            //{
            //    FileSaveTime.TimeErr = 0;
            //}
            //else
            //{
            //    FileSaveTime.TimeErr = FileSaveTime.SystemTime - FileSaveTime.LastSystemTime;
            //}
            //FileSaveTime.LastSystemTime = FileSaveTime.SystemTime;
            //DataSave.Write("{0:D} {1:D} {2:R16} {3:R16} {4:D} {5:D} {6:R16} {7:R16} {8:R16} {9:R16} {10:D} {11:D} \r\n", 
            //    FileSaveTime.SystemTime, FileSaveTime.TimeErr,
            //    m_VehicleImformation.Speed, m_VehicleImformation.Displacement,
            //    m_VehicleImformation.SteeringWheelAgularVelocity, m_VehicleImformation.ActualSteeringWheelAngle,
            //    m_LIN_STP313_ReadData[0].TOF1 / 58.0, m_LIN_STP313_ReadData[1].TOF1 / 58.0,
            //    m_LIN_STP313_ReadData[2].TOF1 / 58.0, m_LIN_STP313_ReadData[3].TOF1 / 58.0,
            //    UltrasonicSamplingTime.SystemTime, UltrasonicSamplingTime.TimeErr);
        }

        #endregion

        #region 线程相关
        #region CAN 接收线程相关
        private void CanReceiveCycleShow()
        {
            if (this.label85.InvokeRequired)
            {
                FlushClient scs = new FlushClient(CanReceiveCycleShow);
                this.Invoke(scs);
            }
            else
            {
                this.label85.Text = CanReceiveTime.TimeErr.ToString() + "ms";
            }
        }
        /// <summary>
        /// 显示车辆信息委托
        /// </summary>
        /// <param name="sdata"></param>
        private delegate void VehicleShow(VehicleImformation sdata);
        private void VehicleInforShow(VehicleImformation sdata)
        {
            VehicleShow m_vehicle = new VehicleShow(VehicleInformationShow);
            this.Invoke(m_vehicle, new object[] { sdata });
        }
        //CAN 接收线程函数
        public void CallToCANReceiveThread()
        {
            while (true)
            {
                try
                {
                    CanReceiveCycleShow();
                    CAN0_ReceiveFunction(ref m_VehicleImformation);
                    CAN1_ReceiveFunction(ref m_VehicleImformation);
                    VehicleInforShow(m_VehicleImformation);
                    Thread.Sleep(10);
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine("Thread Abort Exception {0}", e);
                }
                //finally
                //{
                //    Console.WriteLine("Couldn't catch the Thread Exception");
                //}
            }
        }
        #endregion

        #region 超声波传感器的采样线程
        private delegate int SamplingModDeleg(ComboBox cb); //代理
        private int GetSamplingModule(ComboBox cb)
        {
            if (cb.InvokeRequired)
            {
                SamplingModDeleg getIndex = new SamplingModDeleg(GetSamplingModule);
                IAsyncResult ia = cb.BeginInvoke(getIndex, new object[] { cb });
                return (int)cb.EndInvoke(ia);
            }
            else
            {
                return comboBox2.SelectedIndex;
            }
        }

        private delegate void FlushClient();
        private void SamplingCycleShow()
        {
            if (this.label79.InvokeRequired)
            {
                FlushClient scs = new FlushClient(SamplingCycleShow);
                this.Invoke(scs);
            }
            else
            {
                UltrasonicSamplingTime.SystemTime = timeGetTime();
                UltrasonicSamplingTime.TimeErr = UltrasonicSamplingTime.SystemTime - UltrasonicSamplingTime.LastSystemTime;
                UltrasonicSamplingTime.LastSystemTime = UltrasonicSamplingTime.SystemTime;
                this.label79.Text = UltrasonicSamplingTime.TimeErr.ToString() + "ms";
            }
        }
        private delegate void STP313_DataMapping(LIN_STP313_ReadData dat, ref TextBox[] tx);
        private void STP313_DataShow(LIN_STP313_ReadData show_dat, ref TextBox[] show_tx)
        {
            STP313_DataMapping m_STP313_DataMapping = new STP313_DataMapping(m_UltrasonicObj.DataMapping2Control_STP313);
            this.Invoke(m_STP313_DataMapping,new object[] { show_dat, show_tx });
        }

        private delegate void STP318_DataMapping(LIN_STP318_ReadData dat, ref TextBox tx, ref Label lb);
        private void STP318_DataShow(LIN_STP318_ReadData show_dat, ref TextBox tx, ref Label lb)
        {
            STP318_DataMapping m_STP318_DataMapping = new STP318_DataMapping(m_UltrasonicObj.DataMapping2Control_STP318);
            this.Invoke(m_STP318_DataMapping, new object[] { show_dat, tx, lb });
        }

        private delegate void LRU_STP_Sampling(ref LIN_STP313_ReadData[] LIN_STP313_data);
        private void LinSampling_STP_Sensings(ref LIN_STP313_ReadData[] sdata)
        {
            LRU_STP_Sampling m_sampling = new LRU_STP_Sampling(m_UltrasonicObj.LRU_ScheduleTime);
            this.Invoke(m_sampling, new object[] { sdata });
        }

        private delegate void LRU_STP_TimeSchedule(ref LIN_STP318_ReadData[] m_318Data, ref LIN_STP313_ReadData[] m_313Data, byte step);
        private void LinSampling_STP_TimeSchedule(ref LIN_STP318_ReadData[] m_318Data, ref LIN_STP313_ReadData[] m_313Data, byte step)
        {
            LRU_STP_TimeSchedule m_sampling = new LRU_STP_TimeSchedule(m_UltrasonicObj.TimeScheduleStatus1);
            this.Invoke(m_sampling, new object[] { m_318Data, m_313Data ,step});
        }

 

        private delegate void UI_Show(UInt16 v);
        private void UI_labelShow(UInt16 v)
        {
            UI_Show m_sampling = new UI_Show(cf.UpdateLabelValue);
            this.Invoke(m_sampling, new object[] { v });
        }

        private delegate void UI_ChartShow(LIN_STP318_ReadData[] UPA_data, LIN_STP313_ReadData[] APA_data);
        private void UI_ChartUltrasonicDataShow(LIN_STP318_ReadData[] UPA_data, LIN_STP313_ReadData[] APA_data)
        {
            UI_ChartShow m_sampling = new UI_ChartShow(cf.VehicleUltrasonicDataShow);
            this.Invoke(m_sampling, new object[] { UPA_data, APA_data });
        }
        /// <summary>
        /// 超声波传感器的采样线程
        /// </summary>
        public void CallToUltrasonicSamplingThread()
        {
            while (true)
            {
                try
                {
                    if(GetSamplingModule(comboBox2) == 0)
                    {
                        if(LinDeviceStatus)
                        {
                            SamplingCycleShow();
                            LinSampling_STP_Sensings(ref m_LIN_STP313_ReadData);
                        }
                        if (DataSaveStatus)
                        {
                            DataSave.Write("{0:D} {1:D} {2:R16} {3:R16} {4:D} {5:D} " +
                                            "{6:R16} {7:R16} {8:R16} {9:R16} " +
                                            "{10:R16} {11:R16} {12:R16} {13:R16} " +
                                            "{14:D} {15:D} {16:D} {17:D} " +
                                            "{18:R16} {19:R16} {20:R16} {21:R16}\r\n",
                            UltrasonicSamplingTime.SystemTime, UltrasonicSamplingTime.TimeErr,
                            m_VehicleImformation.Speed, m_VehicleImformation.Displacement,
                            m_VehicleImformation.SteeringWheelAgularVelocity, m_VehicleImformation.ActualSteeringWheelAngle,
                            m_LIN_STP313_ReadData[0].TOF1 / 58.0, m_LIN_STP313_ReadData[1].TOF1 / 58.0,
                            m_LIN_STP313_ReadData[2].TOF1 / 58.0, m_LIN_STP313_ReadData[3].TOF1 / 58.0,
                            m_LIN_STP313_ReadData[0].TOF2 / 58.0, m_LIN_STP313_ReadData[1].TOF2 / 58.0,
                            m_LIN_STP313_ReadData[2].TOF2 / 58.0, m_LIN_STP313_ReadData[3].TOF2 / 58.0,
                            m_LIN_STP313_ReadData[0].Width * 16, m_LIN_STP313_ReadData[1].Width * 16,
                            m_LIN_STP313_ReadData[2].Width * 16, m_LIN_STP313_ReadData[3].Width * 16,
                            m_LIN_STP313_ReadData[0].Level * 3.3 / 255, m_LIN_STP313_ReadData[1].Level * 3.3 / 255,
                            m_LIN_STP313_ReadData[2].Level * 3.3 / 255, m_LIN_STP313_ReadData[3].Level * 3.3 / 255
                            );
                        }
                        for (int k = 0; k < 4; k++) { STP313_DataShow(m_LIN_STP313_ReadData[k], ref SensingControl_LRU[k]); }
                        Thread.Sleep(30);
                    }
                    else if(GetSamplingModule(comboBox2) == 1)
                    {
                        SamplingCycleShow();
                        for(byte i=0;i<4;i++)
                        {
                            if (LinDeviceStatus)
                            {
                                LinSampling_STP_TimeSchedule(ref m_LIN_STP318_ReadData,ref m_LIN_STP313_ReadData,i);
                            }
                            Thread.Sleep(15);
                        }
                        if (DataSaveStatus)
                        {
                            DataSave.Write("{0:D} {1:D} {2:R16} {3:R16} {4:D} {5:D} " +
                                            "{6:R16} {7:R16} {8:R16} {9:R16} " +
                                            "{10:R16} {11:R16} {12:R16} {13:R16} " +
                                            "{14:R16} {15:R16} {16:R16} {17:R16} " +
                                            "{18:R16} {19:R16} {20:R16} {21:R16} " +
                                            "{22:D} {23:D} {24:D} {25:D} " +
                                            "{26:R16} {27:R16} {28:R16} {29:R16}" +
                                            "{30:R16} {31:R16} {32:R16}\r\n",
                            UltrasonicSamplingTime.SystemTime, UltrasonicSamplingTime.TimeErr,
                            m_VehicleImformation.Speed, m_VehicleImformation.Displacement,
                            m_VehicleImformation.SteeringWheelAgularVelocity, m_VehicleImformation.ActualSteeringWheelAngle,
                            m_LIN_STP318_ReadData[0].TOF / 58.0, m_LIN_STP318_ReadData[1].TOF / 58.0,
                            m_LIN_STP318_ReadData[2].TOF / 58.0, m_LIN_STP318_ReadData[3].TOF / 58.0,
                            m_LIN_STP318_ReadData[4].TOF / 58.0, m_LIN_STP318_ReadData[5].TOF / 58.0,
                            m_LIN_STP318_ReadData[6].TOF / 58.0, m_LIN_STP318_ReadData[7].TOF / 58.0,
                            m_LIN_STP313_ReadData[0].TOF1 / 58.0, m_LIN_STP313_ReadData[1].TOF1 / 58.0,
                            m_LIN_STP313_ReadData[2].TOF1 / 58.0, m_LIN_STP313_ReadData[3].TOF1 / 58.0,
                            m_LIN_STP313_ReadData[0].TOF2 / 58.0, m_LIN_STP313_ReadData[1].TOF2 / 58.0,
                            m_LIN_STP313_ReadData[2].TOF2 / 58.0, m_LIN_STP313_ReadData[3].TOF2 / 58.0,
                            m_LIN_STP313_ReadData[0].Width * 16, m_LIN_STP313_ReadData[1].Width * 16,
                            m_LIN_STP313_ReadData[2].Width * 16, m_LIN_STP313_ReadData[3].Width * 16,
                            m_LIN_STP313_ReadData[0].Level * 3.3 / 255, m_LIN_STP313_ReadData[1].Level * 3.3 / 255,
                            m_LIN_STP313_ReadData[2].Level * 3.3 / 255, m_LIN_STP313_ReadData[3].Level * 3.3 / 255,
                            m_VehicleImformation.X, m_VehicleImformation.Y, m_VehicleImformation.Yaw * 57.3
                            );
                        }
                        if (cf.Visible)
                        {
                            //UI_labelShow(m_LIN_STP318_ReadData[0].TOF);
                            UI_ChartUltrasonicDataShow(m_LIN_STP318_ReadData, m_LIN_STP313_ReadData);
                        }
                        for (int k = 0; k < 4; k++) { STP313_DataShow(m_LIN_STP313_ReadData[k], ref SensingControl_LRU[k]); }
                        for (int k = 0; k < 8; k++) { STP318_DataShow(m_LIN_STP318_ReadData[k], ref SensingControl_SRU_TextBox[k], ref SensingControl_SRU_Label[k]); }
                    }
                    else
                    {

                    }
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine("Thread Abort Exception {0}",e);
                }
            }
        }
        #endregion
        #endregion
    }
}
