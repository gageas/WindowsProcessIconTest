using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private const UInt32 WM_GETICON = 0x007f;
        private const UInt32 WM_SETICON = 0x0080;
        private IntPtr hIconReplace;
        private Thread th;

        public Form1()
        {
            InitializeComponent();

        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GETICON)
            {
                if ((uint)m.WParam == 1)
                {
                    var hIconOld = hIconReplace;

                    // 適当な画像を生成
                    Bitmap bmp = new Bitmap(32, 32);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var rand = new Random();
                        g.DrawRectangle(Pens.Black, new Rectangle(5, 5, 20, 20));
                        for (var i = 0; i < 10; i++)
                        {
                            g.DrawLine(Pens.Red, rand.Next(32), rand.Next(32), rand.Next(32), rand.Next(32));
                        }
                    }

                    // アイコン化
                    hIconReplace = (bmp).GetHicon();
                    m.Result = hIconReplace;

                    // 旧アイコンを解放(ここでするのは早いかもしんない)
                    DestroyIcon(hIconOld);

                    var now = System.DateTime.Now;
                    this.Invoke((MethodInvoker)(
                        ()=>this.textBox1.AppendText("WM_GETICON(wParam==1)きた " + now.ToLocalTime()+"."+now.Millisecond.ToString("000") + "\n")
                        ));
                    return;
                }
            }
            base.WndProc(ref m);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (th != null) return;
            th = new Thread(() =>
            {
                var rand = new Random();
                while (true)
                {
                    try
                    {
                        this.Invoke((MethodInvoker)(() =>
                        {
                            var res = SendMessage(this.Handle, WM_SETICON, (IntPtr)0, (IntPtr)(rand.Next()));
                        }));
                    }
                    catch { }
                    Thread.Sleep(1);
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetIcon((IntPtr)(new Random()).Next());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (th != null)
            {
                th.Abort();
                th = null;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Icon = Icon.FromHandle(hIconReplace);
        }

        private void SetIcon(IntPtr hIcon)
        {
            var res = SendMessage(this.Handle, WM_SETICON, (IntPtr)0, hIcon);
            var err = GetLastError();
//            MessageBox.Show(res.ToString() + err.ToString());
        }

        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern UInt32 GetLastError();
    }
}
