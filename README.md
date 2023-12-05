# 简介

**效果图：**

![](https://github.com/zhiyangyou/PNG2Polygon/blob/dev/docImages/xiaoguotu.gif)

**为什么做：**

减少overdraw

**为什么重写写：**	

​		谷歌检索现成可用开源算法花了挺多时间，但是仅找到cocos-2dx的[CCAutoPolygon](https://github.com/cocos2d/cocos2d-x/blob/v4/cocos/2d/CCAutoPolygon.cpp)算法，这个算法有一个严重缺陷，单图像中有若干个明显可以分割的对象，这个算法只会找到一个，导致很多有效像素被截掉，并不能适用于很大一部分图片，遂重构了该算法，沿用其凸包点裁剪了和凸包点外扩的代码。

​		目前讲过最好实现是[TexturePacker](https://www.codeandweb.com/texturepacker)软件的，支持挖洞，网格质量高。但是它并不开源，并且不喜欢其在Unity上的工作流（我需要调用CLI + 解析文本来实现等价的效果，而且有些同事的电脑上没有安装授权后的TexturePacker软件）。

# 算法步骤：

1. 寻找凸包
2. 凸包点裁剪
3. 凸包点外扩
4. 凸包合并
5. 凸包剖分



## TODO（如果有业余时间）

**feature：**

1. 挖洞 （更少的overdraw）
2. 更少的三角形
3. unity侧更多实用功能
4. 选中图像区域实现（Rect），用于图集

**fix：**

1. 点外扩时，小概率遇到部分凸包丢失，导致部分图像丢失
2. native代码的安全性检查和错误返回值
3. unicode系列编码的文件路劲

**perf：**

1. 一些c++写法可以更加高效，整理代码。减少因数据结构不一致导致数据流转的内存拷贝延迟
