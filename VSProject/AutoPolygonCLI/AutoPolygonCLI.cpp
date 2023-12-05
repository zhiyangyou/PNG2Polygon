

#include <iostream>
#include <fstream>
#include <opencv2/highgui.hpp>

#include "CCAutoPolygon.h"
#include "PNGImage.h"

void print2File(PolygonInfo& info, const char* filename)
{
	std::ofstream outputFile(filename);
	outputFile << "-------begin-------";
	outputFile << "\nverts:\n";
	for (int i = 0; i < info.triangles.vertCount; ++i) {
		outputFile << "(" << info.triangles.verts[i].vertices.x << "," << info.triangles.verts[i].vertices.y << ") ";
	}
	outputFile << "\nindeices:\n";
	for (int i = 0; i < info.triangles.indexCount; ++i) {
		outputFile << info.triangles.indices[i] << " ";
	}
	outputFile << "\nuvs:\n";
	for (int i = 0; i < info.triangles.vertCount; ++i) {
		outputFile << "(" << info.triangles.verts[i].texCoords.u << " , " << info.triangles.verts[i].texCoords.v << ")";
	}
	outputFile << "\n-------end-------" << std::endl;
	outputFile.close();
}
int main()
{
	std::cout << "Hello AutoPolygon... \n";
	//PNGImage img("testImages/test2.png");
	PNGImage img("testImages/tp4.png");
	// PNGImage img("testImages/1.png");
	//PNGImage img("E:\\_WorkSpace_E\\_GitHub\\PolygonPNG\\Unity\\UnityProject\\UnityAutoPolygon\\Assets\\TestImages\\test3.png");
	//PNGImage img("D:/_github/PNG2Polygon/Unity/UnityProject/UnityAutoPolygon/Assets/TestImages/test3.png");
	AutoPolygon p(&img);

	for (int i = 1; i <= 1; ++i)
	{
		// int epsilon = i*2;
		float epsilon = 1.0f;
		 epsilon = 53.5f;
		 epsilon = 44.5f;	// 测试洞
		PolygonInfo info;
		p.generateTriangles(info, Rect::ZERO, epsilon);
		printf("epsilon: %f triangleNum: %d\tindexNum:%d\n", epsilon, info.triangles.vertCount, info.triangles.indexCount);
		char filePathBuffer[128] = { 0 };
		sprintf_s(filePathBuffer, sizeof(filePathBuffer), "epsilon-%d.txt", epsilon);
		print2File(info, filePathBuffer);
	}
	//cv::waitKey();
	int a = 0;
}


