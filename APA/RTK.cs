using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APA
{
    class RTK
    {
        public struct SerialImformationStruct
        {
            public string PortName;
            public int BaudRate;
            public int DataBits;
            public StopBits StopBits;
            public Parity Parity;
        }

        public struct RTK_Imformation
        {
            public UInt16 Week;
            public UInt32 Second;
            public float Yaw;
            public float Pitch;
            public float Roll;
            public UInt32 Latitude;
            public UInt32 Longitude;
            public UInt32 Height;
            public float EastVelocity;
            public float WestVelocity;
            public float SkyVelocity;
            public float BaseLineLenght;
            public UInt16 AntennaNumber1;
            public UInt16 AntennaNumber2;
            public UInt16 Status;
        }
        private bool closing;
        private bool listening;
        private bool data_catch;

        private SerialImformationStruct serialinform;

        public bool IsClosing
        {
            set
            {
                closing = value;
            }
            get
            {
                return closing;
            }
        }

        public bool IsListning
        {
            set
            {
                listening = value;
            }
            get
            {
                return listening;
            }
        }

        public bool DataCatch
        {
            set
            {
                data_catch = value;
            }
            get
            {
                return data_catch;
            }
        }

        public string SerialPortName
        {
            set
            {
                serialinform.PortName = value;
            }
            get
            {
                return serialinform.PortName;
            }
        }

        public int SerialBaudRate
        {
            set
            {
                serialinform.BaudRate = value;
            }
            get
            {
                return serialinform.BaudRate;
            }
        }

        public RTK()
        {
            closing = false;
            serialinform.PortName = "COM1";
            serialinform.BaudRate = 115200;
            serialinform.DataBits = 8;
            serialinform.StopBits = StopBits.One;
            serialinform.Parity = Parity.None;
        }
        //查询可用的串口号
        public void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {                                                               //将可用端口号添加到ComboBox                         
            string Buffer;                                              //缓存
            bool ComExist = false;
            MyBox.Items.Clear();                                        //清空ComboBox内容
            for (int i = 1; i < 20; i++)                                //循环
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行                    
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下俩列表
                    MyPort.Close();                                     //关闭
                    ComExist = true;
                }
                catch
                {

                }
            }
            if (ComExist)
            {
                MyBox.SelectedIndex = 0;
                return;
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    MyBox.Items.Add("COM" + (i + 1).ToString());
                }
            }
        }

        public void SerialOperation(SerialPort m_SerialPort,Button m_button)
        {
            try
            {
                if (m_SerialPort.IsOpen)//端口处于打开状态，我可以进行端口关闭操作
                {
                    closing = true;
                    while (listening) Application.DoEvents();
                    //打开时点击，则关闭串口
                    m_SerialPort.Close();
                    closing = false;
                    m_button.Text = "串口关闭";
                    m_button.BackColor = Color.Maroon;
                }
                else
                {
                    m_SerialPort.PortName = serialinform.PortName;
                    m_SerialPort.BaudRate = serialinform.BaudRate;
                    m_SerialPort.DataBits = serialinform.DataBits;
                    m_SerialPort.StopBits = serialinform.StopBits;
                    m_SerialPort.Parity   = serialinform.Parity;
                    try
                    {
                        m_SerialPort.Open();
                        m_button.Text = "串口开启";
                        m_button.BackColor = Color.LimeGreen;
                    }
                    catch (Exception ex)
                    {
                        //捕获到异常信息，创建一个新的comm对象，之前的不能用了。  
                        //serialPort1 = new SerialPort();
                        //现实异常信息给客户。  
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch
            {
                MessageBox.Show("端口错误，请检查端口选择是否正确", "错误提示");
            }
        }
    }
}
