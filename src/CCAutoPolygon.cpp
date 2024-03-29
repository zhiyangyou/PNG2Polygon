/****************************************************************************
Copyright (c) 2008-2010 Ricardo Quesada
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2011      Zynga Inc.
Copyright (c) 2013-2016 Chukong Technoasserties Inc.
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

#include "clipper/clipper.hpp"

#include "CCAutoPolygon.h"
#include "PNGImage.h"
#include "CCGeometry.h"
#include "Vec2.h"
#include "AbsImage.h"

#include <algorithm>
#include <cassert>
#include <limits>
//#include <unordered_set>

//#include <opencv2/opencv.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/highgui.hpp>


#include <iostream>

#include "poly2tri/poly2tri.h"
#include "polypartition/polypartition.h"

#if 0
//#include <TrianglePP/tpp_interface.hpp>
#else
// #include "triangle/triangle.h" 
#include "triangle/triangle.c" 
#endif



#define CC_SAFE_DELETE(p)           do { delete (p); (p) = nullptr; } while(0)
#define CC_SAFE_DELETE_ARRAY(p)     do { if(p) { delete[] (p); (p) = nullptr; } } while(0)



#define CC_RECT_POINTS_TO_PIXELS(r)                                                                        \
    Rect( (r).origin.x  , (r).origin.y  ,    \
            (r).size.width  , (r).size.height   )

static unsigned short quadIndices9[] = {
	0 + 4 * 0,1 + 4 * 0,2 + 4 * 0, 3 + 4 * 0,2 + 4 * 0,1 + 4 * 0,
	0 + 4 * 1,1 + 4 * 1,2 + 4 * 1, 3 + 4 * 1,2 + 4 * 1,1 + 4 * 1,
	0 + 4 * 2,1 + 4 * 2,2 + 4 * 2, 3 + 4 * 2,2 + 4 * 2,1 + 4 * 2,
	0 + 4 * 3,1 + 4 * 3,2 + 4 * 3, 3 + 4 * 3,2 + 4 * 3,1 + 4 * 3,
	0 + 4 * 4,1 + 4 * 4,2 + 4 * 4, 3 + 4 * 4,2 + 4 * 4,1 + 4 * 4,
	0 + 4 * 5,1 + 4 * 5,2 + 4 * 5, 3 + 4 * 5,2 + 4 * 5,1 + 4 * 5,
	0 + 4 * 6,1 + 4 * 6,2 + 4 * 6, 3 + 4 * 6,2 + 4 * 6,1 + 4 * 6,
	0 + 4 * 7,1 + 4 * 7,2 + 4 * 7, 3 + 4 * 7,2 + 4 * 7,1 + 4 * 7,
	0 + 4 * 8,1 + 4 * 8,2 + 4 * 8, 3 + 4 * 8,2 + 4 * 8,1 + 4 * 8,
};


#define TPPLPointIsEq(p1,p2)   (p1).x == (p2).x && (p1).y == (p2).y


const static float PRECISION = 10.0f;

void DisposeTriangleMemory(Triangles tri)
{
	CC_SAFE_DELETE_ARRAY(tri.indices);
	CC_SAFE_DELETE_ARRAY(tri.verts);
}

#define SAFE_FREE_MEMORY(x) if(x){free(x);}
void ResetTriangulateioMemory(triangulateio& io)
{
	memset(&io, 0, sizeof(triangulateio));
}
void DisposeTriangulateioMemory(triangulateio& io, bool isOutIO)
{
	SAFE_FREE_MEMORY(io.pointlist)
		SAFE_FREE_MEMORY(io.pointattributelist)
		SAFE_FREE_MEMORY(io.pointmarkerlist)
		SAFE_FREE_MEMORY(io.trianglelist)
		SAFE_FREE_MEMORY(io.triangleattributelist)
		SAFE_FREE_MEMORY(io.trianglearealist)
		SAFE_FREE_MEMORY(io.neighborlist)
		SAFE_FREE_MEMORY(io.segmentlist)
		SAFE_FREE_MEMORY(io.segmentmarkerlist)
		if (!isOutIO) // avoid double free  --.
		{
			SAFE_FREE_MEMORY(io.holelist) //  out.holelist copy pointer from in.holelist, although the pointer is copied to the        output structure for your convenience.
		}

	SAFE_FREE_MEMORY(io.regionlist)
		SAFE_FREE_MEMORY(io.edgelist)
		SAFE_FREE_MEMORY(io.edgemarkerlist)
		SAFE_FREE_MEMORY(io.normlist)
}


struct Vec2Hash {
	std::size_t operator()(const Vec2& vec) const {
		std::hash<float> hasher;
		return hasher(vec.x) ^ (hasher(vec.y) << 1);
	}
};

//bool  removeDuplicateVec2(const std::vector<Vec2>& vec2Array, std::vector<Vec2>& uniqueVec2) {
//	std::unordered_set<Vec2, Vec2Hash> seenSet;
//	bool hasRepeat = false;
//	for (const Vec2& vec2 : vec2Array) {
//		if (seenSet.find(vec2) == seenSet.end()) {
//			uniqueVec2.push_back(vec2);
//			seenSet.insert(vec2);
//		}
//		else
//		{
//			hasRepeat = true;
//		}
//	}
//	return hasRepeat;
//}

std::pair<float, float> boundingBoxDimensions(const std::vector<Vec2>& points)
{

	if (points.empty())
	{
		assert(false);
		return std::make_pair(0.0, 0.0);
	}

	float min_x = std::numeric_limits<float>::infinity();
	float min_y = std::numeric_limits<float>::infinity();
	float max_x = -std::numeric_limits<float>::infinity();
	float max_y = -std::numeric_limits<float>::infinity();

	for (const auto& point : points)
	{
		min_x = std::min(min_x, point.x);
		min_y = std::min(min_y, point.y);
		max_x = std::max(max_x, point.x);
		max_y = std::max(max_y, point.y);
	}
	float width = max_x - min_x;
	float height = max_y - min_y;

	return std::make_pair(width, height);
}
Triangles MergeAndConvertList2IndexTriangles(std::vector<Triangles>& list, bool releasListTriMemory)
{
	int totalVS = 0;
	int totalIndex = 0;
	Triangles ret;
	for (int i = 0; i < list.size(); i++)
	{
		Triangles tri = list[i];
		totalVS += tri.vertCount;
		totalIndex += tri.indexCount;
	}
	ret.vertCount = totalVS;
	ret.indexCount = totalIndex;
	ret.verts = new (std::nothrow) V3F_C4B_T2F[totalVS];
	ret.indices = new (std::nothrow) unsigned short[totalIndex];
	int indexOffset = 0;

	int vsAddCount = 0;
	int indexAddCount = 0;
	for (int i = 0; i < list.size(); i++)
	{
		Triangles tri = list[i];

		for (int ii = 0; ii < tri.vertCount; ii++)
		{
			*(ret.verts + vsAddCount) = tri.verts[ii];
			vsAddCount++;
		}

		for (int ii = 0; ii < tri.indexCount; ii++)
		{
			*(ret.indices + indexAddCount) = tri.indices[ii] + indexOffset;
			indexAddCount++;
		}

		indexOffset += tri.vertCount;
		if (releasListTriMemory)
		{
			DisposeTriangleMemory(tri);
		}
	}
	return ret;
}


Vec2 computeConvexHullCentroid(const std::vector<Vec2>& convexHull) {
	// 确保凸包不为空
	if (convexHull.empty()) {
		// 处理空凸包的情况，这里简单返回原点
		return Vec2(0.0f, 0.0f);
	}

	// 初始化中心点坐标
	float centerX = 0.0f;
	float centerY = 0.0f;

	// 计算所有顶点的坐标之和
	for (const auto& point : convexHull) {
		centerX += point.x;
		centerY += point.y;
	}

	// 除以顶点数量得到中心点坐标
	centerX /= convexHull.size();
	centerY /= convexHull.size();

	return Vec2(centerX, centerY);
}
void CollectPolyNodeData(
	ClipperLib::PolyNode* node,
	triangulateio& in,
	int& curTotal_PointsNumber,
	int& curTotal_Segments,
	int& curTotal_Holes,
	int& curTotal_SegmentMarkers
)
{

	std::vector<Vec2> boundaryPoints;

	const bool isHole = node->IsHole();
	for (const ClipperLib::IntPoint& p : node->Contour)
	{
		boundaryPoints.emplace_back(p.X, p.Y);
	}
	std::vector<Vec2> uniquePoly ;

	if (!boundaryPoints.empty())
	{
		//points
		int old_CurTotal_PointsNumber = curTotal_PointsNumber;
		curTotal_PointsNumber += boundaryPoints.size();
		in.numberofpoints = curTotal_PointsNumber;
		in.pointlist = (TRI_REAL*)realloc(in.pointlist, in.numberofpoints * 2 * sizeof(TRI_REAL));

		for (int i = old_CurTotal_PointsNumber; i < in.numberofpoints; i++) {
			in.pointlist[i * 2] = boundaryPoints[i - old_CurTotal_PointsNumber].x;
			in.pointlist[i * 2 + 1] = boundaryPoints[i - old_CurTotal_PointsNumber].y;
		}

		//segments
		int old_curTotal_Segments = curTotal_Segments;
		curTotal_Segments += boundaryPoints.size();
		in.numberofsegments = curTotal_Segments;
		in.segmentlist = (int*)realloc(in.segmentlist, in.numberofsegments * 2 * sizeof(int));

		int firstIndex = old_curTotal_Segments + 1;
		for (int i = old_curTotal_Segments; i < in.numberofsegments; i++) {
			in.segmentlist[i * 2] = i + 1;
			in.segmentlist[i * 2 + 1] = ((i + 1) % in.numberofpoints) + 1; //  index start from 1 , not zero  haowuyu ...
		}
#if _cv_debug_yzy
		in.segmentlist[(in.numberofsegments - 1) * 2 + 1] = firstIndex;
		for (int i = old_curTotal_Segments; i < in.numberofsegments; i++) {
			printf("(%d-->%d)\n", in.segmentlist[i * 2], in.segmentlist[i * 2 + 1]);
		}
		printf("isHole %d-----------------------\n", isHole);
#endif

		if (isHole)
		{
			int old_curTotal_Holes = curTotal_Holes;
			curTotal_Holes += 1;
			in.numberofholes = curTotal_Holes;
			in.holelist = (TRI_REAL*)realloc(in.holelist, in.numberofholes * 2 * sizeof(TRI_REAL));
			Vec2 holeP = computeConvexHullCentroid(boundaryPoints);
			for (int i = old_curTotal_Holes; i < in.numberofholes; i++) {
				in.holelist[i * 2] = holeP.x;
				in.holelist[i * 2 + 1] = holeP.y;
			}
		}

		//if (!isHole)
		{
			// segments markers
			int old_curTotal_SegmentMarkers = curTotal_SegmentMarkers;
			curTotal_SegmentMarkers += boundaryPoints.size();
			in.segmentmarkerlist = (int*)malloc(in.numberofsegments * sizeof(int));
			for (int i = old_curTotal_SegmentMarkers; i < in.numberofsegments; i++) {
				in.segmentmarkerlist[i] = 1;  // 标记这是一个约束边
			}
		}

	}

	for (int i = 0; i < node->ChildCount(); i++)
	{
		CollectPolyNodeData(
			node->Childs[i],
			in,
			curTotal_PointsNumber,
			curTotal_Segments,
			curTotal_Holes,
			curTotal_SegmentMarkers
		);
	}

}

Triangles AutoPolygon::MergePolygons(std::vector<std::vector<Vec2>>& polygons, const Rect& realRect)
{
	using Paths = std::vector<ClipperLib::IntPoint>;

	std::vector<Paths> clipperPolygons;

	//TODO  

	for (std::vector<Vec2>& poly : polygons) {
		clipperPolygons.emplace_back();
		Paths& path = clipperPolygons.back();
		for (Vec2& point : poly) {
			path.emplace_back(point.x, point.y);
		}
	}

	ClipperLib::Clipper clipper;
	clipper.AddPaths(clipperPolygons, ClipperLib::ptSubject, true);

	ClipperLib::PolyTree solution;
	clipper.Execute(ClipperLib::ctUnion, solution, ClipperLib::pftNonZero, ClipperLib::pftEvenOdd);

	triangulateio in;
	ResetTriangulateioMemory(in);
	Triangles mergeTri;


	int curTotal_PointsNumber = 0;
	int curTotal_Segments = 0;
	int curTotal_Holes = 0;
	int curTotal_SegmentMarkers = 0;

	for (int i = 0; i < solution.ChildCount(); i++)
	{
		ClipperLib::PolyNode* node = solution.Childs[i];
		CollectPolyNodeData(
			node,
			in,
			curTotal_PointsNumber,
			curTotal_Segments,
			curTotal_Holes,
			curTotal_SegmentMarkers
		);
	}

	//std::vector<Vec2> uniquePoly;
	//for (auto& mergedPolygon : mergedPolygons)
	//{
	//	auto& oldPoly = mergedPolygon;
	//	uniquePoly.clear();

	//	2023年12月5日22:47 : 24 判空移除
	//		if (removeDuplicateVec2(oldPoly, uniquePoly))
	//		{
	//			mergedPolygon = uniquePoly;
	//		}
	//}

	triangulateByTriangle((void*)&in, mergeTri);
	calculateUV(realRect, mergeTri.verts, mergeTri.vertCount);
	DisposeTriangulateioMemory(in, false);
	return mergeTri;
}


PolygonInfo::PolygonInfo()
	: _isVertsOwner(true)
	, _rect(Rect::ZERO)
	, _filename("")
{
	triangles.verts = nullptr;
	triangles.indices = nullptr;
	triangles.vertCount = 0;
	triangles.indexCount = 0;
};


PolygonInfo::PolygonInfo(const PolygonInfo& other)
	: triangles()
	, _isVertsOwner(true)
	, _rect()
{
	_filename = other._filename;
	_isVertsOwner = true;
	_rect = other._rect;
	triangles.verts = new (std::nothrow) V3F_C4B_T2F[other.triangles.vertCount];
	triangles.indices = new (std::nothrow) unsigned short[other.triangles.indexCount];
	assert(triangles.verts && triangles.indices, "not enough memory");
	triangles.vertCount = other.triangles.vertCount;
	triangles.indexCount = other.triangles.indexCount;
	memcpy(triangles.verts, other.triangles.verts, other.triangles.vertCount * sizeof(other.triangles.verts[0]));
	memcpy(triangles.indices, other.triangles.indices, other.triangles.indexCount * sizeof(other.triangles.indices[0]));
};

PolygonInfo& PolygonInfo::operator= (const PolygonInfo& other)
{
	if (this != &other)
	{
		releaseVertsAndIndices();
		_filename = other._filename;
		_isVertsOwner = true;
		_rect = other._rect;
		triangles.verts = new (std::nothrow) V3F_C4B_T2F[other.triangles.vertCount];
		triangles.indices = new (std::nothrow) unsigned short[other.triangles.indexCount];
		assert(triangles.verts && triangles.indices, "not enough memory");
		triangles.vertCount = other.triangles.vertCount;
		triangles.indexCount = other.triangles.indexCount;
		memcpy(triangles.verts, other.triangles.verts, other.triangles.vertCount * sizeof(other.triangles.verts[0]));
		memcpy(triangles.indices, other.triangles.indices, other.triangles.indexCount * sizeof(other.triangles.indices[0]));
	}
	return *this;
}

PolygonInfo::~PolygonInfo()
{
	releaseVertsAndIndices();
}

void PolygonInfo::setQuad(V3F_C4B_T2F_Quad* quad)
{
	releaseVertsAndIndices();
	_isVertsOwner = false;
	triangles.indices = quadIndices9;
	triangles.vertCount = 4;
	triangles.indexCount = 6;
	triangles.verts = (V3F_C4B_T2F*)quad;
}

void PolygonInfo::setQuads(V3F_C4B_T2F_Quad* quad, int numberOfQuads)
{
	assert(numberOfQuads >= 1 && numberOfQuads <= 9, "Invalid number of Quads");

	releaseVertsAndIndices();
	_isVertsOwner = false;
	triangles.indices = quadIndices9;
	triangles.vertCount = 4 * numberOfQuads;
	triangles.indexCount = 6 * numberOfQuads;
	triangles.verts = (V3F_C4B_T2F*)quad;
}

void PolygonInfo::setTriangles(const Triangles& other)
{
	this->releaseVertsAndIndices();
	_isVertsOwner = false;

	this->triangles.vertCount = other.vertCount;
	this->triangles.indexCount = other.indexCount;
	this->triangles.verts = other.verts;
	this->triangles.indices = other.indices;
}

void PolygonInfo::releaseVertsAndIndices()
{
	if (_isVertsOwner)
	{
		if (nullptr != triangles.verts)
		{
			CC_SAFE_DELETE_ARRAY(triangles.verts);
		}

		if (nullptr != triangles.indices)
		{
			CC_SAFE_DELETE_ARRAY(triangles.indices);
		}
	}
}

unsigned int PolygonInfo::getVertCount() const
{
	return (unsigned int)triangles.vertCount;
}

unsigned int PolygonInfo::getTrianglesCount() const
{
	return (unsigned int)triangles.indexCount / 3;
}

float PolygonInfo::getArea() const
{
	float area = 0;
	V3F_C4B_T2F* verts = triangles.verts;
	unsigned short* indices = triangles.indices;
	for (unsigned int i = 0; i < triangles.indexCount; i += 3)
	{
		auto A = verts[indices[i]].vertices;
		auto B = verts[indices[i + 1]].vertices;
		auto C = verts[indices[i + 2]].vertices;
		area += (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y)) / 2;
	}
	return area;
}

AutoPolygon::AutoPolygon(AbsImage* image)
	:_image(nullptr)
	, _data(nullptr)
	, _width(0)
	, _height(0)
	, _scaleFactor(0)
{

	_image = image;
	_data = _image->getData();
	_width = _image->getWidth();
	_height = _image->getHeight();
	_scaleFactor = 1.0f;
}

template <typename T>
void copyVector(const std::vector<T>& source, std::vector<T>& target) {
	target = source;
}

std::vector<std::vector<Vec2>> AutoPolygon::trace(const Rect& rect, float threshold)
{
	std::vector< std::vector<Vec2>> ret;
	Vec2 first = findFirstNoneTransparentPixel(rect, threshold);
	std::vector<Vec2> v = marchSquare(rect, first, threshold);
	ret.push_back(v);
	return ret;
}

std::vector<std::vector<Vec2>>  AutoPolygon::traceByCV(float threshold)
{
	std::vector<std::vector<Vec2>> ret;
	cv::Mat image = cv::imread(this->_image->getFileName(), cv::IMREAD_UNCHANGED);
	if (image.empty()) {
		std::cerr << "Error: Could not read the image." << std::endl;
		return  ret;
	}
	std::vector<cv::Mat> channels;
	cv::split(image, channels);
	cv::Mat thresholded;
	cv::threshold(channels[3], thresholded, threshold, 255, cv::THRESH_BINARY);
#ifdef _cv_debug_yzy
	// cv::imshow("thresholded",thresholded);
#endif

	std::vector<std::vector<cv::Point>> contours;
	// cv::findContours(thresholded, contours, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_SIMPLE);
	cv::findContours(thresholded, contours, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_SIMPLE);
	// cv::findContours(thresholded, contours, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_TC89_KCOS);
	// cv::findContours(thresholded, contours, cv::RETR_LIST, cv::CHAIN_APPROX_SIMPLE);
#ifdef _cv_debug_yzy 
	cv::Mat drawing = cv::Mat::zeros(thresholded.size(), CV_8UC3);
#endif

	for (size_t i = 0; i < contours.size(); ++i) {
#ifdef _cv_debug_yzy
		cv::drawContours(drawing, contours, static_cast<int>(i), cv::Scalar(0, 255, 0), 2);
#endif
		std::vector<cv::Point>& contourPoints = contours[i];
		std::vector<Vec2> v;
		for (const auto& point : contourPoints) {
			v.emplace_back(point.x, point.y);
		}
		ret.push_back(v);
	}

#ifdef _cv_debug_yzy
	// cv::imshow("drawContours",drawing);
#endif

	return ret;
}

Vec2 AutoPolygon::findFirstNoneTransparentPixel(const Rect& rect, float threshold)
{
	bool found = false;
	Vec2 i;
	for (i.y = rect.origin.y; i.y < rect.origin.y + rect.size.height; i.y++)
	{
		if (found)break;
		for (i.x = rect.origin.x; i.x < rect.origin.x + rect.size.width; i.x++)
		{
			auto alpha = getAlphaByPos(i);
			if (alpha > threshold)
			{
				found = true;
				break;
			}
		}
	}
	assert(found, "image is all transparent!");
	return i;
}

unsigned char AutoPolygon::getAlphaByIndex(unsigned int i)
{
	return *(_data + i * 4 + 3);
}
unsigned char AutoPolygon::getAlphaByPos(const Vec2& pos)
{
	return *(_data + ((int)pos.y * _width + (int)pos.x) * 4 + 3);
}

unsigned int AutoPolygon::getSquareValue(unsigned int x, unsigned int y, const Rect& rect, float threshold)
{
	/*
	 checking the 2x2 pixel grid, assigning these values to each pixel, if not transparent
	 +---+---+
	 | 1 | 2 |
	 +---+---+
	 | 4 | 8 | <- current pixel (curx,cury)
	 +---+---+
	 */
	unsigned int sv = 0;
	//NOTE: due to the way we pick points from texture, rect needs to be smaller, otherwise it goes outside 1 pixel
	auto fixedRect = Rect(rect.origin, rect.size - Size(2, 2));

	Vec2 tl = Vec2(x - 1.0f, y - 1.0f);
	sv += (fixedRect.containsPoint(tl) && getAlphaByPos(tl) > threshold) ? 1 : 0;
	Vec2 tr = Vec2(x - 0.0f, y - 1.0f);
	sv += (fixedRect.containsPoint(tr) && getAlphaByPos(tr) > threshold) ? 2 : 0;
	Vec2 bl = Vec2(x - 1.0f, y - 0.0f);
	sv += (fixedRect.containsPoint(bl) && getAlphaByPos(bl) > threshold) ? 4 : 0;
	Vec2 br = Vec2(x - 0.0f, y - 0.0f);
	sv += (fixedRect.containsPoint(br) && getAlphaByPos(br) > threshold) ? 8 : 0;
	// assert(sv != 0 && sv != 15, "square value should not be 0, or 15");
	return sv;
}

