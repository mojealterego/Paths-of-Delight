using System;
using UnityEngine;

namespace PathsOfDelight.Editor
{
    public static class CloudBuildHooks
    {
        public static void PreExport()
        {
            var edition = Environment.GetEnvironmentVariable("POD_EDITION");
            if (string.IsNullOrWhiteSpace(edition))
                edition = "play";

            Debug.Log("Unity Build Automation pre-export. POD_EDITION=" + edition);
            BuildCommand.PrepareCloudBuild(edition);
        }
    }
}
