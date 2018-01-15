using System;
using System.IO;
using SixLabors.ImageSharp;

namespace CSC.Common.Infrastructure.Image
{
	using Image = SixLabors.ImageSharp.Image;
	
	/// <summary>
	/// Downsamples images.
	/// </summary>
	public class ImageProcessor : IImageProcessor
	{
		/// <summary>
		/// Downsamples the given image if the width/height exceeds the
		/// given limits, and returns the result as a JPEG. Returns false
		/// if the image did not need to be downsampled, or if the given
		/// file is not a valid image.
		/// </summary>
		public bool DownsampleImageToJpeg(
			byte[] imageData,
			int maxWidth,
			int maxHeight,
			out byte[] newImageData)
		{
			newImageData = null;
			
			try
			{
				using (var image = Image.Load(imageData))
				{
					bool resized = false;

					if (image.Width > maxWidth)
					{
						image.Mutate(im => im.Resize(maxWidth, 0));
						resized = true;
					}

					if (image.Height > maxHeight)
					{
						image.Mutate(im => im.Resize(0, maxHeight));
						resized = true;
					}

					if (resized)
					{
						using (var stream = new MemoryStream())
						{
							image.SaveAsJpeg(stream);
							newImageData = stream.ToArray();
							return true;
						}
					}

					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}