std::vector<Vec2> AutoPolygon::marchSquare(const Rect& rect, const Vec2& start, float threshold)
{
	int stepx = 0;
	int stepy = 0;
	int prevx = 0;
	int prevy = 0;
	int startx = (int)start.x;
	int starty = (int)start.y;
	int curx = startx;
	int cury = starty;
	unsigned int count = 0;
	std::vector<int> case9s;
	std::vector<int> case6s;
	int i;
	std::vector<int>::iterator it;
	std::vector<Vec2> _points;
	do {
		int sv = getSquareValue(curx, cury, rect, threshold);
		switch (sv) {

		case 1:
		case 5:
		case 13:
			/* going UP with these cases:
			 1          5           13
			 +---+---+  +---+---+  +---+---+
			 | 1 |   |  | 1 |   |  | 1 |   |
			 +---+---+  +---+---+  +---+---+
			 |   |   |  | 4 |   |  | 4 | 8 |
			 +---+---+  +---+---+  +---+---+
			 */
			stepx = 0;
			stepy = -1;
			break;


		case 8:
		case 10:
		case 11:
			/* going DOWN with these cases:
			 8          10          11
			 +---+---+  +---+---+   +---+---+
			 |   |   |  |   | 2 |   | 1 | 2 |
			 +---+---+  +---+---+   +---+---+
			 |   | 8 |  |   | 8 |   |   | 8 |
			 +---+---+  +---+---+  	+---+---+
			 */
			stepx = 0;
			stepy = 1;
			break;


		case 4:
		case 12:
		case 14:
			/* going LEFT with these cases:
			 4          12          14
			 +---+---+  +---+---+   +---+---+
			 |   |   |  |   |   |   |   | 2 |
			 +---+---+  +---+---+   +---+---+
			 | 4 |   |  | 4 | 8 |   | 4 | 8 |
			 +---+---+  +---+---+  	+---+---+
			 */
			stepx = -1;
			stepy = 0;
			break;


		case 2:
		case 3:
		case 7:
			/* going RIGHT with these cases:
			 2          3           7
			 +---+---+  +---+---+   +---+---+
			 |   | 2 |  | 1 | 2 |   | 1 | 2 |
			 +---+---+  +---+---+   +---+---+
			 |   |   |  |   |   |   | 4 |   |
			 +---+---+  +---+---+  	+---+---+
			 */
			stepx = 1;
			stepy = 0;
			break;
		case 9:
			/*
			 +---+---+
			 | 1 |   |
			 +---+---+
			 |   | 8 |
			 +---+---+
			 this should normally go UP, but if we already been here, we go down
			*/
			//find index from xy;
			i = getIndexFromPos(curx, cury);
			it = find(case9s.begin(), case9s.end(), i);
			if (it != case9s.end())
			{
				//found, so we go down, and delete from case9s;
				stepx = 0;
				stepy = 1;
				case9s.erase(it);
			}
			else
			{
				//not found, we go up, and add to case9s;
				stepx = 0;
				stepy = -1;
				case9s.push_back(i);
			}
			break;
		case 6:
			/*
			 6
			 +---+---+
			 |   | 2 |
			 +---+---+
			 | 4 |   |
			 +---+---+
			 this normally go RIGHT, but if its coming from UP, it should go LEFT
			 */
			i = getIndexFromPos(curx, cury);
			it = find(case6s.begin(), case6s.end(), i);
			if (it != case6s.end())
			{
				//found, so we go down, and delete from case9s;
				stepx = -1;
				stepy = 0;
				case6s.erase(it);
			}
			else {
				//not found, we go up, and add to case9s;
				stepx = 1;
				stepy = 0;
				case6s.push_back(i);
			}
			break;
		default:
			assert(false, "this shouldn't happen.");
		}
		//little optimization
		// if previous direction is same as current direction,
		// then we should modify the last vec to current
		curx += stepx;
		cury += stepy;
		if (stepx == prevx && stepy == prevy)
		{
			_points.back().x = (float)(curx - rect.origin.x) / _scaleFactor;
			_points.back().y = (float)(rect.size.height - cury + rect.origin.y) / _scaleFactor;
		}
		else
		{
			_points.push_back(Vec2((float)(curx - rect.origin.x) / _scaleFactor, (float)(rect.size.height - cury + rect.origin.y) / _scaleFactor));
		}

		count++;
		prevx = stepx;
		prevy = stepy;

#if defined(COCOS2D_DEBUG) && (COCOS2D_DEBUG > 0)
		const auto totalPixel = _width * _height;
		assert(count <= totalPixel, "oh no, marching square cannot find starting position");
#endif
	} while (curx != startx || cury != starty);
	return _points;
}

