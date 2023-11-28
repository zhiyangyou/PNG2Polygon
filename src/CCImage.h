/****************************************************************************
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2013-2016 Chukong Technologies Inc.
Copyright (c) 2017-2018 Xiamen Yaji Software Co., Ltd.

http://www.cocos2d-x.org

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/

#include <string>
#ifndef __CC_IMAGE_H__
#define __CC_IMAGE_H__
/// @cond DO_NOT_SHOW


// premultiply alpha, or the effect will be wrong when using other pixel formats in Texture2D,
// such as RGB888, RGB5A1
#define CC_RGB_PREMULTIPLY_ALPHA(vr, vg, vb, va) \
    (unsigned)(((unsigned)((unsigned char)(vr) * ((unsigned char)(va) + 1)) >> 8) | \
    ((unsigned)((unsigned char)(vg) * ((unsigned char)(va) + 1) >> 8) << 8) | \
    ((unsigned)((unsigned char)(vb) * ((unsigned char)(va) + 1) >> 8) << 16) | \
    ((unsigned)(unsigned char)(va) << 24))

class Image
{
public:

	/**
	 * @js ctor
	 */
	Image();
	/**
	 * @js NA
	 * @lua NA
	 */
	virtual ~Image();

	/** Supported formats for Image */
	enum class Format
	{
		//! JPEG
		JPG,
		//! PNG
		PNG,
		//! WebP
		WEBP,
		//! PVR
		PVR,
		//! ETC
		ETC,
		//! S3TC
		S3TC,
		//! ATITC
		ATITC,
		//! TGA
		TGA,
		//! Raw Data
		RAW_DATA,
		//! Unknown format
		UNKNOWN
	};

	// Getters
	unsigned char* getData() { return _data; }
	size_t           getDataLen() { return _dataLen; }
	Format            getFileType() { return _fileType; }

	int               getWidth() { return _width; }
	int               getHeight() { return _height; }
	bool initWithImageFile(const std::string& path) { return true; }
	
	unsigned char* _data;
	size_t _dataLen;
	int _width;
	int _height;
	Format _fileType;
};

#endif    // __CC_IMAGE_H__
