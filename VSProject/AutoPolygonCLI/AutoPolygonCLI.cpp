

#include <iostream>
#include <fstream>
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
	std::cout << "Hello World!\n";
	std::cout << sizeof(V3F_C4B_T2F) << "n";
	PNGImage img("testImages/1.png");
	AutoPolygon p(&img);

	for (int i = 1; i <= 50; ++i)
	{
		int epsilon = i;
		PolygonInfo info;
		p.generateTriangles(info, Rect::ZERO, epsilon);
		printf("epsilon: %d triangleNum: %d\tindexNum:%d\n", epsilon, info.triangles.vertCount, info.triangles.indexCount);
		char filePathBuffer[128] = { 0 };
		sprintf_s(filePathBuffer, sizeof(filePathBuffer), "epsilon-%d.txt", epsilon);
		print2File(info, filePathBuffer);
	}

	int a = 0;
}


