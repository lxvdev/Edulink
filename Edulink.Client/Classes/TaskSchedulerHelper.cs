using System;
using System.Linq;
using System.Runtime.InteropServices;
using TaskScheduler;

namespace Edulink.Classes
{
    public class TaskSchedulerHelper : IDisposable
    {
        TaskSchedulerClass schedulerClass;
        public TaskSchedulerHelper()
        {
            schedulerClass = new TaskSchedulerClass();
            schedulerClass.Connect();

        }
        public void CreateLoginTask(string taskName, string description, string exePath, bool runWithHighestPrivileges = false, string arguments = null)
        {
            try
            {
                ITaskFolder rootFolder = schedulerClass.GetFolder(@"\");
                ITaskDefinition taskDefinition = schedulerClass.NewTask(0);

                taskDefinition.RegistrationInfo.Description = description;
                taskDefinition.RegistrationInfo.Author = "Andrew";

                //taskDef.Principal.UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                taskDefinition.Principal.GroupId = "S-1-5-4"; // Interactive (It runs when a user logs in)
                taskDefinition.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
                taskDefinition.Principal.RunLevel = runWithHighestPrivileges ? _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST : _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;

                ILogonTrigger logonTrigger = (ILogonTrigger)taskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
                logonTrigger.UserId = null;

                IExecAction execAction = (IExecAction)taskDefinition.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                execAction.Path = exePath;
                execAction.Arguments = arguments;

                taskDefinition.Settings.StartWhenAvailable = true;
                taskDefinition.Settings.RunOnlyIfIdle = false;
                taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                taskDefinition.Settings.StopIfGoingOnBatteries = false;
                taskDefinition.Settings.WakeToRun = true;

                taskDefinition.Settings.AllowDemandStart = true;
                taskDefinition.Settings.ExecutionTimeLimit = "PT0S";
                taskDefinition.Settings.RestartInterval = "PT1M";
                taskDefinition.Settings.RestartCount = 9999;

                rootFolder.RegisterTaskDefinition(
                    taskName,
                    taskDefinition,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                    null,
                    null,
                    _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                    null
                );

                Console.WriteLine("Startup task was successfully created.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating task: {ex.Message}");
            }
        }
        public void CreateManualTask(string taskName, string description, string exePath, bool runWithHighestPrivileges = false, string arguments = null)
        {
            try
            {
                ITaskFolder rootFolder = schedulerClass.GetFolder(@"\");
                ITaskDefinition taskDefinition = schedulerClass.NewTask(0);

                taskDefinition.RegistrationInfo.Description = description;

                taskDefinition.Principal.UserId = null;
                taskDefinition.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
                taskDefinition.Principal.RunLevel = runWithHighestPrivileges ? _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST : _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;

                IActionCollection actions = taskDefinition.Actions;
                IExecAction execAction = (IExecAction)actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                execAction.Path = exePath;
                execAction.Arguments = arguments;

                taskDefinition.Settings.StartWhenAvailable = true;
                taskDefinition.Settings.RunOnlyIfIdle = false;
                taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                taskDefinition.Settings.StopIfGoingOnBatteries = false;
                taskDefinition.Settings.WakeToRun = true;

                taskDefinition.Settings.AllowDemandStart = true;
                taskDefinition.Settings.ExecutionTimeLimit = "PT0S";
                taskDefinition.Settings.RestartInterval = "PT1M";
                taskDefinition.Settings.RestartCount = 9999;

                rootFolder.RegisterTaskDefinition(
                    taskName,
                    taskDefinition,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                    null,
                    null,
                    _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                    null
                );

                Console.WriteLine("Manual task was successfully created.");
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
                ITaskFolder rootFolder = schedulerClass.GetFolder(@"\");
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
                ITaskFolder rootFolder = schedulerClass.GetFolder(@"\");

                IRegisteredTask task = rootFolder.GetTask(taskName);

                if (task != null)
                {
                    ITaskDefinition taskDefinition = task.Definition;
                    IActionCollection actions = taskDefinition.Actions;

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
                ITaskFolder rootFolder = schedulerClass.GetFolder(@"\");

                IRegisteredTask task = rootFolder.GetTask(taskName);

                if (task != null)
                {
                    ITaskDefinition taskDefinition = task.Definition;
                    taskDefinition.Principal.RunLevel = runWithHighestPrivileges ? _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST : _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;

                    taskDefinition.Settings.RunOnlyIfIdle = false;
                    taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                    taskDefinition.Settings.StopIfGoingOnBatteries = false;
                    taskDefinition.Settings.WakeToRun = true;

                    taskDefinition.Settings.AllowDemandStart = true;
                    taskDefinition.Settings.ExecutionTimeLimit = "PT0S";
                    taskDefinition.Settings.RestartInterval = "PT1M";
                    taskDefinition.Settings.RestartCount = 9999;

                    IActionCollection actions = taskDefinition.Actions;
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
                        taskName, taskDefinition, (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
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

        public void Dispose()
        {
            Marshal.ReleaseComObject(schedulerClass);
        }
    }
}
