# conceal

Lossless image steganography using the LSB approach for .NET

## Motivation

This project was written to teach myself how steganography can be applied to images.

## Usage

	conceal [options] source target > output

	-f, --format    The output image format (default: png).

	-k, --key       The cryptographic key (no default).

	-l, --lsb       The number of least significant bits (default: 4).

	--help          Display this help screen.
	
## Limitations

The LSB (Least Significant Bit) approach replaces lowest significance bits to embed the image.

1 LSB = 0.39% degradation, requires 1/8th of cover image dimensions

2 LSB = 1.18% degradation, requires 1/4th of cover image dimensions

4 LSB = 5.88% degradation, requires 1/2th of cover image dimensions

This approach applies to lossless storage formats, thus the popular JPEG is not viable.

## Encryption

Encryption uses AES in CFB mode without padding, using a pre-configured salt and user key.

Extracting an image without a valid key will guarantee to yield 'noise' as image.

## Conclusion

Written by Roel "Deathspike" van Uden.
