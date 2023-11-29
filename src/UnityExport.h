#pragma once


#if _MSC_VER 
#define  EXPORT_DLL __declspec(dllexport)
#else
#define  EXPORT_DLL 
#endif // _MSVC_

extern "C" {

	typedef void (*MeshDataWriteCallback)(void* data, int dataLen, int dataType);
	
	EXPORT_DLL int AutoPolygonGenerate(
		unsigned char* data,		//  像素数据
		int dataLen,					//  像素数据长度
		int width,
		int height,
		char* name,						// 名字
		float rect_left,					// rect的左下角点的x
		float rect_bottom,			// rect的左下角点的y
		float rect_width,				// rect的width
		float rect_height,				// rect的height
		float epsilon,					// 算法容差
		float alpha_threshold,		// alpha阈值
		
		MeshDataWriteCallback callback
	);
}