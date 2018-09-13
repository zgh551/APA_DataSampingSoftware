using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using USB2XXX;

namespace APA
{
    public class Ultrasonic
    {
        /*** LIN Device Data Struct ***/
        public struct LIN_STP318_ReadData
        {
            public UInt16 TOF;
            public byte status;
        }

        public struct Axis
        {
            public double x;
            public double y;
            public double yaw;
        }

        public struct LIN_STP313_ReadData
        {
            public UInt16 TOF1;
            public byte Level;
            public byte Width;
            public UInt16 TOF2;
            public byte status;
        }

        #region LIN Device Configure relation varibale
        //Lin设备相关参数
        public Int32 DevNum;

        public Int32[] DevHandles = new Int32[20];
        private Byte LINIndex = 0;

        private string[] SensingStatus = new string[5] { "Blockage", "Noise Error", "Hardware Fault", "Communication Error", "Proximity State" };

        public byte[] LRU_SensingRead_ID = new byte[2] { 0x1f, 0x5E };
        public byte[] SRU_SensingRead_ID = new byte[4] { 0xCf, 0x8E, 0x0D, 0x4C };

        //public byte[,] STP318SensingReadNum = new byte[2, 4] {
        //                                                        { 3,1,1,3 },
        //                                                        { 1,3,3,1 }
        //                                                    };
        public byte[,] STP318SensingReadNum = new byte[2, 4] {
                                                                { 1,1,1,1 },
                                                                { 1,1,1,1 }
                                                            };
        public byte[,,] STP318SensingReadStatus = new byte[4, 4, 2] {
                                                                        { {0,0},{1,0},{2,0},{7,1} },
                                                                        { {3,0},{4,1},{5,1},{6,1} },
                                                                        { {0,0},{5,1},{6,1},{7,1} },
                                                                        { {1,0},{2,0},{3,0},{4,1} }
                                                                    };
        public byte[][][][] STP318SensingRead = new byte[4][][][];
        //public byte[,,] SensingSendStatus = new byte[4, 2, 2]{
        //                                                    { { 0x02, 0x07 },{ 0x08, 0x08 } },//第一次[2->tx ;123->rx][8->tx;8->rx]
        //                                                    { { 0x08, 0x08 },{ 0x02, 0x07 } },//第二次[4->tx ;4->rx][6->tx;567->rx]
        //                                                    { { 0x01, 0x01 },{ 0x04, 0x0E } },//第三次[1->tx ;1->rx][7->tx;678->rx]
        //                                                    { { 0x04, 0x0E },{ 0x01, 0x01 } } //第四次[3->tx ;234->rx][5->tx;5->rx]
        //                                                    };
        public byte[,,] SensingSendStatus = new byte[4, 2, 2]{
                                                            { { 0x02, 0x02 },{ 0x08, 0x08 } },//第一次[2->tx ;2->rx][8->tx;8->rx]
                                                            { { 0x08, 0x08 },{ 0x02, 0x02 } },//第二次[4->tx ;4->rx][6->tx;6->rx]
                                                            { { 0x01, 0x01 },{ 0x04, 0x04 } },//第三次[1->tx ;1->rx][7->tx;7->rx]
                                                            { { 0x04, 0x04 },{ 0x01, 0x01 } } //第四次[3->tx ;3->rx][5->tx;5->rx]
                                                            };
        #endregion

