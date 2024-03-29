/****************************************************************************
Copyright (c) 2008-2010 Ricardo Quesada
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2011      Zynga Inc.
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

#ifndef COCOS_2D_CCAUTOPOLYGON_H__
#define COCOS_2D_CCAUTOPOLYGON_H__

#include <string>
#include <vector>

#include "Vec2.h"
#include "CCGeometry.h"
#include "ccTypes.h"

class AbsImage;

struct Triangles
{
	Triangles(V3F_C4B_T2F* _verts, unsigned short* _indices, unsigned int _vertCount, unsigned int _indexCount)
		: verts(_verts)
		, indices(_indices)
		, vertCount(_vertCount)
		, indexCount(_indexCount)
	{}

	Triangles() {}
	// Triangles(const Triangles & o)
	// {
	// 	this->verts = o.verts;
	// 	this->indices = o.indices;
	// 	this->vertCount = o.vertCount;
	// 	this->indexCount = o.indexCount;
	// }

	/**Vertex data pointer.*/
	V3F_C4B_T2F* verts = nullptr;
	/**Index data pointer.*/
	unsigned short* indices = nullptr;
	/**The number of vertices.*/
	unsigned int vertCount = 0;
	/**The number of indices.*/
	unsigned int indexCount = 0;

	// static void DisposeTriangleMemory(Triangles tri);
	//
	// static Triangles MergeAndConvertList2IndexTriangles(std::vector<Triangles> &list, bool autoRelease);
};


/**
 * @addtogroup _2d
 * @{
 */

 /**
  * PolygonInfo is an object holding the required data to display Sprites.
  * It can be a simple as a triangle, or as complex as a whole 3D mesh
  */
class  PolygonInfo
{
public:
	/// @name Creators
	/// @{
	/**
	 * Creates an empty Polygon info
	 * @memberof PolygonInfo
	 * @return PolygonInfo object
	 */
	PolygonInfo();

	/**
	 * Create an polygoninfo from the data of another Polygoninfo
	 * @param other     another PolygonInfo to be copied
	 * @return duplicate of the other PolygonInfo
	 */
	PolygonInfo(const PolygonInfo& other);
	//  end of creators group
	/// @}

	/**
	 * Copy the member of the other PolygonInfo
	 * @param other     another PolygonInfo to be copied
	 */
	PolygonInfo& operator= (const PolygonInfo& other);
	~PolygonInfo();

	/**
	 * set the data to be a pointer to a quad
	 * the member verts will not be released when this PolygonInfo destructs
	 * as the verts memory are managed by other objects
	 * @param quad  a pointer to the V3F_C4B_T2F_Quad object
	 */
	void setQuad(V3F_C4B_T2F_Quad* quad);

	/**
	 * set the data to be a pointer to a number of Quads
	 * the member verts will not be released when this PolygonInfo destructs
	 * as the verts memory are managed by other objects
	 * @param quad  a pointer to the V3F_C4B_T2F_Quad quads
	 */
	void setQuads(V3F_C4B_T2F_Quad* quads, int numberOfQuads);


	/**
	 * set the data to be a pointer to a triangles
	 * the member verts will not be released when this PolygonInfo destructs
	 * as the verts memory are managed by other objects
	 * @param triangles  a pointer to the Triangles object
	 */
	void setTriangles(const Triangles& triangles);

	/**
	 * get vertex count
	 * @return number of vertices
	 */
	unsigned int getVertCount() const;

	/**
	 * get triangles count
	 * @return number of triangles
	 */
	unsigned int getTrianglesCount() const;

	/**
	 * get sum of all triangle area size
	 * @return sum of all triangle area size
	 */
	float getArea() const;

	const Rect& getRect() const { return _rect; }
	void setRect(const Rect& rect) { _rect = rect; }
	const std::string& getFilename() const { return _filename; }
	void setFilename(const std::string& filename) { _filename = filename; }

	// FIXME: this should be a property, not a public ivar
	Triangles triangles;

protected:
	bool _isVertsOwner;
	Rect _rect;
	std::string _filename;

private:
	void releaseVertsAndIndices();
};


/**
 * AutoPolygon is a helper Object
 * AutoPolygon's purpose is to process an image into 2d polygon mesh in runtime
 * It has functions for each step in the process, from tracing all the points, to triangulation
 * the result can be then passed to Sprite::create() to create a Polygon Sprite
 */
class  AutoPolygon
{
public:
	/**
	 * create an AutoPolygon and initialize it with an image file
	 * the image must be a 32bit PNG for current version 3.7
	 * @param   filename    a path to image file, e.g., "scene1/monster.png".
	 * @return  an AutoPolygon object;
	 */
	AutoPolygon(AbsImage* image);