float AutoPolygon::perpendicularDistance(const Vec2& i, const Vec2& start, const Vec2& end)
{
	float res;
	float slope;
	float intercept;

	if (start.x == end.x)
	{
		res = fabsf(i.x - end.x);
	}
	else if (start.y == end.y)
	{
		res = fabsf(i.y - end.y);
	}
	else {
		slope = (end.y - start.y) / (end.x - start.x);
		intercept = start.y - (slope * start.x);
		res = fabsf(slope * i.x - i.y + intercept) / sqrtf(powf(slope, 2) + 1);
	}
	return res;
}
std::vector<Vec2> AutoPolygon::rdp(const std::vector<Vec2>& v, float optimization)
{
	if (v.size() < 3)
		return v;

	int index = -1;
	float dist = 0;
	//not looping first and last point
	for (size_t i = 1, size = v.size(); i < size - 1; ++i)
	{
		float cdist = perpendicularDistance(v[i], v.front(), v.back());
		if (cdist > dist)
		{
			dist = cdist;
			index = static_cast<int>(i);
		}
	}
	if (dist > optimization)
	{
		std::vector<Vec2>::const_iterator begin = v.begin();
		std::vector<Vec2>::const_iterator end = v.end();
		std::vector<Vec2> l1(begin, begin + index + 1);
		std::vector<Vec2> l2(begin + index, end);

		std::vector<Vec2> r1 = rdp(l1, optimization);
		std::vector<Vec2> r2 = rdp(l2, optimization);

		r1.insert(r1.end(), r2.begin() + 1, r2.end());
		return r1;
	}
	else {
		std::vector<Vec2> ret;
		ret.push_back(v.front());
		ret.push_back(v.back());
		return ret;
	}
}
std::vector<Vec2> AutoPolygon::reduce(const std::vector<Vec2>& points, float epsilon)
{
	auto size = points.size();
	// if there are less than 3 points, then we have nothing
	if (size < 3)
	{
		assert("AUTOPOLYGON: cannot reduce points for %s that has less than 3 points in input, e: %f", _filename.c_str(), epsilon);
		return std::vector<Vec2>();
	}
	// if there are less than 9 points (but more than 3), then we don't need to reduce it
	else if (size < 9)
	{
		assert("AUTOPOLYGON: cannot reduce points for %s e: %f", _filename.c_str(), epsilon);
		return points;
	}
	auto wh = boundingBoxDimensions(points);
	float maxEp = std::min(wh.first, wh.second);
	float ep = clampf(epsilon, 0.0, maxEp / _scaleFactor / 2);
	std::vector<Vec2> result = rdp(points, ep);

	auto last = result.back();
	if (last.y > result.front().y && last.getDistance(result.front()) < ep * 0.5f)
	{
		result.front().y = last.y;
		result.pop_back();
	}
	return result;
}