        public Ultrasonic()
        {
            LINIndex = 0;

            ////第一次
            //STP318SensingRead[0] = new byte[2][][];
            //STP318SensingRead[0][0] = new byte[2][];//device 0
            //STP318SensingRead[0][0][0] = new byte[3] { 0, 1, 2 };//sensing array
            //STP318SensingRead[0][0][1] = new byte[3] { 0, 1, 2 };//ID
            //STP318SensingRead[0][1] = new byte[2][];//device 1
            //STP318SensingRead[0][1][0] = new byte[1] { 7 };
            //STP318SensingRead[0][1][1] = new byte[1] { 3 };
            ////第二次
            //STP318SensingRead[1] = new byte[2][][];
            //STP318SensingRead[1][0] = new byte[2][];//device 0
            //STP318SensingRead[1][0][0] = new byte[1] { 3 };//sensing array
            //STP318SensingRead[1][0][1] = new byte[1] { 3 };//ID
            //STP318SensingRead[1][1] = new byte[2][];//device 1
            //STP318SensingRead[1][1][0] = new byte[3] { 4, 5, 6 };//sensing array
            //STP318SensingRead[1][1][1] = new byte[3] { 0, 1, 2 };//ID
            ////第三次
            //STP318SensingRead[2] = new byte[2][][];
            //STP318SensingRead[2][0] = new byte[2][];//device 0
            //STP318SensingRead[2][0][0] = new byte[1] { 0 };//sensing array
            //STP318SensingRead[2][0][1] = new byte[1] { 0 };//ID
            //STP318SensingRead[2][1] = new byte[2][];//device 1
            //STP318SensingRead[2][1][0] = new byte[3] { 5, 6, 7 };//sensing array
            //STP318SensingRead[2][1][1] = new byte[3] { 1, 2, 3 };//ID
            ////第四次
            //STP318SensingRead[3] = new byte[2][][];
            //STP318SensingRead[3][0] = new byte[2][];//device 0
            //STP318SensingRead[3][0][0] = new byte[3] { 1, 2, 3 };//sensing array
            //STP318SensingRead[3][0][1] = new byte[3] { 1, 2, 3 };//ID
            //STP318SensingRead[3][1] = new byte[2][];//device 1
            //STP318SensingRead[3][1][0] = new byte[1] { 4 };//sensing array
            //STP318SensingRead[3][1][1] = new byte[1] { 0 };//ID
            //第一次
            STP318SensingRead[0] = new byte[2][][];
            STP318SensingRead[0][0] = new byte[2][];//device 0
            STP318SensingRead[0][0][0] = new byte[1] { 1 };//sensing array
            STP318SensingRead[0][0][1] = new byte[1] { 1 };//ID
            STP318SensingRead[0][1] = new byte[2][];//device 1
            STP318SensingRead[0][1][0] = new byte[1] { 7 };
            STP318SensingRead[0][1][1] = new byte[1] { 3 };
            //第二次
            STP318SensingRead[1] = new byte[2][][];
            STP318SensingRead[1][0] = new byte[2][];//device 0
            STP318SensingRead[1][0][0] = new byte[1] { 3 };//sensing array
            STP318SensingRead[1][0][1] = new byte[1] { 3 };//ID
            STP318SensingRead[1][1] = new byte[2][];//device 1
            STP318SensingRead[1][1][0] = new byte[1] { 5 };//sensing array
            STP318SensingRead[1][1][1] = new byte[1] { 1 };//ID
            //第三次
            STP318SensingRead[2] = new byte[2][][];
            STP318SensingRead[2][0] = new byte[2][];//device 0
            STP318SensingRead[2][0][0] = new byte[1] { 0 };//sensing array
            STP318SensingRead[2][0][1] = new byte[1] { 0 };//ID
            STP318SensingRead[2][1] = new byte[2][];//device 1
            STP318SensingRead[2][1][0] = new byte[1] { 6 };//sensing array
            STP318SensingRead[2][1][1] = new byte[1] { 2 };//ID
            //第四次
            STP318SensingRead[3] = new byte[2][][];
            STP318SensingRead[3][0] = new byte[2][];//device 0
            STP318SensingRead[3][0][0] = new byte[1] { 2 };//sensing array
            STP318SensingRead[3][0][1] = new byte[1] { 2 };//ID
            STP318SensingRead[3][1] = new byte[2][];//device 1
            STP318SensingRead[3][1][0] = new byte[1] { 4 };//sensing array
            STP318SensingRead[3][1][1] = new byte[1] { 0 };//ID
        }

        public void ScanningDevice()
        {
            //扫描查找设备
            DevNum = usb_device.USB_ScanDevice(DevHandles);
        }

