using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Example3
{
    public partial class Form1 : Form
    {
        string Conn = "Server=localhost;Database=example3;Uid=root;Pwd=0000;";

        public Form1(){
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e){
            if(textBox1.Text != "" && serialPort1.IsOpen == false)
            {
                serialPort1.PortName = textBox1.Text;
                serialPort1.Open();
                MessageBox.Show("연결 성공");
            }

        }

        private void button2_Click(object sender, EventArgs e){
            if ( serialPort1.IsOpen == true)
            {
                serialPort1.Close();
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
{
    string tag_id = serialPort1.ReadLine().Replace(((char)0x0D).ToString(), "");

    // UI 스레드에서 작업하도록 처리
    if (textBox2.InvokeRequired)
    {
        textBox2.Invoke(new MethodInvoker(delegate {
            textBox2.Text = tag_id;
        }));
    }
    else
    {
        textBox2.Text = tag_id;
    }

    using (MySqlConnection conn = new MySqlConnection(Conn))
    {
        try
        {
            DataSet ds = new DataSet();
            string sql = "SELECT * FROM work_info WHERE id=@id";
            MySqlDataAdapter adpt = new MySqlDataAdapter(sql, conn);
            adpt.SelectCommand.Parameters.AddWithValue("@id", tag_id);
            adpt.Fill(ds, "user");

            if (ds.Tables[0].Rows.Count == 1)
            {
                // UI 업데이트: name, height, weight, phoneNum
                if (textBox3.InvokeRequired)
                {
                    textBox3.Invoke(new MethodInvoker(delegate {
                        textBox3.Text = ds.Tables[0].Rows[0]["name"].ToString();
                        textBox4.Text = ds.Tables[0].Rows[0]["height"].ToString();
                        textBox5.Text = ds.Tables[0].Rows[0]["weight"].ToString();
                        textBox6.Text = ds.Tables[0].Rows[0]["phoneNum"].ToString();
                    }));
                }
                else
                {
                    textBox3.Text = ds.Tables[0].Rows[0]["name"].ToString();
                    textBox4.Text = ds.Tables[0].Rows[0]["height"].ToString();
                    textBox5.Text = ds.Tables[0].Rows[0]["weight"].ToString();
                    textBox6.Text = ds.Tables[0].Rows[0]["phoneNum"].ToString();
                }

                // 이미지 경로 처리
                string img_path = ds.Tables[0].Rows[0]["image"].ToString();
                if (textBox7.InvokeRequired)
                {
                    textBox7.Invoke(new MethodInvoker(delegate {
                        textBox7.Text = img_path;
                    }));
                }
                else
                {
                    textBox7.Text = img_path;
                }

                // 이미지 로드
                string fullPath = Path.Combine(Application.StartupPath, "C:\\Users\\미누\\Desktop\\img", img_path);
                        if (pictureBox1.InvokeRequired)
                {
                    pictureBox1.Invoke(new MethodInvoker(delegate {
                        if (File.Exists(fullPath))
                        {
                            pictureBox1.Image = new Bitmap(fullPath);
                        }
                        else
                        {
                            MessageBox.Show("이미지를 찾을 수 없습니다.");
                        }
                    }));
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        pictureBox1.Image = new Bitmap(fullPath);
                    }
                    else
                    {
                        MessageBox.Show("이미지를 찾을 수 없습니다.");
                    }
                }
            }
            else
            {
                // 등록되지 않은 회원
                if (textBox3.InvokeRequired)
                {
                    textBox3.Invoke(new MethodInvoker(delegate {
                        textBox3.Text = "";
                        textBox4.Text = "";
                        textBox5.Text = "";
                        textBox6.Text = "";
                        textBox7.Text = "";
                        pictureBox1.Image = null;
                    }));
                }
                else
                {
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    pictureBox1.Image = null;
                }

                MessageBox.Show("등록되지 않은 회원입니다. 회원 정보를 저장 해주세요!");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("오류 발생: " + ex.Message);
        }
    }
}

        private void button3_Click(object sender, EventArgs e){
            if (textBox2.Text != "" &&
                textBox3.Text != "" && 
                textBox4.Text != "" &&  
                textBox5.Text != "" &&
                textBox6.Text != "" &&
                textBox7.Text != ""
                ){
                using (MySqlConnection conn = new MySqlConnection(Conn)){
                    conn.Open();
                    string tag_id = textBox2.Text.Replace(((char)0x0D).ToString(), "");

                    string query = "INSERT INTO work_info (id, name, height, weight, phoneNum, image) " +
                           "VALUES (@id, @name, @height, @weight, @phoneNum, @image)";
                    MySqlCommand msc = new MySqlCommand(query, conn);

                    msc.Parameters.AddWithValue("@id", tag_id);
                    msc.Parameters.AddWithValue("@name", textBox3.Text);
                    msc.Parameters.AddWithValue("@height", textBox4.Text);
                    msc.Parameters.AddWithValue("@weight", textBox5.Text);
                    msc.Parameters.AddWithValue("@phoneNum", textBox6.Text);
                    msc.Parameters.AddWithValue("@image", textBox7.Text);

                    msc.ExecuteNonQuery();
                }

                SendDataToArduino(textBox4.Text, textBox5.Text);

                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                pictureBox1.Image = null;

                MessageBox.Show("회원 정보가 저장되었습니다!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK){
                FileInfo fi = new FileInfo(openFileDialog1.FileName);
                textBox7.Text = fi.Name;
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
            }
        }

        private void SendDataToArduino(string height, string weight)
        {
            if (serialPort1.IsOpen)
            {              
                string dataToSend = height + "," + weight + "\n";
                serialPort1.Write(dataToSend);
                Console.Write(dataToSend);
            }
        }
    }
}