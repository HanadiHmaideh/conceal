// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Conceal {
	/// <summary>
	/// Represents the cryptography provider.
	/// </summary>
	public static class Cryptography {
		#region Private
		/// <summary>
		/// Read each pixel from the source image.
		/// </summary>
		/// <param name="Source">The source image.</param>
		private static byte[] _From(Bitmap Source) {
			// Lock the image into system memory.
			BitmapData BitmapData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), ImageLockMode.ReadWrite, Source.PixelFormat);
			// Calculate the length in bytes.
			int Length = BitmapData.Stride * BitmapData.Height;
			// Initialize the buffer.
			byte[] Buffer = new byte[Length];
			// Copy the pixel information from the image to the buffer.
			Marshal.Copy(BitmapData.Scan0, Buffer, 0, Length);
			// Unlock the image from system memory.
			Source.UnlockBits(BitmapData);
			// Return the buffer.
			return Buffer;
		}

		/// <summary>
		/// Write each pixel to the target image.
		/// </summary>
		/// <param name="Target">The target image.</param>
		/// <param name="Buffer">The buffer containing pixel data.</param>
		private static void _To(Bitmap Target, byte[] Buffer) {
			// Lock the image into system memory.
			BitmapData BitmapData = Target.LockBits(new Rectangle(0, 0, Target.Width, Target.Height), ImageLockMode.ReadWrite, Target.PixelFormat);
			// Calculate the length in bytes.
			int Length = BitmapData.Stride * BitmapData.Height;
			// Copy the pixel information from the buffer to the image.
			Marshal.Copy(Buffer, 0, BitmapData.Scan0, Buffer.Length);
			// Unlock the image from system memory.
			Target.UnlockBits(BitmapData);
		}

		/// <summary>
		/// Transform the image.
		/// </summary>
		/// <param name="Key">The key.</param>
		/// <param name="Source">The source image.</param>
		/// <param name="CreateCryptor">The cryptography transformation creation function.</param>
		public static Bitmap _Transform(string Key, Bitmap Source, Func<RijndaelManaged, Func<byte[], byte[], ICryptoTransform>> CreateCryptor) {
			// Initialize the buffer.
			byte[] Buffer;
			// Initalize a new instance of the Bitmap class.
			Bitmap Result = new Bitmap(Source.Width, Source.Height);
			// Initialize a new instance of the Rfc2898DeriveBytes class.
			using (Rfc2898DeriveBytes Rfc2898DeriveBytes = new Rfc2898DeriveBytes(Key, Convert.FromBase64String("U2ltcGxlU3RlZ28="))) {
				// Initialize a new instance of the RijndaelManaged class.
				using (RijndaelManaged RijndaelManaged = new RijndaelManaged()) {
					// Set the symmetric algorithm operation mode.
					RijndaelManaged.Mode = CipherMode.CFB;
					// Set the symmetric algorithm padding mode.
					RijndaelManaged.Padding = PaddingMode.None;
					// Initialize the cryptography transformation.
					using (ICryptoTransform CryptoTransform = CreateCryptor(RijndaelManaged)(Rfc2898DeriveBytes.GetBytes(32), Rfc2898DeriveBytes.GetBytes(16))) {
						// Initialize a new instance of the MemoryStream class.
						using (MemoryStream MemoryStream = new MemoryStream()) {
							// Initialize a new instance of the CryptoStream class.
							using (CryptoStream CryptoStream = new CryptoStream(MemoryStream, CryptoTransform, CryptoStreamMode.Write)) {
								// Check if the source image has an invalid pixel format.
								if (Source.PixelFormat != PixelFormat.Format32bppArgb) {
									// Initalize a new instance of the Bitmap class.
									using (Bitmap Clone = new Bitmap(Source)) {
										// Read each pixel from the source image.
										Buffer = _From(Clone);
									}
								} else {
									// Read each pixel from the source image.
									Buffer = _From(Source);
								}
								// Write the sequence of bytes.
								CryptoStream.Write(Buffer, 0, Buffer.Length);
							}
							// Write each pixel to the target image.
							_To(Result, MemoryStream.ToArray());
							// Return the result.
							return Result;
						}
					}
				}
			}
		}
		#endregion

		#region Public
		/// <summary>
		/// Decode the image.
		/// </summary>
		/// <param name="Key">The decode key.</param>
		/// <param name="Source">The source image.</param>
		public static Bitmap Decode(string Key, Bitmap Source) {
			// Transform the image.
			return _Transform(Key, Source, (RijndaelManaged) => RijndaelManaged.CreateDecryptor);
		}

		/// <summary>
		/// Encode the image.
		/// </summary>
		/// <param name="Key">The encode key.</param>
		/// <param name="Source">The source image.</param>
		public static Bitmap Encode(string Key, Bitmap Source) {
			// Transform the image.
			return _Transform(Key, Source, (RijndaelManaged) => RijndaelManaged.CreateEncryptor);
		}
		#endregion
	}
}