        /// <summary>
        /// 打开超声波设备
        /// </summary>
        /// <param name="DevHandle"></param>
        public bool OpenUltrasonicDevice()
        {
            bool state;
            Int32 ret;
            for (int i = 0; i < DevNum; i++)
            {
                //打开设备
                state = usb_device.USB_OpenDevice(DevHandles[i]);
                if (!state)
                {
                    MessageBox.Show("Open device error!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                //初始化配置LIN
                USB2LIN.LIN_CONFIG LINConfig = new USB2LIN.LIN_CONFIG();
                LINConfig.BaudRate = 19200;
                LINConfig.BreakBits = USB2LIN.LIN_BREAK_BITS_10;
                LINConfig.CheckMode = USB2LIN.LIN_CHECK_MODE_EXT;
                LINConfig.MasterMode = USB2LIN.LIN_MASTER;
                ret = USB2LIN.LIN_Init(DevHandles[i], 0, ref LINConfig);
                if (ret != USB2LIN.LIN_SUCCESS)
                {
                    MessageBox.Show("Config LIN failed!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                else
                {
                    Console.WriteLine("Config LIN Success!");
                }
            }
            return true;
        }

        public void CloseUltrasonicDevice()
        {
            bool state;
            for (int i = 0; i < DevNum; i++)
            {
                //关闭设备
                state = usb_device.USB_CloseDevice(DevHandles[i]);
                if (!state)
                {
                    MessageBox.Show("Close Device Error!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
        }
        /*** STP318 ***/
        /// <summary>
        /// 设置STP318传感器的收发状态
        /// </summary>
        /// <param name="tx">指定传感器的发送状态</param>
        /// <param name="rx">指定传感器的接收状态</param>
        public void InitSensing_STP318(int DevHandle, byte tx, byte rx)
        {
            int ret;
            USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[2];
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
        public LIN_STP318_ReadData ReadData_STP318(int DevHandle, byte id)
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
            }
        }
        /// <summary>
        /// 根据ID号读取相应传感器的数值，一次读取一组数值
        /// </summary>
        /// <param name="id">输入所要读取的ID号</param>
        /// <returns></returns>
        public void STP318_ReadDatas(int DevHandle, byte FrameLenght, byte[] id, ref LIN_STP318_ReadData[] m_stp318datas)
        {
            int ret;

            USB2LIN.LIN_MSG[] msg = new USB2LIN.LIN_MSG[FrameLenght];
            for (int i = 0; i < FrameLenght; i++)
            {
                msg[0].Data = new Byte[9];
                msg[0].DataLen = 3;
                msg[0].ID = id[i];
            }

            IntPtr[] ptArray = new IntPtr[FrameLenght];
            for (int i = 0; i < FrameLenght; i++)
            {
                ptArray[i] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)) * msg.Length);
            }

            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN.LIN_MSG)) * msg.Length);
            Marshal.Copy(ptArray, 0, pt, FrameLenght);
            ////将数组中的数据拷贝到指针所指区域
            for (int k = 0; k < msg.Length; k++)
            {
                Marshal.StructureToPtr(msg[k], (IntPtr)((UInt32)pt + k * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), true);
            }

            ret = USB2LIN.LIN_Read(DevHandle, LINIndex, pt, FrameLenght);
            if (ret < USB2LIN.LIN_SUCCESS)
            {
                Console.WriteLine("LIN read data failed!\n");
                //return rd_msg;
            }
            else
            {
                //msg[0] = (USB2LIN.LIN_MSG)Marshal.PtrToStructure((IntPtr)((UInt32)pt + 0 * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), typeof(USB2LIN.LIN_MSG));
                //msg[1] = (USB2LIN.LIN_MSG)Marshal.PtrToStructure((IntPtr)((UInt32)pt + 1 * Marshal.SizeOf(typeof(USB2LIN.LIN_MSG))), typeof(USB2LIN.LIN_MSG));
                //rd_msg.TOF = BitConverter.ToUInt16(msg[0].Data, 0);
                //rd_msg.status = msg[0].Data[2];
                //return rd_msg;
            }
        }
        /// <summary>
        /// 将STP318传感器的数据映射进指定控件中
        /// </summary>
        /// <param name="dat">input the parameter</param>
        /// <param name="tx"> ref textbox </param>
        /// <param name="lb"> ref label</param>
        /// 
        public void DataMapping2Control_STP318(LIN_STP318_ReadData dat, ref TextBox tx, ref Label lb)
        {
            tx.Text = (dat.TOF / 58.0).ToString();
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
        public void InitSensing_STP313(int DevHandle, byte tx_rx)
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
                Console.WriteLine("LIN write data success!\n");
            }
        }

