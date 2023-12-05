#include <string>
#ifndef __CC_IMAGE_H__
#define __CC_IMAGE_H__

class AbsImage
{
public:
	virtual ~AbsImage() {};
	virtual size_t getPixelArea(float threshold_alpha, int startX, int startY, int width, int height) { return 0; }
	virtual unsigned char* getData() = 0;
	virtual size_t           getDataLen() = 0;
	virtual int               getWidth() = 0;
	virtual int               getHeight() = 0;
	virtual std::string getFileName() = 0;
};
#endif    // __CC_IMAGE_H__
