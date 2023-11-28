# 作用

输入一个png图像，输出一个该png图像的多边形网格。

效果大致是这样的

<img src="docImages\image-20231128232744585.png" alt="image-20231128232744585" style="zoom: 25%;" />

在测试工程中，输出这样的数据

```
-------begin-------
verts:
(3.6,115.7) (0,86.6) (152,91.2) (77.3,121.8) (120.3,137.5) (152,108.2) (44.1,136) (32.9,136) (142.1,42.4) (0,63.9) (63.9,0) (87.5,0) 
indeices:
0 1 2 2 3 0 2 4 3 5 4 2 3 6 0 6 7 0 2 1 8 8 1 9 9 10 8 11 8 10 
uvs:
(0.0236842 , 0.167626)(0 , 0.376978)(1 , 0.343885)(0.508553 , 0.123741)(0.791447 , 0.0107914)(1 , 0.221583)(0.290132 , 0.0215827)(0.216447 , 0.0215827)(0.934868 , 0.694964)(0 , 0.540288)(0.420395 , 1)(0.575658 , 1)
-------end-------

```



# 为什么需要这个

看一下cocos官方的文档解释 https://docs.cocos.com/cocos2d-x/manual/en/sprites/polygon.html

这么好的算法，我希望应用于其他游戏引擎的项目，故将其剥离出来

# 算法从哪里来

1. [cocos-2dx/CCAutoPolygon.cpp](https://github.com/cocos2d/cocos2d-x/blob/v4/cocos/2d/CCAutoPolygon.cpp)
2. [stb_image](https://github.com/nothings/stb/blob/master/stb_image.h)
3. [poly2tri](https://github.com/greenm01/poly2tri/tree/master)  
4. [clpper](https://dl.acm.org/doi/10.1145/129902.129906)

