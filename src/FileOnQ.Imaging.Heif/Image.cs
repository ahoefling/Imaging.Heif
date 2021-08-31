﻿using System;
using System.Runtime.InteropServices;

namespace FileOnQ.Imaging.Heif
{
	internal unsafe class Image : IImage
	{
		LibHeif.ImageHandle* handle;
		IntPtr encoder;
		IntPtr decodingOptions;
		internal Image(LibHeif.ImageHandle* handle)
		{
			if ((IntPtr)handle == IntPtr.Zero)
				throw new NullReferenceException("Unable to use null image handle");

			this.handle = handle;
		}

		public void Save(string filename, int quality = 90)
		{
			CreateEncoder(quality);
			CreateDecodingOptions();
			
			var bitDepth = LibHeif.GetLumaBitsPerPixel(handle);
			if (bitDepth < 0)
				throw new Exception("Input image has undefined bit-dept, unable to save!");

			var hasAlpha = LibHeif.HasAlphaChannel(handle);
			LibHeif.Image* outputImage;
			var decodeError = LibHeif.DecodeImage(
				handle,
				&outputImage,
				LibEncoder.GetColorSpace(encoder, hasAlpha),
				LibEncoder.GetChroma(encoder, hasAlpha, bitDepth),
				decodingOptions);

			if (decodeError.Code != LibHeif.ErrorCode.Ok)
				throw new Exception(Marshal.PtrToStringAnsi(decodeError.Message));

			if ((IntPtr)outputImage != IntPtr.Zero)
			{
				bool saved = LibEncoder.Encode(encoder, handle, outputImage, filename);
				if (!saved)
					throw new Exception("Unable to save image");
			}
		}

		void CreateEncoder(int quality)
		{
			if (encoder == IntPtr.Zero)
				encoder = LibEncoder.InitJpegEncoder(quality);
			else
			{
				LibEncoder.Free(encoder);
				encoder = LibEncoder.InitJpegEncoder(quality);
			}
		}
		void CreateDecodingOptions()
		{
			if (decodingOptions == IntPtr.Zero)
				decodingOptions = LibHeif.DecodingOptionsAllocate();
			else
			{
				LibHeif.FreeDecodingOptions(decodingOptions);
				decodingOptions = LibHeif.DecodingOptionsAllocate();
			}

			LibEncoder.UpdateDecodingOptions(encoder, handle, decodingOptions);
		}

		~Image() => Dispose(false);

		bool isDisposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (isDisposed)
				return;

			if (disposing)
			{
				// free managed resources
			}

			if (decodingOptions != IntPtr.Zero)
			{
				LibHeif.FreeDecodingOptions(decodingOptions);
				decodingOptions = IntPtr.Zero;
			}

			if ((IntPtr)handle != IntPtr.Zero)
			{
				LibHeif.ReleaseImageHandle(handle);
				handle = null;
			}

			if (encoder != IntPtr.Zero)
			{
				LibEncoder.Free(encoder);
				encoder = IntPtr.Zero;
			}
			

			isDisposed = true;
		}
	}
}