	/**
	 * Destructor of AutoPolygon.
	 */
	~AutoPolygon(){}

	/**
	 * trace all the points along the outline of the image,
	 * @warning must create AutoPolygon with filename to use this function
	 * @param   rect    a texture rect for specify an area of the image
	 * @param   threshold   the value when alpha is greater than this value will be counted as opaque, default to 0.0
	 * @return  a vector of vec2 of all the points found in clockwise order
	 * @code
	 * auto ap = AutoPolygon("grossini.png");
	 * auto rect = Rect(100, 100, 200, 200);
	 * std::vector<Vec2> points = ap.trace(rect);//default threshold is 0.0
	 * @endcode
	 */
	std::vector<std::vector<Vec2>> trace(const Rect& rect, float threshold = 0.0f);
	std::vector<std::vector<Vec2>> traceByCV( float threshold = 0.0f);

	/**
	 * reduce the amount of points so its faster for GPU to process and draw
	 * based on Ramer-Douglas-Peucker algorithm
	 * @param   points  a vector of Vec2 points as input
	 * @param   rect    a texture rect for specify an area of the image to avoid over reduction
	 * @param   epsilon the perpendicular distance where points smaller than this value will be discarded
	 * @return  a vector of Vec2 of the remaining points in clockwise order
	 * @code
	 * auto ap = AutoPolygon();
	 * std::vector<Vec2> reduced = ap.reduce(inputPoints, rect);//default epsilon is 2
	 * @endcode
	 */
	std::vector<Vec2> reduce(const std::vector<Vec2>& points, float epsilon = 2.0f);

	/**
	 * expand the points along their edge, useful after you reduce the points that cuts into the sprite
	 * using ClipperLib
	 * @param   points  a vector of Vec2 points as input
	 * @param   rect    a texture rect for specify an area of the image, the expanded points will be clamped in this rect, ultimately resulting in a quad if the expansion is too great
	 * @param   epsilon the distance which the edges will expand
	 * @return  a vector of Vec2 as the result of the expansion
	 * @code
	 * auto ap = AutoPolygon();
	 * std::vector<Vec2> expanded = ap.expand(inputPoints, rect, 2.0);
	 * @endcode
	 */
	std::vector<Vec2> expand(const std::vector<Vec2>& points, const Rect& rect, float epsilon);

	
	void triangulateByTriangle(void* in,Triangles& tri);
	
	void calculateUV(const Rect& rect, V3F_C4B_T2F* verts, size_t count);
	
	PolygonInfo generateTriangles(const Rect& rect = Rect::ZERO, float epsilon = 2.0f, float threshold = 0.05f);
	void generateTriangles(PolygonInfo& infoForFill, const Rect& rect = Rect::ZERO, float epsilon = 2.0f, float threshold = 0.05f);
	Triangles MergePolygons(std::vector<std::vector<Vec2>>& polygons, const Rect& realRect);
	static PolygonInfo generatePolygon(AbsImage * image, const Rect& rect = Rect::ZERO, float epsilon = 2.0f, float threshold = 0.05f);

public:
	Rect getRealRect() { return _realRect; }
	float getTotalArea() { return _realRect.size.width * _realRect.size.height; }
	float getTotalTrianglesArea();
protected:
	Vec2 findFirstNoneTransparentPixel(const Rect& rect, float threshold);
	std::vector<Vec2> marchSquare(const Rect& rect, const Vec2& first, float threshold);
	unsigned int getSquareValue(unsigned int x, unsigned int y, const Rect& rect, float threshold);

	unsigned char getAlphaByIndex(unsigned int i);
	unsigned char getAlphaByPos(const Vec2& pos);

	int getIndexFromPos(unsigned int x, unsigned int y) { return y * _width + x; }
	Vec2 getPosFromIndex(unsigned int i) { return Vec2(static_cast<float>(i % _width), static_cast<float>(i / _width)); }

	std::vector<Vec2> rdp(const std::vector<Vec2>& v, float optimization);
	float perpendicularDistance(const Vec2& i, const Vec2& start, const Vec2& end);

	//real rect is the size that is in scale with the texture file
	Rect getRealRect(const Rect& rect);
	Rect _realRect;
	AbsImage* _image;
	unsigned char* _data;
	unsigned int _width;
	unsigned int _height;
	float _scaleFactor;
	unsigned int _threshold;
};


#endif // #ifndef COCOS_2D_CCAUTOPOLYGON_H__
