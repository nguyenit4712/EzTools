using MihaZupan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Android_Tools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<bool> isTimer = new List<bool>();
        private bool isCheck = true;
        private const string URL = @".\Data\URL.txt";
        private const string ListDefault = @".\Data\Default.txt";
        private const string ListWipe = @".\Data\Wipe.txt";
        private const string PathPackage = @".\Data\Temp\TempPackage.txt";
        private const string path_command = @".\Data\Temp\command.txt";
        private const int SW_RESTORE = 9;
        [DllImport("User32")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static IntPtr BringToFront(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
            return hWnd;
        }
        public static List<string> LoadDevices()
        {
            List<string> list = new List<string>();
            string input = ExecuteCMD("adb devices");
            string pattern = "(?<=List of devices attached)([^\\n]*\\n+)+";
            MatchCollection matchCollection = Regex.Matches(input, pattern, RegexOptions.Singleline);
            if (matchCollection.Count > 0)
            {
                string value = matchCollection[0].Groups[0].Value;
                string[] array = Regex.Split(value, "\r\n");
                string[] array2 = array;
                foreach (string text in array2)
                {
                    if (string.IsNullOrEmpty(text) || !(text != " "))
                    {
                        continue;
                    }

                    string[] array3 = text.Trim().Split('\t');
                    string text2 = array3[0];
                    string text3 = "";
                    try
                    {
                        text3 = array3[1];
                        if (text3 != "device")
                        {
                            continue;
                        }
                    }
                    catch
                    {
                    }

                    list.Add(text2.Trim());
                }
            }

            return list;
        }
        public static string ExecuteCMD(string cmdCommand)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.WorkingDirectory = @".\Data\Platform\";
                processStartInfo.FileName = "cmd.exe";
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Verb = "runas";
                process.StartInfo = processStartInfo;
                process.Start();
                process.StandardInput.WriteLine(cmdCommand);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }
        class Todo
        {
            public string countryCode { get; set; }
            public string regionName { get; set; }
            public string city { get; set; }
            public string query { get; set; }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@".\Data\Script\");
            System.IO.FileSystemInfo[] files = di.GetFileSystemInfos();
            checkLBScript.Items.AddRange(files);
            cbbScript.Items.AddRange(files);
            cbbScript.SelectedIndex = 0;
            txtbox_sendtext.Text = File.ReadAllText(path_command);
            txtWipe.Text = File.ReadAllText(ListWipe);
            txtUrl.Text = File.ReadAllText(URL);
            toolStripComboBox1.SelectedIndex = 0;
            try
            {
                string PathKill = @".\Data\Kill.txt";
                string[] linesKill = File.ReadAllLines(PathKill);
                foreach (string lineKill in linesKill)
                {
                    txtb_default.Text= lineKill;
                }
            }
            catch
            {
            }
        }
        void Start_Timer()
        {
            Task.Run(() =>
            {
                int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                int val_index = index;
                int i = 0;
                while (true)
                {
                    if (dataGridView.Rows[val_index].Cells[16].Value.ToString() == "Stoped")
                    {
                        return;
                    }
                    int phut = i / 60;
                    int giay = i-(phut*60);

                    string pPhut = "";
                    string gGiay = "";
                    if(phut < 10)
                    {
                        pPhut = "0" + phut.ToString();
                    }
                    else
                    {
                        pPhut = phut.ToString();
                    }
                    if (giay < 10)
                    {
                        gGiay = "0" + giay.ToString();
                    }
                    else
                    {
                        gGiay = giay.ToString();
                    }
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[13].Value = pPhut + ":"+ gGiay;
                    }));
                    Thread.Sleep(1000);
                    i++;
                }
            });
        }
        private void Home_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    dataGridView.Rows.Clear();
                    btnShows.Focus();
                }));
                var listDevice = LoadDevices();
                listDevice.Sort();
                List<string> DevSerial = new List<string>();
                if (listDevice != null && listDevice.Count > 0)
                {
                    foreach (var deviceID in listDevice)
                    {
                        Invoke(new Action(() =>
                        {
                            if (toolStripComboBox1.SelectedItem.ToString() == "TCP IP")
                            {
                                if (deviceID.ToLower().Contains('.'))
                                {
                                    DevSerial.Add(deviceID);
                                    DevSerial.Sort();
                                }
                            }
                            else
                            {
                                if (!deviceID.ToLower().Contains('.'))
                                {
                                    DevSerial.Add(deviceID);
                                    DevSerial.Sort();
                                }
                            }
                        }));
                    }
                    if (DevSerial != null && DevSerial.Count > 0)
                    {
                        Invoke(new Action(() =>
                        {
                            ProgressBar.Value = 0;
                            ProgressBar.Maximum = DevSerial.Count;
                        }));
                        for (int i = 0; i < DevSerial.Count; i++)
                        {
                            Invoke(new Action(() =>
                            {
                                try
                                {
                                    string PathListDevices = @".\Data\ListDevices\ListDevices.txt";
                                    string[] linesDevices = File.ReadAllLines(PathListDevices);
                                    foreach (string lineDevices in linesDevices)
                                    {
                                        string ipAddr = lineDevices.Substring(0, lineDevices.IndexOf("|"));
                                        string HostAddr = lineDevices.Substring(lineDevices.IndexOf("|") + 1);
                                        string HostAddr1 = HostAddr.Substring(0, HostAddr.IndexOf("|"));
                                        string PortWmWare = HostAddr.Substring(HostAddr.IndexOf("|") + 1);
                                        string PortWmWare1 = PortWmWare.Substring(0, PortWmWare.IndexOf("|"));
                                        string Note = PortWmWare.Substring(PortWmWare.IndexOf("|") + 1);
                                        if (DevSerial[i].Replace(":5555", "") == ipAddr)
                                        {
                                            dataGridView.Rows.Add(true, i + 1, DevSerial[i].Replace(":5555", ""), HostAddr1, PortWmWare1, Note, "Connect", "", "Show", "Wipe", "Kill", "", "", "", "Fix", "Start", "Stop");
                                            ProgressBar.Value++;
                                            return;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                                dataGridView.Rows.Add(true, i + 1, DevSerial[i].Replace(":5555", ""), "", "", "", "Connect", "", "Show", "Wipe", "Kill", "", "", "", "Fix", "Start", "Stop");
                                ProgressBar.Value++;
                            }));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có thiết bị kết nối, Vui lòng Kiểm tra lại !");
                    }
                }
                else
                {
                    MessageBox.Show("Không có thiết bị kết nối, Vui lòng Kiểm tra lại !");
                }
            });
        } 
        private void dataGridView_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            string Site = txtUrl.Text.ToString();
            string pack = txtb_default.Text.ToString();
            //Tick
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_Tick")
            {
                Task.Run(() =>
                {
                    if (Convert.ToBoolean(dataGridView.Rows[e.RowIndex].Cells[0].Value) == true)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[0].Value = false;
                    }
                    else
                    {
                        dataGridView.Rows[e.RowIndex].Cells[0].Value = true;
                    }
                });
            }
            //Kill
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_Kill")
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    string cmd_kill = $"adb.exe -s {ListDevice} shell  am force-stop {pack}";
                    Process cmd = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = @".\Data\Platform\";
                    startInfo.FileName = "cmd.exe";
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    cmd.StartInfo = startInfo;
                    cmd.Start();
                    cmd.StandardInput.WriteLine(cmd_kill);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                    cmd.Close();
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Kill " + pack + " success";
                    }));
                });
            }
            //Connect
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_Connect")
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Connecting to device...";
                    }));
                    Thread.Sleep(200);
                    if (ListDevice.ToLower().Contains('.'))
                    {
                        string cmd_Connect = $"adb.exe connect {ListDevice}";
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        process.StartInfo.WorkingDirectory = @".\Data\Platform\";
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = $"/C {cmd_Connect}";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardInput = true;
                        process.Start();
                        string Connect = "";
                        while (!process.HasExited)
                        {
                            Connect += process.StandardOutput.ReadToEnd();
                        }
                        process.Close();
                        Connect = Connect.Substring(0, Connect.IndexOf(":5555"));
                        Connect = Connect.Substring(0, Connect.IndexOf(" "));
                        if (Connect == "cannot" || Connect == "fail")
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Fail to connect";
                            }));
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Connected";
                            }));
                        }
                    }
                    else
                    {
                        string cmd_Connect = $"adb.exe -s {ListDevice} get-state";
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        process.StartInfo.WorkingDirectory = @".\Data\Platform\";
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = $"/C {cmd_Connect}";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardInput = true;
                        process.Start();
                        string Connect = "";
                        while (!process.HasExited)
                        {
                            Connect += process.StandardOutput.ReadToEnd();
                        }
                        process.Close();

                        if (Connect.Trim() == "device")
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Connected";
                            }));
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Fail to connect";
                            }));
                        }
                    }
                });
            }
            //Show
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_show")
            {
                Task.Run(() =>
                {
                    try
                    {
                        string _FPS = nmrFPS.Value.ToString();
                        string _BMR = txtBMR.Text.ToString();
                        string _MS = txtMS.Text.ToString();
                        int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                        int val_index = index;
                        string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                        if (cbTitle.Checked)
                        {
                            if (dataGridView.Rows[val_index].Cells[5].Value.ToString().Trim() != "")
                            {
                                string title = dataGridView.Rows[val_index].Cells[5].Value.ToString();
                                IntPtr hwnd_Check = UnsafeNativeMethods.FindWindow(null, title);
                                StringBuilder stringBuilder_Check = new StringBuilder(256);
                                UnsafeNativeMethods.GetWindowText(hwnd_Check, stringBuilder_Check, stringBuilder_Check.Capacity);
                                if (stringBuilder_Check.ToString().Trim() != title)
                                {
                                    if (cbHome.Checked)
                                    {
                                        Task.Run(() =>
                                        {
                                            string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                            $"input keyevent 3 \n" +
                                            $"su \n" +
                                            $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            cmd.WaitForExit();
                                            cmd.Close();
                                        });
                                    }
                                    int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                    int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                    int width_scrcpy = (width_screenWin / 8) - 0;
                                    int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                    int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                    int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                    if ((Number_scrcpy / 8) == 0)
                                    {
                                        int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                        if (Number_scrcpy < 16)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Scrcpy => " + ListDevice;
                                            }));
                                            int mShow = 0;
                                        AA:
                                            string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmdCommandShow);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                            IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                            StringBuilder stringBuilder = new StringBuilder(256);
                                            UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                            if ((stringBuilder.ToString().Trim() != title))
                                            {
                                                if (mShow > 2)
                                                    goto A;
                                                mShow++;
                                                cmd.Close();
                                                goto AA;
                                            }
                                        A:
                                            cmd.Close();
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Show " + ListDevice;
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                        if (Number_scrcpy < 16)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Scrcpy => " + ListDevice;
                                            }));
                                            int nShows = 0;
                                        BB:

                                            string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmdCommandShow);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                            IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                            StringBuilder stringBuilder = new StringBuilder(256);
                                            UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                            if ((stringBuilder.ToString().Trim() != title))
                                            {
                                                if (nShows > 2)
                                                    goto B;
                                                nShows++;
                                                cmd.Close();
                                                goto BB;
                                            }
                                        B:
                                            cmd.Close();
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Show " + ListDevice;
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[val_index].Cells[7].Value = ListDevice + " is shown";
                                    }));
                                    BringToFront(hwnd_Check);
                                    ShowWindow(hwnd_Check, SW_RESTORE);
                                }
                            }
                            else
                            {
                                string title = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                                IntPtr hwnd_Check = UnsafeNativeMethods.FindWindow(null, title);
                                StringBuilder stringBuilder_Check = new StringBuilder(256);
                                UnsafeNativeMethods.GetWindowText(hwnd_Check, stringBuilder_Check, stringBuilder_Check.Capacity);
                                if (stringBuilder_Check.ToString().Trim() != title)
                                {
                                    if (cbHome.Checked)
                                    {
                                        Task.Run(() =>
                                        {
                                            string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                            $"input keyevent 3 \n" +
                                            $"su \n" +
                                            $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            cmd.WaitForExit();
                                            cmd.Close();
                                        });
                                    }
                                    int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                    int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                    int width_scrcpy = (width_screenWin / 8) - 0;
                                    int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                    int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                    int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                    if ((Number_scrcpy / 8) == 0)
                                    {
                                        int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                        if (Number_scrcpy < 16)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Scrcpy => " + ListDevice;
                                            }));
                                            int mShow = 0;
                                        AA:
                                            string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmdCommandShow);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                            IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                            StringBuilder stringBuilder = new StringBuilder(256);
                                            UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                            if ((stringBuilder.ToString().Trim() != title))
                                            {
                                                if (mShow > 2)
                                                    goto A;
                                                mShow++;
                                                cmd.Close();
                                                goto AA;
                                            }
                                        A:
                                            cmd.Close();
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Show " + ListDevice;
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                        if (Number_scrcpy < 16)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Scrcpy => " + ListDevice;
                                            }));
                                            int nShows = 0;
                                        BB:

                                            string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmdCommandShow);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                            IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                            StringBuilder stringBuilder = new StringBuilder(256);
                                            UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                            if ((stringBuilder.ToString().Trim() != title))
                                            {
                                                if (nShows > 2)
                                                    goto B;
                                                nShows++;
                                                cmd.Close();
                                                goto BB;
                                            }
                                        B:
                                            cmd.Close();
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = "Show " + ListDevice;
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[val_index].Cells[7].Value = ListDevice + " is shown";
                                    }));
                                    BringToFront(hwnd_Check);
                                    ShowWindow(hwnd_Check, SW_RESTORE);
                                }
                            }
                        }
                        else
                        {
                            string title = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                            IntPtr hwnd_Check = UnsafeNativeMethods.FindWindow(null, title);
                            StringBuilder stringBuilder_Check = new StringBuilder(256);
                            UnsafeNativeMethods.GetWindowText(hwnd_Check, stringBuilder_Check, stringBuilder_Check.Capacity);
                            if (stringBuilder_Check.ToString().Trim() != title)
                            {
                                if (cbHome.Checked)
                                {
                                    Task.Run(() =>
                                    {
                                        string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                        $"input keyevent 3 \n" +
                                        $"su \n" +
                                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        cmd.WaitForExit();
                                        cmd.Close();
                                    });
                                }
                                int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                int width_scrcpy = (width_screenWin / 8) - 0;
                                int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                if ((Number_scrcpy / 8) == 0)
                                {
                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                    if (Number_scrcpy < 16)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = "Scrcpy => " + ListDevice;
                                        }));
                                        int mShow = 0;
                                    AA:
                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                        StringBuilder stringBuilder = new StringBuilder(256);
                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                        if ((stringBuilder.ToString().Trim() != title))
                                        {
                                            if (mShow > 2)
                                                goto A;
                                            mShow++;
                                            cmd.Close();
                                            goto AA;
                                        }
                                    A:
                                        cmd.Close();
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = "Show " + ListDevice;
                                        }));
                                    }
                                }
                                else
                                {
                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                    if (Number_scrcpy < 16)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = "Scrcpy => " + ListDevice;
                                        }));
                                        int nShows = 0;
                                    BB:

                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                        StringBuilder stringBuilder = new StringBuilder(256);
                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                        if ((stringBuilder.ToString().Trim() != title))
                                        {
                                            if (nShows > 2)
                                                goto B;
                                            nShows++;
                                            cmd.Close();
                                            goto BB;
                                        }
                                    B:
                                        cmd.Close();
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = "Show " + ListDevice;
                                        }));
                                    }
                                }
                            }
                            else
                            {
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = ListDevice + " is shown";
                                }));
                                BringToFront(hwnd_Check);
                                ShowWindow(hwnd_Check, SW_RESTORE);
                            }
                        }
                        
                        
                    }
                    catch { }
                });
            }
            //Wipe
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_wipe")
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    string[] linesClear = System.IO.File.ReadAllLines(ListWipe);
                    for (int l = 0; l < linesClear.Length; l++)
                    {
                        string cmdCommandClear = $"adb.exe -s {ListDevice}  shell pm clear {linesClear[l]}";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdCommandClear);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Wipe " + linesClear[l];
                        }));
                    }
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Wipe done";
                    }));
                });
            }
            //Fix
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_Fix")
            {

                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "fixing...";
                    }));
                    Random RNG = new Random();
                    const string range = "abcde0123456789abcde0123456789";
                    var chars = Enumerable.Range(0, 16).Select(x => range[RNG.Next(0, range.Length)]);
                    string RandomID = new string(chars.ToArray());

                    Random SRN = new Random();
                    const string rangeSRN = "abcdef0123456789abcdef0123456789";
                    var SerialNo = Enumerable.Range(0, 12).Select(x => rangeSRN[SRN.Next(0, rangeSRN.Length)]);
                    string RandomSerialNo = new string(SerialNo.ToArray());

                    Random RILN = new Random();
                    const string rangeRILN = "QWERTYUIOPASDFGHJKLZXCVBNM0123456789";
                    var Ril_Number = Enumerable.Range(0, 6).Select(x => rangeRILN[RILN.Next(0, rangeRILN.Length)]);
                    string SerialRil_Number = new string(Ril_Number.ToArray());
                    string Ril = "R39H" + SerialRil_Number;
                    string cmdCommandChange = "";
                    if (cb_usage.Checked)
                    {
                        cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                        $"input keyevent 3 \n" +
                        $"su \n" +
                        $"su -c  mount -o rw,remount / \n" +
                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done;" +
                        $"am force-stop {pack} \n" +
                        $"settings put secure android_id {RandomID} \n" +
                        $"rm -rf /data/data/{pack}/* \n" +
                        $"appops set {pack} GET_USAGE_STATS deny";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdCommandChange);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Clear usage acccess";
                        }));
                    }
                    if(cb_wipeGG.Checked)
                    {
                        cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                        $"input keyevent 3 \n" +
                        $"su \n" +
                        $"su -c  mount -o rw,remount / \n" +
                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done;" +
                        $"am force-stop {pack} \n" +
                        $"settings put secure android_id {RandomID} \n" +
                        $"rm -rf /data/data/com.google.android.gms/* \n" +
                        $"rm -rf /data/data/com.google.android.gsf/* \n" +
                        $"rm -rf /data/data/com.google.android.ims/* \n" +
                        $"rm -rf /data/data/com.android.vending/* \n" +
                        $"pm clear com.android.vending \n" +
                        $"pm clear com.google.android.gms \n" +
                        $"pm clear com.google.android.gsf \n" +
                        $"pm clear com.google.android.ims \n" +
                        $"rm -rf /data/data/{pack}/* \n";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdCommandChange);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Wipe google";
                        }));
                    }
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Fix done";
                    }));
                });
            }
            //Start Script
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_Start")
            {
                string p1 = cbbScript.SelectedItem.ToString();
                int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                int val_index = index;
                string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                dataGridView.Rows[val_index].Cells[16].Value = "Stop";
                dataGridView.Rows[val_index].Cells[15].Value = "Started";
                isTimer.Add(true);
                isTimer[val_index] = true;
                Thread T = new Thread(() =>
                {
                    Start_Timer();
                    string PathScript = @".\Data\Script\" + p1;
                    string[] lines = File.ReadAllLines(PathScript);
                    while (true)
                    {
                        if (dataGridView.Rows[val_index].Cells[16].Value.ToString() == "Stoped")
                        {
                            return;
                        }
                        for (int k = 0; k < lines.Length; k++)
                        {
                            if (dataGridView.Rows[val_index].Cells[16].Value.ToString() == "Stoped")
                            {
                                return;
                            }
                            int kk = k;
                            if (Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == false) return;
                            string searchKey = lines[kk].Substring(0, lines[kk].IndexOf(" "));
                            if (searchKey == "sleep")
                            {
                                string TSleep = lines[kk].Replace("sleep ", "");
                                string TSleepX = TSleep.Substring(0, TSleep.IndexOf(" "));
                                string TSleepY = TSleep.Substring(TSleep.LastIndexOf(" ") + 1);
                                int parsedTSleepX = int.Parse(TSleepX);
                                int parsedTSleepY = int.Parse(TSleepY);
                                int numberX = parsedTSleepX;
                                int numberY = parsedTSleepY;
                                Random timeSleep = new Random();
                                int rd = timeSleep.Next(numberX, numberY);
                                while (rd >= 0)
                                {
                                    if (dataGridView.Rows[val_index].Cells[16].Value.ToString() == "Stoped")
                                    {
                                        return;
                                    }
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[val_index].Cells[7].Value = "sleep " + rd;
                                    }));
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                    rd--;
                                }
                            }
                            else
                            {
                                if (dataGridView.Rows[val_index].Cells[16].Value.ToString() == "Stoped")
                                {
                                    return;
                                }
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = lines[kk];
                                }));
                                Process cmd = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                startInfo.WorkingDirectory = @".\Data\Platform\";
                                startInfo.FileName = "cmd.exe";
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardInput = true;
                                startInfo.RedirectStandardOutput = true;
                                cmd.StartInfo = startInfo;
                                string cmd_script = $"adb.exe -s {ListDevice} shell  \n" +
                                                    $"su \n" +
                                                    $"{lines[kk]}";
                                cmd.Start();
                                cmd.StandardInput.WriteLine(cmd_script);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                cmd.Close();
                            }
                        }
                    }
                });
                T.Start();
                T.IsBackground= true;
                if (dataGridView.Rows[val_index].Cells[16].Value.ToString() == "Stoped")
                {
                    T.Abort();
                }
            }
            //Stop Script
            if (dataGridView.Columns[e.ColumnIndex].Name == "dv_Stop")
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[15].Value = "Start";
                        dataGridView.Rows[val_index].Cells[16].Value = "Stoped";
                        dataGridView.Rows[val_index].Cells[7].Value = "Stop script";
                    }));
                });
            }
        }
        private void radSelect_CheckedChanged(object sender, EventArgs e)
        {
            Thread T_CheckedChanged = new Thread(() =>
            {
                Invoke(new Action(() =>
                {
                    if (dataGridView.RowCount == 0)
                    {
                        return;
                    }
                    if (radSelect.Checked == false)
                    {
                        for (int i = 0; i < dataGridView.RowCount; i++)
                        {
                            dataGridView.Rows[i].Cells[0].Value = false;
                            dataGridView.Rows[i].DefaultCellStyle.ForeColor = Color.Red;
                            
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dataGridView.RowCount; i++)
                        {
                            dataGridView.Rows[i].Cells[0].Value = true;
                            dataGridView.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                        }
                    }
                }));
            });
            T_CheckedChanged.Start();
            
        }
        private void btnStartscript_Click(object sender, EventArgs e)
        {
            string p1 = cbbScript.SelectedItem.ToString();
            btnStartscript.Enabled = false;
            btnStopscript.Enabled = true;
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                int j = i;
                if (Convert.ToBoolean(dataGridView.Rows[j].Cells[0].Value) == true)
                {
                    string ListDevice = dataGridView.Rows[j].Cells[2].Value.ToString();
                    Task.Run(() =>
                    {
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[j].Cells[15].Value = "Started";
                            dataGridView.Rows[j].Cells[16].Value = "Stop";
                        }));
                        Task.Run(() =>
                        {
                            
                            int i_Elapse = 0;
                            while (true)
                            {
                                if (dataGridView.Rows[j].Cells[16].Value.ToString() == "Stoped") return;
                                int phut = i_Elapse / 60;
                                int giay = i_Elapse - (phut * 60);

                                string pPhut = "";
                                string gGiay = "";
                                if (phut < 10)
                                {
                                    pPhut = "0" + phut.ToString();
                                }
                                else
                                {
                                    pPhut = phut.ToString();
                                }
                                if (giay < 10)
                                {
                                    gGiay = "0" + giay.ToString();
                                }
                                else
                                {
                                    gGiay = giay.ToString();
                                }
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[j].Cells[13].Value = pPhut + ":" + gGiay;
                                }));
                                Thread.Sleep(1000);
                                i_Elapse++;
                            }
                        });
                        string PathScript = @".\Data\Script\" + p1;
                        string[] lines = File.ReadAllLines(PathScript);
                        while (true)
                        {
                            if (dataGridView.Rows[j].Cells[16].Value.ToString() == "Stoped") return;
                            for (int k = 0; k < lines.Length; k++)
                            {
                                int kk = k;
                                if (dataGridView.Rows[j].Cells[16].Value.ToString() == "Stoped") return;
                                string searchKey = lines[kk].Substring(0, lines[kk].IndexOf(" "));
                                if (searchKey == "sleep")
                                {
                                    string TSleep = lines[kk].Replace("sleep ", "");
                                    string TSleepX = TSleep.Substring(0, TSleep.IndexOf(" "));
                                    string TSleepY = TSleep.Substring(TSleep.LastIndexOf(" ") + 1);
                                    int parsedTSleepX = int.Parse(TSleepX);
                                    int parsedTSleepY = int.Parse(TSleepY);
                                    int numberX = parsedTSleepX;
                                    int numberY = parsedTSleepY;
                                    Random timeSleep = new Random();
                                    int rd = timeSleep.Next(numberX, numberY);
                                    while (rd >= 0)
                                    {
                                        if (dataGridView.Rows[j].Cells[16].Value.ToString() == "Stoped") return;
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[j].Cells[7].Value = "sleep " + rd;
                                        }));
                                        Thread.Sleep(TimeSpan.FromSeconds(1));
                                        rd--;
                                    }
                                }
                                else
                                {
                                    if (dataGridView.Rows[j].Cells[16].Value.ToString() == "Stoped") return;
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[j].Cells[7].Value = lines[kk];
                                    }));
                                    Process cmd = new Process();
                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.CreateNoWindow = true;
                                    startInfo.UseShellExecute = false;
                                    startInfo.RedirectStandardInput = true;
                                    startInfo.RedirectStandardOutput = true;
                                    cmd.StartInfo = startInfo;
                                    string cmd_script = $"adb.exe -s {ListDevice} shell  \n" +
                                                        $"su \n" +
                                                        $"{lines[kk]}";
                                    cmd.Start();
                                    cmd.StandardInput.WriteLine(cmd_script);
                                    cmd.StandardInput.Flush();
                                    cmd.StandardInput.Close();
                                    cmd.WaitForExit();
                                    cmd.Close();
                                }
                            }
                        }
                    });
                }
            }
            
        }
       
       
        private void checkLBScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            Thread T_checkLBScript = new Thread(() =>
            {
                Invoke(new Action(() =>
                {
                    int index = checkLBScript.SelectedIndex;
                    int count = checkLBScript.Items.Count;
                    for (int x = 0; x < count; x++)
                    {
                        if (index != x)
                        {
                            checkLBScript.SetItemChecked(x, false);
                        }
                    }
                    if (checkLBScript.CheckedItems.Count > 0)
                    {
                        string PathScript = @".\Data\Script\";
                        for (int i = 0; i < checkLBScript.CheckedItems.Count; i++)
                        {
                            PathScript += "" + checkLBScript.CheckedItems[i].ToString();
                        }
                        txtConten.Text = File.ReadAllText(PathScript);
                    }
                }));
            });
            T_checkLBScript.Start();
        }
        private void btnsaveScript_Click(object sender, EventArgs e)
        {
            Thread T_saveScript = new Thread(() =>
            {
                Invoke(new Action(() =>
                {
                    if (checkLBScript.CheckedItems.Count > 0)
                    {
                        string PathScript = @".\Data\Script\";
                        for (int i = 0; i < checkLBScript.CheckedItems.Count; i++)
                        {
                            PathScript += "" + checkLBScript.CheckedItems[i].ToString();
                        }
                        File.WriteAllText(PathScript, txtConten.Text);
                        txtConten.Text = File.ReadAllText(PathScript);
                    }
                    else
                    {
                        MessageBox.Show("Chưa chọn Script");
                    }
                }));
            });
            T_saveScript.Start();
        }
        private void radWipe_CheckedChanged(object sender, EventArgs e)
        {
            Thread T_radWipe = new Thread(() =>
            {
                Invoke(new Action(() =>
                {
                    if (!radDefault.Checked)
                        txtWipe.Text = File.ReadAllText(ListWipe);
                    else
                        txtWipe.Text = File.ReadAllText(ListDefault);
                }));
            });
            T_radWipe.Start();
        }
        private void btnStopscript_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < dataGridView.RowCount; j++)
            {
                int valj = j;
                Thread T_Stopscript = new Thread(() =>
                {
                    if (Convert.ToBoolean(dataGridView.Rows[valj].Cells[0].Value) == true)
                    {
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[valj].Cells[15].Value = "Start";
                            dataGridView.Rows[valj].Cells[16].Value = "Stoped";
                            dataGridView.Rows[valj].Cells[7].Value = "Stoped script";
                            dataGridView.Rows[valj].Cells[13].Value = "";
                        }));
                    }
                    else
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[valj].Cells[7].Value = "";
                        }));
                });
                T_Stopscript.Start();
                T_Stopscript.IsBackground = true;
            }
            btnStartscript.Enabled = true;
        }
        private void AddFromFile_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            btnShows.Focus();
            string PathListDevices = @".\Data\ListDevices\ListDevices.txt";
            ProgressBar.Value = 0;
            string[] linesDevices = File.ReadAllLines(PathListDevices);
            int Linesdev = File.ReadLines(PathListDevices).Count();
            ProgressBar.Maximum = Linesdev;
            for(int i = 0; i < Linesdev; i++)
            {
                int ii = i;
                string ipAddr = linesDevices[ii].Substring(0, linesDevices[ii].IndexOf("|"));
                string HostAddr = linesDevices[ii].Substring(linesDevices[ii].IndexOf("|") + 1);
                string HostAddr1 = HostAddr.Substring(0, HostAddr.IndexOf("|"));
                string PortWmWare = HostAddr.Substring(HostAddr.IndexOf("|") + 1);
                string PortWmWare1 = PortWmWare.Substring(0, PortWmWare.IndexOf("|"));
                string Note = PortWmWare.Substring(PortWmWare.IndexOf("|") + 1);

                dataGridView.Rows.Add(false, ii + 1, ipAddr, HostAddr1, PortWmWare1, Note, "Connect", "Connecting to device...", "Show", "Wipe", "Kill", "", "", "", "Fix", "Start", "Stop");
                Task.Run(() =>
                {
                    if (!ipAddr.ToLower().Contains('.'))
                    {
                        if (Check_Conect(ipAddr))
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[ii].Cells[7].Value = "Conected";
                                ProgressBar.Value++;
                                dataGridView.Rows[ii].Cells[0].Value = true;
                            }));
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[ii].Cells[7].Value = "Fail to conect";
                                ProgressBar.Value++;
                                dataGridView.Rows[ii].Cells[0].Value = false;
                            }));
                        }
                    }
                    else
                    {
                        if (Check_ConectTCP(ipAddr))
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[ii].Cells[7].Value = "Conected";
                                ProgressBar.Value++;
                                dataGridView.Rows[ii].Cells[0].Value = true;
                            }));
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[ii].Cells[7].Value = "Fail to conect";
                                ProgressBar.Value++;
                                dataGridView.Rows[ii].Cells[0].Value = false;
                            }));
                        }
                    }
                    
                });
                

            }
        }
        private  void button4_Click(object sender, EventArgs e)
        {
            if(dataGridView.Rows.Count>0)
            {
                btnstartcheckip.Enabled = false;
                btnStopCheckip.Enabled = true;
                isCheck = false;
                for (int m = 0; m < dataGridView.Rows.Count; m++)
                {
                    int index = m;
                    if ((dataGridView.Rows[index].Cells[3].Value.ToString() !="") && (dataGridView.Rows[index].Cells[4].Value.ToString()!=""))
                    {
                        string Sockproxy = dataGridView.Rows[index].Cells[3].Value.ToString();
                        string Sockport = dataGridView.Rows[index].Cells[4].Value.ToString();
                        int parsedSockport = int.Parse(Sockport);
                        Thread T = new Thread(async () =>
                        {
                            while (true)
                            {
                                try
                                {
                                    if (isCheck) { return; }
                                    var proxy = new HttpToSocks5Proxy(Sockproxy, parsedSockport);
                                    var handler = new HttpClientHandler { Proxy = proxy };
                                    HttpClient httpClient = new HttpClient(handler, true);
                                    var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://www.ip-api.com/json"));
                                    var jsonString = await result.Content.ReadAsStringAsync();
                                    Todo todo = JsonConvert.DeserializeObject<Todo>(jsonString);
                                    if (todo != null)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[index].Cells[11].Value = todo.query;
                                            dataGridView.Rows[index].Cells[12].Value = todo.countryCode + "-" + todo.regionName;

                                        }));
                                    }
                                    else
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[index].Cells[11].Value = "Error";
                                            dataGridView.Rows[index].Cells[12].Value = "Error";
                                        }));
                                    }
                                }
                                catch
                                {
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[index].Cells[11].Value = "Error";
                                        dataGridView.Rows[index].Cells[12].Value = "Error";
                                    }));
                                }
                                Thread.Sleep(TimeSpan.FromMinutes(int.Parse(nmrcheckip.Value.ToString())));
                            }
                        });
                        T.Start();
                        T.IsBackground = true;
                        if (isCheck) { T.Abort(); }
                    }
                }
            }
        }
        private void btnStopCheckip_Click(object sender, EventArgs e)
        {
            isCheck=true;
            btnstartcheckip.Enabled = true;
            

            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                int j = i;
                dataGridView.Rows[j].Cells[11].Value = "" ;
                dataGridView.Rows[j].Cells[12].Value = "";
            }
        }
        private void btnLuu_Click(object sender, EventArgs e)
        {
            string PathListDevices = @".\Data\ListDevices\ListDevices.txt";
            if (dataGridView.RowCount > 0)
            {
                File.WriteAllText(PathListDevices, "");
                for (int i = 0; i < dataGridView.RowCount; i++)
                {
                    int j = i;
                    string Ip = dataGridView.Rows[j].Cells[2].Value.ToString();
                    string Host = dataGridView.Rows[j].Cells[3].Value.ToString();
                    string Port = dataGridView.Rows[j].Cells[4].Value.ToString();
                    string Note = dataGridView.Rows[j].Cells[5].Value.ToString();
                    StreamWriter writer = new StreamWriter(PathListDevices, true);
                    writer.WriteLine(Ip + "|" + Host + "|" + Port + "|" + Note);
                    writer.Dispose();
                }
            }
        }
        /// <summary>
        /// Replaces text in a file.
        /// </summary>
        /// <param name="filePath">Path of the text file.</param>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="replaceText">Text to replace the search text.</param>
        static public void ReplaceInFile(string filePath, string searchText, string replaceText)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();
            content = Regex.Replace(content, searchText, replaceText);
            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(content);
            writer.Close();
        }
        public bool  Check_Conect(string Device)
        {
            bool Res_CON;
            string cmd_Connect = $"adb.exe -s {Device} get-state";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.WorkingDirectory = @".\Data\Platform\";
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/C {cmd_Connect}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();
            string Connect = "";
            while (!process.HasExited)
            {
                Connect += process.StandardOutput.ReadToEnd();
            }
            process.Close();

            if (Connect.Trim() == "device")
            {
                Res_CON = true;
            }
            else
            {
                Res_CON = false;
            }
            return Res_CON;
        }
        public bool Check_ConectTCP(string Device)
        {
            bool Res_CON;
            string cmd_Connect = $"adb.exe connect {Device}";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.WorkingDirectory = @".\Data\Platform\";
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/C {cmd_Connect}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();
            string Connect = "";
            while (!process.HasExited)
            {
                Connect += process.StandardOutput.ReadToEnd();
            }
            process.Close();

            Connect = Connect.Substring(0, Connect.IndexOf(":5555"));
            
            Connect = Connect.Substring(0, Connect.IndexOf(" "));
            if (Connect == "cannot" || Connect == "fail")
            {
                Res_CON= false;
            }
            else
            {
                Res_CON = true;
            }

            return Res_CON;
        }
        private void btnShows_Click(object sender, EventArgs e)
        {
            string title = "";
            if (dataGridView.Rows.Count > 0)
            {
                Task.Run(() =>
                {
                    string _FPS = nmrFPS.Value.ToString();
                    string _BMR = txtBMR.Text.ToString();
                    string _MS = txtMS.Text.ToString();
                    Invoke(new Action(() =>
                    {
                        btnShows.Enabled = false;
                    }));
                    if (radCheck.Checked)
                    {
                        for (int i = 0; i < dataGridView.Rows.Count; i++)
                        {
                            int vali = i;
                            if (Convert.ToBoolean(dataGridView.Rows[vali].Cells[0].Value) == true)
                            {
                                if (cbTitle.Checked)
                                {
                                    if (dataGridView.Rows[vali].Cells[5].Value.ToString().Trim() !="")
                                    {
                                        title = dataGridView.Rows[vali].Cells[5].Value.ToString();
                                        string ListDevice = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                        IntPtr hwnd_check = UnsafeNativeMethods.FindWindow(null, title);
                                        StringBuilder stringBuilder_check = new StringBuilder(256);
                                        UnsafeNativeMethods.GetWindowText(hwnd_check, stringBuilder_check, stringBuilder_check.Capacity);
                                        if (stringBuilder_check.ToString().Trim() != title)
                                        {
                                            try
                                            {
                                                if (cbHome.Checked)
                                                {
                                                    Task.Run(() =>
                                                    {
                                                        string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                                        $"input keyevent 3 \n" +
                                                        $"su \n" +
                                                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        cmd.WaitForExit();
                                                        cmd.Close();
                                                    });

                                                }
                                                int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                                int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                                int width_scrcpy = (width_screenWin / 8) - 0;
                                                int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                                int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                                int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                                if ((Number_scrcpy / 8) == 0)
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int nShow = 0;
                                                    AA:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (nShow > 1) goto A;
                                                            nShow++;
                                                            cmd.Close();
                                                            goto AA;
                                                        }
                                                    A:
                                                        cmd.Close();
                                                        if (nShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int mShow = 0;
                                                    BB:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (mShow > 1) goto B;
                                                            mShow++;
                                                            cmd.Close();
                                                            goto BB;
                                                        }
                                                    B:
                                                        cmd.Close();
                                                        if (mShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            BringToFront(hwnd_check);
                                            ShowWindow(hwnd_check, SW_RESTORE);
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[vali].Selected = true;
                                                Invoke(new Action(() =>
                                                {
                                                    dataGridView.Rows[vali].Selected = true;
                                                    dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                }));
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        title = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                        string ListDevice = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                        IntPtr hwnd_check = UnsafeNativeMethods.FindWindow(null, title);
                                        StringBuilder stringBuilder_check = new StringBuilder(256);
                                        UnsafeNativeMethods.GetWindowText(hwnd_check, stringBuilder_check, stringBuilder_check.Capacity);
                                        if (stringBuilder_check.ToString().Trim() != title)
                                        {
                                            try
                                            {
                                                if (cbHome.Checked)
                                                {
                                                    Task.Run(() =>
                                                    {
                                                        string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                                        $"input keyevent 3 \n" +
                                                        $"su \n" +
                                                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        cmd.WaitForExit();
                                                        cmd.Close();
                                                    });

                                                }
                                                int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                                int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                                int width_scrcpy = (width_screenWin / 8) - 0;
                                                int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                                int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                                int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                                if ((Number_scrcpy / 8) == 0)
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int nShow = 0;
                                                    AA:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (nShow > 1) goto A;
                                                            nShow++;
                                                            cmd.Close();
                                                            goto AA;
                                                        }
                                                    A:
                                                        cmd.Close();
                                                        if (nShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int mShow = 0;
                                                    BB:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (mShow > 1) goto B;
                                                            mShow++;
                                                            cmd.Close();
                                                            goto BB;
                                                        }
                                                    B:
                                                        cmd.Close();
                                                        if (mShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            BringToFront(hwnd_check);
                                            ShowWindow(hwnd_check, SW_RESTORE);
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[vali].Selected = true;
                                                Invoke(new Action(() =>
                                                {
                                                    dataGridView.Rows[vali].Selected = true;
                                                    dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                }));
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    title = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                    string ListDevice = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                    IntPtr hwnd_check = UnsafeNativeMethods.FindWindow(null, title);
                                    StringBuilder stringBuilder_check = new StringBuilder(256);
                                    UnsafeNativeMethods.GetWindowText(hwnd_check, stringBuilder_check, stringBuilder_check.Capacity);
                                    if (stringBuilder_check.ToString().Trim() != title)
                                    {
                                        try
                                        {
                                            if (cbHome.Checked)
                                            {
                                                Task.Run(() =>
                                                {
                                                    string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                                    $"input keyevent 3 \n" +
                                                    $"su \n" +
                                                    $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                                    Process cmd = new Process();
                                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                                    startInfo.FileName = "cmd.exe";
                                                    startInfo.CreateNoWindow = true;
                                                    startInfo.UseShellExecute = false;
                                                    startInfo.RedirectStandardInput = true;
                                                    startInfo.RedirectStandardOutput = true;
                                                    cmd.StartInfo = startInfo;
                                                    cmd.Start();
                                                    cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                                    cmd.StandardInput.Flush();
                                                    cmd.StandardInput.Close();
                                                    cmd.WaitForExit();
                                                    cmd.Close();
                                                });

                                            }
                                            int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                            int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                            int width_scrcpy = (width_screenWin / 8) - 0;
                                            int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                            int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                            int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                            if ((Number_scrcpy / 8) == 0)
                                            {
                                                int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                                if (Number_scrcpy < 16)
                                                {
                                                    int nShow = 0;
                                                AA:
                                                    Invoke(new Action(() =>
                                                    {
                                                        dataGridView.Rows[vali].Selected = true;
                                                        dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                    }));

                                                    string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                    Process cmd = new Process();
                                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                                    startInfo.FileName = "cmd.exe";
                                                    startInfo.CreateNoWindow = true;
                                                    startInfo.UseShellExecute = false;
                                                    startInfo.RedirectStandardInput = true;
                                                    startInfo.RedirectStandardOutput = true;
                                                    cmd.StartInfo = startInfo;
                                                    cmd.Start();
                                                    cmd.StandardInput.WriteLine(cmdCommandShow);
                                                    cmd.StandardInput.Flush();
                                                    cmd.StandardInput.Close();
                                                    Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                    IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, ListDevice);
                                                    StringBuilder stringBuilder = new StringBuilder(256);
                                                    UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                    if ((stringBuilder.ToString().Trim() != ListDevice))
                                                    {
                                                        if (nShow > 1) goto A;
                                                        nShow++;
                                                        cmd.Close();
                                                        goto AA;
                                                    }
                                                A:
                                                    cmd.Close();
                                                    if (nShow > 1)
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                        }));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                                if (Number_scrcpy < 16)
                                                {
                                                    int mShow = 0;
                                                BB:
                                                    Invoke(new Action(() =>
                                                    {
                                                        dataGridView.Rows[vali].Selected = true;
                                                        dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                    }));

                                                    string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                    Process cmd = new Process();
                                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                                    startInfo.FileName = "cmd.exe";
                                                    startInfo.CreateNoWindow = true;
                                                    startInfo.UseShellExecute = false;
                                                    startInfo.RedirectStandardInput = true;
                                                    startInfo.RedirectStandardOutput = true;
                                                    cmd.StartInfo = startInfo;
                                                    cmd.Start();
                                                    cmd.StandardInput.WriteLine(cmdCommandShow);
                                                    cmd.StandardInput.Flush();
                                                    cmd.StandardInput.Close();
                                                    Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                    IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, ListDevice);
                                                    StringBuilder stringBuilder = new StringBuilder(256);
                                                    UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                    if ((stringBuilder.ToString().Trim() != ListDevice))
                                                    {
                                                        if (mShow > 1) goto B;
                                                        mShow++;
                                                        cmd.Close();
                                                        goto BB;
                                                    }
                                                B:
                                                    cmd.Close();
                                                    if (mShow > 1)
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                        }));
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        BringToFront(hwnd_check);
                                        ShowWindow(hwnd_check, SW_RESTORE);
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[vali].Selected = true;
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[vali].Selected = true;
                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                            }));
                                        }));
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dataGridView.Rows.Count; i++)
                        {
                            int vali = i;
                            if (Convert.ToBoolean(dataGridView.Rows[vali].Cells[0].Value) == false)
                            {
                                if (cbTitle.Checked)
                                {
                                    if (dataGridView.Rows[vali].Cells[5].Value.ToString().Trim() != "")
                                    {
                                        title = dataGridView.Rows[vali].Cells[5].Value.ToString();
                                        string ListDevice = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                        IntPtr hwnd_check = UnsafeNativeMethods.FindWindow(null, title);
                                        StringBuilder stringBuilder_check = new StringBuilder(256);
                                        UnsafeNativeMethods.GetWindowText(hwnd_check, stringBuilder_check, stringBuilder_check.Capacity);
                                        if (stringBuilder_check.ToString().Trim() != title)
                                        {
                                            try
                                            {
                                                if (cbHome.Checked)
                                                {
                                                    Task.Run(() =>
                                                    {
                                                        string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                                        $"input keyevent 3 \n" +
                                                        $"su \n" +
                                                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        cmd.WaitForExit();
                                                        cmd.Close();
                                                    });

                                                }
                                                int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                                int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                                int width_scrcpy = (width_screenWin / 8) - 0;
                                                int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                                int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                                int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                                if ((Number_scrcpy / 8) == 0)
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int nShow = 0;
                                                    AA:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (nShow > 1) goto A;
                                                            nShow++;
                                                            cmd.Close();
                                                            goto AA;
                                                        }
                                                    A:
                                                        cmd.Close();
                                                        if (nShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int mShow = 0;
                                                    BB:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (mShow > 1) goto B;
                                                            mShow++;
                                                            cmd.Close();
                                                            goto BB;
                                                        }
                                                    B:
                                                        cmd.Close();
                                                        if (mShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            BringToFront(hwnd_check);
                                            ShowWindow(hwnd_check, SW_RESTORE);
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[vali].Selected = true;
                                                Invoke(new Action(() =>
                                                {
                                                    dataGridView.Rows[vali].Selected = true;
                                                    dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                }));
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        title = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                        string ListDevice = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                        IntPtr hwnd_check = UnsafeNativeMethods.FindWindow(null, title);
                                        StringBuilder stringBuilder_check = new StringBuilder(256);
                                        UnsafeNativeMethods.GetWindowText(hwnd_check, stringBuilder_check, stringBuilder_check.Capacity);
                                        if (stringBuilder_check.ToString().Trim() != title)
                                        {
                                            try
                                            {
                                                if (cbHome.Checked)
                                                {
                                                    Task.Run(() =>
                                                    {
                                                        string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                                        $"input keyevent 3 \n" +
                                                        $"su \n" +
                                                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        cmd.WaitForExit();
                                                        cmd.Close();
                                                    });

                                                }
                                                int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                                int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                                int width_scrcpy = (width_screenWin / 8) - 0;
                                                int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                                int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                                int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                                if ((Number_scrcpy / 8) == 0)
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int nShow = 0;
                                                    AA:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (nShow > 1) goto A;
                                                            nShow++;
                                                            cmd.Close();
                                                            goto AA;
                                                        }
                                                    A:
                                                        cmd.Close();
                                                        if (nShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                                    if (Number_scrcpy < 16)
                                                    {
                                                        int mShow = 0;
                                                    BB:
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                        }));

                                                        string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {title} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                        Process cmd = new Process();
                                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                                        startInfo.FileName = "cmd.exe";
                                                        startInfo.CreateNoWindow = true;
                                                        startInfo.UseShellExecute = false;
                                                        startInfo.RedirectStandardInput = true;
                                                        startInfo.RedirectStandardOutput = true;
                                                        cmd.StartInfo = startInfo;
                                                        cmd.Start();
                                                        cmd.StandardInput.WriteLine(cmdCommandShow);
                                                        cmd.StandardInput.Flush();
                                                        cmd.StandardInput.Close();
                                                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                        IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, title);
                                                        StringBuilder stringBuilder = new StringBuilder(256);
                                                        UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                        if ((stringBuilder.ToString().Trim() != title))
                                                        {
                                                            if (mShow > 1) goto B;
                                                            mShow++;
                                                            cmd.Close();
                                                            goto BB;
                                                        }
                                                    B:
                                                        cmd.Close();
                                                        if (mShow > 1)
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                            }));
                                                        }
                                                        else
                                                        {
                                                            Invoke(new Action(() =>
                                                            {
                                                                dataGridView.Rows[vali].Selected = true;
                                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                            }));
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            BringToFront(hwnd_check);
                                            ShowWindow(hwnd_check, SW_RESTORE);
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[vali].Selected = true;
                                                Invoke(new Action(() =>
                                                {
                                                    dataGridView.Rows[vali].Selected = true;
                                                    dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                }));
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    string ListDevice = dataGridView.Rows[vali].Cells[2].Value.ToString();
                                    IntPtr hwnd_check = UnsafeNativeMethods.FindWindow(null, ListDevice);
                                    StringBuilder stringBuilder_check = new StringBuilder(256);
                                    UnsafeNativeMethods.GetWindowText(hwnd_check, stringBuilder_check, stringBuilder_check.Capacity);
                                    if (stringBuilder_check.ToString().Trim() != ListDevice)
                                    {
                                        try
                                        {
                                            if (cbHome.Checked)
                                            {
                                                Task.Run(() =>
                                                {
                                                    string cmd_CloseRecentApps = $"adb.exe -s {ListDevice} shell \n " +
                                                    $"input keyevent 3 \n" +
                                                    $"su \n" +
                                                    $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n";
                                                    Process cmd = new Process();
                                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                                    startInfo.FileName = "cmd.exe";
                                                    startInfo.CreateNoWindow = true;
                                                    startInfo.UseShellExecute = false;
                                                    startInfo.RedirectStandardInput = true;
                                                    startInfo.RedirectStandardOutput = true;
                                                    cmd.StartInfo = startInfo;
                                                    cmd.Start();
                                                    cmd.StandardInput.WriteLine(cmd_CloseRecentApps);
                                                    cmd.StandardInput.Flush();
                                                    cmd.StandardInput.Close();
                                                    cmd.WaitForExit();
                                                    cmd.Close();
                                                });

                                            }
                                            int width_screenWin = Screen.PrimaryScreen.Bounds.Width;
                                            int heigh_screenWin = Screen.PrimaryScreen.Bounds.Height;
                                            int width_scrcpy = (width_screenWin / 8) - 0;
                                            int heigh_scrcpy = (heigh_screenWin / 2) - 150;
                                            int Number_scrcpy = Process.GetProcessesByName("scrcpy").Length;
                                            int Px = ((Number_scrcpy % 8) * width_scrcpy);
                                            if ((Number_scrcpy / 8) == 0)
                                            {
                                                int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 30;
                                                if (Number_scrcpy < 16)
                                                {
                                                    int nShow = 0;
                                                AA:
                                                    Invoke(new Action(() =>
                                                    {
                                                        dataGridView.Rows[vali].Selected = true;
                                                        dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                    }));

                                                    string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {ListDevice} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                    Process cmd = new Process();
                                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                                    startInfo.FileName = "cmd.exe";
                                                    startInfo.CreateNoWindow = true;
                                                    startInfo.UseShellExecute = false;
                                                    startInfo.RedirectStandardInput = true;
                                                    startInfo.RedirectStandardOutput = true;
                                                    cmd.StartInfo = startInfo;
                                                    cmd.Start();
                                                    cmd.StandardInput.WriteLine(cmdCommandShow);
                                                    cmd.StandardInput.Flush();
                                                    cmd.StandardInput.Close();
                                                    Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                    IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, ListDevice);
                                                    StringBuilder stringBuilder = new StringBuilder(256);
                                                    UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                    if ((stringBuilder.ToString().Trim() != ListDevice))
                                                    {
                                                        if (nShow > 1) goto A;
                                                        nShow++;
                                                        cmd.Close();
                                                        goto AA;
                                                    }
                                                A:
                                                    cmd.Close();
                                                    if (nShow > 1)
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                        }));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                int Py = ((Number_scrcpy / 8) * heigh_scrcpy) + 65;
                                                if (Number_scrcpy < 16)
                                                {
                                                    int mShow = 0;
                                                BB:
                                                    Invoke(new Action(() =>
                                                    {
                                                        dataGridView.Rows[vali].Selected = true;
                                                        dataGridView.Rows[vali].Cells[7].Value = "Scrcpy => " + ListDevice;
                                                    }));

                                                    string cmdCommandShow = $"scrcpy.exe -s {ListDevice} --window-x {Px} --window-y {Py} --window-width {width_scrcpy} --window-height {heigh_scrcpy}  --window-title {ListDevice} -b{_BMR} -m{_MS} --max-fps {_FPS} --lock-video-orientation=0 --render-driver=opengl";
                                                    Process cmd = new Process();
                                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                                    startInfo.WorkingDirectory = @".\Data\Platform\";
                                                    startInfo.FileName = "cmd.exe";
                                                    startInfo.CreateNoWindow = true;
                                                    startInfo.UseShellExecute = false;
                                                    startInfo.RedirectStandardInput = true;
                                                    startInfo.RedirectStandardOutput = true;
                                                    cmd.StartInfo = startInfo;
                                                    cmd.Start();
                                                    cmd.StandardInput.WriteLine(cmdCommandShow);
                                                    cmd.StandardInput.Flush();
                                                    cmd.StandardInput.Close();
                                                    Thread.Sleep(TimeSpan.FromSeconds(int.Parse(nmr_delayscr.Value.ToString())));
                                                    IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, ListDevice);
                                                    StringBuilder stringBuilder = new StringBuilder(256);
                                                    UnsafeNativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
                                                    if ((stringBuilder.ToString().Trim() != ListDevice))
                                                    {
                                                        if (mShow > 1) goto B;
                                                        mShow++;
                                                        cmd.Close();
                                                        goto BB;
                                                    }
                                                B:
                                                    cmd.Close();
                                                    if (mShow > 1)
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Fail  !!!";
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        Invoke(new Action(() =>
                                                        {
                                                            dataGridView.Rows[vali].Selected = true;
                                                            dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                                        }));
                                                    }

                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        BringToFront(hwnd_check);
                                        ShowWindow(hwnd_check, SW_RESTORE);
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[vali].Selected = true;
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[vali].Selected = true;
                                                dataGridView.Rows[vali].Cells[7].Value = "Show " + ListDevice;
                                            }));
                                        }));
                                    }
                                }
                            }
                        }
                    }
                    Invoke(new Action(() =>
                    {
                        btnShows.Enabled = true;
                    }));
                });
            }
        }
        private void btnSavewipe_Click(object sender, EventArgs e)
        {
            Thread T_Savewipe = new Thread( () =>
            {
                Invoke(new Action(() =>
                {
                    if (!radDefault.Checked)
                    {
                        File.WriteAllText(ListWipe, txtWipe.Text);
                        txtWipe.Text = File.ReadAllText(ListWipe);
                    }
                    else
                    {
                        File.WriteAllText(ListDefault, txtWipe.Text);
                        txtWipe.Text = File.ReadAllText(ListDefault);
                    }
                }));

            });
            T_Savewipe.IsBackground = true;
            T_Savewipe.Start();
        }
        private void killADBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread T_KillADB = new Thread(() =>
            {
                foreach (var process in Process.GetProcessesByName("adb"))
                {
                    process.Kill();
                }
            });
            T_KillADB.IsBackground = true;
            T_KillADB.Start();
        }
        private async void btnGetpackage_Click(object sender, EventArgs e)
        {

        }
        private void btnAddpackage_Click(object sender, EventArgs e)
        {

        }
        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods

        {
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]

            internal static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", SetLastError = true)]

            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }
        private void btnAddURL_Click(object sender, EventArgs e)
        {
            string Site = txtUrl.Text.ToString();
            int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
            int val_index = index;
            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
            Thread T_Url = new Thread(async () =>
            {
                if (dataGridView.RowCount > 0)
                {
                    Task t = new Task( () =>
                    {
                        string cmd_url = $"adb.exe -s {ListDevice} shell  \n" +
                        $"pm clear org.bromite.bromite \n" +
                        $"pm clear com.android.chrome \n" +
                        $"am start -a  android.intent.action.VIEW -d {Site}";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmd_url);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Goto " + Site;
                        }));
                    });
                    t.Start();
                    await t;
                }
            });
            T_Url.IsBackground = true;
            T_Url.Start();
        }
        private static readonly HttpClient client = new HttpClient();
        private void txtUrl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            txtUrl.Clear();
        }
        private async void btngetLS_Click(object sender, EventArgs e)
        {
            btngetLS.Enabled = false;
            cbgetApps.Items.Clear();
            int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
            int val_index = index;
            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
            string cmdCommandGetpackage = $"adb.exe -s {ListDevice}  shell pm list packages -3";
            
            Task Packages = new Task(() =>
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.StartInfo.WorkingDirectory = @".\Data\Platform\";
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {cmdCommandGetpackage}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.Start();
                string packages = "";
                while (!process.HasExited)
                {
                    packages += process.StandardOutput.ReadToEnd();
                }
                packages = packages.Replace("package:", "");
                process.Close();
                File.WriteAllText(PathPackage, packages);
            });
            Packages.Start();
            await Packages;
            string[] linespackages = File.ReadAllLines(PathPackage);
            foreach (var linepackage in linespackages)
            {
                Task t = new Task(() =>
                {
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            string[] linesDefault = File.ReadAllLines(ListDefault);
                            foreach (string lineDefault in linesDefault)
                            {
                                if (linepackage == lineDefault)
                                {
                                    goto ednadd;
                                }
                            }
                        }
                        catch
                        {
                        }

                        cbgetApps.Items.Add(linepackage);
                    ednadd:
                        Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    }));
                });
                t.Start();
                await t;
            }
            btngetLS.Enabled = true;
        }
        private void btnopenone_Click(object sender, EventArgs e)
        {
             
            string PkgOnline = cbgetApps.SelectedItem.ToString();
            if (cb_Keep.Checked)
            {
                if (cbgetApps.SelectedIndex != -1)
                {
                    if (cb_Alldevices.Checked)
                    {
                        for (int i = 0; i < dataGridView.Rows.Count; i++)
                        {
                            int ii = i;
                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    if (!cb_Keep.Checked) return;
                                    if (Convert.ToBoolean(dataGridView.Rows[ii].Cells[0].Value) == true)
                                    {
                                        string ListDevice = dataGridView.Rows[ii].Cells[2].Value.ToString();

                                        string cmdCommand = $"adb.exe -s {ListDevice} shell \n " +
                                                $"su \n" +
                                                $"dumpsys activity activities | grep mResumedActivity |  cut -d \"{{\" -f2 | cut -d \" \" -f3 | cut -d \"/\" -f1 \n";
                                        string input = ExecuteCMD(cmdCommand);
                                        string pattern = $"(?<={PkgOnline})([^\\n]*\\n+)+";
                                        MatchCollection matchCollection = Regex.Matches(input, pattern, RegexOptions.Singleline);
                                        if (matchCollection.Count > 0)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[ii].Cells[7].Value = $"{PkgOnline} is active";
                                            }));
                                            Thread.Sleep(3000);
                                        }
                                        else
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[ii].Cells[7].Value = $"{PkgOnline} is opened";
                                            }));
                                            string cmdopenPK = $"adb.exe -s {ListDevice} shell  \n" +
                                                                                            $"su \n" +
                                                                                            $"monkey -p {PkgOnline} 1";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmdopenPK);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            cmd.WaitForExit();
                                            cmd.Close();
                                        }
                                    }
                                }
                            });
                        }
                        
                    }
                    else
                    {
                        if (cbgetApps.SelectedIndex != -1)
                        {

                            int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                            int val_index = index;
                            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();

                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    if (!cb_Keep.Checked) return;
                                    if (Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == true)
                                    {


                                        string cmdCommand = $"adb.exe -s {ListDevice} shell \n " +
                                                $"su \n" +
                                                $"dumpsys activity activities | grep mResumedActivity |  cut -d \"{{\" -f2 | cut -d \" \" -f3 | cut -d \"/\" -f1 \n";
                                        string input = ExecuteCMD(cmdCommand);
                                        string pattern = $"(?<={PkgOnline})([^\\n]*\\n+)+";
                                        MatchCollection matchCollection = Regex.Matches(input, pattern, RegexOptions.Singleline);
                                        if (matchCollection.Count > 0)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = $"{PkgOnline} is active";
                                            }));
                                            Thread.Sleep(3000);
                                        }
                                        else
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[val_index].Cells[7].Value = $"{PkgOnline} is opened";
                                            }));
                                            string cmdopenPK = $"adb.exe -s {ListDevice} shell  \n" +
                                                                                            $"su \n" +
                                                                                            $"monkey -p {PkgOnline} 1";
                                            Process cmd = new Process();
                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.WorkingDirectory = @".\Data\Platform\";
                                            startInfo.FileName = "cmd.exe";
                                            startInfo.CreateNoWindow = true;
                                            startInfo.UseShellExecute = false;
                                            startInfo.RedirectStandardInput = true;
                                            startInfo.RedirectStandardOutput = true;
                                            cmd.StartInfo = startInfo;
                                            cmd.Start();
                                            cmd.StandardInput.WriteLine(cmdopenPK);
                                            cmd.StandardInput.Flush();
                                            cmd.StandardInput.Close();
                                            cmd.WaitForExit();
                                            cmd.Close();
                                        }
                                    }
                                }
                            });
                            
                        }
                    }
                }
            }
            else
            {
                if (cbgetApps.SelectedIndex != -1)
                {
                    if (cb_Alldevices.Checked)
                    {
                        for (int i = 0; i < dataGridView.Rows.Count; i++)
                        {
                            int ii = i;
                            Task.Run(() =>
                            {

                                if (Convert.ToBoolean(dataGridView.Rows[ii].Cells[0].Value) == true)
                                {
                                    string ListDevice = dataGridView.Rows[ii].Cells[2].Value.ToString();
                                    string cmdCommand = $"adb.exe -s {ListDevice} shell \n " +
                                            $"su \n" +
                                            $"dumpsys activity activities | grep mResumedActivity |  cut -d \"{{\" -f2 | cut -d \" \" -f3 | cut -d \"/\" -f1 \n";
                                    string input = ExecuteCMD(cmdCommand);
                                    string pattern = $"(?<={PkgOnline})([^\\n]*\\n+)+";
                                    MatchCollection matchCollection = Regex.Matches(input, pattern, RegexOptions.Singleline);
                                    if (matchCollection.Count > 0)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[ii].Cells[7].Value = $"{PkgOnline} is active";
                                        }));
                                        Thread.Sleep(3000);
                                    }
                                    else
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[ii].Cells[7].Value = $"{PkgOnline} is opened";
                                        }));
                                        string cmdopenPK = $"adb.exe -s {ListDevice} shell  \n" +
                                                                                        $"su \n" +
                                                                                        $"monkey -p {PkgOnline} 1";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmdopenPK);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        cmd.WaitForExit();
                                        cmd.Close();
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        if (cbgetApps.SelectedIndex != -1)
                        {
                            int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                            int val_index = index;
                            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                            Task.Run(() =>
                            {
                                if (Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == true)
                                {
                                    string cmdCommand = $"adb.exe -s {ListDevice} shell \n " +
                                            $"su \n" +
                                            $"dumpsys activity activities | grep mResumedActivity |  cut -d \"{{\" -f2 | cut -d \" \" -f3 | cut -d \"/\" -f1 \n";
                                    string input = ExecuteCMD(cmdCommand);
                                    string pattern = $"(?<={PkgOnline})([^\\n]*\\n+)+";
                                    MatchCollection matchCollection = Regex.Matches(input, pattern, RegexOptions.Singleline);
                                    if (matchCollection.Count > 0)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = $"{PkgOnline} is active";
                                        }));
                                        Thread.Sleep(3000);
                                    }
                                    else
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = $"{PkgOnline} is opened";
                                        }));
                                        string cmdopenPK = $"adb.exe -s {ListDevice} shell  \n" +
                                                                                        $"su \n" +
                                                                                        $"monkey -p {PkgOnline} 1";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmdopenPK);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        cmd.WaitForExit();
                                        cmd.Close();
                                    }
                                }
                            });
                           
                        }
                    }
                }

            }

        }
        private void btaddall_Click(object sender, EventArgs e)
        {
            string _pkname = cbgetApps.Text.Trim();
            if (cb_Alldevices.Checked)
            {
                for (int i = 0; i < dataGridView.RowCount; i++)
                {
                    int ii = i;
                    if (Convert.ToBoolean(dataGridView.Rows[ii].Cells[0].Value) == true)
                    {
                        Task.Run(() =>
                        {
                            if (cbLS.Checked)
                            {
                                string DevIP = dataGridView.Rows[ii].Cells[2].Value.ToString();
                                string cmdrunadd = $"adb.exe -s {DevIP} shell  \n" +
                                                    $"su \n" +
                                                    $"/data/adb/lspd/bin/cli scope set -a app.adzchanger {_pkname}/0";
                                Process cmd = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.WorkingDirectory = @".\Data\Platform\";
                                startInfo.FileName = "cmd.exe";
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardInput = true;
                                startInfo.RedirectStandardOutput = true;
                                cmd.StartInfo = startInfo;
                                cmd.Start();
                                cmd.StandardInput.WriteLine(cmdrunadd);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                cmd.Close();
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[ii].Cells[7].Value = $"LS => {_pkname}";
                                }));
                            }
                            if (cbMagisk.Checked)
                            {
                                string DevIP = dataGridView.Rows[ii].Cells[2].Value.ToString();
                                string cmdrunadd = $"adb.exe -s {DevIP} shell  \n" +
                                                    $"su \n" +
                                                    $"magisk --denylist add {_pkname}";

                                Process cmd = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.WorkingDirectory = @".\Data\Platform\";
                                startInfo.FileName = "cmd.exe";
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardInput = true;
                                startInfo.RedirectStandardOutput = true;
                                cmd.StartInfo = startInfo;
                                cmd.Start();
                                cmd.StandardInput.WriteLine(cmdrunadd);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                cmd.Close();
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[ii].Cells[7].Value = $"Magisk => {_pkname}";
                                }));
                            }
                        });
                    }
                        
                }
            }
            else
            {
                int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                int val_index = index;
                string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                Task.Run(() =>
                {
                    if (cbLS.Checked)
                    {
                        string DevIP = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                        string cmdrunadd = $"adb.exe -s {DevIP} shell  \n" +
                                            $"su \n" +
                                            $"/data/adb/lspd/bin/cli scope set -a app.adzchanger {_pkname}/0";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdrunadd);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = $"LS => {_pkname}";
                        }));
                    }
                    if (cbMagisk.Checked)
                    {
                        string DevIP = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                        string cmdrunadd = $"adb.exe -s {DevIP} shell  \n" +
                                            $"su \n" +
                                            $"magisk --denylist add {_pkname}";

                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdrunadd);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = $"Magisk => {_pkname}";
                        }));
                    }
                });
            }

        }
        private void fullRebootToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Rebooting";
                    }));
                    
                    string cmd_Connect = $"adb.exe -s {ListDevice} reboot";
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    process.StartInfo.WorkingDirectory = @".\Data\Platform\";
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/C {cmd_Connect}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.Start();
                    if (!ListDevice.ToLower().Contains('.'))
                    {
                        while (Check_Conect(ListDevice))
                        {
                            Thread.Sleep(1000);
                        }
                        while (!Check_Conect(ListDevice))
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Booting...";
                            }));
                            Thread.Sleep(1000);
                        }
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                        }));
                    }
                    else
                    {
                        while (Check_ConectTCP(ListDevice))
                        {
                            Thread.Sleep(1000);
                        }
                        
                        while (!Check_ConectTCP(ListDevice))
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Booting...";
                            }));
                            Thread.Sleep(1000);
                        }
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                        }));
                    }
                }
                catch { }
            });
        }
        private void startTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(dataGridView.Rows.Count > 0)
            {
                int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                for(int i = 0;i< dataGridView.Rows.Count; i++)
                {
                    isTimer.Add(false);
                }
                int val_index = index;
                
                isTimer[val_index] = false;
                Thread timer = new Thread(() =>
                {
                    int i = 0;
                    if (Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == true)
                    {
                        while (true)
                        {
                            if (isTimer[val_index]) return;
                            int phut = i / 60;
                            int giay = i - (phut * 60);

                            string pPhut = "";
                            string gGiay = "";
                            if (phut < 10)
                            {
                                pPhut = "0" + phut.ToString();
                            }
                            else
                            {
                                pPhut = phut.ToString();
                            }
                            if (giay < 10)
                            {
                                gGiay = "0" + giay.ToString();
                            }
                            else
                            {
                                gGiay = giay.ToString();
                            }
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[13].Value = pPhut + ":" + gGiay;
                            }));
                            Thread.Sleep(1000);
                            i++;
                        }
                    }
                });
                timer.Start();
                timer.IsBackground = true;
                if (isTimer[val_index]) timer.Abort();
            }
        }
        private void btn_fullwipe_Click(object sender, EventArgs e)
        {
            string pack = txtb_default.Text.ToString();
            if(!cb_Alldevices.Checked)
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "full wipe...";
                    }));
                    Random RNG = new Random();
                    const string range = "abcde0123456789abcde0123456789";
                    var chars = Enumerable.Range(0, 16).Select(x => range[RNG.Next(0, range.Length)]);
                    string RandomID = new string(chars.ToArray());

                    Random SRN = new Random();
                    const string rangeSRN = "abcdef0123456789abcdef0123456789";
                    var SerialNo = Enumerable.Range(0, 12).Select(x => rangeSRN[SRN.Next(0, rangeSRN.Length)]);
                    string RandomSerialNo = new string(SerialNo.ToArray());

                    Random RILN = new Random();
                    const string rangeRILN = "QWERTYUIOPASDFGHJKLZXCVBNM0123456789";
                    var Ril_Number = Enumerable.Range(0, 6).Select(x => rangeRILN[RILN.Next(0, rangeRILN.Length)]);
                    string SerialRil_Number = new string(Ril_Number.ToArray());
                    string Ril = "R39H" + SerialRil_Number;
                    string cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                    $"settings put global development_settings_enabled 1 \n" +
                    $"settings put global adb_enabled 1 \n" +
                    $"input keyevent 3 \n" +
                    $"su \n" +
                    $"su -c  mount -o rw,remount / \n" +
                    $"su -c mv /sdcard/ZDATA/RRS /data/local/tmp/RRS \n" +
                    $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n" +
                    $"am force-stop {pack} \n" +
                    $"settings put secure android_id {RandomID} \n" +
                    $"rm -rf /data/data/com.google.android.gms/* \n" +
                    $"rm -rf /data/data/com.google.android.gsf/* \n" +
                    $"rm -rf /data/data/com.google.android.ims/* \n" +
                    $"rm -rf /data/data/com.android.vending/* \n" +
                    $"pm clear com.android.vending \n" +
                    $"pm clear com.google.android.gms \n" +
                    $"pm clear com.google.android.gsf \n" +
                    $"pm clear com.google.android.ims \n" +
                    $"pm clear com.google.ar.core \n" +
                    $"rm -f /data/system/users/0/settings_ssaid.xml \n" +
                    $"rm -f /data/data/com.google.android.gms/shared_prefs/adid_settings.xml \n" +
                    $"rm -rf /data/data/{pack}/* \n" +
                    $"rm -rf /data/system/netstats/* \n" +
                    $"rm -rf /sdcard/* \n" +
                    $"su -c mkdir -p /sdcard/ZDATA/RRS \n" +
                    $"su -c mv /data/local/tmp/RRS /sdcard/ZDATA/ \n" +
                    $"su -c  reboot";
                    Process cmd = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = @".\Data\Platform\";
                    startInfo.FileName = "cmd.exe";
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    cmd.StartInfo = startInfo;
                    cmd.Start();
                    cmd.StandardInput.WriteLine(cmdCommandChange);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    if (!ListDevice.ToLower().Contains('.'))
                    {
                        while (Check_Conect(ListDevice))
                        {
                            Thread.Sleep(1000);
                        }
                        while (!Check_Conect(ListDevice))
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Rebooting...";
                            }));
                            Thread.Sleep(1000);
                        }
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                        }));
                    }
                    else
                    {
                        while (Check_ConectTCP(ListDevice))
                        {
                            Thread.Sleep(1000);
                        }
                        while (!Check_ConectTCP(ListDevice))
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Rebooting...";
                            }));
                            Thread.Sleep(1000);
                        }
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                        }));
                    }
                });
            }
            else
            {
                for(int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    int val_index = i;
                    if(Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == true)
                    {
                        Task.Run(() =>
                        {
                            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "full wipe...";
                            }));
                            Random RNG = new Random();
                            const string range = "abcde0123456789abcde0123456789";
                            var chars = Enumerable.Range(0, 16).Select(x => range[RNG.Next(0, range.Length)]);
                            string RandomID = new string(chars.ToArray());

                            Random SRN = new Random();
                            const string rangeSRN = "abcdef0123456789abcdef0123456789";
                            var SerialNo = Enumerable.Range(0, 12).Select(x => rangeSRN[SRN.Next(0, rangeSRN.Length)]);
                            string RandomSerialNo = new string(SerialNo.ToArray());

                            Random RILN = new Random();
                            const string rangeRILN = "QWERTYUIOPASDFGHJKLZXCVBNM0123456789";
                            var Ril_Number = Enumerable.Range(0, 6).Select(x => rangeRILN[RILN.Next(0, rangeRILN.Length)]);
                            string SerialRil_Number = new string(Ril_Number.ToArray());
                            string Ril = "R39H" + SerialRil_Number;
                            string cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                            $"settings put global development_settings_enabled 1 \n" +
                            $"settings put global adb_enabled 1 \n" +
                            $"input keyevent 3 \n" +
                            $"su \n" +
                            $"su -c  mount -o rw,remount / \n" +
                            $"su -c mv /sdcard/ZDATA/RRS /data/local/tmp/RRS \n" +
                            $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done; \n" +
                            $"am force-stop {pack} \n" +
                            $"settings put secure android_id {RandomID} \n" +
                            $"rm -rf /data/data/com.google.android.gms/* \n" +
                            $"rm -rf /data/data/com.google.android.gsf/* \n" +
                            $"rm -rf /data/data/com.google.android.ims/* \n" +
                            $"rm -rf /data/data/com.android.vending/* \n" +
                            $"pm clear com.android.vending \n" +
                            $"pm clear com.google.android.gms \n" +
                            $"pm clear com.google.android.gsf \n" +
                            $"pm clear com.google.android.ims \n" +
                            $"pm clear com.google.ar.core \n" +
                            $"rm -f /data/system/users/0/settings_ssaid.xml \n" +
                            $"rm -f /data/data/com.google.android.gms/shared_prefs/adid_settings.xml \n" +
                            $"rm -rf /data/data/{pack}/* \n" +
                            $"rm -rf /data/system/netstats/* \n" +
                            $"rm -rf /sdcard/* \n" +
                            $"su -c mkdir -p /sdcard/ZDATA/RRS \n" +
                            $"su -c mv /data/local/tmp/RRS /sdcard/ZDATA/ \n" +
                            $"su -c  reboot";
                            Process cmd = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WorkingDirectory = @".\Data\Platform\";
                            startInfo.FileName = "cmd.exe";
                            startInfo.CreateNoWindow = true;
                            startInfo.UseShellExecute = false;
                            startInfo.RedirectStandardInput = true;
                            startInfo.RedirectStandardOutput = true;
                            cmd.StartInfo = startInfo;
                            cmd.Start();
                            cmd.StandardInput.WriteLine(cmdCommandChange);
                            cmd.StandardInput.Flush();
                            cmd.StandardInput.Close();
                            if (!ListDevice.ToLower().Contains('.'))
                            {
                                while (Check_Conect(ListDevice))
                                {
                                    Thread.Sleep(1000);
                                }
                                while (!Check_Conect(ListDevice))
                                {
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[val_index].Cells[7].Value = "Rebooting...";
                                    }));
                                    Thread.Sleep(1000);
                                }
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                                }));
                            }
                            else
                            {
                                while (Check_ConectTCP(ListDevice))
                                {
                                    Thread.Sleep(1000);
                                }
                                while (!Check_ConectTCP(ListDevice))
                                {
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[val_index].Cells[7].Value = "Rebooting...";
                                    }));
                                    Thread.Sleep(1000);
                                }
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                                }));
                            }
                        });
                    }
                }
            }
        }
        private void btn_Install_Click(object sender, EventArgs e)
        {
            try
            {
                if(dataGridView.SelectedRows.Count > 0)
                {
                    
                    OpenFileDialog choofdlog = new OpenFileDialog();
                    choofdlog.Filter = "Apk Files (*.apk)|*.apk";
                    choofdlog.FilterIndex = 1;
                    choofdlog.Multiselect = false;

                    if (choofdlog.ShowDialog() == DialogResult.OK)
                    {
                        string sFileName = choofdlog.FileName;
                        if (!cb_Alldevices.Checked)
                        {
                            int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                            int val_index = index;
                            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Install " + sFileName;
                            }));
                            Task.Run(() =>
                            {
                                string cmdCommandstop = $"adb.exe -s {ListDevice} install -r \"{sFileName}\"";
                                Process cmd = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.WorkingDirectory = @".\Data\Platform\";
                                startInfo.FileName = "cmd.exe";
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardInput = true;
                                startInfo.RedirectStandardOutput = true;
                                cmd.StartInfo = startInfo;
                                cmd.Start();
                                cmd.StandardInput.WriteLine(cmdCommandstop);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                int timer_install = 0;
                                string pPhut = "";
                                string gGiay = "";
                                while (!cmd.HasExited)
                                {
                                    Thread.Sleep(1000);
                                    int phut = timer_install / 60;
                                    int giay = timer_install - (phut * 60);

                                    
                                    if (phut < 10)
                                    {
                                        pPhut = "0" + phut.ToString();
                                    }
                                    else
                                    {
                                        pPhut = phut.ToString();
                                    }
                                    if (giay < 10)
                                    {
                                        gGiay = "0" + giay.ToString();
                                    }
                                    else
                                    {
                                        gGiay = giay.ToString();
                                    }
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[val_index].Cells[7].Value = "Installing " + pPhut+":"+gGiay;
                                    }));
                                    
                                    timer_install++;
                                }
                                
                                cmd.Close();
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = "Install success " + pPhut + ":" + gGiay;
                                }));
                            });
                        }
                        else
                        {
                            for(int i =0;i < dataGridView.Rows.Count; i++)
                            {
                                int ii = i;
                                if(Convert.ToBoolean(dataGridView.Rows[ii].Cells[0].Value) == true)
                                {
                                    string ListDevice = dataGridView.Rows[ii].Cells[2].Value.ToString();
                                    Invoke(new Action(() =>
                                    {
                                        dataGridView.Rows[ii].Cells[7].Value = "Install " + sFileName;
                                    }));
                                    Task.Run(() =>
                                    {
                                        string cmdCommandstop = $"adb.exe -s {ListDevice} install -r \"{sFileName}\"";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmdCommandstop);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        int timer_install = 0;
                                        string pPhut = "";
                                        string gGiay = "";
                                        while (!cmd.HasExited)
                                        {
                                            Thread.Sleep(1000);
                                            int phut = timer_install / 60;
                                            int giay = timer_install - (phut * 60);


                                            if (phut < 10)
                                            {
                                                pPhut = "0" + phut.ToString();
                                            }
                                            else
                                            {
                                                pPhut = phut.ToString();
                                            }
                                            if (giay < 10)
                                            {
                                                gGiay = "0" + giay.ToString();
                                            }
                                            else
                                            {
                                                gGiay = giay.ToString();
                                            }
                                            Invoke(new Action(() =>
                                            {
                                                dataGridView.Rows[ii].Cells[7].Value = "Installing " + pPhut + ":" + gGiay;
                                            }));

                                            timer_install++;
                                        }

                                        cmd.Close();
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[ii].Cells[7].Value = "Install success " + pPhut + ":" + gGiay;
                                        }));
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        private void btn_Default_Click(object sender, EventArgs e)
        {
            if(cbgetApps.SelectedIndex != -1)
            {
                txtb_default.Text = cbgetApps.SelectedItem.ToString();
                string PathKill = @".\Data\Kill.txt";
                File.WriteAllText(PathKill, txtb_default.Text.ToString());
            }
        }
        private void stopTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count > 0)
            {
                int i = 0;
                int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                int val_index = index;
                isTimer[val_index] = true;
            }
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView.Focus();
        }
        private void btn_Fix_Click(object sender, EventArgs e)
        {
            string pack = txtb_default.Text.ToString();
           
            if (!cb_Alldevices.Checked)
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "fixing...";
                    }));
                    Random RNG = new Random();
                    const string range = "abcde0123456789abcde0123456789";
                    var chars = Enumerable.Range(0, 16).Select(x => range[RNG.Next(0, range.Length)]);
                    string RandomID = new string(chars.ToArray());

                    Random SRN = new Random();
                    const string rangeSRN = "abcdef0123456789abcdef0123456789";
                    var SerialNo = Enumerable.Range(0, 12).Select(x => rangeSRN[SRN.Next(0, rangeSRN.Length)]);
                    string RandomSerialNo = new string(SerialNo.ToArray());

                    Random RILN = new Random();
                    const string rangeRILN = "QWERTYUIOPASDFGHJKLZXCVBNM0123456789";
                    var Ril_Number = Enumerable.Range(0, 6).Select(x => rangeRILN[RILN.Next(0, rangeRILN.Length)]);
                    string SerialRil_Number = new string(Ril_Number.ToArray());
                    string Ril = "R39H" + SerialRil_Number;
                    if (cb_usage.Checked)
                    {
                        string cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                        $"input keyevent 3 \n" +
                        $"su \n" +
                        $"su -c  mount -o rw,remount / \n" +
                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done;" +
                        $"am force-stop {pack} \n" +
                        $"settings put secure android_id {RandomID} \n" +
                        $"rm -rf /data/data/{pack}/* \n" +
                        $"appops set {pack} GET_USAGE_STATS deny";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdCommandChange);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Clear usage acccess";
                        }));
                    }
                    if (cb_wipeGG.Checked)
                    {
                        string cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                        $"input keyevent 3 \n" +
                        $"su \n" +
                        $"su -c  mount -o rw,remount / \n" +
                        $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done;" +
                        $"am force-stop {pack} \n" +
                        $"settings put secure android_id {RandomID} \n" +
                        $"rm -rf /data/data/com.google.android.gms/* \n" +
                        $"rm -rf /data/data/com.google.android.gsf/* \n" +
                        $"rm -rf /data/data/com.google.android.ims/* \n" +
                        $"rm -rf /data/data/com.android.vending/* \n" +
                        $"pm clear com.android.vending \n" +
                        $"pm clear com.google.android.gms \n" +
                        $"pm clear com.google.android.gsf \n" +
                        $"pm clear com.google.android.ims \n" +
                        $"rm -rf /data/data/{pack}/* \n";
                        Process cmd = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = @".\Data\Platform\";
                        startInfo.FileName = "cmd.exe";
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        cmd.StartInfo = startInfo;
                        cmd.Start();
                        cmd.StandardInput.WriteLine(cmdCommandChange);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.Close();
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Wipe google";
                        }));
                    }
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Fix done";
                    }));
                });
            }
            else
            {
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    int val_index = i;
                    if (Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == true)
                    {
                        Task.Run(() =>
                        {
                            string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "fixing...";
                            }));
                            Random RNG = new Random();
                            const string range = "abcde0123456789abcde0123456789";
                            var chars = Enumerable.Range(0, 16).Select(x => range[RNG.Next(0, range.Length)]);
                            string RandomID = new string(chars.ToArray());

                            Random SRN = new Random();
                            const string rangeSRN = "abcdef0123456789abcdef0123456789";
                            var SerialNo = Enumerable.Range(0, 12).Select(x => rangeSRN[SRN.Next(0, rangeSRN.Length)]);
                            string RandomSerialNo = new string(SerialNo.ToArray());

                            Random RILN = new Random();
                            const string rangeRILN = "QWERTYUIOPASDFGHJKLZXCVBNM0123456789";
                            var Ril_Number = Enumerable.Range(0, 6).Select(x => rangeRILN[RILN.Next(0, rangeRILN.Length)]);
                            string SerialRil_Number = new string(Ril_Number.ToArray());
                            string Ril = "R39H" + SerialRil_Number;
                            if (cb_usage.Checked)
                            {
                                string cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                                $"input keyevent 3 \n" +
                                $"su \n" +
                                $"su -c  mount -o rw,remount / \n" +
                                $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done;" +
                                $"am force-stop {pack} \n" +
                                $"settings put secure android_id {RandomID} \n" +
                                $"rm -rf /data/data/{pack}/* \n" +
                                $"appops set {pack} GET_USAGE_STATS deny";
                                Process cmd = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.WorkingDirectory = @".\Data\Platform\";
                                startInfo.FileName = "cmd.exe";
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardInput = true;
                                startInfo.RedirectStandardOutput = true;
                                cmd.StartInfo = startInfo;
                                cmd.Start();
                                cmd.StandardInput.WriteLine(cmdCommandChange);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                cmd.Close();
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = "Clear usage acccess";
                                }));
                            }
                            if (cb_wipeGG.Checked)
                            {
                                string cmdCommandChange = $"adb.exe -s {ListDevice} shell \n" +
                                $"input keyevent 3 \n" +
                                $"su \n" +
                                $"su -c  mount -o rw,remount / \n" +
                                $"Apps=$(dumpsys window a | grep \"/\" | cut -d \"{{\" -f2 | cut -d \"/\" -f1 | cut -d \" \" -f2); for App in $Apps; do am force-stop $App; done;" +
                                $"am force-stop {pack} \n" +
                                $"settings put secure android_id {RandomID} \n" +
                                $"rm -rf /data/data/com.google.android.gms/* \n" +
                                $"rm -rf /data/data/com.google.android.gsf/* \n" +
                                $"rm -rf /data/data/com.google.android.ims/* \n" +
                                $"rm -rf /data/data/com.android.vending/* \n" +
                                $"pm clear com.android.vending \n" +
                                $"pm clear com.google.android.gms \n" +
                                $"pm clear com.google.android.gsf \n" +
                                $"pm clear com.google.android.ims \n" +
                                $"rm -rf /data/data/{pack}/* \n";
                                Process cmd = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.WorkingDirectory = @".\Data\Platform\";
                                startInfo.FileName = "cmd.exe";
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardInput = true;
                                startInfo.RedirectStandardOutput = true;
                                cmd.StartInfo = startInfo;
                                cmd.Start();
                                cmd.StandardInput.WriteLine(cmdCommandChange);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                cmd.Close();
                                Invoke(new Action(() =>
                                {
                                    dataGridView.Rows[val_index].Cells[7].Value = "Wipe google";
                                }));
                            }
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = "Fix done";
                            }));
                        });
                    }
                }
            }
        }
        private void settupEZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                int val_index = index;
                string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                if (!ListDevice.ToLower().Contains('.') && Check_Conect(ListDevice))
                {
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "setup EzTool....";
                    }));
                    string cmdmove =
                    $"adb.exe -s {ListDevice} push adbwifi /sdcard/ \n" +
                    $"adb.exe -s {ListDevice} shell  \n" +
                    $"settings put global development_settings_enabled 1 \n" +
                    $"settings put global adb_enabled 1 \n" +
                    $"su \n" +
                    $"su -c  mount -o rw,remount / \n" +
                    $"mv -f /sdcard/adbwifi /data/adb/post-fs-data.d/adbwifi \n" +
                    $"chmod 0755 /data/adb/post-fs-data.d/adbwifi \n" +
                    $"chmod 0755 /data/adb/modules/zygisk_shamiko/service.sh \n" +
                    $"rm -rf /product/app/Photos/* \n" +
                    $"rm -rf /product/priv-app/GoogleFeedback/* \n" +
                    $"rm -rf /product/priv-app/RecorderPrebuilt/* \n" +
                    $"rm -rf /product/app/Chrome/* \n" +
                    $"rm -rf /product/priv-app/Velvet/* \n" +
                    $"rm -rr /product/app/Photos \n" +
                    $"rm -rr /product/app/PrebuiltDeskClockGoogle \n" +
                    $"rm -rr /product/priv-app/GoogleFeedback \n" +
                    $"rm -rr /product/priv-app/RecorderPrebuilt \n" +
                    $"rm -rr /product/app/Chrome \n" +
                    $"rm -rr /product/priv-app/Velvet \n" +
                    $"su -c  reboot";
                    Process cmd = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = @".\Data\Platform\";
                    startInfo.FileName = "cmd.exe";
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    cmd.StartInfo = startInfo;
                    cmd.Start();
                    cmd.StandardInput.WriteLine(cmdmove);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    while (Check_Conect(ListDevice))
                    {
                        Thread.Sleep(1000);
                    }

                    int i = 0;
                    while (!Check_Conect(ListDevice))
                    {
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Rebooting..." + i;
                        }));
                        Thread.Sleep(1000);
                        i++;
                    }
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                    }));
                }
                if (ListDevice.ToLower().Contains('.') && Check_ConectTCP(ListDevice))
                {
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "setup EzTool....";
                    }));
                    string cmdmove = $"adb.exe -s {ListDevice} push adbwifi /sdcard/ \n" +
                    $"adb.exe -s {ListDevice} shell  \n" +
                    $"settings put global development_settings_enabled 1 \n" +
                    $"settings put global adb_enabled 1 \n" +
                    $"su \n" +
                    $"su -c  mount -o rw,remount / \n" +
                    $"mv -f /sdcard/adbwifi /data/adb/post-fs-data.d/adbwifi \n" +
                    $"chmod 0755 /data/adb/post-fs-data.d/adbwifi \n" +
                    $"rm -rf /product/app/Photos/* \n" +
                    $"rm -rf /product/priv-app/GoogleFeedback/* \n" +
                    $"rm -rf /product/priv-app/RecorderPrebuilt/* \n" +
                    $"rm -rr /product/app/Photos \n" +
                    $"rm -rr /product/app/PrebuiltDeskClockGoogle \n" +
                    $"rm -rr /product/priv-app/GoogleFeedback \n" +
                    $"rm -rr /product/priv-app/RecorderPrebuilt \n" +
                    $"su -c  reboot";
                    Process cmd = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = @".\Data\Platform\";
                    startInfo.FileName = "cmd.exe";
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    cmd.StartInfo = startInfo;
                    cmd.Start();
                    cmd.StandardInput.WriteLine(cmdmove);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    while (Check_ConectTCP(ListDevice))
                    {
                        Thread.Sleep(1000);
                    }
                    while (!Check_ConectTCP(ListDevice))
                    {
                        Invoke(new Action(() =>
                        {
                            dataGridView.Rows[val_index].Cells[7].Value = "Rebooting...";
                        }));
                        Thread.Sleep(1000);
                    }
                    Invoke(new Action(() =>
                    {
                        dataGridView.Rows[val_index].Cells[7].Value = "Rebooted";
                    }));
                }
            });
        }
        private void btn_sendtext_Click(object sender, EventArgs e)
        {
            if (cb_Alldevices.Checked)
            {
                Thread T = new Thread(() =>
                {
                    for (int i = 0; i < dataGridView.Rows.Count; i++)
                    {
                        int val_index = i;
                        Task.Run(() =>
                        {
                            if (Convert.ToBoolean(dataGridView.Rows[val_index].Cells[0].Value) == true)
                            {
                                string[] lines = txtbox_sendtext.Lines;
                                string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                                for (int ii = 0; ii < lines.Length; ii++)
                                {
                                    int j = ii;
                                    if (lines[j] != "")
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            dataGridView.Rows[val_index].Cells[7].Value = $"{lines[j]}";
                                        }));
                                        string cmd_command = $"adb.exe -s {ListDevice} shell \n" +
                                        $"su \n" +
                                        $"su -c  mount -o rw,remount / \n" +
                                        $"{lines[j]}";
                                        Process cmd = new Process();
                                        ProcessStartInfo startInfo = new ProcessStartInfo();
                                        startInfo.WorkingDirectory = @".\Data\Platform\";
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.CreateNoWindow = true;
                                        startInfo.UseShellExecute = false;
                                        startInfo.RedirectStandardInput = true;
                                        startInfo.RedirectStandardOutput = true;
                                        cmd.StartInfo = startInfo;
                                        cmd.Start();
                                        cmd.StandardInput.WriteLine(cmd_command);
                                        cmd.StandardInput.Flush();
                                        cmd.StandardInput.Close();
                                        cmd.WaitForExit();
                                        cmd.Close();
                                    }
                                }
                            }
                        });
                    }
                });
                T.Start();
                T.IsBackground = true;
            }
            else
            {
                Task.Run(() =>
                {
                    int index = dataGridView.Rows.IndexOf(dataGridView.SelectedRows[0]);
                    int val_index = index;
                    string ListDevice = dataGridView.Rows[val_index].Cells[2].Value.ToString();
                    string[] lines = txtbox_sendtext.Lines;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        int j = i;
                        if (lines[j] != "")
                        {
                            Invoke(new Action(() =>
                            {
                                dataGridView.Rows[val_index].Cells[7].Value = $"{lines[j]}";
                            }));
                            string cmd_command = $"adb.exe -s {ListDevice} shell \n" +
                            $"su \n" +
                            $"su -c  mount -o rw,remount / \n" +
                            $"{lines[j]}";
                            Process cmd = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WorkingDirectory = @".\Data\Platform\";
                            startInfo.FileName = "cmd.exe";
                            startInfo.CreateNoWindow = true;
                            startInfo.UseShellExecute = false;
                            startInfo.RedirectStandardInput = true;
                            startInfo.RedirectStandardOutput = true;
                            cmd.StartInfo = startInfo;
                            cmd.Start();
                            cmd.StandardInput.WriteLine(cmd_command);
                            cmd.StandardInput.Flush();
                            cmd.StandardInput.Close();
                            cmd.WaitForExit();
                            cmd.Close();
                        }

                    }
                });
            }
        }
        private void btn_savecommand_Click(object sender, EventArgs e)
        {
            string[] lines = txtbox_sendtext.Lines;
            if (lines !=null)
            {
                File.WriteAllText(path_command, "");
                for (int i = 0; i < lines.Length; i++)
                {
                    int j = i;
                    StreamWriter writer = new StreamWriter(path_command, true);
                    writer.WriteLine(lines[j]);
                    writer.Dispose();
                }
            }
        }

        private void toolStripUpdatedevices_Click(object sender, EventArgs e)
        {
            string PathListDevices = @".\Data\ListDevices\ListDevices.txt";
            if (dataGridView.RowCount > 0)
            {
                File.WriteAllText(PathListDevices, "");
                for (int i = 0; i < dataGridView.RowCount; i++)
                {
                    int j = i;
                    string Ip = dataGridView.Rows[j].Cells[2].Value.ToString();
                    string Host = dataGridView.Rows[j].Cells[3].Value.ToString();
                    string Port = dataGridView.Rows[j].Cells[4].Value.ToString();
                    string Note = dataGridView.Rows[j].Cells[5].Value.ToString();
                    StreamWriter writer = new StreamWriter(PathListDevices, true);
                    writer.WriteLine(Ip + "|" + Host + "|" + Port + "|" + Note);
                    writer.Dispose();
                }
            }
        }

        private void btn_addwipe_Click(object sender, EventArgs e)
        {
            StreamWriter writer = new StreamWriter(ListWipe, true);
            writer.WriteLine(cbgetApps.SelectedItem.ToString().Trim());
            writer.Dispose();
        }

        private void btn_adddefault_Click(object sender, EventArgs e)
        {
            StreamWriter writer = new StreamWriter(ListDefault, true);
            writer.WriteLine(cbgetApps.SelectedItem.ToString().Trim());
            writer.Dispose();
        }
    }
    
}
