namespace CSC.Common.Infrastructure.Image
{
	/// <summary>
	/// Downsamples images.
	/// </summary>
	public interface IImageProcessor
	{
		/// <summary>
		/// Downsamples the given image if the width/height exceeds the
		/// given limits, and returns the result as a JPEG. Returns false
		/// if the image did not need to be downsampled, or if the given
		/// file is not a valid image.
		/// </summary>
		bool DownsampleImageToJpeg(
			byte[] imageData,
			int maxWidth,
			int maxHeight,
			out byte[] newImageData);
	}
}