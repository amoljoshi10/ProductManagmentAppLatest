using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductManagementApp.Telemetry
{
    public class CustomeTelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            //telemetry.Context.Operation.Id = CorelationTelemetryProvider.GetOperationId();
        }

    }
}
