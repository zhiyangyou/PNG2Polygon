

#include "PNGImage.h"

#include <cassert>
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"


PNGImage::PNGImage(const char* filename)
{

	_fileName = "";
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
	_fileName = std::string(filename);
	_data = data;
	_dataLen = _width * _height * numChannels;
}

PNGImage::~PNGImage()
{
	if (_data)
	{
		stbi_image_free(_data);
	}
}

unsigned char* PNGImage::getData()
{
	return _data;
}

size_t PNGImage::getDataLen()
{
	return _dataLen;
}

int PNGImage::getWidth()
{
	return _width;
}

int PNGImage::getHeight()
{
	return _height;
}

std::string PNGImage::getFileName()
{
	return _fileName;
}
