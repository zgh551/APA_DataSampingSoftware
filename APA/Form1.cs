using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using USB2XXX;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
//unsafe public struct VCI_CAN_OBJ  //使用不安全代码
//{
//    public uint ID;
//    public uint TimeStamp;
//    public byte TimeFlag;
//    public byte SendType;
//    public byte RemoteFlag;//是否是远程帧
//    public byte ExternFlag;//是否是扩展帧
//    public byte DataLen;

//    public fixed byte Data[8];

//    public fixed byte Reserved[3];

//}
//2.定义CAN信息帧的数据类型。
public struct VCI_CAN_OBJ
{
    public UInt32 ID;
    public UInt32 TimeStamp;
    public byte TimeFlag;
    public byte SendType;
    public byte RemoteFlag;//是否是远程帧
    public byte ExternFlag;//是否是扩展帧
    public byte DataLen;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Data;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Reserved;

    public void Init()
    {
        Data = new byte[8];
        Reserved = new byte[3];
    }
}

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
/*** LIN Device Data Struct ***/
public struct LIN_STP318_ReadData
{
    public UInt16 TOF;
    public byte status;
}

public struct LIN_STP313_ReadData
{
    public UInt16 TOF1;
    public byte Level;
    public byte Width;
    public UInt16 TOF2;
    public byte status;
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
        static UInt32 m_devtype = 21;//USBCAN-2e-u
        //usb-e-u 波特率
        static UInt32[] GCanBrTab = new UInt32[10]{
                    0x060003, 0x060004, 0x060007,
                        0x1C0008, 0x1C0011, 0x160023,
                        0x1C002C, 0x1600B3, 0x1C00E0,
                        0x1C01C1
                };
        

        const UInt32 STATUS_OK = 1;

        UInt32 m_bOpen = 0;
        UInt32 m_devind = 0;
        UInt32 m_canind = 0;

        VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[50];

        UInt32[] m_arrdevtype = new UInt32[20];

        string[] DeviceType = new string[2]{ "USBCAN_2E_U", "USBCAN_E_U" };
        string[] BaudRate = new string[10] { "1000kbps","800kbps","500kbps","250kbps","125kbps","100kbps","50kbps","20kbps","10kbps","5kbps"};
        string[] ECU_Status = new string[8] { "待机模式","自动驾驶模式","未知", "未知", "手动模式", "手动介入恢复模式", "警告模式", "错误模式" };
        string[] ComunicationStatus = new string[2] { "通信正常", "通信异常" };
        #endregion

        #region LIN Device Configure relation varibale
        //Lin设备相关参数
        Int32[] DevHandles = new Int32[20];
        Int32 DevHandle = 0;
        Byte LINIndex = 0;
        Int32 DevNum;
        #endregion

        #region Sensing Relation Control Status Variable
        string[] SensingStatus = new string[5] { "Blockage","Noise Error","Hardware Fault","Communication Error","Proximity State"};

        TextBox[] SensingControl_9  = new TextBox[5];
        TextBox[] SensingControl_10 = new TextBox[5];
        TextBox[] SensingControl_11 = new TextBox[5];
        TextBox[] SensingControl_12 = new TextBox[5];

        public LIN_STP313_ReadData[] m_LIN_STP313_ReadData = new LIN_STP313_ReadData[4];
        #endregion

        #region Time Relation Function Interface and Variable
        [DllImport("winmm")]
        static extern uint timeGetTime();
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        UInt64 SystemTime, LastSystemTime,TimeErr;
        #endregion

        #region File Operation Relation Variable
        StreamWriter DataSave;
        string FilePath, localFilePath, newFileName, fileNameExt;
        bool DataSaveStatus = false;
        #endregion

        #region Vehicle Relation Variable
        double VehicleSpeed = 0, VehicleDistance = 0;
        Int16  VehicleAngle = 0;
        UInt16 VehicleAgularVelocity = 0;
        #endregion

        
        ChartShowForm cf = new ChartShowForm();
        public delegate void LRU_STP_Test(ref LIN_STP313_ReadData[] LIN_STP313_data);

        #region 函数方法
        /// <summary>
        /// CAN0 Receive Function
        /// </summary>
        unsafe public void CAN0_ReceiveFunction()
        {
            UInt32 res = new UInt32();
            res = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);
            if (res == 0)
                return;
            //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0],50, 100);

