#include "AbsImage.h"

class PNGImage : public  AbsImage
{

public:
	PNGImage(const char* filename);
	virtual ~PNGImage();

private:
	unsigned char* _data;
	size_t _dataLen;
	int _width;
	int _height;
	std::string _fileName;
public:
	unsigned char* getData() override;
	size_t getDataLen() override;
	int getWidth() override;
	int getHeight() override;
	std::string getFileName() override;

};