using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DriverExploits;
using Loadlibrayy.Injection;
using Loadlibrayy.Logger;
using Loadlibrayy.Helpers;
using System.IO;

namespace Loadlibrayy.Forms
{
    public partial class NewInjectionForm : Form
    {
        //private Process g_SelectedProcess;
        private Process[] pros;
        public NewInjectionForm()
        {
            InitializeComponent();
        }

        private void InjectButton_Click(object sender, EventArgs e)
        {
             pros = Process.GetProcessesByName("TslGame");
            // 检查进程是否存在
            if (pros.Length == 0)
            {
                Log.ShowError("请先运行游戏,并且匹配成功进入游戏等待倒计时大厅", "错误");
                return;
            }
            // SANITY CHECKS
            //if (g_SelectedProcess == null)
            //{
            //    Log.ShowError("请选择您要注入的程序!", "错误");
            //    return;
            //}

            FileInfo fi = new FileInfo("./System.Runtime.CompilerServices.Unsafe.dll");

            // OpenFileDialog fileDialog = new OpenFileDialog()
            //{
            //    Filter = "Dynamic Link Library|*.dll",
            //    Multiselect = false
            //};
            //if (fileDialog.ShowDialog() != DialogResult.OK)
            //    return;

            // 可驱动负载
            bool driverLoaded = false;
            if (ElevateHandleCheckbox.Checked)
            {
                if (!(driverLoaded = ElevateHandle.Driver.Load()))
                {
                    Log.ShowError("CPUZ141.sys failed to load", "错误");
                    return;
                }
                ElevateHandle.UpdateDynamicData(); // 更新内核偏移
                ElevateHandle.Attach();            // 附加到当前进程
                ElevateHandle.Elevate((ulong)pros[0].Handle, 0x1fffff);
            }
            InjectionOptions options = new InjectionOptions()
            {
                ElevateHandle = ElevateHandleCheckbox.Checked,
                EraseHeaders = EraseHeadersCheckbox.Checked,
                CreateLoaderReference = LinkModuleCheckbox.Checked,
                LoaderImagePath = fi.FullName
            };
            ExecutionType executionType = 0;
            switch (TypeCombo.SelectedIndex)
            {
                case 0:
                    executionType = ExecutionType.CreateThread;
                    break;
                case 1:
                    executionType = ExecutionType.HijackThread;
                    break;
            }

            IInjectionMethod injectionMethod = null;
            switch (ModeCombo.SelectedIndex)
            {
                case 0: // 手工地图
                    injectionMethod = new ManualMapInjection(pros[0], executionType, options);
                    break;

                case 1: // 加载库
                    injectionMethod = new LoadLibraryInjection(pros[0], executionType, options);
                    break;
            }

            injectionMethod.InjectImage(fi.FullName);

            if (driverLoaded)
                ElevateHandle.Driver.Unload();
        }
        private void SelectButton_Click(object sender, EventArgs e)
        {
            TaskListForm taskListForm = new TaskListForm();
            taskListForm.ShowDialog();

            if (taskListForm.SelectedProcess != null)
                pros[0] = taskListForm.SelectedProcess;
        }

        private void NewInjectionForm_Load(object sender, EventArgs e)
        {

            this.ModeCombo.SelectedIndex = 0;
            this.TypeCombo.SelectedIndex = 0;
            Random random = new Random();
            // FADE FORM
            Timer fadeTimer = new Timer();
            fadeTimer.Tick += delegate
            {
                this.Opacity += 0.1;
               // this.CreditLabel.ForeColor = Color.FromArgb((this.CreditLabel.ForeColor.R + 10) % 255, 0, 0);
            };
            fadeTimer.Start();
        }

       
        
    }
}
