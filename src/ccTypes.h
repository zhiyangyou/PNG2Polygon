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
#pragma once

#include <string>

#include "CCGeometry.h"
#include "Vec3.h"


/**
 * @addtogroup base
 * @{
 */



struct Color4B;
struct Color4F;

/**
 * RGB color composed of bytes 3 bytes.
 * @since v3.0
 */
struct   Color3B
{
	Color3B() :r(0), g(0), b(0) {}
	Color3B(uint8_t _r, uint8_t _g, uint8_t _b) :r(_r), g(_g), b(_b) {}
	explicit Color3B(const Color4B& color);
	explicit Color3B(const Color4F& color);

	bool operator==(const Color3B& right) const;
	bool operator==(const Color4B& right) const;
	bool operator==(const Color4F& right) const;
	bool operator!=(const Color3B& right) const;
	bool operator!=(const Color4B& right) const;
	bool operator!=(const Color4F& right) const;

	bool equals(const Color3B& other) const
	{
		return (*this == other);
	}

	uint8_t r = 0;
	uint8_t g = 0;
	uint8_t b = 0;


};

/**
 * RGBA color composed of 4 bytes.
 * @since v3.0
 */
struct   Color4B
{
	Color4B() :r(0), g(0), b(0), a(0) {}
	Color4B(uint8_t _r, uint8_t _g, uint8_t _b, uint8_t _a)
	{
		set(_r, _g, _b, _a);
	}
	explicit Color4B(const Color3B& color, uint8_t _a = 255);
	explicit Color4B(const Color4F& color);

	inline void set(uint8_t _r, uint8_t _g, uint8_t _b, uint8_t _a)
	{
		r = _r;
		g = _g;
		b = _b;
		a = _a;
	}

	bool operator==(const Color4B& right) const;
	bool operator==(const Color3B& right) const;
	bool operator==(const Color4F& right) const;
	bool operator!=(const Color4B& right) const;
	bool operator!=(const Color3B& right) const;
	bool operator!=(const Color4F& right) const;

	uint8_t r = 0;
	uint8_t g = 0;
	uint8_t b = 0;
	uint8_t a = 0;


};


/**
 * RGBA color composed of 4 floats.
 * @since v3.0
 */
struct   Color4F
{
	Color4F();
	Color4F(float _r, float _g, float _b, float _a);
	explicit Color4F(const Color3B& color, float _a = 1.0f);
	explicit Color4F(const Color4B& color);

	bool operator==(const Color4F& right) const;
	bool operator==(const Color3B& right) const;
	bool operator==(const Color4B& right) const;
	bool operator!=(const Color4F& right) const;
	bool operator!=(const Color3B& right) const;
	bool operator!=(const Color4B& right) const;

	bool equals(const Color4F& other) const
	{
		return (*this == other);
	}

	float r = 0.f;
	float g = 0.f;
	float b = 0.f;
	float a = 0.f;

};

Color4F& operator+=(Color4F& lhs, const Color4F& rhs);
Color4F operator+(Color4F lhs, const Color4F& rhs);

Color4F& operator-=(Color4F& lhs, const Color4F& rhs);
Color4F operator-(Color4F lhs, const Color4F& rhs);

Color4F& operator*=(Color4F& lhs, const Color4F& rhs);
Color4F operator*(Color4F lhs, const Color4F& rhs);
Color4F& operator*=(Color4F& lhs, float rhs);
Color4F operator*(Color4F lhs, float rhs);

Color4F& operator/=(Color4F& lhs, const Color4F& rhs);
Color4F operator/(Color4F lhs, const Color4F& rhs);
Color4F& operator/=(Color4F& lhs, float rhs);
Color4F operator/(Color4F lhs, float rhs);


/** A vertex composed of 2 floats: x, y
 @since v3.0
 */
 // struct Vertex2F
 // {
 //     Vertex2F(float _x, float _y) :x(_x), y(_y) {}

 //     Vertex2F(): x(0.f), y(0.f) {}

 //     GLfloat x;
 //     GLfloat y;
 // };


 /** A vertex composed of 2 floats: x, y
  @since v3.0
  */
  // struct Vertex3F
  // {
  //     Vertex3F(float _x, float _y, float _z)
  //         : x(_x)
  //         , y(_y)
  //         , z(_z)
  //     {}

  //     Vertex3F(): x(0.f), y(0.f), z(0.f) {}

  //     GLfloat x;
  //     GLfloat y;
  //     GLfloat z;
  // };

  /** @struct Tex2F
   * A TEXCOORD composed of 2 floats: u, y
   * @since v3.0
   */
struct   Tex2F {
	Tex2F(float _u, float _v) : u(_u), v(_v) {}

	Tex2F() {}

	float u = 0.f;
	float v = 0.f;
};

/** @struct PointSprite
 * Vec2 Sprite component.
 */
struct   PointSprite
{
	Vec2       pos;        // 8 bytes
	Color4B    color;      // 4 bytes
	float      size = 0.f;       // 4 bytes
};

/** @struct Quad2
 * A 2D Quad. 4 * 2 floats.
 */
struct   Quad2
{
	Vec2        tl;
	Vec2        tr;
	Vec2        bl;
	Vec2        br;
};

