PhotoLibraryGenerator
=====================

A command-line photo library generator, generates xhtml from template files.

Why another image gallery generator? I wanted a photo image gallery generator
that's free, and generates galleries that don't require heavy use of Javascript.
Furthermore, I wanted all generated html and css to be very simple and easy to
edit. Finally, I wanted to make sure that it's easy for viewers to download
original images.

My image gallery generator automatically resizes images. It's very configurable,
so you can have thumbnails, small images, large images, and original images.

The image quality is also configurable, so you can make a page with large,
quick-loading images, yet still keep the originals available.

Xhtml, instead of html, is used because it's very easy to manipulate xml from
.Net.

This image gallery is command-line based, and runs in either .Net or mono.
(Developed on Mac.)

----------------------

Quick start:

Windows:
PhotoLibraryGenerator.exe [configuration file] [path to source images] [path where the library goes] [title]

Mac / Linux
mono PhotoLibraryGenerator.exe [configuration file] [path to source images] [path where the library goes] [title]

Examples, using the included template:
(Windows) PhotoLibraryGenerator.exe config.xml C:\images C:\library "My photo library"
(Mac / Linux) mono PhotoLibraryGenerator.exe config.xml /images /library "My photo library"

----------------------

Configuring the templates

Setting image sizes
To set image sizes, in your config.xml, add or edit a size tag. Every size tag
must have a name attribute, which you will use from xhtml to identify which size
you're including. The size tag supports the following attributes:
- width: The width of the image.
- maxwidth The maximum width of the image
- minWidth: The minimum width of the image
- height: The height of the image
- minHeight: The minimum height of the image
- maxHeight: The maximum height of the image
- pixels: The image will be resized to contain approximately this many pixels
- quality: JPEG quality, between 1 and 100

Examples:
Re-encode the image at 15 quality:
 <size name="smallfile" quality="15" />

Keep the width between 300-800 pixels, and the height at 600 pixels:
 <size name="600tall" minwidth="300" maxwidth="800" height="600" quality="50" />


Configuring the xhtml
The photo library generator uses a custom namespace, "generator." You should declare
it with the xhtml namespace at the top of your file:

<?xml version="1.0" encoding="utf-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:generator="generator">

The generator then looks for <generator:image> tags. It makes a copy of the tag's
contents for each image. (Images are sorted by date.)

The following blocks exist for all images:
	[name] 	The image's original file name
	[date_long]  A long form of the date. This is always generated in your computer's time zone.
	[time_long] = A long form of the time. This is always generated in your computer's time zone.
	[date_short] = A short form of the date. This is always generated in your computer's time zone.
	[time_short] = A short form of the time. This is always generated in your computer's time zone.
	[datetime_milliseconds] = The time, in milliseconds. You can generate a valid (UTC) Javascript date with new Date([datetime_milliseconds])
	[datetime_net] = .Net's round-trip date string
	[datetime] = Javscript (and thus JSON's) round-trip date string
	
A note on dates: The generator always uses your local time zone. If you want to
convert to your viewer's time zone, you will need to use some Javascript.

The generator then replaces [...] blocks with values generated for each image. The
block's names start with the names of the size tags. Thus, from the sample config.xml:
	<size name="thumbnail"...
	<size name="normal"...
	<size name="original"...
	
There are 3 prefixes for [... blocks
	[thumbnail_...
	[normal_...
	[original_...
	
All supportd blocks are:
	[..._image]  The filename for the resized image
	[..._height]  The resized image's height
	[..._width]  The resized image's width
	[..._size]  The resized image's size, changed to KB, MB, or GB
	[..._sizebytes]  The resized image's size, in bytes

See the included index.html, and thumbnails.xhtml, for examples.