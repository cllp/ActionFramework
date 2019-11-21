﻿using System;
using Action = ActionFramework.Action;
using System.Collections.Generic;
using ActionFramework.Helpers.Data.Interface;
using ActionFramework.Helpers.Data;
using ActionFramework.Configuration;

namespace Assets
{
    public class GetDeviceInfo : Action
    {
        public string SenseConnectionString { get; set; }
        private IDataService _dataService;

        public override object Run(dynamic obj)
        {
            _dataService = DataFactory.GetDataService(ConfigurationManager.Settings["AgentSettings:AgentConnectionString"]);

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("devEui", obj);

                var result = _dataService.GetSingle<dynamic>("spGetDeviceInfo", parameters);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetDeviceInfo Exception");
                //LogFactory.File.Error(ex, $"Action Search caused an exception");
                throw ex;//return StatusCode(500, ex);
            }
        }

    }
}
