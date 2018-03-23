using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BuildStatus
{
    [TestFixture]
    public class DockerStatus
    {
        private static readonly WebClient webClient = new WebClient();
        //https://hub.docker.com/v2/repositories/vstk/airlock.consumer/buildhistory/?page_size=10
        //https://hub.docker.com/v2/repositories/vstk/

        [TestCaseSource(nameof(ProjectNames))]
        public void DockerProjectStatus(string projectName)
        {
            var json = webClient.DownloadString(
                $"https://hub.docker.com/v2/repositories/vstk/{projectName}/buildhistory/?page_size=100");
            dynamic jObject = JObject.Parse(json);
            if (jObject.count == 0)
                throw new Exception("build count is 0");
            //dockertag_name
            var masterLastBuild = ((JArray)jObject.results).FirstOrDefault(x => (string)x["dockertag_name"] == "latest");
            if (masterLastBuild == null)
                throw new Exception("master build not found");
            int status = (int)masterLastBuild["status"];
            if (status < 0)
                throw new Exception("build failed");
        }


        private static IEnumerable<object[]> ProjectNames()
        {
            var json = webClient.DownloadString($"https://hub.docker.com/v2/repositories/vstk/");
            dynamic jObject = JObject.Parse(json);
            int count = jObject.count;
            if (count == 0)
                throw new Exception("project count is 0");
            foreach (var projObj in ((JArray)jObject.results))
            {
                yield return new object[] { (string)projObj["name"] };
            }
        }
    }
}