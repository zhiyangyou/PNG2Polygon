#pragma once
#include "AbsImage.h"


class UnityImage : public  AbsImage
{

public:
	UnityImage() = delete;
	UnityImage(const UnityImage&) = delete;
	UnityImage(const UnityImage&&) = delete;
	UnityImage(unsigned char* _data,
		size_t _dataLen,
		int _width,
		int _height,
		char* _fileName);
public:
	unsigned char* getData() override;
	size_t getDataLen() override;
	int getWidth() override;
	int getHeight() override;
	std::string getFileName() override;
private:
	unsigned char* _data;
	size_t _dataLen;
	int _width;
	int _height;
	std::string _fileName;

};