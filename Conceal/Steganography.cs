// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System;
using System.Drawing;
using System.Net;

namespace Conceal {
	/// <summary>
	/// Represents the steganography provider.
	/// </summary>
	public static class Steganography {
		#region Private
		/// <summary>
		/// Create a color using a callback for each channel.
		/// </summary>
		/// <param name="Source">The source color.</param>
		/// <param name="Target">The target color.</param>
		/// <param name="Callback">The callback.</param>
		private static Color _Create(Color Source, Color Target, Func<byte, byte, byte> Callback) {
			// Invoke the callback for the alpha channel.
			byte A = Callback(Source.A, Target.A);
			// Invoke the callback for the red channel.
			byte R = Callback(Source.R, Target.R);
			// Invoke the callback for the green channel.
			byte G = Callback(Source.G, Target.G);
			// Invoke the callback for the blue channel.
			byte B = Callback(Source.B, Target.B);
			// Return the modified color.
			return Color.FromArgb(A, R, G, B);
		}

		/// <summary>
		/// Create a color using a callback for each channel.
		/// </summary>
		/// <param name="Source">The source color.</param>
		/// <param name="Callback">The callback.</param>
		private static Color _Create(Color Source, Func<byte, byte> Callback) {
			// Invoke the callback for the alpha channel.
			byte A = Callback(Source.A);
			// Invoke the callback for the red channel.
			byte R = Callback(Source.R);
			// Invoke the callback for the green channel.
			byte G = Callback(Source.G);
			// Invoke the callback for the blue channel.
			byte B = Callback(Source.B);
			// Return the modified color.
			return Color.FromArgb(A, R, G, B);
		}
		#endregion

		#region Public
		/// <summary>
		/// Embed the target image into the source image.
		/// </summary>
		/// <param name="Source">The source image.</param>
		/// <param name="Target">The target image.</param>
		/// <param name="LSB">The number of least significant bits.</param>
		public static Bitmap Embed(Bitmap Source, Bitmap Target, byte LSB) {
			// Initialize the size of a step.
			byte SizeOfStep = (byte)(8 / LSB);
			// Check if the target image cannot be contained in the source image.
			if (Source.Width / SizeOfStep != Target.Width || Source.Height / SizeOfStep != Target.Height) {
				// Return null.
				return null;
			} else {
				// Initialize a new instance of the Bitmap class.
				Bitmap Result = new Bitmap(Source);
				// Iterate through the X-axis of the source image.
				for (int X = 0; X < Source.Width; X += SizeOfStep) {
					// Iterate through the Y-axis of the source image.
					for (int Y = 0; Y < Source.Height; Y += SizeOfStep) {
						// Iterate through each step.
						for (byte CurrentStep = 0; CurrentStep < SizeOfStep; CurrentStep++) {
							// Initialize the preservation bits.
							byte Bits = 0;
							// Initialize the bit shift.
							byte Shift = (byte)(CurrentStep * LSB);
							// Iterate through the number of least significant bits.
							for (byte N = 0; N < LSB; N++) {
								// Add the shifted bits to the preservation bits.
								Bits |= (byte)(1 << (Shift + N));
							}
							// Create a color using a callback for each channel and add the color to the result.
							Result.SetPixel(X + CurrentStep, Y + CurrentStep, _Create(Source.GetPixel(X + CurrentStep, Y + CurrentStep), Target.GetPixel(X / SizeOfStep, Y / SizeOfStep), (S, T) => {
								// Clear the right bits from the source pixel.
								S = (byte)(S >> LSB << LSB);
								// Preserve the bits from the target pixel.
								T = (byte)(T & Bits);
								// Add the preserved bits to the source pixel.
								return (byte)(S | (T >> Shift));
							}));
						}
					}
				}
				// Return the result.
				return Result;
			}
		}

		/// <summary>
		/// Embed the target image into the source image.
		/// </summary>
		/// <param name="Key">The encode key.</param>
		/// <param name="Source">The path to the source image.</param>
		/// <param name="Target">The path to the target image.</param>
		/// <param name="LSB">The number of least significant bits.</param>
		public static Bitmap Embed(string Key, string Source, string Target, byte LSB) {
			// Check if the encode key is invalid.
			if (string.IsNullOrWhiteSpace(Key)) {
				// Embed the target image into the source image.
				return Embed(Source, Target, LSB);
			}
			// Initialize a new instance of the Bitmap class.
			using (Bitmap SourceImage = (Bitmap)Bitmap.FromStream(new WebClient().OpenRead(Source))) {
				// Initialize a new instance of the Bitmap class.
				using (Bitmap TargetImage = (Bitmap)Bitmap.FromStream(new WebClient().OpenRead(Target))) {
					// Encode the image.
					using (Bitmap EncodedImage = Cryptography.Encode(Key, TargetImage)) {
						// Embed the target image into the source image.
						return Embed(SourceImage, EncodedImage, LSB);
					}
				}
			}
		}