std::vector<Vec2> AutoPolygon::expand(const std::vector<Vec2>& points, const Rect& rect, float epsilon)
{
	auto size = points.size();
	// if there are less than 3 points, then we have nothing
	if (size < 3)
	{
		assert("AUTOPOLYGON: cannot expand points for %s with less than 3 points, e: %f", _filename.c_str(), epsilon);
		return std::vector<Vec2>();
	}
	ClipperLib::Path subj;
	ClipperLib::PolyTree solution;
	ClipperLib::PolyTree out;
	for (const auto& pt : points)
	{
		subj << ClipperLib::IntPoint(static_cast<ClipperLib::cInt>(pt.x * PRECISION), static_cast<ClipperLib::cInt>(pt.y * PRECISION));
	}
	ClipperLib::ClipperOffset co;
	co.AddPath(subj, ClipperLib::jtMiter, ClipperLib::etClosedPolygon);
	co.Execute(solution, epsilon * PRECISION);

	ClipperLib::PolyNode* p = solution.GetFirst();
	if (!p)
	{
		assert("AUTOPOLYGON: Clipper failed to expand the points");
		return points;
	}
	while (p->IsHole()) {
		p = p->GetNext();
	}

	//turn the result into simply polygon (AKA, fix overlap)

	//clamp into the specified rect
	ClipperLib::Clipper cl;
	cl.StrictlySimple(true);
	cl.AddPath(p->Contour, ClipperLib::ptSubject, true);
	//create the clipping rect
	ClipperLib::Path clamp;
	clamp.push_back(ClipperLib::IntPoint(0, 0));
	clamp.push_back(ClipperLib::IntPoint(static_cast<ClipperLib::cInt>(rect.size.width / _scaleFactor * PRECISION), 0));
	clamp.push_back(ClipperLib::IntPoint(static_cast<ClipperLib::cInt>(rect.size.width / _scaleFactor * PRECISION),
		static_cast<ClipperLib::cInt>(rect.size.height / _scaleFactor * PRECISION)));
	clamp.push_back(ClipperLib::IntPoint(0, static_cast<ClipperLib::cInt>(rect.size.height / _scaleFactor * PRECISION)));
	cl.AddPath(clamp, ClipperLib::ptClip, true);
	cl.Execute(ClipperLib::ctIntersection, out);

	std::vector<Vec2> outPoints;
	ClipperLib::PolyNode* p2 = out.GetFirst();
	while (p2->IsHole()) {
		p2 = p2->GetNext();
	}
	for (const auto& pt : p2->Contour)
	{
		outPoints.push_back(Vec2(pt.X / PRECISION, pt.Y / PRECISION));
	}
	return outPoints;
}



