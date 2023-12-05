
#include "UnityExport.h"

#include <PNGImage.h>

#include "CCAutoPolygon.h"
#include "UnityImage.h"

// Unity的图片原点在左下角， opencv的在左上角..
void FilpY(Triangles& tri, int rectH)
{
	auto* verts = tri.verts;
	float fRectH = (float)rectH;
	for (int i = 0; i < tri.vertCount; i++)
	{
		verts[i].vertices.y = fRectH - verts[i].vertices.y;
	}
}

extern "C" {
#define CheckData  if (!data) {return -1;}
#define CheckName  if (!name) {return -2;}
#define CheckFileName  if (!png32FileNameFullPath) {return -3;}
#define CheckWH  if (width == 0 || height == 0) {return -4;}
#define CheckWHValid  if (width * height * 4 != dataLen) {return -5;}

#define CheckCallBack  if (!callback) {return -6;}
#define CheckEpsilon  if (epsilon<=0) {return -7;}
#define CheckRectWH  if (rect_width<=0||rect_height<=0) {return -8;}
#define CheckRectLB  if (rect_left<0||rect_bottom<0) {return -9;}
#define SetAlphaValid  		if (alpha_threshold < 0) { alpha_threshold = 0;}

	int DoGenPolygon(AbsImage* image, MeshDataWriteCallback& callback, OverdrawInfo& overdrawCallback,
		int  rect_left,
		int  rect_bottom,
		int  rect_width,
		int  rect_height,
		float epsilon,
		float alpha_threshold,
		bool needFlipY
	)
	{
		PolygonInfo info;
		AutoPolygon p(image);
		p.generateTriangles(info, Rect(rect_left, rect_bottom, rect_width, rect_height), epsilon, alpha_threshold);

		int pointSize = info.triangles.vertCount * sizeof(V3F_C4B_T2F);
		int indexSize = info.triangles.indexCount * sizeof(unsigned short);

		auto rect = p.getRealRect();
		float triangleArea = info.getArea();
		if (needFlipY)
		{
			FilpY(info.triangles, p.getRealRect().size.height);
		}
		size_t pixelArea = image->getPixelArea(alpha_threshold, rect.origin.x, rect.origin.y, rect.size.width, rect.size.height);
		float fullArea = rect.size.width * rect.size.height;

		callback((void*)info.triangles.verts, pointSize, 0);
		callback((void*)info.triangles.indices, indexSize, 1);
		overdrawCallback(triangleArea, pixelArea, fullArea);
		return 0;
	}

	int AutoPolygonGenerateByPngFile(
		char* png32FileNameFullPath,
		int rect_left,
		int rect_bottom,
		int rect_width,
		int rect_height,
		float epsilon,
		float alpha_threshold,
		MeshDataWriteCallback callback,
		OverdrawInfo overdrawCallback
	)
	{
		CheckFileName;
		SetAlphaValid;
		CheckCallBack;
		CheckEpsilon;
		//CheckRectWH;
		CheckRectLB;

		PNGImage pngImage(png32FileNameFullPath);
		return DoGenPolygon(&pngImage, callback, overdrawCallback, rect_left, rect_bottom, rect_width, rect_height, epsilon, alpha_threshold, true);
	}

	int AutoPolygonGenerate(
		unsigned char* data,
		int dataLen,
		int width,
		int height,
		char* name,
		int rect_left,
		int rect_bottom,
		int rect_width,
		int rect_height,
		float epsilon,
		float alpha_threshold,
		MeshDataWriteCallback callback,
		OverdrawInfo overdrawCallback
	)
	{
		SetAlphaValid;
		CheckData;
		CheckName;
		CheckWH;
		CheckWHValid;
		CheckCallBack;
		CheckEpsilon;
		//CheckRectWH;
		CheckRectLB;
		UnityImage unityImage(data, dataLen, width, height, name);
		return DoGenPolygon(&unityImage, callback, overdrawCallback, rect_left, rect_bottom, rect_width, rect_height, epsilon, alpha_threshold, true);
	}
}
