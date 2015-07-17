using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Aldurcraft.Persistent40.Test
{
    [TestClass]
    public class GeneralTests
    {
        public TestContext TestContext { get; set; }
        class TestLogger : IPersistentLogger
        {
            public void LogDebug(string message, object source, Exception exception = null)
            {
                WriteTrace(message, source, exception);
            }

            public void LogInfo(string message, object source, Exception exception = null)
            {
                WriteTrace(message, source, exception);
            }

            public void LogError(string message, object source, Exception exception = null)
            {
                WriteTrace(message, source, exception);
            }

            void WriteTrace(string message, object source, Exception exception)
            {
                Trace.WriteLine(string.Format("{0}; source: {1}; exception: {2}", message, source, exception));
            }
        }
        private PersistentFactory CreateTestFactory(bool cleanOldDataStore = true)
        {
            var dataStorePath = Path.Combine(TestContext.TestRunDirectory, "persistent40liteDatastore");

            if (cleanOldDataStore)
            {
                var dirinfo = new DirectoryInfo(dataStorePath);
                if (dirinfo.Exists) dirinfo.Delete(true);
            }

            var logger = new TestLogger();
            var storage = new PlainFilePersistentStorage(logger, dataStorePath);
            var serializer = new JsonPersistentSerializer(logger);

            var factory = new PersistentFactory(storage, serializer, logger);

            return factory;
        }

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
        }

        [TestMethod]
        public void SaveAndLoad()
        {
            var factory = CreateTestFactory();

            var testList = new List<string>() { "SomeNames", "SomeNames" };
            var p40 = factory.Create<SomeSettings>("SaveAndLoad");
            p40.Data.SomeName = "SomeName";
            p40.Data.SomeNames = testList;
            p40.Data.Nested = new SomeSettings.NestedClass() { NestedData = "ChangedNestedData" };
            p40.Save();
            p40.Dispose();

            for (int j = 0; j < 2; j++)
            {
                var newp40 = factory.Create<SomeSettings>("SaveAndLoad");
                Assert.IsTrue(testList.Count == newp40.Data.SomeNames.Count);
                for (int i = 0; i < testList.Count; i++)
                {
                    Assert.IsTrue(testList[i] == newp40.Data.SomeNames[i]);
                }
                Assert.AreEqual("ChangedNestedData", newp40.Data.Nested.NestedData);

                newp40.Save();
                newp40.Dispose();
            }
        }

        [TestMethod]
        public void SaveAndLoadMultiple()
        {
            var factory = CreateTestFactory();

            var testList = new List<string>() { "SomeNames", "SomeNames" };
            var p40 = factory.Create<SomeSettings>("SaveAndLoadMultiple");
            p40.Data.SomeName = "SomeName";
            p40.Data.SomeNames = testList;
            p40.Data.Nested = new SomeSettings.NestedClass() { NestedData = "ChangedNestedData" };
            p40.Save();
            p40.Dispose();

            var p41 = factory.Create<SomeSettings>("SaveAndLoadMultiple");
            p41.Data.SomeName = "Another";
            p41.Save();
            p41.Dispose();

            for (int j = 0; j < 2; j++)
            {
                var newp40 = factory.Create<SomeSettings>("SaveAndLoadMultiple");
                Assert.IsTrue(testList.Count == newp40.Data.SomeNames.Count);
                for (int i = 0; i < testList.Count; i++)
                {
                    Assert.IsTrue(testList[i] == newp40.Data.SomeNames[i]);
                }
                Assert.AreEqual("ChangedNestedData", newp40.Data.Nested.NestedData);
                newp40.Save();
                newp40.Dispose();

                var newp41 = factory.Create<SomeSettings>("SaveAndLoadMultiple");
                Assert.IsTrue(newp41.Data.SomeName == "Another");
                newp41.Dispose();
            }
        }

        [TestMethod]
        public void ReferentialIntegrity()
        {
            var factory = CreateTestFactory();

            var testList = new List<string>() { "SomeNames", "SomeNames" };
            var p40 = factory.Create<ReferentialData>("ReferentialIntegrity");
            p40.Save();
            p40.Dispose();

            var newp40 = factory.Create<ReferentialData>("ReferentialIntegrity");
            Assert.IsTrue(newp40.Data.class1 == newp40.Data.class2);
        }

        [TestMethod]
        public void PrivateSerialization()
        {
            var factory = CreateTestFactory();

            var p40 = factory.Create<WithPrivates>("PrivateSerialization");
            p40.Data.SetPrivateMember();
            p40.Save();
            p40.Dispose();

            var newp40 = factory.Create<WithPrivates>("PrivateSerialization");
            Assert.IsTrue(newp40.Data.GetPrivateValue() == 43);
            Assert.IsTrue(newp40.Data.publicMember.Something == 42);
        }

        [TestMethod]
        public void SingletonWorks()
        {
            var factory = CreateTestFactory();

            var p40 = factory.Create<ReferentialData>("SingletonWorks");
            p40.Save();

            bool excThrown = false;
            try
            {
                var newp40 = factory.Create<ReferentialData>("SingletonWorks");
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.StartsWith("This objectId is already locked"))
                {
                    excThrown = true;
                }
            }
            Assert.IsTrue(excThrown);
        }

        [TestMethod]
        public void VerifyCantSaveDifferentTypeSameSession()
        {
            var factory = CreateTestFactory();

            var p40 = factory.Create<SomeSettings>("VerifyCantSaveDifferentTypeSameSession");
            p40.Save();
            p40.Dispose();

            try
            {
                using (var newp40 = factory.Create<SomeSettingsRenamed>("VerifyCantSaveDifferentTypeSameSession")) { }
            }
            catch (Exception exception)
            {
                if (
                    !exception.Message.StartsWith(
                        "This objectId was used with different runtime type during this session"))
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void SerializeUnusualType()
        {
            var factory = CreateTestFactory();

            var persistent = factory.Create<UnusualData>("SerializeUnusualType");
            var version = new Version(1, 2, 3, 4);
            var someEnum = SomeEnum.two;
            persistent.Data.VersionData = version;
            persistent.Data.SomeEnumData = someEnum;
            persistent.Data.NestedDictionary = new Dictionary<string, Dictionary<string, string>>();
            persistent.Data.NestedDictionary["key"] = new Dictionary<string, string>();
            persistent.Data.NestedDictionary["key"]["innerKey"] = "innerValue";
            persistent.Save();

            persistent.Dispose();

            var persistent2 = factory.Create<UnusualData>("SerializeUnusualType");
            Assert.IsTrue(persistent2.Data.VersionData == version);
            Assert.IsTrue(persistent2.Data.SomeEnumData == someEnum);
            Assert.IsTrue(persistent2.Data.NestedDictionary["key"]["innerKey"] == "innerValue");
        }

        [TestMethod]
        public void VerifyCannotUseDisposed()
        {
            var factory = CreateTestFactory();

            var persistent = factory.Create<UnusualData>("VerifyCannotUseDisposed");
            persistent.Data.SomeEnumData = SomeEnum.two;
            persistent.Save();
            persistent.Dispose();
            bool exceptionThrown = false;
            try
            {
                persistent.Save();
            }
            catch (Exception exception)
            {
                if (exception.Message == "Cannot use this object after disposal")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public void AutoSave()
        {
            var factory = CreateTestFactory();

            var persistent = factory.Create<Simple>("AutoSave");
            persistent.Data.Value = 5;
            persistent.RequireSave();
            Thread.Sleep(TimeSpan.FromSeconds(10));
            persistent.Update();
            persistent.Dispose();

            var persistent2 = factory.Create<Simple>("AutoSave");
            Assert.IsTrue(persistent2.Data.Value == 5);
        }

        [TestMethod]
        public void SaveAndLoadToDifferentType()
        {
            var factory = CreateTestFactory();
            var p1 = factory.Create<SomeSettings>("Settings");
            p1.Data.SomeName = "THIS IS SPARTA!";
            p1.Save();
            p1.Dispose();

            var factory2 = CreateTestFactory(false);
            var p2 = factory2.Create<SomeSettingsRenamed>("Settings");
            Assert.IsTrue(p2.Data.SomeName == "THIS IS SPARTA!");
        }

        [TestMethod]
        public void AreIdCaseInsensitive()
        {
            var factory = CreateTestFactory();

            var persistent1 = factory.Create<Simple>("AreIdCaseInsensitive");
            bool excThrown = false;
            try
            {
                var persistent2 = factory.Create<Simple>("areidcaseinsensitive");
            }
            catch (Exception exception)
            {
                if (exception.Message.StartsWith("This objectId is already locked"))
                {
                    excThrown = true;
                }
            }
            Assert.IsTrue(excThrown);
        }

        [TestMethod]
        public void ObjectPreservesDefaults()
        {
            var factory = CreateTestFactory();
            var persistent1 = factory.Create<Type1>("ObjectPreservesDefaults");
            persistent1.Dispose();

            var factory2 = CreateTestFactory();
            var persistent2 = factory2.Create<IncompatibleToType1>("ObjectPreservesDefaults");
            Assert.IsTrue(persistent2.Data.SomeOtherValue == 1 && persistent2.Data.SomeOtherValueInitInCtor == 1);
        }

        #region performance

        [TestMethod]
        public void PerformanceTest()
        {
            var sw = new Stopwatch();
            var factory = CreateTestFactory();

            sw.Start();
            var persistent = factory.Create<FancyData>("fancyData");
            sw.Stop();
            Say("Persistent initial create: " + sw.Elapsed);
            sw.Reset();

            persistent.Data.SomeData = new byte[1024 * 64];
            for (int i = 0; i < 1024; i++)
            {
                persistent.Data.SomeData[i] = 123;
            }

            sw.Start();
            persistent.Save();
            sw.Stop();
            Say("Save initial data: " + sw.Elapsed);
            sw.Reset();

            persistent.Dispose();

            //var allPersistents = new List<Persistent<FancyData>>();

            for (int i = 0; i < 10; i++)
            {
                sw.Reset();
                sw.Start();
                using (var persistentInLoop = factory.Create<FancyData>("fancyData" + i))
                {
                    sw.Stop();
                    Say("Create same objectid loop " + i + " perf: " + sw.Elapsed);
                    sw.Reset();

                    persistent.Data.SomeData = new byte[1024 * 64];
                    for (int j = 0; j < 1024 * j; j++)
                    {
                        persistentInLoop.Data.SomeData[i] = 123;
                    }

                    sw.Start();
                    persistentInLoop.Save();
                    sw.Stop();
                }
                Say("Save same persistent loop " + i + " perf: " + sw.Elapsed);
                sw.Reset();
                Say("");
            }
            Assert.IsTrue(true);
        }

        void Say(object obj)
        {
            Trace.WriteLine(obj.ToString());
        }

        #endregion performance

        #region utility

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        class Type1
        {
            public int SomeValue = 5;
            public int SomeValueInitInCtor;

            public Type1()
            {
                SomeValueInitInCtor = 5;
            }
        }

        class IncompatibleToType1
        {
            public int SomeOtherValue = 1;
            public int SomeOtherValueInitInCtor;

            public IncompatibleToType1()
            {
                SomeOtherValueInitInCtor = 1;
            }
        }

        class Simple
        {
            public int Value;
        }

        class UnusualData
        {
            public Version VersionData;
            public SomeEnum SomeEnumData;
            public Dictionary<string, Dictionary<string, string>> NestedDictionary;
        }

        enum SomeEnum { one, two }

        class SomeSettings
        {
            public string SomeName { get; set; }
            public List<string> SomeNames { get; set; }
            public NestedClass Nested { get; set; }

            public class NestedClass
            {
                public string NestedData = "NestedData";
            }
        }

        class FancyData
        {
            public byte[] SomeData;
        }

        class SomeSettingsRenamed
        {
            public string SomeName { get; set; }
            public List<string> SomeNames { get; set; }
            public NestedClass Nested { get; set; }

            public class NestedClass
            {
                public string NestedData = "NestedData";
            }
        }

        class ReferentialData
        {
            public ReferenceType class1;
            public ReferenceType class2;
            public ReferentialData()
            {
                class1 = class2 = new ReferenceType();
            }
        }

        class WithPrivates
        {
            [JsonProperty]
            private ReferenceType privateMember;
            public ReferenceType publicMember = new ReferenceType() { Something = 42 };

            public void SetPrivateMember()
            {
                privateMember = new ReferenceType() { Something = 43 };
            }

            public int GetPrivateValue()
            {
                return privateMember.Something;
            }
        }

        class ReferenceType
        {
            public int Something = 1;
        }

        #endregion utility
    }
}
