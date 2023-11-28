
#include "CCImage.h"
#include "CCImage.h"

#include <cassert>
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"
Image::Image(const std::string& path) :
	_data(nullptr)
	, _dataLen(0)
	, _fileType(Image::Format::PNG)
	, _height(0)
	, _width(0)
{
	initWithImageFile(path);
}

Image::~Image()
{
	if (_data)
	{
		stbi_image_free(_data);
	}
}

bool Image::initWithImageFile(const std::string& path)
{
	const char* filename = path.c_str();

	int   numChannels;
	unsigned char* data = stbi_load(filename, &_width, &_height, &numChannels, 0);
	if (!data)
	{
		assert(false, "load png file failed...");
	}
	if (numChannels != 4)
	{
		assert(false, "png file must be 4 channels");
	}

	_data = data;
	_dataLen = _width * _height * numChannels;

	return _dataLen > 0;
}

