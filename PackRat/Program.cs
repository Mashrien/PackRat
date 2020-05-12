using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PackRat;

namespace PackRatUI {
    static class Program {
        public static DbgWin LogWin; // must be 1st
        public static Archive Archive; // must be 2nd
        public static PackRatUI PRUI;
        public static ProgressWin ProgWin;

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args) {
                if (arg.ToLower() == "no_recursion")
                    Utils.DebugFlags |= Utils.Debug.NO_RECURSION;
                if (arg.ToLower() == "log_to_console")
                    Utils.DebugFlags |= Utils.Debug.LOG_TO_CONSOLE;
                if (arg.ToLower() == "no_threading")
                    Utils.DebugFlags |= Utils.Debug.NO_THREADING;
                if (arg.ToLower() == "no_collisions")
                    Utils.DebugFlags |= Utils.Debug.NO_COLLISIONS;
                if (arg.ToLower() == "overwrite_files")
                    Utils.DebugFlags |= Utils.Debug.OVERWRITE_FILES;
                if (arg.ToLower() == "no_reports")
                    Utils.DebugFlags |= Utils.Debug.NO_REPORTS;
                if (arg.ToLower() == "no_compression")
                    Utils.DebugFlags |= Utils.Debug.NO_COMPRESSION;
                if (arg.ToLower() == "no_encryption")
                    Utils.DebugFlags |= Utils.Debug.NO_ENCRYPTION;
                if (arg.ToLower() == "no_hashing")
                    Utils.DebugFlags |= Utils.Debug.NO_HASHING;
                if (arg.ToLower() == "leave_temp_files")
                    Utils.DebugFlags |= Utils.Debug.LEAVE_TEMP_FILES;

                }

            LogWin = new DbgWin();
            PRUI = new PackRatUI();
            ProgWin = new ProgressWin();

            DbgWin.Log($"CMD Opts: {Utils.DebugFlags.ToString()}", LogType.Warning);

            Application.Run(PRUI);
            }
        }
    }
