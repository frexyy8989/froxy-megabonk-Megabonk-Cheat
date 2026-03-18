using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Froxy_Megabonk
{
    public class FlatComboBox : ComboBox
    {
        private Color _borderColor = Color.Gray;

        public FlatComboBox()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            BackColor = Color.FromArgb(25, 25, 25);
            ForeColor = Color.White;
            FlatStyle = FlatStyle.Flat;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var brush = new SolidBrush(BackColor)) {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            string text = SelectedItem != null ? SelectedItem.ToString() : "";
            TextRenderer.DrawText(e.Graphics, text, Font, ClientRectangle, ForeColor, BackColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            using (var pen = new Pen(_borderColor, 1)) {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            using (SolidBrush textBrush = new SolidBrush(ForeColor)) {
                e.Graphics.DrawString(Items[e.Index].ToString(), Font, textBrush, e.Bounds);
            }
        }
    }

    public partial class MainForm : Form
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private IntPtr _processHandle = IntPtr.Zero;
        private IntPtr _gameAssemblyAddress = IntPtr.Zero;
        private IntPtr _unityPlayerAddress = IntPtr.Zero;
        
        private TextBox txtSilver, txtGold, txtHealth, txtMaxHealth;
        private Button btnSilver, btnGold, btnHealth, btnMaxHealth, btnAttach;
        private CheckBox chkGodMode;
        private FlatComboBox cmbGodModeType;
        private Label lblStatus;
        private Timer godModeTimer, healthLoopTimer, maxHealthLoopTimer;

        public MainForm()
        {
            InitializeFormSettings();
            SetupInterface();
            SetupTimers();
            this.Load += new EventHandler(MainFormLoad);
            this.Icon = new System.Drawing.Icon("icon.ico");
        }

        private void InitializeFormSettings()
        {
            this.Text = "Froxy-Megabonk v1";
            this.Size = new Size(410, 420); 
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
		    base.OnFormClosing(e);
		    Environment.Exit(0);
		}
        private void SetupInterface()
        {
            Label lblLogo = new Label { Text = "FROXY-MEGABONK", Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold), ForeColor = Color.FromArgb(0, 255, 255), Location = new Point(0, 10), Size = new Size(410, 35), TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(lblLogo);

            AddHackRow(60, "Silver:", Color.FromArgb(35, 35, 35), out txtSilver, out btnSilver, new EventHandler((s, e) => ApplySilverHack()));
            AddHackRow(105, "Gold:", Color.FromArgb(184, 134, 11), out txtGold, out btnGold, new EventHandler((s, e) => ApplyGoldHack()));
            AddHackRow(150, "Health:", Color.FromArgb(139, 0, 0), out txtHealth, out btnHealth, new EventHandler(ToggleHealthLoop));
            btnHealth.Text = "OFF";
            AddHackRow(195, "Max HP:", Color.FromArgb(139, 0, 0), out txtMaxHealth, out btnMaxHealth, new EventHandler(ToggleMaxHealthLoop));
            btnMaxHealth.Text = "OFF";

            Label lblGod = new Label { Text = "God Mode:", Location = new Point(25, 250), Size = new Size(70, 30), TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.LightGray };
            cmbGodModeType = new FlatComboBox { Location = new Point(100, 252), Size = new Size(140, 25) };
            cmbGodModeType.Items.AddRange(new string[] { "Alternatif 1", "Alternatif 2" });
            cmbGodModeType.SelectedIndex = 0;

            chkGodMode = new CheckBox { Text = "OFF", Appearance = Appearance.Button, FlatStyle = FlatStyle.Flat, Location = new Point(250, 252), Size = new Size(115, 26), BackColor = Color.FromArgb(40, 40, 40), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            chkGodMode.FlatAppearance.BorderColor = Color.Gray; 
            chkGodMode.FlatAppearance.BorderSize = 1;
            chkGodMode.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 20);
            chkGodMode.TabStop = false;
            chkGodMode.CheckedChanged += new EventHandler(ChkGodMode_CheckedChanged);

            this.Controls.Add(lblGod); 
            this.Controls.Add(cmbGodModeType); 
            this.Controls.Add(chkGodMode);

            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(10, 10, 10) };
            lblStatus = new Label { Text = "DURUM: Bekleniyor...", Location = new Point(5, 10), Size = new Size(250, 20), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };
            btnAttach = new Button { Text = "ATTACH", Location = new Point(310, 7), Size = new Size(80, 26), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 8, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAttach.FlatAppearance.BorderColor = Color.Gray;
            btnAttach.FlatAppearance.BorderSize = 1;
            btnAttach.FlatAppearance.MouseDownBackColor = Color.FromArgb(15, 15, 15);
            btnAttach.TabStop = false;
            btnAttach.Click += new EventHandler((s, e) => attach());
            
            pnlBottom.Controls.Add(lblStatus); pnlBottom.Controls.Add(btnAttach);
            this.Controls.Add(pnlBottom);
        }

        private void AddHackRow(int y, string label, Color c, out TextBox tb, out Button b, EventHandler ev) {
            Label lbl = new Label { Text = label, Location = new Point(25, y + 5), Size = new Size(70, 25), ForeColor = Color.LightGray };
            
            tb = new TextBox { 
                Location = new Point(100, y), 
                Size = new Size(140, 25), 
                BackColor = Color.FromArgb(25, 25, 25), 
                ForeColor = Color.White, 
                BorderStyle = BorderStyle.FixedSingle 
            };
            
            b = new Button { Text = "UYGULA", Location = new Point(250, y - 1), Size = new Size(115, 28), FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White };
            b.FlatAppearance.BorderColor = Color.Gray; 
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb((int)(c.R * 0.7), (int)(c.G * 0.7), (int)(c.B * 0.7));
            b.TabStop = false;
            b.Click += ev;
            
            this.Controls.Add(lbl); this.Controls.Add(tb); this.Controls.Add(b);
        }

        private void SetupTimers() { godModeTimer = new Timer { Interval = 100 }; godModeTimer.Tick += (s, e) => RunGodModeLogic(); healthLoopTimer = new Timer { Interval = 5000 }; healthLoopTimer.Tick += (s, e) => ApplyHealthHack(); maxHealthLoopTimer = new Timer { Interval = 5000 }; maxHealthLoopTimer.Tick += (s, e) => ApplyMaxHealthHack(); }
        private void RunGodModeLogic() { if (_processHandle == IntPtr.Zero) return; int bw; if (cmbGodModeType.SelectedIndex == 0) { int[] o1 = { 0x40, 0xB8, 0x8, 0x78, 0x40, 0x160, 0x4BC }; IntPtr f1 = GetFinalAddress((IntPtr)((long)_gameAssemblyAddress + 0x02F83018), o1); if (f1 != IntPtr.Zero) WriteProcessMemory(_processHandle, f1, BitConverter.GetBytes(0), 4, out bw); } else { int[] o2 = { 0x4C8, 0x10, 0x0, 0x10, 0x18, 0x0, 0x13C }; IntPtr f2 = GetFinalAddress((IntPtr)((long)_unityPlayerAddress + 0x01C6A7C8), o2); if (f2 != IntPtr.Zero) WriteProcessMemory(_processHandle, f2, BitConverter.GetBytes(0), 4, out bw); } }
        private void ApplySilverHack() { try { int[] o = { 0x50, 0x40, 0xB8, 0x0, 0x100, 0x30, 0x14 }; IntPtr f = GetFinalAddress((IntPtr)((long)_gameAssemblyAddress + 0x02F840B8), o); int val; if(int.TryParse(txtSilver.Text, out val)) { byte[] buf = new byte[4]; int br; ReadProcessMemory(_processHandle, f, buf, 4, out br); int bw; WriteProcessMemory(_processHandle, f, BitConverter.GetBytes(BitConverter.ToInt32(buf, 0) + val), 4, out bw); } } catch { } }
        private void ApplyGoldHack() { try { int[] o = { 0x40, 0x80, 0x30, 0xB8, 0x68, 0x90, 0x68 }; IntPtr f = GetFinalAddress((IntPtr)((long)_gameAssemblyAddress + 0x02F77F00), o); float val; if(float.TryParse(txtGold.Text, out val)) { byte[] buf = new byte[4]; int br; ReadProcessMemory(_processHandle, f, buf, 4, out br); int bw; WriteProcessMemory(_processHandle, f, BitConverter.GetBytes(BitConverter.ToSingle(buf, 0) + val), 4, out bw); } } catch { } }
        private void ApplyHealthHack() { try { int[] o = { 0x40, 0xB8, 0x0, 0x78, 0x48, 0x40, 0x10 }; IntPtr f = GetFinalAddress((IntPtr)((long)_gameAssemblyAddress + 0x02F6B020), o); int val; if(int.TryParse(txtHealth.Text, out val)) { int bw; WriteProcessMemory(_processHandle, f, BitConverter.GetBytes(val), 4, out bw); } } catch { } }
        private void ApplyMaxHealthHack() { try { int[] o = { 0x20, 0xB8, 0x30, 0x78, 0x48, 0x20, 0x14 }; IntPtr f = GetFinalAddress((IntPtr)((long)_gameAssemblyAddress + 0x02F60EA8), o); int val; if(int.TryParse(txtMaxHealth.Text, out val)) { int bw; WriteProcessMemory(_processHandle, f, BitConverter.GetBytes(val), 4, out bw); } } catch { } }
        private void ToggleHealthLoop(object s, EventArgs e) { if (healthLoopTimer.Enabled) { healthLoopTimer.Stop(); btnHealth.Text = "OFF"; btnHealth.BackColor = Color.FromArgb(139, 0, 0); } else { ApplyHealthHack(); healthLoopTimer.Start(); btnHealth.Text = "ON"; btnHealth.BackColor = Color.Green; } }
        private void ToggleMaxHealthLoop(object s, EventArgs e) { if (maxHealthLoopTimer.Enabled) { maxHealthLoopTimer.Stop(); btnMaxHealth.Text = "OFF"; btnMaxHealth.BackColor = Color.FromArgb(139, 0, 0); } else { ApplyMaxHealthHack(); maxHealthLoopTimer.Start(); btnMaxHealth.Text = "ON"; btnMaxHealth.BackColor = Color.Green; } }
        private void ChkGodMode_CheckedChanged(object s, EventArgs e) { if (chkGodMode.Checked) { godModeTimer.Start(); chkGodMode.Text = "ON"; chkGodMode.BackColor = Color.Green; } else { godModeTimer.Stop(); chkGodMode.Text = "OFF"; chkGodMode.BackColor = Color.FromArgb(40, 40, 40); } }
        private IntPtr GetFinalAddress(IntPtr bAddr, int[] offsets) { byte[] b = new byte[8]; int br; if (!ReadProcessMemory(_processHandle, bAddr, b, 8, out br)) return IntPtr.Zero; IntPtr cur = (IntPtr)BitConverter.ToInt64(b, 0); for (int i = 0; i < offsets.Length - 1; i++) { if (cur == IntPtr.Zero) return IntPtr.Zero; if (!ReadProcessMemory(_processHandle, (IntPtr)((long)cur + offsets[i]), b, 8, out br)) return IntPtr.Zero; cur = (IntPtr)BitConverter.ToInt64(b, 0); } return (IntPtr)((long)cur + offsets[offsets.Length - 1]); }
        void attach() { Process[] procs = Process.GetProcessesByName("Megabonk"); if (procs.Length == 0) { lblStatus.ForeColor = Color.Orange; lblStatus.Text = "DURUM: Oyun Bulunamadı!"; return; } _processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, procs[0].Id); _gameAssemblyAddress = IntPtr.Zero; _unityPlayerAddress = IntPtr.Zero; foreach (ProcessModule m in procs[0].Modules) { if (m.ModuleName == "GameAssembly.dll") _gameAssemblyAddress = m.BaseAddress; if (m.ModuleName == "UnityPlayer.dll") _unityPlayerAddress = m.BaseAddress; } if (_gameAssemblyAddress != IntPtr.Zero) { lblStatus.ForeColor = Color.LimeGreen; lblStatus.Text = "DURUM: Bağlantı Tamam!"; } }
        void MainFormLoad(object s, EventArgs e) { attach(); }
    }
}