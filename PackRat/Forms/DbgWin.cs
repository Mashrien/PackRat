using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using PackRat;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace PackRatUI {
    public partial class DbgWin : Form {
        private Graphics lvGraphics;
        private static int longestString = 1;

        private List<LogQueueItem> LogQueue;
        private List<LogQueueItem> LogQueueBusy;
        private Timer logTimer;
        private Timer gcTimer;
        private static bool processing;

        public static long memSize=0;

        public DbgWin() {
            InitializeComponent();
            logTimer = new Timer();
            gcTimer = new Timer();
            LogQueue = new List<LogQueueItem>();
            LogQueueBusy = new List<LogQueueItem>();
            logTimer.Interval = 25;
            gcTimer.Interval = 500;
            //gcTimer.Tick += delegate { GC.Collect(3); };
            logTimer.Tick += LogTimer_Tick;
            logTimer.Start();
            gcTimer.Start();
            lvGraphics = listView1.CreateGraphics();

            }

        private void LogTimer_Tick(object sender, EventArgs e) {

            if (LogQueue.Count == 0) {
                memSize = GC.GetTotalMemory(false);
                this.Text = $"DbgWin (Timer: {logTimer.Enabled}) {Utils.SizeSuffix(memSize)}";
                return;
                }
            else {
                memSize = GC.GetTotalMemory(true);
                this.Text = $"DbgWin (Timer: {logTimer.Enabled}) {Utils.SizeSuffix(memSize)}";
                logTimer.Stop();
                processing = true;
                }

            List<LogQueueItem> tempList = new List<LogQueueItem>();
            int c = 0;
            for (int i = 0; i < 50; i++) {
                if (i > LogQueue.Count - 1)
                    break;
                c++; //lol
                tempList.Add(LogQueue[i]);
                }
            LogQueue.RemoveRange(0, c);
            foreach (LogQueueItem li in tempList) {
                LogFromQueue(li.text, li.type, li.caller);
                }
            tempList.Clear();
            logTimer.Start(); processing = false;
            }

        public static void Log(string line, LogType lt) {

            StackFrame[] sf = new StackTrace().GetFrames();
            string callerName = "";
            string className = "";
            for (int i = 1; i < sf.Count() - 1; i++) {
                StackFrame f = sf[i];
                if (f.GetMethod().Name == ".ctor") {
                    callerName = f.GetMethod().ReflectedType.Name;
                    className = f.GetMethod().ReflectedType.FullName;
                    if (callerName.ToUpper() == "LOG")
                        continue;
                    break;
                    }
                if (f.GetMethod().Name == "Log")
                    continue;
                callerName = f.GetMethod().Name;
                className = f.GetMethod().ReflectedType.FullName;
                if (callerName.ToUpper() == "LOG")
                    continue;
                break;
                }

            className = className.Remove(0, className.IndexOf('.') + 1);
            string memberName = className + "->" + callerName + "";

            if (processing) {
                Program.LogWin.LogQueueBusy.Add(new LogQueueItem(line, lt, memberName));
                return;
                }
            else {
                if (Program.LogWin.LogQueueBusy.Count != 0) {
                    foreach (LogQueueItem li in Program.LogWin.LogQueueBusy) {
                        Program.LogWin.LogQueue.Add(li);
                        }
                    Program.LogWin.LogQueueBusy.Clear();
                    }
                }

            Program.LogWin.LogQueue.Add(new LogQueueItem(line, lt, memberName));
            }

        public static void LogFromQueue(string line, LogType lt, string memberName) {
            ListViewItem lvi = null;
            int newMaxLen = memberName.Length;
            string[] lines = line.Split(Environment.NewLine.ToCharArray());
            string ltName = lt.ToString();
            Color foreColor = LTtoColor(lt);

            foreach (string l in lines) {
                if (l.Length == 0) continue;
                List<string> croppedLines = Regex.Split(l, @"(?:(.{1,56})(?:\s|$)|(.{56}))")
                    .Where(x => x.Length > 0)
                    .ToList();
                foreach (string cl in croppedLines) {
                    lvi = new ListViewItem(new string[] { ltName, memberName, cl });
                    lvi.UseItemStyleForSubItems = true;
                    lvi.ForeColor = foreColor;
                    Trace.WriteLine(cl);
                    Program.LogWin.Invoke((MethodInvoker)delegate {
                        Program.LogWin.listView1.Items.Add(lvi);
                        Program.LogWin.listView1.Invalidate();
                        });
                    memberName = new string(' ', memberName.Length - 1);
                    ltName = "";
                    }
                }
            Program.LogWin.listView1.Invoke((MethodInvoker)delegate {
                if (newMaxLen > longestString) {
                    Program.LogWin.listView1.Columns[1].Width =
                        (int)Program.LogWin.lvGraphics.MeasureString(new string('X', newMaxLen), Program.LogWin.listView1.Font).Width;
                    Program.LogWin.listView1.Columns[2].Width =
                        (Program.LogWin.listView1.Width - Program.LogWin.listView1.Columns[1].Width) - Program.LogWin.listView1.Columns[0].Width - 18;
                    longestString = newMaxLen;
                    }
                Program.LogWin.listView1.EnsureVisible(Program.LogWin.listView1.Items.Count - 1);
                });
            }

        private static Color LTtoColor(LogType lt) {
            Color lviColor = Color.Black;
            switch (lt) {
                case LogType.Info:
                    lviColor = Color.Cyan;
                    break;

                case LogType.Critical:
                    lviColor = Color.Red;
                    break;

                case LogType.Init:
                    lviColor = Color.Green;
                    break;

                case LogType.Parse:
                    lviColor = Color.GreenYellow;
                    break;

                case LogType.Warning:
                    lviColor = Color.IndianRed;
                    break;

                case LogType.Error:
                    lviColor = Color.MediumVioletRed;
                    break;

                case LogType.IO:
                    lviColor = Color.BlueViolet;
                    break;

                case LogType.FTable:
                    lviColor = Color.DarkGreen;
                    break;

                case LogType.GUI:
                    lviColor = Color.FromArgb(0, 112, 166);
                    break;

                case LogType.Exception:
                    lviColor = Color.FromArgb(250, 20, 150);
                    break;
                }
            return lviColor;
            }

        private void DbgWin_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            listView1.SelectedItems.Clear();
            }
        }

    public class LogQueueItem {
        public string text;
        public LogType type;
        public string caller;
        public LogQueueItem(string s, LogType l, string c) {
            text = s;
            type = l;

            if (c.Length > 24) {
                c = c.Remove(24, c.Length - 24);
                }

            caller = c;
            }
        public LogQueueItem() {
            }

        }

    }
