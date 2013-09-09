$(document).ready(function() {

	// Do not procede if there are no images
	if (images.length == 0) {
		return;
	}

	var imageIndex = 0;

	var currentImageTag = null;
	var nextImageTag = null;
	var previousImageTag = null;
	var prevspan = $('#prevspan');
	var previmagespan = $('#previmagespan');
	var nextspan = $('#nextspan');
	var nextimagespan = $('#nextimagespan');
	var imagespan = $('#imagespan');
	var nextlink = $('#nextlink');
	var prevlink = $('#prevlink');
	var imagelink = $('#imagelink');

	prevlink.hide();

	// Initialize the first image
	if (images.length > 0) {
		currentImageTag = $('#mainimage');
		currentImageTag.attr('src', images[0].image);
	}

	// Initialize the next image
	if (images.length > 1) {
		nextImageTag = $('#nextimage');
		nextImageTag.attr('src', images[1].image);
		nextImageTag.attr('width', images[1].thumbnail_width);
		nextImageTag.attr('height', images[1].thumbnail_height);
	} else {
		nextspan.hide();
	}
	
	var jWindow = $(window);
	var slideshownav = $('.slideshownav');

	// Resizing code
	// ----------------
	var handleResize = function() {
		var width = jWindow.width() - 20;	
		var height = jWindow.height() - 20;
		
		var aspectRatio = images[imageIndex].width / images[imageIndex].height;
		
		// Cacluate dimesions that fill height
		var widthByHeight = aspectRatio * height;
		
		// Cacluate dimesions that fill width
		var heightByWidth = width / aspectRatio;
		
		if (widthByHeight <= width)
		{
			currentImageTag.css({top: 10, left: ((width - widthByHeight) / 2) + 10});
			currentImageTag.attr('width', widthByHeight);
			currentImageTag.attr('height', height);
		}
		else
		{
			currentImageTag.css({left: 10, top: ((height - heightByWidth) / 2) + 10});
			currentImageTag.attr('width', width);
			currentImageTag.attr('height', heightByWidth);
		}
	}
	
	jWindow.resize(function () {
   		handleResize();
	});
	handleResize();
	
	// Hide navigation except when the mouse moves
	// -------------
	var mousemovecancel = null;
	jWindow.mousemove(function() {
	    clearTimeout(mousemovecancel);
	    slideshownav.show();
	    mousemovecancel = setTimeout(function() {
	    	slideshownav.hide();
	    }, 1500);
	});
	
	slideshownav.hide();
	
	var setImage = function() {
	
		// show / hide previous link
		if (imageIndex > 0) {
			prevlink.show();
		} else {
			prevlink.hide();
			previousImageTag = null;
		}
	
		// show / hide next link
		if (imageIndex < images.length - 1) {
			nextlink.show();
		} else {
			nextlink.hide();
		}
		
		handleResize();
	    clearTimeout(mousemovecancel);
	    mousemovecancel = setTimeout(function() {
	    	slideshownav.hide();
	    }, 250);
	};
	
	var next = function() {
		if (imageIndex < images.length - 1) {
		
			currentImageTag.remove();
			
			if (null != previousImageTag) {
				previousImageTag.remove();
			}
			
			previousImageTag = currentImageTag;
			previousImageTag.attr('width', images[imageIndex].thumbnail_width);
			previousImageTag.attr('height', images[imageIndex].thumbnail_height);
			previousImageTag.removeClass('slideshowimage');
		
			previmagespan.append(previousImageTag);
			
			nextImageTag.remove();
			currentImageTag = nextImageTag;
			nextImageTag.addClass('slideshowimage');
			
			imagespan.append(nextImageTag);
		
			imageIndex++;
			
			if (imageIndex < images.length - 1) {
				nextImageTag = $('<img />');
				nextImageTag.attr('width', images[imageIndex + 1].thumbnail_width);
				nextImageTag.attr('height', images[imageIndex + 1].thumbnail_height);
				nextImageTag.attr('src', images[imageIndex + 1].image);
				nextimagespan.append(nextImageTag);
			} else {
				nextImageTag = null;
			}

			setImage();
		}
		
		return false;
	};
	
	var prev = function() {
		if (imageIndex > 0) {
			currentImageTag.remove();
			
			if (null != nextImageTag) {
				nextImageTag.remove();
			}
			
			nextImageTag = currentImageTag;
			nextImageTag.attr('width', images[imageIndex].thumbnail_width);
			nextImageTag.attr('height', images[imageIndex].thumbnail_height);
			nextImageTag.removeClass('slideshowimage');
		
			nextimagespan.append(nextImageTag);
			
			previousImageTag.remove();
			currentImageTag = previousImageTag;
			previousImageTag.addClass('slideshowimage');
			
			imagespan.append(previousImageTag);
		
			imageIndex--;
			
			if (imageIndex > 0) {
				previousImageTag = $('<img />');
				previousImageTag.attr('width', images[imageIndex - 1].thumbnail_width);
				previousImageTag.attr('height', images[imageIndex - 1].thumbnail_height);
				previousImageTag.attr('src', images[imageIndex - 1].image);
				previmagespan.append(previousImageTag);
			} else {
				previousImageTag = null;
			}

			setImage();
		}
		
		return false;
	};
	
	nextlink.click(next);
	imagelink.click(next);
	prevlink.click(prev);
	$(document).keydown(function(e) {
		if (e.keyCode == 37 || e.keyCode == 38) { 
			prev();
			return false;
		} else if (e.keyCode == 39 || e.keyCode == 40 || e.keyCode == 13 || e.keyCode == 32) { 
			next();
			return false;
		}
	});
});