void AutoPolygon::triangulateByTriangle(/*const std::vector<Vec2>& boundaryPoints,*/ void* pIn, Triangles& tri)
{
	triangulateio& in = *((triangulateio*)pIn);

	char options[] = "p";
	struct triangulateio  out;
	ResetTriangulateioMemory(out);

	::triangulate(options, &in, &out, nullptr);

	int indexCount = out.numberoftriangles * 3;
	int vertCount = out.numberofpoints;
	tri.verts = new  V3F_C4B_T2F[vertCount];
	tri.indices = new unsigned short[indexCount];
	tri.indexCount = indexCount;
	tri.vertCount = vertCount;

	for (int i = 0; i < out.numberoftriangles; ++i)
	{
		const int i1 = out.trianglelist[i * 3] - 1;
		const int i2 = out.trianglelist[i * 3 + 1] - 1;
		const int i3 = out.trianglelist[i * 3 + 2] - 1;

		tri.indices[i * 3] = i1;
		tri.indices[i * 3 + 1] = i2;
		tri.indices[i * 3 + 2] = i3;
	}

	for (int i = 0; i < out.numberofpoints; ++i)
	{
		tri.verts[i].vertices.x = out.pointlist[i * 2];
		tri.verts[i].vertices.y = out.pointlist[i * 2 + 1];
	}
	DisposeTriangulateioMemory(out, true);
}


