﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using ActionFramework.Core.Log;
using ActionFramework.Agent.Log;

namespace ActionFramework.Agent.App
{
    public class AppRepository
    {
        //public AppRepository()
        //{
        //    if (!Agent.AgentIsAvailable())
        //    {
        //        throw new Exception("Agent is not available.");
        //    }
        //}

        public List<ActionFramework.Core.App.App> GetInstalledApps()
        {
            var apps = new List<ActionFramework.Core.App.App>();
            var appsPath = GetInstalledAppsDirectory();
            foreach (var appDirectory in Directory.GetDirectories(appsPath))
            {
                var pathSegements = appDirectory.Split('/');
                var appName = pathSegements[pathSegements.Length - 1];
                var filePath = Path.Combine(appDirectory, appName + ".dll");
                if (File.Exists(filePath))
                {
                    var appAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);

                    foreach (Type type in appAssembly.GetTypes())
                    {
                        bool isAssignedFrom = IsActionType(type);
                        bool containsActionBase = type.Name.Contains("ActionBase");
                        bool containsIAction = type.Name.Contains("IAction");

                        if (isAssignedFrom && !containsIAction && !containsActionBase)
                            types.Add(type);

                        var appType = appAssembly.GetType(appName);
                        var appInstance = Activator.CreateInstance(appType) as ActionFramework.Core.App.App;
                        apps.Add(appInstance);
                    }
                }
            }

            return apps;
        }

       /* private static bool IsActionType(Type type)
        {
            System.Reflection.TypeFilter actionFilter = new System.Reflection.TypeFilter(ActionTypeFilter);
            Type[] interfaces = type.FindInterfaces(actionFilter, typeof(IAction));
            return interfaces.Count() > 0;
        }*/

        public ActionFramework.Core.App.App GetApp(string appName)
        {
            var filePath = Path.Combine(GetInstalledAppsDirectory(), appName, appName + ".dll");

            if (!File.Exists(filePath))
            {
                return null;
            }

            var appAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);
            var appType = appAssembly.GetType(appName + "." + appName);
            var appInstance = Activator.CreateInstance(appType) as ActionFramework.Core.App.App;
            return appInstance;
        }

        public bool RunAction(string appName, string actionName)
        {
            var success = false;
            var app = GetApp(appName);
            var action = app?.Actions.FirstOrDefault(a => a.ActionName == actionName);
            if (action != null)
            { 
                var actionLog = new ActionLog(action.ActionName);
                actionLog.StartRunDate = DateTime.UtcNow;

                string actionMessage;
                try
                {
                    success = action.Execute(out actionMessage);
                }
                catch (Exception e)
                {
                    actionMessage = e.Message;
                }

                actionLog.Success = success;
                actionLog.EndRunDate = DateTime.UtcNow;
                actionLog.LogMessage = actionMessage;
                ActionLogRepository.SaveActionLog(appName, actionLog);
            }
            return success;
        }

        private string GetInstalledAppsDirectory()
        {
            //get path to the running application's directory
            //var applicationPath = ProjectRootResolver.ResolveRootDirectory(Directory.GetCurrentDirectory());
            var applicationPath = Directory.GetCurrentDirectory();

            const string installedAppsDirectoryName = "InstalledApps";
            var installedAppsPath = Path.Combine(applicationPath, installedAppsDirectoryName); //todo: check if this works on different OS (Linux)

            if (!Directory.Exists(installedAppsPath))
            {
                Directory.CreateDirectory(installedAppsPath);
            }

            return installedAppsPath;
        }

        public string GetAppDirectory(string appName)
        {
            var appDirectory = Path.Combine(GetInstalledAppsDirectory(), appName);
            if (!Directory.Exists(appDirectory))
            {
                return null;
            }

            return appDirectory;
        }

    }
}