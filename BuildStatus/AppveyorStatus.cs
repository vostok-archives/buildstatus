using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BuildStatus
{
    public class AppveyorStatus
    {
        private static readonly string apiToken = Environment.GetEnvironmentVariable("APPVEYOR_TOKEN");
        private static readonly HttpClient httpClient = new HttpClient();

        static AppveyorStatus()
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        }

        [Test, TestCaseSource(nameof(ProjectObjects))]
        public void AppveyorProjectStatus(string projectName)
        {
            using (var response = httpClient.GetAsync($"https://ci.appveyor.com/api/projects/vostok/{projectName}/history?recordsNumber=10&branch=master").Result)
            {
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                var history = JToken.Parse(json);

                var lastBuild = ((JArray)history["builds"]).FirstOrDefault(b => (string)b["status"] != "cancelled" && (string)b["status"] != "running");
                if (lastBuild == null)
                    return;
                var status = (string)lastBuild["status"];
                if (status != "success")
                {
                    throw new Exception("build failed with status " + status);
                }
            }
            //Console.WriteLine(role.Value<string>("name"));
        }

        private static IEnumerable<object[]> ProjectObjects()
        {
            // get the list of roles
            using (var response = httpClient.GetAsync("https://ci.appveyor.com/api/projects").Result)
            {
                response.EnsureSuccessStatusCode();

                var json = response.Content.ReadAsStringAsync().Result;
                var projects = JArray.Parse(json);
                foreach (var project in projects)
                {
                    yield return new object[] { (string)project["slug"] };
                }
            }
        }
    }
}