void AutoPolygon::calculateUV(const Rect& rect, V3F_C4B_T2F* verts, size_t count)
{
	/*
	 whole texture UV coordination
	 0,0                  1,0
	 +---------------------+
	 |                     |0.1
	 |                     |0.2
	 |     +--------+      |0.3
	 |     |texRect |      |0.4
	 |     |        |      |0.5
	 |     |        |      |0.6
	 |     +--------+      |0.7
	 |                     |0.8
	 |                     |0.9
	 +---------------------+
	 0,1                  1,1
	 */

	assert(_width && _height, "please specify width and height for this AutoPolygon instance");
	auto texWidth = _width;
	auto texHeight = _height;

	auto end = &verts[count];
	for (auto i = verts; i != end; ++i)
	{
		// for every point, offset with the center point
		float u = (i->vertices.x * _scaleFactor + rect.origin.x) / texWidth;
		float v = (rect.origin.y + rect.size.height - i->vertices.y * _scaleFactor) / texHeight;
		i->texCoords.u = u;
		i->texCoords.v = v;
	}
}

Rect AutoPolygon::getRealRect(const Rect& rect)
{
	Rect realRect = rect;
	//check rect to see if its zero
	if (realRect.equals(Rect::ZERO))
	{
		//if the instance doesn't have width and height, then the whole operation is kaput
		assert(_height && _width, "Please specify a width and height for this instance before using its functions");
		realRect = Rect(0, 0, (float)_width, (float)_height);
	}
	else {
		//rect is specified, so convert to real rect
		realRect = CC_RECT_POINTS_TO_PIXELS(rect);
	}
	return realRect;
}

