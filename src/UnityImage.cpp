

#include "UnityImage.h"

UnityImage::UnityImage(
	unsigned char* data,
	size_t dataLen,
	int width,
	int height,
	char* fileName
) :_data(data)
, _dataLen(dataLen)
, _width(width)
, _height(height)
, _fileName(fileName)
{

}
unsigned char* UnityImage::getData()
{
	return _data;
}

size_t UnityImage::getDataLen()
{
	return _dataLen;
}

int UnityImage::getWidth()
{
	return _width;
}

int UnityImage::getHeight()
{
	return _height;
}

std::string UnityImage::getFileName()
{
	return _fileName;
}

