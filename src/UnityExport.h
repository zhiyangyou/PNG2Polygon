#pragma once
#if _MSC_VER 
#define  EXPORT_DLL __declspec(dllexport)
#else
#define  EXPORT_DLL 
#endif // _MSVC_

extern "C" {

	typedef void (*MeshDataWriteCallback)(void* data, int dataLen, int dataType);
	typedef void (*OverdrawInfo)(float triangleArea, int alphaThresholdPixels, float fullArea);

	EXPORT_DLL int AutoPolygonGenerate(
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
	);

	EXPORT_DLL int AutoPolygonGenerateByPngFile(
		char* png32FileNameFullPath,
		int rect_left,
		int rect_bottom,
		int rect_width,
		int rect_height,
		float epsilon,
		float alpha_threshold,
		MeshDataWriteCallback callback,
		OverdrawInfo overdrawCallback
	);
}