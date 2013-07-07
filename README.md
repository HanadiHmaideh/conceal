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

## Examples

Hiding 'dog.png' with 'pony.png' as cover, written to 'hidden.png'

	conceal pony.png dog.png > hidden.png

Hiding 'dog.png' with 'pony.png' as cover, written to 'hidden.bmp' with bmp format

	conceal -f bmp pony.png dog.png > hidden.bmp
	
Hiding 'dog.png' with 'pony.png' as cover using 2 LSB (1/4th size), written to 'hidden.png'

	conceal -l 2 pony.png dog.png > hidden.png

Hiding 'dog.png' with 'pony.png' as cover using 'secret' as key, written to 'hidden.png'

	conceal -k secret pony.png dog.png > hidden.png
	
Extracting from 'hidden.png' to 'dog.png'

	conceal hidden.png > dog.png
	
Extracting from 'hidden.png' to 'dog.bmp' with bmp format

	conceal -f bmp hidden.png > dog.bmp

Extracting from 'hidden.png' to 'dog.png' using 2 LSB (1/4th size)

	conceal -l 2 hidden.png > dog.png
	
Extracting from 'hidden.png' to 'dog.png'  using 'secret' as key

	conceal -k secret hidden.png > dog.png
	
## Encryption

Encryption uses AES in CFB mode without padding. An invalid key will yield an image with 'noise'.

## Limitations

The LSB (Least Significant Bit) approach replaces lowest significance bits to embed the image.

1 LSB = 0.39% degradation, requires 1/8th of cover image dimensions

2 LSB = 1.18% degradation, requires 1/4th of cover image dimensions

4 LSB = 5.88% degradation, requires 1/2th of cover image dimensions

This approach applies to lossless storage formats, thus the popular JPEG is not viable.

## Conclusion

Written by Roel "Deathspike" van Uden.