        /// <summary>
        /// STP313 Read data function
        /// </summary>
        /// <param name="id">the id od the input datat</param>
        /// <returns></returns>
        public LIN_STP313_ReadData ReadData_STP313(int DevHandle, byte id)
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
        public void DataMapping2Control_STP313(LIN_STP313_ReadData dat, ref TextBox[] tx)
        {
            tx[0].Text = ((dat.TOF1) / 58.0).ToString();
            tx[1].Text = ((dat.TOF2) / 58.0).ToString();
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
        /// 遍历传感器数据一次
        /// </summary>
        /// <param name="m_318Data"></param>
        /// <param name="m_313Data"></param>
        public void ScheduleOneTime(ref LIN_STP318_ReadData[] m_318Data, ref LIN_STP313_ReadData[] m_313Data)
        {

            for (int i = 0; i < DevNum; i++)
            {
                InitSensing_STP318(DevHandles[i], 0x01, 0x01);
                System.Threading.Thread.Sleep(30);
                m_318Data[4 * i] = ReadData_STP318(DevHandles[0], 0xCF);

                InitSensing_STP318(DevHandles[i], 0x02, 0x02);
                System.Threading.Thread.Sleep(30);
                m_318Data[4 * i + 1] = ReadData_STP318(DevHandles[0], 0x8E);

                InitSensing_STP318(DevHandles[i], 0x04, 0x04);
                System.Threading.Thread.Sleep(30);
                m_318Data[4 * i + 2] = ReadData_STP318(DevHandles[0], 0x0D);

                InitSensing_STP318(DevHandles[i], 0x08, 0x08);
                System.Threading.Thread.Sleep(30);
                m_318Data[4 * i + 3] = ReadData_STP318(DevHandles[0], 0x4C);

                InitSensing_STP313(DevHandles[i], 0x03);
                System.Threading.Thread.Sleep(50);
                m_313Data[2 * i]     = ReadData_STP313(DevHandles[i], 0x1f);
                m_313Data[2 * i + 1] = ReadData_STP313(DevHandles[i], 0x5E);
            }
        }
        /// <summary>
        /// 传感器的调度时序
        /// </summary>
        /// <param name="LIN_STP313_data"> ref STP313 Struct data </param>
        public void LRU_ScheduleTime(ref LIN_STP313_ReadData[] LIN_STP313_data)
        {
            for (int i = 0; i < DevNum; i++)
            {
                //9号传感器
                LIN_STP313_data[2 * i + 0] = ReadData_STP313(DevHandles[i], 0x1f);
                //10号传感器
                LIN_STP313_data[2 * i + 1] = ReadData_STP313(DevHandles[i], 0x5E);
                InitSensing_STP313(DevHandles[i], 0x03);
            }
        }

        /// <summary>
        /// 12组传感器调度轮寻模式
        /// </summary>
        /// <param name="m_318Data"></param>
        /// <param name="m_313Data"></param>
        /// <param name="step"></param>
        public void TimeScheduleStatus1(ref LIN_STP318_ReadData[] m_318Data, ref LIN_STP313_ReadData[] m_313Data, byte step)
        {
            for (int i = 0; i < DevNum; i++)
            {
                InitSensing_STP318(DevHandles[i], SensingSendStatus[step, i, 0], SensingSendStatus[step, i, 1]);
                for (int m = 0; m < 2; m++)
                {
                    m_313Data[2 * i + m] = ReadData_STP313(DevHandles[i], LRU_SensingRead_ID[m]);
                }
                InitSensing_STP313(DevHandles[i], 0x03);
                //System.Threading.Thread.Sleep(60);
                for (int k = 0; k < STP318SensingReadNum[i, step]; k++)
                {
                    m_318Data[STP318SensingRead[step][i][0][k]] = ReadData_STP318(DevHandles[i], SRU_SensingRead_ID[STP318SensingRead[step][i][1][k]]);
                }
            }
        }
    }
}
