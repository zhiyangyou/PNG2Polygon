# 简介

**效果图：**

![](https://github.com/zhiyangyou/PNG2Polygon/blob/main/docImages/xiaoguotu.gif)

**为什么做：**

减少overdraw

**为什么重写：**	

​		谷歌检索现成可用开源算法花了挺多时间，但是仅找到cocos-2dx的[CCAutoPolygon](https://github.com/cocos2d/cocos2d-x/blob/v4/cocos/2d/CCAutoPolygon.cpp)算法，这个算法有一个严重缺陷，单图像中有若干个明显可以分割的对象，这个算法只会找到一个，导致很多有效像素被截掉，并不能适用于很大一部分图片，遂重构了该算法，沿用其凸包点裁剪和凸包点外扩的算法。

​		目前见过最好实现是[TexturePacker](https://www.codeandweb.com/texturepacker)软件的，支持挖洞，多边形包围紧凑，网格质量高。但是它并不开源，并且不喜欢其在Unity上的工作流（需要调用CLI + 解析文本来实现等价的效果，而且有些同事的电脑上没有安装授权后的TexturePacker软件）。

# 步骤：

1. 寻找凸包
2. 凸包点裁剪
3. 凸包点外扩
4. 凸包合并
5. 凸包剖分
