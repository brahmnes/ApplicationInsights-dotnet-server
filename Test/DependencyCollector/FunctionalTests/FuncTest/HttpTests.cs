﻿namespace FuncTest
{
    using System;
    using System.Linq;
    using FuncTest.Helpers;
    using FuncTest.Serialization;
    using AI;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;

    /// <summary>
    /// Tests RDD Functionality for a ASP.NET WebApplication in DOTNET 4.5.1, DOTNET 4.6,
    /// and DOTNET Core.
    /// ASPX451 refers to the test application throughout the functional test context.
    /// The same app is used for testing 4.5.1 4.6, and Core scenarios.
    /// </summary>
    [TestClass]
    public class HttpTests
    {
        /// <summary>
        /// Query string to specify Outbound HTTP Call .
        /// </summary>
        private const string QueryStringOutboundHttp = "?type=http&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call POST.
        /// </summary>
        private const string QueryStringOutboundHttpPost = "?type=httppost&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call POST.
        /// </summary>
        private const string QueryStringOutboundHttpPostFailed = "?type=failedhttppost&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call which fails.
        /// </summary>
        private const string QueryStringOutboundHttpFailed = "?type=failedhttp&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call which fails.
        /// </summary>
        private const string QueryStringOutboundHttpFailedAtDns = "?type=failedhttpinvaliddns&count=";

        /// <summary>
        /// Query string to specify Outbound Azure sdk Call .
        /// </summary>
        private const string QueryStringOutboundAzureSdk = "?type=azuresdk{0}&count={1}";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url
        /// <c>http://msdn.microsoft.com/en-us/library/ms228967(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync1 = "?type=httpasync1&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url and which fails.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228967(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync1Failed = "?type=failedhttpasync1&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228962(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync2 = "?type=httpasync2&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url and which fails.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228962(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync2Failed = "?type=failedhttpasync2&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228968(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync3 = "?type=httpasync3&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url and which fails.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228968(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync3Failed = "?type=failedhttpasync3&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228972(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync4 = "?type=httpasync4&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url and which fails.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228972(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsync4Failed = "?type=failedhttpasync4&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228972(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsyncAwait1 = "?type=httpasyncawait1&count=";

        /// <summary>
        /// Query string to specify Outbound HTTP Call in async way as described in below url and which fails.
        /// <c>http://msdn.microsoft.com/en-us/library/ms228972(v=vs.110).aspx</c>
        /// </summary>
        private const string QueryStringOutboundHttpAsyncAwait1Failed = "?type=failedhttpasyncawait1&count=";

        /// <summary>
        /// Resource Name for bing.
        /// </summary>
        private Uri ResourceNameHttpToBing = new Uri("http://www.bing.com");

        /// <summary>
        /// Resource Name for failed request.
        /// </summary>
        private Uri ResourceNameHttpToFailedRequest = new Uri("http://google.com/404");

        /// <summary>
        /// Resource Name for failed at DNS request.
        /// </summary>
        private Uri ResourceNameHttpToFailedAtDnsRequest = new Uri("http://abcdefzzzzeeeeadadad.com");


        /// <summary>
        /// Maximum access time for the initial call - This includes an additional 1-2 delay introduced before the very first call by Profiler V2.
        /// </summary>        
        private readonly TimeSpan AccessTimeMaxHttpInitial = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Maximum access time for calls after initial - This does not incur perf hit of the very first call.
        /// </summary>        
        private readonly TimeSpan AccessTimeMaxHttpNormal = TimeSpan.FromSeconds(3);

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            DeploymentAndValidationTools.Initialize();

            AzureStorageHelper.Initialize();
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            AzureStorageHelper.Cleanup();

            DeploymentAndValidationTools.CleanUp();
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            DeploymentAndValidationTools.SdkEventListener.Start();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Assert.IsFalse(DeploymentAndValidationTools.SdkEventListener.FailureDetected, "Failure is detected. Please read test output logs.");
            DeploymentAndValidationTools.SdkEventListener.Stop();
        }

        #region 451

        private const string Aspx451TestAppFolder = "..\\TestApps\\ASPX451\\App\\";

        private static void EnsureNet451Installed()
        {
            if (!RegistryCheck.IsNet451Installed)
            {
                Assert.Inconclusive(".Net Framework 4.5.1 is not installed");
            }
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForSyncHttpAspx451()
        {
            EnsureNet451Installed();

            // Execute and verify calls which succeeds            
            this.ExecuteSyncHttpTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, 1, AccessTimeMaxHttpNormal, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForSyncHttpPostCallAspx451()
        {
            EnsureNet451Installed();

            // Execute and verify calls which succeeds            
            this.ExecuteSyncHttpPostTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, 1, AccessTimeMaxHttpNormal, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForSyncHttpFailedAspx451()
        {
            EnsureNet451Installed();

            // Execute and verify calls which fails.            
            this.ExecuteSyncHttpTests(DeploymentAndValidationTools.Aspx451TestWebApplication, false, 1, AccessTimeMaxHttpInitial, "404");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAsync1HttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, 1, AccessTimeMaxHttpNormal, QueryStringOutboundHttpAsync1, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForHttpAspx451WithHttpClient()
        {
            EnsureNet451Installed();

            this.ExecuteSyncHttpClientTests(DeploymentAndValidationTools.Aspx451TestWebApplication, AccessTimeMaxHttpNormal, "404");
        }

        [TestMethod]
        [Description("Verify RDD is collected for failed Async Http Calls in ASPX 4.5.1 application")]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForFailedAsync1HttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncTests(DeploymentAndValidationTools.Aspx451TestWebApplication, false, 1, AccessTimeMaxHttpInitial, QueryStringOutboundHttpAsync1Failed, "404");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAsync2HttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, 1, AccessTimeMaxHttpNormal, QueryStringOutboundHttpAsync2, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForFailedAsync2HttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncTests(DeploymentAndValidationTools.Aspx451TestWebApplication, false, 1, AccessTimeMaxHttpInitial, QueryStringOutboundHttpAsync2Failed, "404");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAsync3HttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, 1, AccessTimeMaxHttpNormal, QueryStringOutboundHttpAsync3, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForFailedAsync3HttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncTests(DeploymentAndValidationTools.Aspx451TestWebApplication, false, 1, AccessTimeMaxHttpInitial, QueryStringOutboundHttpAsync3Failed, "404");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAsyncWithCallBackHttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncWithCallbackTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAsyncAwaitHttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncAwaitTests(DeploymentAndValidationTools.Aspx451TestWebApplication, true, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForFailedAsyncAwaitHttpAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAsyncAwaitTests(DeploymentAndValidationTools.Aspx451TestWebApplication, false, "404");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAzureSdkBlobAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAzureSDKTests(DeploymentAndValidationTools.Aspx451TestWebApplication, 1, "blob", "http://127.0.0.1:11000");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAzureSdkQueueAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAzureSDKTests(DeploymentAndValidationTools.Aspx451TestWebApplication, 1, "queue", "http://127.0.0.1:11001");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestRddForAzureSdkTableAspx451()
        {
            EnsureNet451Installed();

            this.ExecuteAzureSDKTests(DeploymentAndValidationTools.Aspx451TestWebApplication, 1, "table", "http://127.0.0.1:11002");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolderWin32)]
        public void TestRddForWin32ApplicationPool()
        {
            EnsureNet451Installed();

            this.ExecuteSyncHttpTests(DeploymentAndValidationTools.Aspx451TestWebApplicationWin32, true, 1, AccessTimeMaxHttpInitial, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.Net451)]
        [Description("Validates that DependencyModule collects telemety for outbound connections to non existent hosts. This request is expected to fail at DNS resolution stage, and hence will not contain http code in result.")]
        [DeploymentItem(Aspx451TestAppFolder, DeploymentAndValidationTools.Aspx451AppFolder)]
        public void TestDependencyCollectionForFailedRequestAtDnsResolution()
        {
            EnsureNet451Installed();

            var queryString = QueryStringOutboundHttpFailedAtDns;
            var resourceNameExpected = ResourceNameHttpToFailedAtDnsRequest;
            DeploymentAndValidationTools.Aspx451TestWebApplication.ExecuteAnonymousRequest(queryString);

            //// The above request would have trigged RDD module to monitor and create RDD telemetry
            //// Listen in the fake endpoint and see if the RDDTelemtry is captured
            var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
            var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();

            Assert.AreEqual(
                1,
                httpItems.Length,
                "Total Count of Remote Dependency items for HTTP collected is wrong.");

            var httpItem = httpItems[0];

            // Since the outbound request would fail at DNS resolution, there won't be any http code to collect.
            // This will be a case where success = false, but resultCode is empty            
            Assert.AreEqual(false, httpItem.data.baseData.success, "Success flag collected is wrong.");
            Assert.AreEqual("NameResolutionFailure", httpItem.data.baseData.resultCode, "Result code collected is wrong.");
            string actualSdkVersion = httpItem.tags[new ContextTagKeys().InternalSdkVersion];
            Assert.IsTrue(actualSdkVersion.Contains(DeploymentAndValidationTools.ExpectedSDKPrefix), "Actual version:" + actualSdkVersion);
        }

        #endregion 451

        #region Core

        private static void EnsureDotNetCoreInstalled()
        {
            string output = "";
            string error = "";
            DotNetCoreProcess process = new DotNetCoreProcess("--version")
                .RedirectStandardOutputTo((string outputMessage) => output += outputMessage)
                .RedirectStandardErrorTo((string errorMessage) => error += errorMessage)
                .Run();

            if (process.ExitCode.Value != 0 || !string.IsNullOrEmpty(error))
            {
                Assert.Inconclusive(".Net Core is not installed");
            }
            else
            {
                // Look for first dash to get semantic version. (for example: 1.0.0-preview2-003156)
                int dashIndex = output.IndexOf('-');
                Version version = new Version(dashIndex == -1 ? output : output.Substring(0, dashIndex));

                Version minVersion = new Version("1.0.0");
                if (version < minVersion)
                {
                    Assert.Inconclusive($".Net Core version ({output}) must be greater than or equal to {minVersion}.");
                }
            }
        }

        private const string AspxCoreTestAppFolder = "..\\TestApps\\AspxCore\\";

        [TestMethod]
        [TestCategory(TestCategory.NetCore)]
        [DeploymentItem(AspxCoreTestAppFolder, DeploymentAndValidationTools.AspxCoreAppFolder)]
        public void TestRddForSyncHttpAspxCore()
        {
            EnsureDotNetCoreInstalled();

            // Execute and verify calls which succeeds            
            this.ExecuteSyncHttpTests(DeploymentAndValidationTools.AspxCoreTestWebApplication, true, 1, AccessTimeMaxHttpNormal, "200");
        }

        [TestMethod]
        [TestCategory(TestCategory.NetCore)]
        [DeploymentItem(AspxCoreTestAppFolder, DeploymentAndValidationTools.AspxCoreAppFolder)]
        public void TestRddForSyncHttpPostCallAspxCore()
        {
            EnsureDotNetCoreInstalled();

            // Execute and verify calls which succeeds            
            this.ExecuteSyncHttpPostTests(DeploymentAndValidationTools.AspxCoreTestWebApplication, true, 1, AccessTimeMaxHttpNormal, "200");
        }

        [TestMethod]
        [Ignore] // Don't run this test until .NET Core writes diagnostic events for failed HTTP requests
        [TestCategory(TestCategory.NetCore)]
        [DeploymentItem(AspxCoreTestAppFolder, DeploymentAndValidationTools.AspxCoreAppFolder)]
        public void TestRddForSyncHttpFailedAspxCore()
        {
            EnsureDotNetCoreInstalled();

            // Execute and verify calls which fails.            
            this.ExecuteSyncHttpTests(DeploymentAndValidationTools.AspxCoreTestWebApplication, false, 1, AccessTimeMaxHttpInitial, "200");
        }

        #endregion Core

        #region helpers

        /// <summary>
        /// Helper to execute Async Http tests.
        /// </summary>
        /// <param name="testWebApplication">The test application for which tests are to be executed.</param>
        /// <param name="success">Indicates if the tests should test success or failure case.</param> 
        /// <param name="count">Number to RDD calls to be made by the test application. </param> 
        /// <param name="accessTimeMax">Approximate maximum time taken by RDD Call.  </param>
        /// <param name="url">url</param> 
        private void ExecuteAsyncTests(TestWebApplication testWebApplication, bool success, int count,
            TimeSpan accessTimeMax, string url, string resultCodeExpected)
        {
            var resourceNameExpected = success ? ResourceNameHttpToBing : ResourceNameHttpToFailedRequest;

            testWebApplication.DoTest(
                application =>
                {
                    var queryString = url;
                    application.ExecuteAnonymousRequest(queryString + count);
                    application.ExecuteAnonymousRequest(queryString + count);
                    application.ExecuteAnonymousRequest(queryString + count);

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured
                    var allItems =
                        DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(
                            DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);

                    var httpItems =
                        allItems.Where(i => i.data.baseData.type == "Http").ToArray();

                    Assert.AreEqual(
                        3 * count,
                        httpItems.Length,
                        "Total Count of Remote Dependency items for HTTP collected is wrong.");

                    foreach (var httpItem in httpItems)
                    {
                        this.Validate(httpItem, resourceNameExpected, accessTimeMax, success, "GET", resultCodeExpected);
                    }
                });
        }

        /// <summary>
        /// Helper to execute Sync Http tests
        /// </summary>
        /// <param name="testWebApplication">The test application for which tests are to be executed</param>
        /// <param name="success">indicates if the tests should test success or failure case</param>   
        /// <param name="count">number to RDD calls to be made by the test application.  </param> 
        /// <param name="accessTimeMax">approximate maximum time taken by RDD Call.  </param> 
        private void ExecuteSyncHttpTests(TestWebApplication testWebApplication, bool success, int count, TimeSpan accessTimeMax,
            string resultCodeExpected)
        {
            testWebApplication.DoTest(
                application =>
                {
                    var queryString = success ? QueryStringOutboundHttp : QueryStringOutboundHttpFailed;
                    var resourceNameExpected = success ? ResourceNameHttpToBing : ResourceNameHttpToFailedRequest;
                    application.ExecuteAnonymousRequest(queryString + count);

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured
                    var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
                    var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();

                    Assert.AreEqual(
                        count,
                        httpItems.Length,
                        "Total Count of Remote Dependency items for HTTP collected is wrong.");

                    foreach (var httpItem in httpItems)
                    {
                        this.Validate(httpItem, resourceNameExpected, accessTimeMax, success, "GET", resultCodeExpected);
                    }
                });
        }

        private void ExecuteSyncHttpClientTests(TestWebApplication testWebApplication, TimeSpan accessTimeMax, string resultCodeExpected)
        {
            testWebApplication.DoTest(
                application =>
                {
                    var queryString = "?type=httpClient&count=1";
                    var resourceNameExpected = new Uri("http://www.google.com/404");
                    application.ExecuteAnonymousRequest(queryString);

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured
                    var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
                    var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();

                    Assert.AreEqual(
                        1,
                        httpItems.Length,
                        "Total Count of Remote Dependency items for HTTP collected is wrong.");

                    foreach (var httpItem in httpItems)
                    {
                        // This is a call to google.com/404 which will fail but typically takes longer time. So accesstime can more than normal.
                        this.Validate(httpItem, resourceNameExpected, accessTimeMax.Add(TimeSpan.FromSeconds(15)), false, "GET", resultCodeExpected);
                    }
                });
        }

        /// <summary>
        /// Helper to execute Sync Http tests
        /// </summary>
        /// <param name="testWebApplication">The test application for which tests are to be executed</param>
        /// <param name="success">indicates if the tests should test success or failure case</param>   
        /// <param name="count">number to RDD calls to be made by the test application.  </param> 
        /// <param name="accessTimeMax">approximate maximum time taken by RDD Call.  </param> 
        private void ExecuteSyncHttpPostTests(TestWebApplication testWebApplication, bool success, int count, TimeSpan accessTimeMax, string resultCodeExpected)
        {
            testWebApplication.DoTest(
                application =>
                {
                    var queryString = success ? QueryStringOutboundHttpPost : QueryStringOutboundHttpPostFailed;
                    var resourceNameExpected = success ? ResourceNameHttpToBing : ResourceNameHttpToFailedRequest;
                    application.ExecuteAnonymousRequest(queryString + count);

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured
                    var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
                    var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();

                    // Validate the RDD Telemetry properties
                    Assert.AreEqual(
                        count,
                        httpItems.Length,
                        "Total Count of Remote Dependency items for HTTP collected is wrong.");

                    foreach (var httpItem in httpItems)
                    {
                        this.Validate(httpItem, resourceNameExpected, accessTimeMax, success, "POST", resultCodeExpected);
                    }
                });
        }

        /// <summary>
        /// Helper to execute Async http test which uses Callbacks.
        /// </summary>
        /// <param name="testWebApplication">The test application for which tests are to be executed</param>
        /// <param name="success">indicates if the tests should test success or failure case</param> 
        private void ExecuteAsyncWithCallbackTests(TestWebApplication testWebApplication, bool success, string resultCodeExpected)
        {
            var resourceNameExpected = success ? ResourceNameHttpToBing : ResourceNameHttpToFailedRequest;

            testWebApplication.DoTest(
                application =>
                {
                    application.ExecuteAnonymousRequest(success ? QueryStringOutboundHttpAsync4 : QueryStringOutboundHttpAsync4Failed);

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured

                    var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
                    var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();

                    // Validate the RDD Telemetry properties
                    Assert.AreEqual(
                        1,
                        httpItems.Length,
                        "Total Count of Remote Dependency items for HTTP collected is wrong.");
                    this.Validate(httpItems[0], resourceNameExpected, AccessTimeMaxHttpInitial, success, "GET", resultCodeExpected);
                });
        }

        /// <summary>
        /// Helper to execute Async http test which uses async,await pattern (.NET 4.5 or higher only)
        /// </summary>
        /// <param name="testWebApplication">The test application for which tests are to be executed</param>
        /// <param name="success">indicates if the tests should test success or failure case</param> 
        private void ExecuteAsyncAwaitTests(TestWebApplication testWebApplication, bool success, string resultCodeExpected)
        {
            var resourceNameExpected = success ? ResourceNameHttpToBing : ResourceNameHttpToFailedRequest;

            testWebApplication.DoTest(
                application =>
                {
                    application.ExecuteAnonymousRequest(success ? QueryStringOutboundHttpAsyncAwait1 : QueryStringOutboundHttpAsyncAwait1Failed);

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured

                    var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
                    var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();

                    // Validate the RDD Telemetry properties
                    Assert.AreEqual(
                        1,
                        httpItems.Length,
                        "Total Count of Remote Dependency items for HTTP collected is wrong.");
                    this.Validate(httpItems[0], resourceNameExpected, AccessTimeMaxHttpInitial, success, "GET", resultCodeExpected);
                });
        }

        /// <summary>
        /// Helper to execute Azure SDK tests.
        /// </summary>
        /// <param name="testWebApplication">The test application for which tests are to be executed.</param>
        /// <param name="count">number to RDD calls to be made by the test application.</param> 
        /// <param name="type"> type of azure call.</param> 
        /// <param name="expectedUrl">expected url for azure call.</param> 
        private void ExecuteAzureSDKTests(TestWebApplication testWebApplication, int count, string type, string expectedUrl)
        {
            testWebApplication.DoTest(
                application =>
                {
                    application.ExecuteAnonymousRequest(string.Format(QueryStringOutboundAzureSdk, type, count));

                    //// The above request would have trigged RDD module to monitor and create RDD telemetry
                    //// Listen in the fake endpoint and see if the RDDTelemtry is captured                      
                    var allItems = DeploymentAndValidationTools.SdkEventListener.ReceiveAllItemsDuringTimeOfType<TelemetryItem<RemoteDependencyData>>(DeploymentAndValidationTools.SleepTimeForSdkToSendEvents);
                    var httpItems = allItems.Where(i => i.data.baseData.type == "Http").ToArray();
                    int countItem = 0;

                    foreach (var httpItem in httpItems)
                    {
                        TimeSpan accessTime = TimeSpan.Parse(httpItem.data.baseData.duration);
                        Assert.IsTrue(accessTime.TotalMilliseconds >= 0, "Access time should be above zero for azure calls");

                        string actualSdkVersion = httpItem.tags[new ContextTagKeys().InternalSdkVersion];
                        Assert.IsTrue(actualSdkVersion.Contains(DeploymentAndValidationTools.ExpectedSDKPrefix), "Actual version:" + actualSdkVersion);

                        var url = httpItem.data.baseData.data;
                        if (url.Contains(expectedUrl))
                        {
                            countItem++;
                        }
                        else
                        {
                            Assert.Fail("ExecuteAzureSDKTests.url not matching for " + url);
                        }
                    }

                    Assert.IsTrue(countItem >= count, "Azure " + type + " access captured " + countItem + " is less than " + count);
                });
        }

        #endregion

        private void Validate(TelemetryItem<RemoteDependencyData> itemToValidate,
            Uri expectedUrl,
            TimeSpan accessTimeMax,
            bool successFlagExpected,
            string verb,
            string resultCodeExpected)
        {
            if ("rddp" == DeploymentAndValidationTools.ExpectedSDKPrefix)
            {
                Assert.AreEqual(verb + " " + expectedUrl.AbsolutePath, itemToValidate.data.baseData.name, "For StatusMonitor implementation we expect verb to be collected.");
                Assert.AreEqual(expectedUrl.Host, itemToValidate.data.baseData.target);
                Assert.AreEqual(expectedUrl.OriginalString, itemToValidate.data.baseData.data);
            }

            DeploymentAndValidationTools.Validate(itemToValidate, accessTimeMax, successFlagExpected, resultCodeExpected);
        }
    }
}