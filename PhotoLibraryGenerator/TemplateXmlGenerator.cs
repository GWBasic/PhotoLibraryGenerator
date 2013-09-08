using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace PhotoLibraryGenerator
{
	public static class TemplateXmlGenerator
	{
		private const string NamespaceURI = "generator";
		private const string imageName = "image";
		private const string titleName = "title";

		public static void GenerateTemplate(string templateXmlFile, string destination, IEnumerable<string> imageNames, Dictionary<string, Dictionary<string, string>> replacementStrings, string title)
		{
			var xml = new XmlDocument();
			xml.Load(templateXmlFile);
			
			var templateNodes = TemplateXmlGenerator.Flatten(xml.ChildNodes).Where(
				n => n.LocalName == TemplateXmlGenerator.imageName && n.NamespaceURI == TemplateXmlGenerator.NamespaceURI).ToArray();
			
			foreach (var templateNode in templateNodes)
			{
				foreach (var imageName in imageNames)
				{
					TemplateXmlGenerator.ImplementTemplate(xml, templateNode, imageName, replacementStrings[imageName]);
				}

				templateNode.ParentNode.RemoveChild(templateNode);
			}

			TemplateXmlGenerator.ImplementTitle(xml, title);
			
			var filename = Path.GetFileName(templateXmlFile);
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}
			xml.Save(Path.Combine(destination, filename));
		}
		
		public static void GenerateDetails(string detailXmlFile, string destination, IEnumerable<string> imageNames, Dictionary<string, Dictionary<string, string>> replacementStrings, string title)
		{
			var xmlText = File.ReadAllText(detailXmlFile);

			foreach (var imageName in imageNames)
			{
				var xml = new XmlDocument();
				xml.LoadXml(xmlText);

				var templateNodes = TemplateXmlGenerator.Flatten(xml.ChildNodes).Where(
					n => n.LocalName == TemplateXmlGenerator.imageName && n.NamespaceURI == TemplateXmlGenerator.NamespaceURI).ToArray();
				
				foreach (var templateNode in templateNodes)
				{
					TemplateXmlGenerator.ImplementTemplate(xml, templateNode, imageName, replacementStrings[imageName]);
					templateNode.ParentNode.RemoveChild(templateNode);
				}

				TemplateXmlGenerator.ImplementTitle(xml, title);

				var destinationPath = Path.Combine(destination, imageName + ".xhtml");
				if (File.Exists(destinationPath))
				{
					File.Delete(destinationPath);
				}
				xml.Save(destinationPath);
			}
		}
		
		private static IEnumerable<XmlNode> Flatten(XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				yield return node;

				foreach (var subNode in TemplateXmlGenerator.Flatten(node.ChildNodes))
				{
					yield return subNode;
				}
			}
		}

		private static void ImplementTemplate(XmlDocument xml, XmlNode templateNode, string imageName, Dictionary<string, string> strings)
		{
			var newNode = templateNode.Clone();

			var childNodes = new List<XmlNode>();
			childNodes.AddRange(TemplateXmlGenerator.Flatten(newNode.ChildNodes));

			foreach (var node in childNodes)
			{
				foreach (var kvp in strings)
				{
					var name = '[' + kvp.Key + ']';
					var value = kvp.Value;

					if (node is XmlText)
					{
						var textNode = (XmlText)node;
						textNode.InnerText = textNode.InnerText.Replace(name, value);
					}
					else
					{
						foreach (XmlAttribute attribube in node.Attributes)
						{
							attribube.Value = attribube.Value.Replace(name, value);
						}
					}
				}
			}

			newNode = xml.ImportNode(newNode, deep: true);
			childNodes.Clear();
			childNodes.AddRange(newNode.ChildNodes.Cast<XmlNode>());

			foreach (var childNode in childNodes)
			{
				templateNode.ParentNode.InsertBefore(childNode, templateNode);
			}
		}

		private static void ImplementTitle(XmlDocument xml, string title)
		{
			var titleNodes = TemplateXmlGenerator.Flatten(xml.ChildNodes).Where(
				n => n.LocalName == TemplateXmlGenerator.titleName && n.NamespaceURI == TemplateXmlGenerator.NamespaceURI).ToArray();

			foreach (var titleNode in titleNodes)
			{
				var newNode = xml.CreateTextNode(title);

				titleNode.ParentNode.InsertBefore(newNode, titleNode);
				titleNode.ParentNode.RemoveChild(titleNode);
			}
		}
	}
}

