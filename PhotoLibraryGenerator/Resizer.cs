using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace PhotoLibraryGenerator
{
	public class Resizer
	{
		public static IEnumerable<Action> GenerateActions(Config config, string path, IEnumerable<string> imageNames, string destination, Dictionary<string, Dictionary<string, string>> replacementStrings)
		{
			foreach (var imageName in imageNames)
			{
				yield return new Resizer()
				{
					config = config,
					imagePath = Path.Combine(path, imageName),
					imageName = imageName,
					destination = destination,
					strings = replacementStrings[imageName]
				}.Resize;
			}
		}

		private Config config;
		private string imagePath;
		private string imageName;
		private string destination;
		private Dictionary<string, string> strings;

		private void Resize()
		{
			Console.WriteLine("Resizing {0}", this.imagePath);
			
			using (var image = Image.FromFile(this.imagePath))
			{
				foreach (var size in this.config.Sizes)
				{
					this.Resize(
						image,
						size,
						imagePath,
						destinationPath: Path.Combine(Path.Combine(this.destination, size.Name), this.imageName));
				}
			}
		}

		private void Resize(Image image, Config.Size size, string imagePath, string destinationPath)
		{
			if (".jpg" != Path.GetExtension(destinationPath))
			{
				var destination = Path.GetDirectoryName(destinationPath);
				var destinationFileName = Path.GetFileNameWithoutExtension(destinationPath);
				destinationPath = Path.Combine(destination, destinationFileName + ".jpg");
			}

			if (File.Exists(destinationPath))
			{
				File.Delete(destinationPath);
			}

			int resizedWidth;
			int resizedHeight;
			
			double aspectRatio = Convert.ToDouble(image.Width) / Convert.ToDouble(image.Height);
			double inverseAspectRatio = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
			
			// First, figure out what the size of the returned image will be based on specified height or width

			if (null != size.Pixels)
			{
				// area = width * height;
				// aspectRatio = width / height;

				// width = area / height;
				// width = height * aspectRatio;
				// height = area / width;
				// height = width / aspectRatio;

				// width = area / (width / aspectRatio)
				// width^2 / aspectRatio = area
				// width^2 = area * aspectRatio
				// width = sqrt(area * aspectRatio)

				// height = area / (height * aspectRatio) 
				// height^2 * aspectRatio = area
				// height^2 = area / aspectRatio
				// height = sqrt(area / aspectRatio)

				resizedWidth = Convert.ToInt32(Math.Sqrt(aspectRatio * Convert.ToDouble(size.Pixels.Value)));
				resizedHeight = Convert.ToInt32(Math.Sqrt(Convert.ToDouble(size.Pixels.Value) / aspectRatio));
			}
			else if ((null != size.Width) && (null != size.Height))
			{
				resizedHeight = size.Height.Value;
				resizedWidth = size.Width.Value;
				aspectRatio = Convert.ToDouble(resizedWidth) / Convert.ToDouble(resizedHeight);
				inverseAspectRatio = Convert.ToDouble(resizedHeight) / Convert.ToDouble(resizedWidth);
			}
			else if (null != size.Width)
			{
				resizedWidth = size.Width.Value;
				resizedHeight = Convert.ToInt32(
					Convert.ToDouble(size.Width.Value) * inverseAspectRatio);
			}
			else if (null != size.Height)
			{
				resizedHeight = size.Height.Value;
				resizedWidth = Convert.ToInt32(
					Convert.ToDouble(size.Height.Value) * aspectRatio);
			}
			else
			{
				resizedWidth = image.Width;
				resizedHeight = image.Height;
			}
			
			// Second, make sure that maxes and mins aren't violated
			
			if (null != size.MaxWidth)
				if (resizedWidth > size.MaxWidth.Value)
			{
				resizedWidth = size.MaxWidth.Value;
				resizedHeight = Convert.ToInt32(
					Convert.ToDouble(size.MaxWidth.Value) * inverseAspectRatio);
			}
			
			if (null != size.MaxHeight)
				if (resizedHeight > size.MaxHeight.Value)
			{
				resizedHeight = size.MaxHeight.Value;
				resizedWidth = Convert.ToInt32(
					Convert.ToDouble(size.MaxHeight.Value) * aspectRatio);
			}
			
			if (null != size.MinWidth)
				if (resizedWidth < size.MinWidth.Value)
			{
				resizedWidth = size.MinWidth.Value;
				resizedHeight = Convert.ToInt32(
					Convert.ToDouble(size.MinWidth.Value) * inverseAspectRatio);
			}
			
			if (null != size.MinHeight)
				if (resizedHeight < size.MinHeight.Value)
			{
				resizedHeight = size.MinHeight.Value;
				resizedWidth = Convert.ToInt32(
					Convert.ToDouble(size.MinHeight.Value) * aspectRatio);
			}

			this.strings[size.Name + "_height"] = resizedHeight.ToString();
			this.strings[size.Name + "_width"] = resizedWidth.ToString();
			this.strings[size.Name + "_image"] = size.Name + "/" + Path.GetFileName(destinationPath);

			// Now, in the rare chance that size doesn't change, and there's no quality set, don't do anything
			if ((resizedWidth == image.Width) && (resizedHeight == image.Height) && (null == size.Quality))
			{
				File.Copy(this.imagePath, destinationPath);

				var len = new FileInfo(this.imagePath).Length;

				this.strings[size.Name + "_sizebytes"] = len.ToString();
				this.strings[size.Name + "_size"] = Resizer.GenerateSize(len);
			}
			else
			{
				// The image must be resized.
				// reference: http://www.glennjones.net/Post/799/Highqualitydynamicallyresizedimageswithnet.htm
				
				var thumbnail = new Bitmap(resizedWidth, resizedHeight);
				Graphics graphic = System.Drawing.Graphics.FromImage(thumbnail);
				
				// Set up high-quality resize mode
				graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphic.SmoothingMode = SmoothingMode.AntiAlias;
				graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphic.CompositingQuality = CompositingQuality.HighQuality;
				
				// do the resize
				graphic.DrawImage(image, 0, 0, resizedWidth, resizedHeight);
				
				// Save as a high quality JPEG
				var info = ImageCodecInfo.GetImageEncoders();
				var encoderParameters = new EncoderParameters(1);
				encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, size.Quality ?? 90L);

				using (var stream = File.OpenWrite(destinationPath))
				{
					thumbnail.Save(stream, info[1], encoderParameters);

					this.strings[size.Name + "_sizebytes"] = stream.Length.ToString();
					this.strings[size.Name + "_size"] = Resizer.GenerateSize(stream.Length);

					stream.Flush();
					stream.Close();
				}
			}
		}

		private static string[] sizes = { "B", "KB", "MB", "GB" };

		private static string GenerateSize(long len)
		{
			int order = 0;
			while (len >= 1024 && order + 1 < Resizer.sizes.Length) 
			{
				order++;
				len = len/1024;
			}
			
			// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
			// show a single decimal place, and no space.
			return string.Format("{0:0.##} {1}", len, Resizer.sizes[order]);
		}
	}
}
