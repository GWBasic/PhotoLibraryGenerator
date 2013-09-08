using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace PhotoLibraryGenerator
{
	public class Config
	{
		public Config (string configFile)
		{
			/*using (var stream = File.OpenRead(configFile))
			using (var configReader = XmlReader.Create(stream))
			{
				configReader.MoveToContent();

				while (configReader.Read())
				{

				}
			}*/

			var configXml = new XmlDocument();
			configXml.Load(configFile);

			foreach (XmlNode node in configXml.DocumentElement.ChildNodes)
			{
				switch (node.Name)
				{
					case "size":
						this.sizes.Add(new Size(
							pixelsAttribute: node.Attributes["pixels"],
							nameAttribute: node.Attributes["name"],
							maxWidthAttribute: node.Attributes["maxwidth"],
							maxHeightAttribute: node.Attributes["maxheight"],
							minWidthAttribute: node.Attributes["minwidth"],
							minHeightAttribute: node.Attributes["minheight"],
							widthAttribute: node.Attributes["width"],
							heightAttribute: node.Attributes["height"],
							qualityAttribute: node.Attributes["quality"]));
					break;

					case "template":
						this.templates.Add(new XhtmlFile(node.Attributes["file"].Value));
						break;

					case "detail":
						this.details.Add(new XhtmlFile(node.Attributes["file"].Value));
						break;
						
					case "copy":
						this.copyFiles.Add(new XhtmlFile(node.Attributes["file"].Value));
						break;

					default:
						Console.WriteLine("I do not know how to handle {0}", node.OuterXml);
						break;
				}
			}
		}

		public IEnumerable<Size> Sizes 
		{
			get 
			{
				foreach (var size in this.sizes)
				{
					yield return size;
				}
			}
		}
		private readonly List<Size> sizes = new List<Size>();

		public IEnumerable<XhtmlFile> Templates 
		{
			get 
			{
				foreach (var template in this.templates)
				{
					yield return template;
				}
			}
		}
		private readonly List<XhtmlFile> templates = new List<XhtmlFile>();

		public IEnumerable<XhtmlFile> Details 
		{
			get
			{
				foreach (var detail in this.details)
				{
					yield return detail;
				}
			}
		}
		private readonly List<XhtmlFile> details = new List<XhtmlFile>();

		public IEnumerable<XhtmlFile> CopyFiles 
		{
			get 
			{
				foreach (var copyFile in this.copyFiles)
				{
					yield return copyFile;
				}
			}
		}
		private readonly List<XhtmlFile> copyFiles = new List<XhtmlFile>();

		public class Size
		{
			public Size(
				XmlAttribute pixelsAttribute,
				XmlAttribute nameAttribute,
				XmlAttribute minWidthAttribute,
				XmlAttribute minHeightAttribute,
				XmlAttribute maxWidthAttribute,
				XmlAttribute maxHeightAttribute,
				XmlAttribute widthAttribute,
				XmlAttribute heightAttribute,
				XmlAttribute qualityAttribute)
			{
				this.name = nameAttribute.Value;

				if (null != pixelsAttribute)
				{
					int pixels;
					if (int.TryParse(pixelsAttribute.Value, out pixels))
					{
						this.pixels = pixels;
					}
				}

				if (null != minWidthAttribute)
				{
					int maxWidth;
					if (int.TryParse(minWidthAttribute.Value, out maxWidth))
					{
						this.maxWidth = maxWidth;
					}
				}

				if (null != minHeightAttribute)
				{
					int minHeight;
					if (int.TryParse(minHeightAttribute.Value, out minHeight))
					{
						this.minHeight = minHeight;
					}
				}

				if (null != maxWidthAttribute)
				{
					int maxWidth;
					if (int.TryParse(maxWidthAttribute.Value, out maxWidth))
					{
						this.maxWidth = maxWidth;
					}
				}
				
				if (null != maxHeightAttribute)
				{
					int maxHeight;
					if (int.TryParse(maxHeightAttribute.Value, out maxHeight))
					{
						this.maxHeight = maxHeight;
					}
				}

				if (null != widthAttribute)
				{
					int width;
					if (int.TryParse(widthAttribute.Value, out width))
					{
						this.width = width;
					}
				}
				
				if (null != heightAttribute)
				{
					int height;
					if (int.TryParse(heightAttribute.Value, out height))
					{
						this.height = height;
					}
				}
				
				if (null != qualityAttribute)
				{
					long quality;
					if (long.TryParse(qualityAttribute.Value, out quality))
					{
						this.quality = quality;
					}
				}
			}

			public string Name 
			{
				get { return this.name; }
			}
			private readonly string name;

			public int? Pixels
			{
				get { return this.pixels;	}
			}
			private readonly int? pixels = null;
			
			public int? MaxWidth 
			{
				get { return this.maxWidth;	}
			}
			private readonly int? maxWidth = null;

			public int? MaxHeight 
			{
				get { return this.maxHeight; }
			}
			private readonly int? maxHeight = null;

			public int? MinWidth 
			{
				get { return this.minWidth;	}
			}
			private readonly int? minWidth = null;
			
			public int? MinHeight 
			{
				get { return this.minHeight; }
			}
			private readonly int? minHeight = null;

			public int? Width 
			{
				get { return this.width;	}
			}
			private readonly int? width = null;
			
			public int? Height 
			{
				get { return this.height; }
			}
			private readonly int? height = null;

			public long? Quality
			{
				get { return quality; }
			}
			private readonly long? quality = null;
		}

		public class XhtmlFile
		{
			public XhtmlFile(string file)
			{
				this.file = file;
			}

			public string File 
			{
				get { return this.file; }
			}
			private readonly string file;
		}
	}
}