            /////////////////////////////////////
            //UInt32 con_maxlen = 50;
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)res);


            res = VCI_Receive(m_devtype, m_devind, m_canind, pt, res, 100);
            ////////////////////////////////////////////////////////

            String str = "";
            for (UInt32 i = 0; i < res; i++)
            {
                VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));

                str = "接收到数据: ";
                str += "  帧ID:0x" + System.Convert.ToString((Int32)obj.ID, 16);
                str += "  帧格式:";
                if (obj.RemoteFlag == 0)
                    str += "数据帧 ";
                else
                    str += "远程帧 ";
                if (obj.ExternFlag == 0)
                    str += "标准帧 ";
                else
                    str += "扩展帧 ";

                //////////////////////////////////////////
                if (obj.RemoteFlag == 0)
                {
                    str += "数据: ";
                    byte len = (byte)(obj.DataLen % 9);
                    byte j = 0;
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[0], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[1], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[2], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[3], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[4], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[5], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[6], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[7], 16);

                }
                //listBox1.Items.Add(str);
                //listBox1.SelectedIndex = listBox1.Items.Count - 1;
                if (obj.ID == 0x0C1)
                {
                    VehicleAngle = (Int16)((UInt16)(obj.Data[2] << 8 | obj.Data[3]) * 0.1 - 780);
                    VehicleAgularVelocity = (UInt16)(obj.Data[1] * 25);
                    label11.Text = ECU_Status[obj.Data[0]];//ECU状态
                    label12.Text = ComunicationStatus[obj.Data[5]];
                    textBox2.Text = Convert.ToString(VehicleAgularVelocity) + "°/s";//转角速度
                    textBox3.Text = Convert.ToString((obj.Data[4] - 128) * 0.07) + " Nm";//扭矩
                    
                    textBox4.Text = Convert.ToString(VehicleAngle) + "°";//目标角度
                    textBox5.Text = Convert.ToString((Int16)((UInt16)(obj.Data[6] << 8 | obj.Data[7]) * 0.1 - 780)) + "°";//实际角度
                }
            }
            Marshal.FreeHGlobal(pt);
        }

        /// <summary>
        /// CAN1 Receive Function
        /// </summary>
        unsafe void CAN1_ReceiveFunction()
        {
            UInt32 res = new UInt32();
            res = VCI_GetReceiveNum(m_devtype, m_devind, 1);
            if (res == 0)
                return;
            //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0],50, 100);

            /////////////////////////////////////
            //UInt32 con_maxlen = 50;
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)res);


            res = VCI_Receive(m_devtype, m_devind, 1, pt, res, 100);
            ////////////////////////////////////////////////////////

            String str = "";
            for (UInt32 i = 0; i < res; i++)
            {
                VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));

                str = "接收到数据: ";
                str += "  帧ID:0x" + System.Convert.ToString((Int32)obj.ID, 16);
                str += "  帧格式:";
                if (obj.RemoteFlag == 0)
                    str += "数据帧 ";
                else
                    str += "远程帧 ";
                if (obj.ExternFlag == 0)
                    str += "标准帧 ";
                else
                    str += "扩展帧 ";

                //////////////////////////////////////////
                if (obj.RemoteFlag == 0)
                {
                    str += "数据: ";
                    byte len = (byte)(obj.DataLen % 9);
                    byte j = 0;
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[0], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[1], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[2], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[3], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[4], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[5], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[6], 16);
                    if (j++ < len)
                        str += " " + System.Convert.ToString(obj.Data[7], 16);

                }
                //listBox1.Items.Add(str);
                //listBox1.SelectedIndex = listBox1.Items.Count - 1;
                if (obj.ID == 0x300)
                {
                    VehicleSpeed = (UInt16)(obj.Data[0] << 8 | obj.Data[1]) / 15.4583;
                    textBox33.Text = Convert.ToString(VehicleSpeed) + "km/h";//实际角度
                    //label11.Text = ECU_Status[obj.Data[0]];//ECU状态
                    //label12.Text = ComunicationStatus[obj.Data[5]];
                    //textBox2.Text = Convert.ToString(obj.Data[1] * 25) + "°/s";//转角速度
                    //textBox3.Text = Convert.ToString((obj.Data[4] - 128) * 0.07) + " Nm";//扭矩
                    //textBox4.Text = Convert.ToString((Int16)((UInt16)(obj.Data[2] << 8 | obj.Data[3]) * 0.1 - 780)) + "°";//目标角度
                    //textBox5.Text = Convert.ToString((Int16)((UInt16)(obj.Data[6] << 8 | obj.Data[7]) * 0.1 - 780)) + "°";//实际角度
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
            sendobj.Init();
            sendobj.SendType = 0;//0 -> 正常发送 ;2 -> 自发自收(byte)comboBox_SendType.SelectedIndex;
            sendobj.RemoteFlag = 0;//标准帧 (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = 0;// 标准帧数(byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = 0x215;// System.Convert.ToUInt32("0x" + textBox_ID.Text, 16);
            sendobj.DataLen = 8;

            byte[] b_temp = BitConverter.GetBytes((Convert.ToInt16(textBox_Angle.Text)+780)*10);

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
        /// 测试地盘CAN接收的函数
        /// </summary>
        unsafe void RecTestSendData()
        {
            if (m_bOpen == 0)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            sendobj.Init();
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
        /// 测试速度CAN接收的函数
        /// </summary>
        unsafe void RecTestSpeedSendData()
        {
            if (m_bOpen == 0)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            sendobj.Init();
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
        //end of  CAN function

        // following is the LIN Device function
        /*** STP318 ***/
        /// <summary>
        /// 设置STP318传感器的收发状态
        /// </summary>
        /// <param name="tx">指定传感器的发送状态</param>
        /// <param name="rx">指定传感器的接收状态</param>
        void InitSensing_STP318(int DevHandle,byte tx,byte rx)
        {
            int ret;
            USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[1];
            msg[0].Data = new Byte[9];

            msg[0].Data[0] = tx;
            msg[0].Data[1] = rx;
            msg[0].DataLen = 2;
            msg[0].ID = 0x80;
            ret = USB2LIN.LIN_Write(DevHandle, LINIndex, msg, 1);
            if (ret != USB2LIN.LIN_SUCCESS)
            {
                MessageBox.Show("LIN write data failed!\n", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                //Console.WriteLine("LIN write data success!\n");
            }
            //延时
            //System.Threading.Thread.Sleep(20);
        }

        /// <summary>
        /// 根据ID号读取相应传感器的数值
        /// </summary>
        /// <param name="id">输入所要读取的ID号</param>
        /// <returns></returns>
        LIN_STP318_ReadData ReadData_STP318(int DevHandle,byte id)
        {
            int ret;
            LIN_STP318_ReadData rd_msg = new LIN_STP318_ReadData();
            USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[1];
            msg[0].Data = new Byte[9];
            msg[0].DataLen = 3;
            msg[0].ID = id;
            
            IntPtr[] ptArray = new IntPtr[1];
            ptArray[0] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)) * msg.Length);
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)));
            Marshal.Copy(ptArray, 0, pt, 1);
            //将数组中的数据拷贝到指针所指区域
            for (int k = 0; k < msg.Length; k++)
            {
                Marshal.StructureToPtr(msg[k], (IntPtr)((UInt32)pt + k * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), true);
            }

            ret = USB2LIN.LIN_Read(DevHandle, LINIndex, pt, 1);
            if (ret < USB2LIN.LIN_SUCCESS)
            {
                Console.WriteLine("LIN read data failed!\n");
                return rd_msg;
            }
            else
            {
                msg[0] = (USB2LIN.LIN_MSG)Marshal.PtrToStructure((IntPtr)((UInt32)pt + 0 * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), typeof(USB2LIN.LIN_MSG));

                rd_msg.TOF = BitConverter.ToUInt16(msg[0].Data, 0);
                rd_msg.status = msg[0].Data[2];
                return rd_msg;
                //Console.Write("Master LIN Read: ID = 0x{0:X2} Data = ", msg[0].ID);
                //for (int i = 0; i < msg[0].DataLen; i++)
                //{
                //    Console.Write("0x{0:X2} ", msg[0].Data[i]);
                //}
                //Console.WriteLine("");
            }
        }

        /// <summary>
        /// 将STP318传感器的数据映射进指定控件中
        /// </summary>
        /// <param name="dat">input the parameter</param>
        /// <param name="tx"> ref textbox </param>
        /// <param name="lb"> ref label</param>
        /// 
        void DataMapping2Control_STP318(LIN_STP318_ReadData dat, ref TextBox tx, ref Label lb)
        {
            tx.Text = ((dat.TOF - 110) / 58.0).ToString();
            if (dat.status == 1)
            {
                lb.Text = SensingStatus[0];
            }
            else if (dat.status == 2)
            {
                lb.Text = SensingStatus[1];
            }
            else if (dat.status == 4)
            {
                lb.Text = SensingStatus[2];
            }
            else if (dat.status == 8)
            {
                lb.Text = SensingStatus[3];
            }
            else if (dat.status == 16)
            {
                lb.Text = SensingStatus[4];
            }
            else
            {
                lb.Text = "正常";
            }
        }
        /*** STP313 ***/
        /// <summary>
        /// STP313 Send and receive function
        /// </summary>
        /// 
        void InitSensing_STP313(int DevHandle, byte tx_rx)
        {
            int ret;
            USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[1];
            msg[0].Data = new Byte[9];

            msg[0].Data[0] = tx_rx;
            msg[0].DataLen = 1;
            msg[0].ID = 0xC1;
            ret = USB2LIN.LIN_Write(DevHandle, LINIndex, msg, 1);
            if (ret != USB2LIN.LIN_SUCCESS)
            {
                MessageBox.Show("LIN write data failed!\n", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                //Console.WriteLine("LIN write data success!\n");
            }
            //延时
            //System.Threading.Thread.Sleep(20);
        }

        /// <summary>
        /// STP313 Read data function
        /// </summary>
        /// <param name="id">the id od the input datat</param>
        /// <returns></returns>
        LIN_STP313_ReadData ReadData_STP313(int DevHandle,byte id)
        {
            int ret;
            LIN_STP313_ReadData rd_msg = new LIN_STP313_ReadData();
            USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[1];
            msg[0].Data = new Byte[9];
            msg[0].DataLen = 7;
            msg[0].ID = id;

            IntPtr[] ptArray = new IntPtr[1];
            ptArray[0] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)) * msg.Length);
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)));
            Marshal.Copy(ptArray, 0, pt, 1);
            //将数组中的数据拷贝到指针所指区域
            for (int k = 0; k < msg.Length; k++)
            {
                Marshal.StructureToPtr(msg[k], (IntPtr)((UInt32)pt + k * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), true);
            }

            ret = USB2LIN.LIN_Read(DevHandle, LINIndex, pt, 1);
            if (ret < USB2LIN.LIN_SUCCESS)
            {
                Console.WriteLine("LIN read data failed!\n");
                return rd_msg;
            }
            else
            {
                msg[0] = (USB2LIN.LIN_MSG)Marshal.PtrToStructure((IntPtr)((UInt32)pt + 0 * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), typeof(USB2LIN.LIN_MSG));

                rd_msg.TOF1 = BitConverter.ToUInt16(msg[0].Data, 0);
                rd_msg.Level = msg[0].Data[2];
                rd_msg.Width = msg[0].Data[3];
                rd_msg.TOF2 = BitConverter.ToUInt16(msg[0].Data, 4);
                rd_msg.status = msg[0].Data[6];
                return rd_msg;
            }
        }

        /// <summary>
        /// 将STP313传感器的数据映射到指定的控件中
        /// </summary>
        /// <param name="dat"></param>
        /// <param name="tx"></param>
        /// <param name="lb"></param>
        void DataMapping2Control_STP313(LIN_STP313_ReadData dat, ref TextBox[] tx)
        {
            tx[0].Text = ((dat.TOF1 - 110) / 58.0).ToString();
            tx[1].Text = ((dat.TOF2 - 110) / 58.0).ToString();
            tx[2].Text = (dat.Width * 16).ToString();
            tx[3].Text = (dat.Level * 3.3 / 255).ToString();

            if (dat.status == 1)
            {
                tx[4].Text = SensingStatus[0];
            }
            else if (dat.status == 2)
            {
                tx[4].Text = SensingStatus[1];
            }
            else if (dat.status == 4)
            {
                tx[4].Text = SensingStatus[2];
            }
            else if (dat.status == 8)
            {
                tx[4].Text = SensingStatus[3];
            }
            else if (dat.status == 16)
            {
                tx[4].Text = SensingStatus[4];
            }
            else
            {
                tx[4].Text = "正常";
            }
        }

        /// <summary>
        /// 传感器的调度时序
        /// </summary>
        /// <param name="LIN_STP313_data"> ref STP313 Struct data </param>
        public void LRU_ScheduleTime(ref LIN_STP313_ReadData[] LIN_STP313_data)
        {
            //IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)));
            //USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[1];
            //int ret = USB2LIN.LIN_Read(DevHandle, LINIndex, pt, 1);
            //int ret1 = USB2LIN.LIN_Read(DevHandle, LINIndex, pt, 1);
            //ret = USB2LIN.LIN_Write(DevHandle, LINIndex, msg, 1);
            for(int i=0;i< DevNum;i++)
            {
                //9号传感器
                LIN_STP313_data[2 * i + 0] = ReadData_STP313(DevHandles[i], 0x1f);           
                //10号传感器
                LIN_STP313_data[2 * i + 1] = ReadData_STP313(DevHandles[i], 0x5E);
            }

            for(int i=0; i< DevNum;i++)
            {
                InitSensing_STP313(DevHandles[i], 0x03);
            }

            //DataMapping2Control_STP313(LIN_STP313_data[0], ref SensingControl_9);
            //DataMapping2Control_STP313(LIN_STP313_data[1], ref SensingControl_10);
            //System.Threading.Thread.Sleep(20);
        }

        //void TimeScheduleStatus1()
        //{
        //    LIN_STP318_ReadData m_STP318_rev_data = new LIN_STP318_ReadData();
        //    LIN_STP313_ReadData m_STP313_rev_data = new LIN_STP313_ReadData();
        //    //Start
        //    InitSensing_STP318(0x02, 0x07);//2 send ,1,2,3 receive

        //    //9号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x1f);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_9);
        //    //10号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x5E);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_10);

        //    InitSensing_STP313(0x03);
        //    System.Threading.Thread.Sleep(30);
        //    //Step 2 -> 30ms
        //    m_STP318_rev_data = ReadData_STP318(0xcf);
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox1, ref label26);

        //    m_STP318_rev_data = ReadData_STP318(0x8E);
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox6, ref label27);

        //    m_STP318_rev_data = ReadData_STP318(0x0D);
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox7, ref label28);
        //    System.Threading.Thread.Sleep(20);
        //    //Step3 -> 50ms
        //    InitSensing_STP318(0x08, 0x08);//configure 4 send and 4 receive

        //    //9号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x1f);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_9);
        //    //10号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x5E);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_10);

        //    InitSensing_STP313(0x03);
        //    System.Threading.Thread.Sleep(30);
        //    // 80 ms
        //    m_STP318_rev_data = ReadData_STP318(0x4C);//read sensing 4 data
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox8, ref label29);
        //    // 100ms
        //    InitSensing_STP318(0x01, 0x01);//configure sensing one tx and rx
        //    //9号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x1f);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_9);
        //    //10号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x5E);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_10);
        //    InitSensing_STP313(0x03);
        //    System.Threading.Thread.Sleep(30);
        //    // 130ms
        //    m_STP318_rev_data = ReadData_STP318(0xcf);//read sensing one data
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox1, ref label26);
        //    // 150ms
        //    InitSensing_STP318(0x04, 0x0E);//configure sensing one tx and rx
        //    //9号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x1f);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_9);
        //    //10号传感器
        //    m_STP313_rev_data = ReadData_STP313(0x5E);
        //    DataMapping2Control_STP313(m_STP313_rev_data, ref SensingControl_10);
        //    InitSensing_STP313(0x03);
        //    System.Threading.Thread.Sleep(30);
        //    // 180ms
        //    m_STP318_rev_data = ReadData_STP318(0x8E);
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox6, ref label27);
        //    m_STP318_rev_data = ReadData_STP318(0x0D);
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox7, ref label28);
        //    m_STP318_rev_data = ReadData_STP318(0x4C);
        //    DataMapping2Control_STP318(m_STP318_rev_data, ref textBox8, ref label29);
        //}

        private delegate void FlushClient(); //代理
        private void SamplingCycleShow()
        {
            if(this.label79.InvokeRequired)
            {
                FlushClient scs = new FlushClient(SamplingCycleShow);
                this.Invoke(scs);
            }
            else
            {
                SystemTime = timeGetTime();
                TimeErr = SystemTime - LastSystemTime;
                LastSystemTime = SystemTime;

                this.label79.Text = TimeErr.ToString() + "ms";
            }
        }
        
        private void test2(ref LIN_STP313_ReadData[] ts)
        {
            LRU_STP_Test m_test = new LRU_STP_Test(LRU_ScheduleTime);
            this.Invoke(m_test , new object[] { ts });
        }
        //CAN 接收线程函数
        public void CallToCANReceiveThread()
        {
            while (true)
            {
                try
                {
                    test();
                    test2(ref m_LIN_STP313_ReadData);
                    //this.LRU_ScheduleTime(ref this.m_LIN_STP313_ReadData);
                    //this.Invoke(LRU_STP_Test, DELEGATEMESSAGE);
                    //test(ref m_LIN_STP313_ReadData);
                    Thread.Sleep(30);
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine("Thread Abort Exception");
                }
                //finally
                //{
                //    Console.WriteLine("Couldn't catch the Thread Exception");
                //}
            }
        }
        #endregion

        #region 控件事件
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UInt16 i;
            for(i=0;i<2;i++)
            {
                comboBox_DevType.Items.Add(DeviceType[i]);
            }
            m_arrdevtype[0] = VCI_USBCAN_2E_U;
            m_arrdevtype[1] = VCI_USBCAN_E_U;

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
            SensingControl_9 = new TextBox[5] { textBox13, textBox14, textBox15, textBox16, textBox17 };
            SensingControl_10 = new TextBox[5] { textBox18, textBox19, textBox20, textBox21, textBox22 };
            SensingControl_11 = new TextBox[5] { textBox23, textBox24, textBox25, textBox26, textBox27 };
            SensingControl_12 = new TextBox[5] { textBox28, textBox29, textBox30, textBox31, textBox32 };

        }

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

                    MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    VCI_CloseDevice(m_devtype, m_devind);
                    return;
                }
                if (VCI_SetReference(m_devtype, m_devind, 1, 0, (byte*)&baud) != STATUS_OK)
                {

                    MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            timer_rev.Enabled = m_bOpen == 1 ? true : false;
        }

        private void button_StartCan_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 0)
                return;
            VCI_StartCAN(m_devtype, m_devind, m_canind);
            VCI_StartCAN(m_devtype, m_devind, 1);
        }

        private void button_Reset_Click(object sender, EventArgs e)
        {
            if (m_bOpen == 0)
                return;
            VCI_ResetCAN(m_devtype, m_devind, m_canind);
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
        //Lin设备扫描
        private void button7_Click(object sender, EventArgs e)
        {
            
            //扫描查找设备
            DevNum = usb_device.USB_ScanDevice(DevHandles);
            if (DevNum <= 0)
            {
                MessageBox.Show("No device connected!", "错误",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                for(int i=0;i< DevNum;i++)
                {
                    if (!comboBox1.Items.Contains(DevHandles[i]))
                    {
                        comboBox1.Items.Add(DevHandles[i]);
                        
                    }
                }
                comboBox1.SelectedIndex = 0;
            }
        }
        //Lin 设备连接
        private void button8_Click(object sender, EventArgs e)
        {
            bool state;
            Int32 ret;
            for(int i=0;i < DevNum;i++)
            {
                usb_device.DEVICE_INFO DevInfo = new usb_device.DEVICE_INFO();
                //打开设备
                state = usb_device.USB_OpenDevice(DevHandles[i]);
                if (!state)
                {
                    MessageBox.Show("Open device error!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    button8.BackColor = Color.Green;
                }
                //获取固件信息
                StringBuilder FuncStr = new StringBuilder(256);
                state = usb_device.DEV_GetDeviceInfo(DevHandles[i], ref DevInfo, FuncStr);
                if (!state)
                {
                    MessageBox.Show("Get device infomation error!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    //Console.WriteLine("Firmware Info:");
                    //Console.WriteLine("    Name:" + Encoding.Default.GetString(DevInfo.FirmwareName));
                    //Console.WriteLine("    Build Date:" + Encoding.Default.GetString(DevInfo.BuildDate));
                    //Console.WriteLine("    Firmware Version:v{0}.{1}.{2}", (DevInfo.FirmwareVersion >> 24) & 0xFF, (DevInfo.FirmwareVersion >> 16) & 0xFF, DevInfo.FirmwareVersion & 0xFFFF);
                    //Console.WriteLine("    Hardware Version:v{0}.{1}.{2}", (DevInfo.HardwareVersion >> 24) & 0xFF, (DevInfo.HardwareVersion >> 16) & 0xFF, DevInfo.HardwareVersion & 0xFFFF);
                    //Console.WriteLine("    Functions:" + DevInfo.Functions.ToString("X8"));
                    //Console.WriteLine("    Functions String:" + FuncStr);
                }
                //初始化配置LIN
                USB2LIN.LIN_CONFIG LINConfig = new USB2LIN.LIN_CONFIG();
                LINConfig.BaudRate = 19200;
                LINConfig.BreakBits = USB2LIN.LIN_BREAK_BITS_10;
                LINConfig.CheckMode = USB2LIN.LIN_CHECK_MODE_EXT;
                LINConfig.MasterMode = USB2LIN.LIN_MASTER;
                ret = USB2LIN.LIN_Init(DevHandles[i], LINIndex, ref LINConfig);
                if (ret != USB2LIN.LIN_SUCCESS)
                {
                    MessageBox.Show("Config LIN failed!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    Console.WriteLine("Config LIN Success!");
                }
            }
        }

        /// <summary>
        /// 保存路径规划
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
            saveFileDialog1.FileName = "data.txt";
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
                ////给文件名前加上时间
                //newFileName = DateTime.Now.ToString("yyyyMMddHHmmss") +"_"+ textBox34.Text + "_" + fileNameExt;

                //DataSave = new StreamWriter(FilePath + "\\" + newFileName, true, Encoding.ASCII);
                //DataSave = new StreamWriter(saveFileDialog1.FileName, true, Encoding.ASCII);
            }
        }
        //CAN数据定时查询接收
        private void timer_rev_Tick(object sender, EventArgs e)
        {
            CAN0_ReceiveFunction();
            CAN1_ReceiveFunction();
        }

        //波形图显示
        private void button10_Click(object sender, EventArgs e)
        {
            if (!cf.Visible)
            {
                cf.Show();
            }
        }

        /// <summary>
        /// STP 313 传感器的更新周期测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            double distance;
            TimeErr = 0;
            timeBeginPeriod(1);
            InitSensing_STP313(DevHandles[0], 0x01);
            SystemTime = timeGetTime();
            while (TimeErr < 300)
            {
                LIN_STP313_ReadData test_s = ReadData_STP313(DevHandles[0], 0x1f);
                distance = (test_s.TOF1 - 110) / 58.0;
                listBox2.Items.Add(distance);

                TimeErr = timeGetTime() - SystemTime ;

            }
            timeEndPeriod(1);
        }


        private void button15_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }

        /// <summary>
        /// 保存CAN总线的相关数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            if (!DataSaveStatus)
            {
                VehicleDistance = 0.0;
                timer2.Enabled = true;
                timeBeginPeriod(1);
                DataSaveStatus = true;
            }
            else
            {
                timer2.Enabled = false;
                timeEndPeriod(1);
                DataSave.Close();
                DataSaveStatus = false;
            }
            button13.Text = DataSaveStatus ? "取消保存" : "开始保存";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            SystemTime = timeGetTime();
            if (LastSystemTime == 0)
            {
                TimeErr = 0;
            }
            else
            {
                TimeErr = SystemTime - LastSystemTime;
            }
            LastSystemTime = SystemTime;

            label79.Text = TimeErr.ToString() + "ms";
            VehicleDistance += VehicleSpeed * TimeErr / 3600.0;
            DataSave.Write("{0:D} {1:D} {2:R16} {3:R16} {4:D} {5:D}\r\n",
                SystemTime, TimeErr, VehicleDistance, VehicleSpeed,VehicleAngle, VehicleAgularVelocity);
        }
        #endregion
        //CAN接收测试
        private void button6_Click(object sender, EventArgs e)
        {
            RecTestSendData();
            RecTestSpeedSendData();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            LIN_STP318_ReadData m_STP318_test_data = new LIN_STP318_ReadData();
            LIN_STP313_ReadData m_STP313_test_data = new LIN_STP313_ReadData();

            InitSensing_STP318(DevHandles[0], 0x01, 0x01);
            System.Threading.Thread.Sleep(20);
            m_STP318_test_data = ReadData_STP318(DevHandles[0], 0xcf);
            DataMapping2Control_STP318(m_STP318_test_data, ref textBox1, ref label26);

            InitSensing_STP318(DevHandles[0], 0x02, 0x02);
            System.Threading.Thread.Sleep(20);
            m_STP318_test_data = ReadData_STP318(DevHandles[0], 0x8E);
            DataMapping2Control_STP318(m_STP318_test_data, ref textBox6, ref label27);

            InitSensing_STP318(DevHandles[0], 0x04, 0x04);
            System.Threading.Thread.Sleep(20);
            m_STP318_test_data = ReadData_STP318(DevHandles[0], 0x0D);
            DataMapping2Control_STP318(m_STP318_test_data, ref textBox7, ref label28);

            InitSensing_STP318(DevHandles[0], 0x08, 0x08);
            System.Threading.Thread.Sleep(20);
            m_STP318_test_data = ReadData_STP318(DevHandles[0], 0x4C);
            DataMapping2Control_STP318(m_STP318_test_data, ref textBox8, ref label29);

            InitSensing_STP313(DevHandles[0], 0x03);
            System.Threading.Thread.Sleep(40);
            m_STP313_test_data = ReadData_STP313(DevHandles[0], 0x1f);
            DataMapping2Control_STP313(m_STP313_test_data, ref SensingControl_9);
            m_STP313_test_data = ReadData_STP313(DevHandles[0], 0x5E);
            DataMapping2Control_STP313(m_STP313_test_data, ref SensingControl_10);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SystemTime = timeGetTime();
            if (LastSystemTime == 0)
            {
                TimeErr = 0;
            }
            else
            {
                TimeErr = SystemTime - LastSystemTime;
            }
            LastSystemTime = SystemTime;
            LRU_ScheduleTime(ref m_LIN_STP313_ReadData);

            label79.Text = TimeErr.ToString() + "ms";
            double SensingData9 = (m_LIN_STP313_ReadData[0].TOF1 - 110) / 58.0;
            double SensingData10 = (m_LIN_STP313_ReadData[1].TOF1 - 110) / 58.0;

            //double SensingData11 = (m_LIN_STP313_ReadData[2].TOF1 - 110) / 58.0;
            //double SensingData12 = (m_LIN_STP313_ReadData[3].TOF1 - 110) / 58.0;

            double SensingData9_Sencond = (m_LIN_STP313_ReadData[0].TOF2 - 110) / 58.0;
            double SensingData9_Level = m_LIN_STP313_ReadData[0].Level * 3.3 / 255;
            uint SensingData9_Width = (uint)(m_LIN_STP313_ReadData[0].Width * 16);

            double SensingData10_Sencond = (m_LIN_STP313_ReadData[1].TOF2 - 110) / 58.0;
            double SensingData10_Level = m_LIN_STP313_ReadData[1].Level * 3.3 / 255;
            uint SensingData10_Width = (uint)(m_LIN_STP313_ReadData[1].Width * 16);

            VehicleDistance += VehicleSpeed * TimeErr / 3600.0;
            DataSave.Write("{0:D} {1:D} {2:R16} {3:R16} {4:R16} {5:R16} {6:R16} {7:D} {8:R16} {9:R16} {10:D} {11:D} {12:R16}\r\n",
                SystemTime, TimeErr, SensingData9, SensingData10, VehicleDistance,
                SensingData9_Sencond, SensingData9_Level, SensingData9_Width,
                SensingData10_Sencond, SensingData10_Level, SensingData10_Width,
                VehicleAngle, VehicleSpeed);
            
        }

        //开始保存
        private void button12_Click(object sender, EventArgs e)
        {
            if(!DataSaveStatus)
            {
                //ThreadStart childref = new ThreadStart(CallToCANReceiveThread);
                //Thread childThread = new Thread(childref);
                //childThread.IsBackground = true;
                //childThread.Start();
                //给文件名前加上时间
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + textBox34.Text + "_" + fileNameExt;
                DataSave = new StreamWriter(FilePath + "\\" + newFileName, true, Encoding.ASCII);
                VehicleDistance = 0.0;
                timer1.Enabled = true;
                timeBeginPeriod(1);
                DataSaveStatus = true;
            }
            else
            {
                timer1.Enabled = false;
                timeEndPeriod(1);
                DataSave.Close();
                DataSaveStatus = false;

            }
            button12.Text = DataSaveStatus ? "取消保存" : "开始保存";
            button12.BackColor = DataSaveStatus ? Color.Green : Color.Red;

        }
    }
}
