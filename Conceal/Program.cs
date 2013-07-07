// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Conceal {
	/// <summary>
	/// Represents the application.
	/// </summary>
	public class Program {
		#region Constructor
		/// <summary>
		/// Initialize a new instance of the Program class.
		/// </summary>
		public Program() {
			// Set the output image format.
			Format = "png";
			// Set the number of least significant bits.
			LSB = 4;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Contains the output image format
		/// </summary>
		[Option('f', "format", HelpText = "The output image format (default: png).")]
		public string Format { get; set; }

		/// <summary>
		/// Contains each item.
		/// </summary>
		[ValueList(typeof(List<string>), MaximumElements = 2)]
		public IList<string> Items { get; set; }

		/// <summary>
		/// Contains the cryptographic key
		/// </summary>
		[Option('k', "key", HelpText = "The cryptographic key (no default).")]
		public string Key { get; set; }

		/// <summary>
		/// Contains the number of least significant bits.
		/// </summary>
		[Option('l', "lsb", HelpText = "The number of least significant bits (default: 4).")]
		public byte LSB { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		[HelpOption]
		public override string ToString() {
			// Create a new instance of the HelpText class using common values.
			HelpText Text = HelpText.AutoBuild(this);
			// Add a text line before options usage strings.
			Text.AddPreOptionsLine("\r\n  Usage: conceal [options] source target > output");
			// Return the text.
			return Text.ToString();
		}
		#endregion

		#region Static
		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="Arguments">Each command line argument.</param>
		public static void Main(string[] Arguments) {
			// Initialize a new instance of the Program class.
			Program Options = new Program();
			// Parse the command line arguments and check if a image is provided.
			if (CommandLine.Parser.Default.ParseArguments(Arguments, Options) && Options.Items.Count != 0) {
				// Attempt the following code.
				try {
					// Initialize the image format.
					PropertyInfo PropertyInfo = typeof(ImageFormat).GetProperties().SingleOrDefault(x => x.Name.Equals(Options.Format, StringComparison.OrdinalIgnoreCase));
					// Check if the image format is valid.
					if (PropertyInfo != null) {
						// Check if an additional image is provided to be embedded.
						if (Options.Items.Count >= 2) {
							// Embed the target image into the source image.
							using (Bitmap Bitmap = Steganography.Embed(Options.Key, Options.Items[0], Options.Items[1], Options.LSB)) {
								// Acquire the standard output.
								using (Stream StandardOutput = Console.OpenStandardOutput()) {
									// Save the image to the standard output.
									Bitmap.Save(StandardOutput, (ImageFormat)PropertyInfo.GetValue(null, null));
								}
							}
						} else {
							// Embed an image from the source image.
							using (Bitmap Bitmap = Steganography.Extract(Options.Key, Options.Items[0], Options.LSB)) {
								// Acquire the standard output.
								using (Stream StandardOutput = Console.OpenStandardOutput()) {
									// Save the image to the standard output.
									Bitmap.Save(StandardOutput, (ImageFormat)PropertyInfo.GetValue(null, null));
								}
							}
						}
					}
				} catch {
					// Stop the function.
					return;
				}
			}
		}
		#endregion
	}
}