/** @struct Quad3
 * A 3D Quad. 4 * 3 floats.
 */
struct   Quad3 {
	Vec3        bl;
	Vec3        br;
	Vec3        tl;
	Vec3        tr;
};

/** @struct V2F_C4B_T2F
 * A Vec2 with a vertex point, a tex coord point and a color 4B.
 */
struct V2F_C4B_T2F
{
	/// vertices (2F)
	Vec2       vertices;
	/// colors (4B)
	Color4B        colors;
	/// tex coords (2F)
	Tex2F          texCoords;
};

/** @struct V2F_C4B_PF
 *
 */
struct V2F_C4B_PF
{
	/// vertices (2F)
	Vec2       vertices;
	/// colors (4B)
	Color4B        colors;
	/// pointsize
	float      pointSize = 0.f;
};

/** @struct V2F_C4F_T2F
 * A Vec2 with a vertex point, a tex coord point and a color 4F.
 */
struct   V2F_C4F_T2F
{
	/// vertices (2F)
	Vec2       vertices;
	/// colors (4F)
	Color4F        colors;
	/// tex coords (2F)
	Tex2F          texCoords;
};

/** @struct V3F_C4B_T2F
 * A Vec2 with a vertex point, a tex coord point and a color 4B.
 */
struct   V3F_C4B_T2F
{
	/// vertices (3F)
	Vec3     vertices;            // 12 bytes

	/// colors (4B)
	Color4B      colors;              // 4 bytes

	// tex coords (2F)
	Tex2F        texCoords;           // 8 bytes
};

/** @struct V3F_T2F
 * A Vec2 with a vertex point, a tex coord point.
 */
struct   V3F_T2F
{
	/// vertices (2F)
	Vec3       vertices;
	/// tex coords (2F)
	Tex2F          texCoords;
};

/** @struct V3F_C4F
* A Vec3 with a vertex point, a color.
*/
struct   V3F_C4F
{
	/// vertices (3F)
	Vec3 vertices;
	/// vertices (4F)
	Color4F colors;
};

/** @struct V2F_C4B_T2F_Triangle
 * A Triangle of V2F_C4B_T2F.
 */
struct   V2F_C4B_T2F_Triangle
{
	V2F_C4B_T2F a;
	V2F_C4B_T2F b;
	V2F_C4B_T2F c;
};

/** @struct V2F_C4B_T2F_Quad
 * A Quad of V2F_C4B_T2F.
 */
struct   V2F_C4B_T2F_Quad
{
	/// bottom left
	V2F_C4B_T2F    bl;
	/// bottom right
	V2F_C4B_T2F    br;
	/// top left
	V2F_C4B_T2F    tl;
	/// top right
	V2F_C4B_T2F    tr;
};

/** @struct V3F_C4B_T2F_Quad
 * 4 Vertex3FTex2FColor4B.
 */
struct   V3F_C4B_T2F_Quad
{
	/// top left
	V3F_C4B_T2F    tl;
	/// bottom left
	V3F_C4B_T2F    bl;
	/// top right
	V3F_C4B_T2F    tr;
	/// bottom right
	V3F_C4B_T2F    br;
};

/** @struct V2F_C4F_T2F_Quad
 * 4 Vertex2FTex2FColor4F Quad.
 */
struct   V2F_C4F_T2F_Quad
{
	/// bottom left
	V2F_C4F_T2F    bl;
	/// bottom right
	V2F_C4F_T2F    br;
	/// top left
	V2F_C4F_T2F    tl;
	/// top right
	V2F_C4F_T2F    tr;
};

/** @struct V3F_T2F_Quad
 *
 */
struct   V3F_T2F_Quad
{
	/// bottom left
	V3F_T2F    bl;
	/// bottom right
	V3F_T2F    br;
	/// top left
	V3F_T2F    tl;
	/// top right
	V3F_T2F    tr;
};


namespace backend {
	enum class BlendFactor : uint32_t;
}

/** @struct BlendFunc
 * Blend Function used for textures.
 */
struct   BlendFunc
{
	/** source blend function */
	backend::BlendFactor src;
	/** destination blend function */
	backend::BlendFactor dst;

	/** Blending disabled. Uses {BlendFactor::ONE, BlendFactor::ZERO} */
	static const BlendFunc DISABLE;
	/** Blending enabled for textures with Alpha premultiplied. Uses {BlendFactor::ONE, BlendFactor::ONE_MINUS_SRC_ALPHA} */
	static const BlendFunc ALPHA_PREMULTIPLIED;
	/** Blending enabled for textures with Alpha NON premultiplied. Uses {BlendFactor::SRC_ALPHA, BlendFactor::ONE_MINUS_SRC_ALPHA} */
	static const BlendFunc ALPHA_NON_PREMULTIPLIED;
	/** Enables Additive blending. Uses {BlendFactor::SRC_ALPHA, BlendFactor::ONE} */
	static const BlendFunc ADDITIVE;

	bool operator==(const BlendFunc& a) const
	{
		return src == a.src && dst == a.dst;
	}

	bool operator!=(const BlendFunc& a) const
	{
		return src != a.src || dst != a.dst;
	}

	bool operator<(const BlendFunc& a) const
	{
		return src < a.src || (src == a.src && dst < a.dst);
	}
};





// end group
/// @}
