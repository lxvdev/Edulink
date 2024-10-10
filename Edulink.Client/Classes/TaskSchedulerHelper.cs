using System;
using System.Linq;
using TaskScheduler;

namespace Edulink.Classes
{
    public class TaskSchedulerHelper
    {
        TaskSchedulerClass schedulerClass;
        public TaskSchedulerHelper()
        {
            schedulerClass = new TaskSchedulerClass();
            schedulerClass.Connect();

        }
        public void CreateManualTask(string taskName, string description, string exePath, bool runWithHighestPrivileges = false, string args = null)
        {
            try
            {
                ITaskFolder rootFolder = schedulerClass.GetFolder("\\");
                ITaskDefinition taskDef = schedulerClass.NewTask(0);
                taskDef.RegistrationInfo.Description = description;
                taskDef.Principal.UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                taskDef.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;

                taskDef.Principal.RunLevel = runWithHighestPrivileges ? _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST : _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;

                taskDef.Settings.RunOnlyIfIdle = false;
                taskDef.Settings.DisallowStartIfOnBatteries = false;
                taskDef.Settings.StopIfGoingOnBatteries = false;
                taskDef.Settings.WakeToRun = true;

                taskDef.Settings.AllowDemandStart = true;
                taskDef.Settings.ExecutionTimeLimit = "PT0S";
                taskDef.Settings.RestartInterval = "PT1M";
                taskDef.Settings.RestartCount = 9999;

                IActionCollection actionCollection = taskDef.Actions;
                IAction action = actionCollection.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                IExecAction execAction = (IExecAction)action;
                execAction.Path = exePath;
                execAction.Arguments = args;

                IRegisteredTask regTask = rootFolder.RegisterTaskDefinition(
                    taskName, taskDef, (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                    null, null, _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, "");

                Console.WriteLine("Manual task successfully created.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating task: {ex.Message}");
            }
        }
        public void DeleteTask(string taskName)
        {
            try
            {
                ITaskFolder rootFolder = schedulerClass.GetFolder("\\");
                rootFolder.DeleteTask(taskName, 0);

                Console.WriteLine("Task successfully deleted.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting task: {ex.Message}");
            }
        }
        public bool TaskExistsWithCorrectPath(string taskName, string expectedExePath)
        {
            try
            {
                ITaskFolder rootFolder = schedulerClass.GetFolder("\\");

                IRegisteredTask task = rootFolder.GetTask(taskName);

                if (task != null)
                {
                    ITaskDefinition taskDef = task.Definition;
                    IActionCollection actions = taskDef.Actions;

                    IExecAction execAction = (IExecAction)actions.Cast<IAction>().FirstOrDefault(a => a is IExecAction);

                    if (execAction != null && execAction.Path.Equals(expectedExePath, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Task exists and the executable path is correct.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Task exists, but the executable path is incorrect.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Task does not exist.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking task existence: {ex.Message}");
                return false;
            }
        }
        public void UpdateTask(string taskName, string newExePath, bool runWithHighestPrivileges = false, string newArgs = null)
        {
            try
            {
                ITaskFolder rootFolder = schedulerClass.GetFolder("\\");

                IRegisteredTask task = rootFolder.GetTask(taskName);

                if (task != null)
                {
                    ITaskDefinition taskDef = task.Definition;
                    taskDef.Principal.RunLevel = runWithHighestPrivileges ? _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST : _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;

                    taskDef.Settings.RunOnlyIfIdle = false;
                    taskDef.Settings.DisallowStartIfOnBatteries = false;
                    taskDef.Settings.StopIfGoingOnBatteries = false;
                    taskDef.Settings.WakeToRun = true;

                    taskDef.Settings.AllowDemandStart = true;
                    taskDef.Settings.ExecutionTimeLimit = "PT0S";
                    taskDef.Settings.RestartInterval = "PT1M";
                    taskDef.Settings.RestartCount = 9999;

                    IActionCollection actions = taskDef.Actions;
                    IExecAction execAction = (IExecAction)actions.Cast<IAction>().FirstOrDefault(a => a is IExecAction);

                    if (execAction == null)
                    {
                        execAction = (IExecAction)actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                        Console.WriteLine("No ExecAction found, adding a new one.");
                    }
                    else
                    {
                        Console.WriteLine("ExecAction found, updating existing one.");
                    }

                    execAction.Path = newExePath;
                    execAction.Arguments = newArgs;

                    rootFolder.RegisterTaskDefinition(
                        taskName, taskDef, (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                        null, null, _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, null);

                    Console.WriteLine("Task successfully updated.");
                }
                else
                {
                    throw new Exception("Task does not exist.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating task: {ex.Message}");
            }
        }
    }
}
