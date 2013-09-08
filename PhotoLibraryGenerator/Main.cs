using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace PhotoLibraryGenerator
{
	class MainClass
	{
		private static readonly DateTime JavascriptEpoc = new DateTime(1970, 1, 1, 0,0,0, DateTimeKind.Utc);

		public static void Main (string[] args)
		{
			try
			{
				var configFile = "config.xml";

				if (args.Length > 0)
				{
					configFile = args[0];
				}

				configFile = Path.GetFullPath(configFile);

				var config = new Config(configFile);

				string path;
				if (args.Length > 1)
				{
					path = Path.GetFullPath(args[1]);
				}
				else
				{
					path = Environment.CurrentDirectory;
				}

				string destination;
				if (args.Length > 2)
				{
					destination = Path.GetFullPath(args[2]);
				}
				else
				{
					destination = Path.Combine(path, "generated");
				}

				if (false == Directory.Exists(destination))
				{
					Directory.CreateDirectory(destination);
				}

				var title = "Image Gallery";
				if (args.Length > 3)
				{
					title = args[3];
				}

				foreach (var size in config.Sizes)
				{
					var sizePath = Path.Combine(destination, size.Name);
					if (false == Directory.Exists(sizePath))
					{
						Directory.CreateDirectory(sizePath);
					}
				}

				var imageFilesAndDates = MainClass.GetImageFilesAndDates(path);
				var imageNames = imageFilesAndDates.Keys.OrderBy(n => imageFilesAndDates[n]).ToArray();

				var replacementStrings = new Dictionary<string, Dictionary<string, string>>();

				foreach (var imageName in imageNames)
				{
					var strings = new Dictionary<string, string>();
					replacementStrings[imageName] = strings;

					strings["name"] = imageName;

					var date = imageFilesAndDates[imageName];
					var localDate = date.ToLocalTime();
					strings["date_long"] = localDate.ToLongDateString();
					strings["time_long"] = localDate.ToLongTimeString();
					strings["date_short"] = localDate.ToShortDateString();
					strings["time_short"] = localDate.ToShortTimeString();
					strings["datetime_milliseconds"] = (date - MainClass.JavascriptEpoc).TotalMilliseconds.ToString("R");
					strings["datetime_net"] = date.ToString("o");
					strings["datetime"] = date.ToString("r");
				}

				MainClass.actions = new Queue<Action>(Resizer.GenerateActions(config, path, imageNames, destination, replacementStrings));

				var threads = new List<Thread>();
				for (var ctr = 0; ctr < Environment.ProcessorCount - 1; ctr++)
				{
					var thread = new Thread(MainClass.DoWork)
					{
						Name = "Thread " + ctr.ToString()
					};
					thread.Start();

					threads.Add(thread);
				}

				MainClass.DoWork();

				foreach (var thread in threads)
				{
					thread.Join();
				}

				var templatePath = Path.GetDirectoryName(configFile);
				foreach (var template in config.Templates)
				{
					TemplateXmlGenerator.GenerateTemplate(Path.Combine(templatePath, template.File), destination, imageNames, replacementStrings, title);
				}

				foreach (var template in config.Details)
				{
					TemplateXmlGenerator.GenerateDetails(Path.Combine(templatePath, template.File), destination, imageNames, replacementStrings, title);
				}

				foreach (var copyFile in config.CopyFiles)
				{
					var sourceFileName = Path.Combine(templatePath, copyFile.File);
					var destFileName = Path.Combine(destination, copyFile.File);

					if (File.Exists(destFileName))
					{
						File.Delete(destFileName);
					}

					File.Copy(sourceFileName, destFileName);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		private static HashSet<string> knownExtensions = new HashSet<string>()
		{
			"png", "gif", "jpg", "jpeg"
		};

		private static Dictionary<string, DateTime> GetImageFilesAndDates(string path)
		{
			var imageFilesAndDates = new Dictionary<string, DateTime>();
			foreach (var file in Directory.GetFiles(path))
			{
				var extension = Path.GetExtension(file).Substring(1);

				if (MainClass.knownExtensions.Contains(extension))
				{
					imageFilesAndDates[Path.GetFileName(file)] = File.GetLastWriteTimeUtc(file);
				}
			}

			return imageFilesAndDates;
		}

		private static Queue<Action> actions;

		private static void DoWork()
		{
			try
			{
				Action action;

				while (true)
				{
					lock (MainClass.actions)
					{
						if (MainClass.actions.Count <= 0)
						{
							return;
						}

						action = MainClass.actions.Dequeue();
					}

					action();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Environment.Exit(-1);
			}
		}
	}
}
