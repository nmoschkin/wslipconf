using Microsoft.Win32.TaskScheduler;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WSLIPConf.Helpers
{
    public class TaskTool
    {
        // schtasks /create /sc onlogon /tn MyProgram /rl highest /tr "exeFullPath"


        public static void EnableOnStartup()
        {

            var task = TaskService.Instance.NewTask();
            var exec = Process.GetCurrentProcess().MainModule.FileName;

            var asm = Assembly.GetExecutingAssembly().GetName().Name;

            task.Triggers.Add(new LogonTrigger());

            task.Actions.Add(exec, "/silent");
            task.Principal.RunLevel = TaskRunLevel.Highest;

            TaskService.Instance.RootFolder.RegisterTaskDefinition(asm, task);

        }

        public static void DisableOnStartup()
        {
            var asm = Assembly.GetExecutingAssembly().GetName().Name;
            TaskService.Instance.RootFolder.DeleteTask(asm, false);
        }

        public static bool GetIsEnabled()
        {
            var asm = Assembly.GetExecutingAssembly().GetName().Name;

            foreach (var t in TaskService.Instance.RootFolder.Tasks)
            {
                if (t.Name == asm) return true;   
            }

            return false;
        }

    }
}