PolygonInfo AutoPolygon::generateTriangles(const Rect& rect, float epsilon, float threshold)
{
	PolygonInfo ret;
	generateTriangles(ret, rect, epsilon, threshold);
	return ret;
}

void drawCVTriangle(cv::Mat& imageTri, const char* name, Triangles& tri, cv::Scalar color , bool isFill )
{
	std::vector<cv::Point> trianglePoints;
	for (int i = 0; i < tri.indexCount / 3; i++)
	{
		trianglePoints.clear();
		Vec3& p1 = tri.verts[tri.indices[i * 3]].vertices;
		Vec3& p2 = tri.verts[tri.indices[i * 3 + 1]].vertices;
		Vec3& p3 = tri.verts[tri.indices[i * 3 + 2]].vertices;
		trianglePoints.emplace_back(p1.x, p1.y);
		trianglePoints.emplace_back(p2.x, p2.y);
		trianglePoints.emplace_back(p3.x, p3.y);
		if (!isFill)
		{
			cv::polylines(imageTri, trianglePoints, true, color, 2, cv::LINE_AA);
		}
		else
		{
			cv::fillPoly(imageTri, trianglePoints, color,  cv::LINE_AA);
		}
		
	}
	cv::imshow(name, imageTri);
}

void drawCVPoints(cv::Mat& img, const char* name, std::vector<Vec2>& ps, int index, cv::Scalar color)
{
	std::vector<cv::Point> cvPoints;
	std::vector<std::vector<cv::Point>>  Contours;
	for (Vec2& v : ps)
	{
		cvPoints.push_back(cv::Point(v.x, v.y));
	}
	if (cvPoints.size() > 0)
	{
		Contours.push_back(cvPoints);
	}
	cv::drawContours(img, Contours, 0, color, 2, cv::LINE_AA);

	cv::imshow(name, img);
}

