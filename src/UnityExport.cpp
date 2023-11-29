
#include "UnityExport.h"

#include "CCAutoPolygon.h"
#include "UnityImage.h"

extern "C" {

	int AutoPolygonGenerate(
		unsigned char* data,
		size_t dataLen,
		int width,
		int height,
		char* name,
		float rect_left,
		float rect_bottom,
		float rect_width,
		float rect_height,
		float epsilon,
		float alpha_threshold,
		MeshDataWriteCallback callback
	)
	{
		// TODO 容错，各种以外情况需要即使退出，否则闪退
		UnityImage unityImage(data, dataLen, width, height, name);
		PolygonInfo info;
		AutoPolygon p(&unityImage);
		p.generateTriangles(info, Rect(rect_left, rect_bottom, rect_width, rect_height), epsilon, alpha_threshold);

		int pointSize = info.triangles.vertCount * sizeof(V3F_C4B_T2F);
		int indexSize = info.triangles.indexCount * sizeof(unsigned short);

		callback((void*)info.triangles.verts, pointSize, 0);
		callback((void*)info.triangles.indices, indexSize, 1);

		return 0;
	}
}