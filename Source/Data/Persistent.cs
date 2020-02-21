using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace PawnRules.Data
{
    internal static class Persistent
    {
        private const string ExportsDirectoryName = "Plans";
        private const string ExportsExtension = ".xml";
        private const string ExportPrefix = "Plan_";

        private static readonly Regex ValidNameRegex = new Regex("^(?:[\\p{L}\\p{N}_\\-]|[\\p{L}\\p{N}_\\-]+[\\p{L}\\p{N}_\\- ]*[\\p{L}\\p{N}_\\-]+)$");

        private static readonly DirectoryInfo ExportsDirectory = Mod.ConfigDirectory.CreateSubdirectory(ExportsDirectoryName);

        private static FileInfo GetPlanFile(string name) => new FileInfo(Path.Combine(ExportsDirectory.FullName, ExportPrefix + name + ExportsExtension));
        private static string GetPlanName(FileInfo file) => Path.GetFileNameWithoutExtension(file.Name).Substring(ExportPrefix.Length);

        public static IEnumerable<string> GetPlans() => ExportsDirectory.GetFiles(ExportPrefix + "*" + ExportsExtension).OrderByDescending(file => file.LastAccessTime).Select(GetPlanName);

        public static void DeletePlan(string name)
        {
            var file = GetPlanFile(name);

            if (!file.Exists) { return; }

            file.Delete();
        }

        public static bool Load(string name)
        {
            var file = GetPlanFile(name);
            if (!file.Exists) { return false; }

            var xml = XDocument.Load(file.FullName).Root;

            Registry.FromXml(xml);

            return true;
        }

        public static void Save(string name = null)
        {
            var file = GetPlanFile(name.NullOrEmpty() ? CreateDefaultName() : name);
            if (file.Exists) { file.Delete(); }

            var doc = new XDocument();
            doc.Add(Registry.ToXml());

            doc.Save(file.FullName);
        }

        public static bool NameIsValid(string name) => !name.NullOrEmpty() && (name.Length <= (250 - ExportsDirectory.FullName.Length)) && ValidNameRegex.IsMatch(name);

        public static string CreateDefaultName() => Faction.OfPlayer.Name + "_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
    }
}