		/// <summary>
		/// Embed the target image into the source image.
		/// </summary>
		/// <param name="Source">The path to the source image.</param>
		/// <param name="Target">The path to the target image.</param>
		/// <param name="LSB">The number of least significant bits.</param>
		public static Bitmap Embed(string Source, string Target, byte LSB) {
			// Initialize a new instance of the Bitmap class.
			using (Bitmap SourceImage = (Bitmap)Bitmap.FromStream(new WebClient().OpenRead(Source))) {
				// Initialize a new instance of the Bitmap class.
				using (Bitmap TargetImage = (Bitmap)Bitmap.FromStream(new WebClient().OpenRead(Target))) {
					// Embed the target image into the source image.
					return Embed(SourceImage, TargetImage, LSB);
				}
			}
		}

		/// <summary>
		/// Extract an image from the source image.
		/// </summary>
		/// <param name="Source">The source image.</param>
		/// <param name="LSB">The number of least significant bits.</param>
		public static Bitmap Extract(Bitmap Source, byte LSB) {
			// Initialize the size of a step.
			byte SizeOfStep = (byte)(8 / LSB);
			// Check if the target image cannot be contained in the source image.
			if (Source.Width % SizeOfStep != 0 || Source.Height % SizeOfStep != 0) {
				// Return null.
				return null;
			} else {
				// Initialize a new instance of the Bitmap class.
				Bitmap Result = new Bitmap(Source.Width / SizeOfStep, Source.Height / SizeOfStep);
				// Iterate through the X-axis of the source image.
				for (int X = 0; X < Source.Width; X += SizeOfStep) {
					// Iterate through the Y-axis of the source image.
					for (int Y = 0; Y < Source.Height; Y += SizeOfStep) {
						// Initialize each partial channel and the preservation bits.
						byte A = 0, Bits = 0, R = 0, G = 0, B = 0;
						// Iterate through the number of least significant bits.
						for (byte N = 0; N < LSB; N++) {
							// Add the shifted bits to the preservation bits.
							Bits |= (byte)(1 << N);
						}
						// Iterate through each step.
						for (byte CurrentStep = 0; CurrentStep < SizeOfStep; CurrentStep++) {
							// Create a color using a callback for each channel.
							Color PartialColor = _Create(Source.GetPixel(X + CurrentStep, Y + CurrentStep), (S) => {
								// Extract the bits from the source pixel.
								S = (byte)(S & Bits);
								// Shift the bits according to the position.
								return (byte)(S << CurrentStep * LSB);
							});
							// Add the alpha channel of the partial color.
							A += PartialColor.A;
							// Add the red channel of the partial color.
							R += PartialColor.R;
							// Add the green channel of the partial color.
							G += PartialColor.G;
							// Add the blue channel of the partial color.
							B += PartialColor.B;
						}
						// Add the color to the result.
						Result.SetPixel(X / SizeOfStep, Y / SizeOfStep, Color.FromArgb(A, R, G, B));
					}
				}
				// Return the result.
				return Result;
			}
		}

		/// <summary>
		/// Extract an image from the source image.
		/// </summary>
		/// <param name="Key">The decode key.</param>
		/// <param name="Source">The path to the source image.</param>
		/// <param name="LSB">The number of least significant bits.</param>
		public static Bitmap Extract(string Key, string Source, byte LSB) {
			// Check if the encode key is invalid.
			if (string.IsNullOrWhiteSpace(Key)) {
				// Extract an image from the source image.
				return Extract(Source, LSB);
			}
			// Initialize a new instance of the Bitmap class.
			using (Bitmap SourceImage = (Bitmap)Bitmap.FromStream(new WebClient().OpenRead(Source))) {
				// Extract an image from the source image.
				using (Bitmap ExtractedImage = Extract(SourceImage, LSB)) {
					// Decode the image.
					return Cryptography.Decode(Key, ExtractedImage);
				}
			}
		}

		/// <summary>
		/// Extract an image from the source image.
		/// </summary>
		/// <param name="Source">The path to the source image.</param>
		/// <param name="LSB">The number of least significant bits.</param>
		public static Bitmap Extract(string Source, byte LSB) {
			// Initialize a new instance of the Bitmap class.
			using (Bitmap SourceImage = (Bitmap)Bitmap.FromStream(new WebClient().OpenRead(Source))) {
				// Extract an image from the source image.
				return Extract(SourceImage, LSB);
			}
		}
		#endregion
	}
}