void AutoPolygon::generateTriangles(PolygonInfo& infoForFill, const Rect& rect /*= Rect::ZERO*/, float epsilon /*= 2.0f*/, float threshold /*= 0.05f*/)
{
	Rect realRect = getRealRect(rect);
	_realRect = realRect;
	auto originpolygons = traceByCV(threshold);
	int count = 0;
#ifdef _cv_debug_yzy 
	cv::Mat imgOriginPoints = cv::imread(this->_image->getFileName());
	cv::Mat imgOriginReduce = cv::imread(this->_image->getFileName());
	cv::Mat imgOriginExpand = cv::imread(this->_image->getFileName());
	cv::Mat imgTri = cv::imread(this->_image->getFileName());

#endif
	std::vector<std::vector<Vec2>> expandLists;
	for (std::vector<Vec2>& pOrigin : originpolygons)
	{
		std::vector<Vec2> reducePoly = reduce(pOrigin, epsilon);
		std::vector<Vec2> expandPoly = expand(reducePoly, realRect, epsilon);
		expandLists.push_back(expandPoly);
#ifdef _cv_debug_yzy
		//drawCVPoints(imgOriginPoints, "origin points", pOrigin, count, cv::Scalar(255, 255, 1));
		//drawCVPoints(imgOriginReduce, "reduce points", reducePoly, count, cv::Scalar(0, 255, 0));
		drawCVPoints(imgOriginExpand, "expand points", expandPoly, count, cv::Scalar(255, 0, 0));
		//drawCVTriangle( imgTri,"triangles", tri, realRect,count++,cv::Scalar(0, 0, 255));
#endif
	}
	Triangles totalTri = MergePolygons(expandLists, realRect);

#ifdef _cv_debug_yzy	
	cv::Mat imgTriMerge = cv::imread(this->_image->getFileName());
	cv::Mat imgTriMergeForFill = cv::imread(this->_image->getFileName());
#endif

	
#ifdef _cv_debug_yzy	
	drawCVTriangle(imgTriMerge, "merged triangles", totalTri, cv::Scalar(0, 255, 255),true);
	drawCVTriangle(imgTriMergeForFill, "merged triangles fill ", totalTri, cv::Scalar(0, 255, 255),false);
#endif

	//std::vector<Triangles> vecTris;
	//vecTris.emplace_back(totalTri);
	//Triangles outTri = MergeAndConvertList2IndexTriangles(vecTris, true);
	infoForFill.triangles = totalTri;
	infoForFill.setFilename(_image->getFileName());
	infoForFill.setRect(realRect);
#ifdef _cv_debug_yzy	
	cv::waitKey();
#endif
}

PolygonInfo AutoPolygon::generatePolygon(AbsImage* image, const Rect& rect, float epsilon, float threshold)
{
	AutoPolygon ap(image);
	return ap.generateTriangles(rect, epsilon, threshold);
}


