
# 大型岩石材质文档


![效果图](Imgs/%E6%95%88%E6%9E%9C%E5%9B%BE.png)

---

## 要求:

1.	岩石体积大于10mX10mX10m体积，实现方式应保证岩石贴图精度的条件下尽量降低渲染开销；
2.	岩石表面需要能够融合一层地表材质，例如上图中的草地tile，美术对融合区域和融合方式应可控；
3.	（选做）岩石与地形、其他岩石穿插实现柔和过渡，而不是像上图2所示有明显接缝。

---

### 环境 unity版本使用2021.2.7 urp使用12.7版本

---

## 实现:

实现所在的场景路径：`Project\Assets\Scenes\SampleScene.unity`

- 实现了基础pbr材质，根据urp模板中修改而成。
    - 修改了pbr材质的编辑器材质面板，操作更简便。
    - 根据项目需求定制需要的pbr遮罩贴图通道采样。
    - 实现了细节纹理和基础纹理的混合。

![材质测试](Imgs/%E6%9D%90%E8%B4%A8%E6%B5%8B%E8%AF%95.png)

[基础细节石头材质编辑器面板](Imgs/%E6%9D%90%E8%B4%A8%20(1).png)

[基础石头材质编辑器面板](Imgs/%E6%9D%90%E8%B4%A8%20(3).png)

[基础草地材质编辑器面板](Imgs/%E6%9D%90%E8%B4%A8%20(4).png)

---

- 实现了多层pbr材质，初步实现方法是将多个pbr材质的surfacedata进行混合，使用多个贴图采样器去接收来自不同材质的贴图和属性。
    - 混合遮罩的计算
        - 通过多层材质本身的layermask和procedural layermask多材质混合。
        - 多层材质有两种不同的混合方式，分别是贴图控制和顶点颜色控制。
        - 额外的混合方式是通过计算ps阶段的世界法线y通道得到的procedural layermask，用于混合在顶面的材质。
        - 混合计算过程会将pbr材质中的高度图添加到计算中，得到有比较自然的高度混合。 
    - 色调混合
        - 当遮罩模式作为贴图时，顶点颜色会被作为色调运算进行混合。
        - 当顶点颜色作为遮罩时，贴图颜色会被作为色调运算进行混合。
    - 混合权重
        - 混合计算的结果可以通过minmaxslider进行调节，暂时定义固定的最小值和最大值。
    - 通过材质编辑器面板中的材质field去逐个接收来自不同材质的贴图和属性，并且将其赋值到多层材质中。
        - 暂时将序列化的object field数据序列化到了scriptable object中，并且在打开材质时会查找当前目录下的scriptable object.
    - 多层材质可以控制层次数，暂时实现的是2层。
        - 现在是一一对应的贴图采样器，后面可以使用sampler_state进行优化复用采样器。
        - 使用贴图数组也同样可以优化采样器过多的问题。
    - 目前贴图的采样器在两层pbr材质下打到了14个，优化方向可以优先减少采样器，采取复用采样器的策略。

![材质的多功能](Imgs/%E6%9D%90%E8%B4%A8%E5%8A%9F%E8%83%BD.png)

![多层材质](Imgs/%E6%9D%90%E8%B4%A8%20(2).png)

---

- 无额外渲染开销的静态柔和过渡效果
    - 通过修改模型数据中的顶点法线，将目标模型和自身模型顶点进行邻近值运算，找到模型中最接近的顶点作为遮罩，使用该遮罩作为顶点法线传递的遮罩，将顶点的法线传递过去以达到过渡柔和的效果。
    - 关于柔和过渡效果的另一种做法。
        - 将需要进行混合的材质和地形，从上空拍一张带有深度、法线、颜色的图，再从需要混合的模型中进行贴图采样和混合。

---

- 后续的优化和制作方向
    - 重载的材质面板代码进行抽象和优化
    - 对自定义库进行整理和抽象删除不需要的属性和表面数据。
    - 减少使用到的keyword
    - 添加lod 着色器运算
    - 多层材质中使用复用采样器
    - 减少材质面板中的多余操作或重复操作，提高整体效率。

- 实现过程发现的问题
    - 非propertity数据不能序列化到材质中。
    - 当采样贴图类型为法线贴图时，贴图中的alpha通道将无法读取。
    - unity 将smoothness存到alpha通道是因为specular color占用了rgb的三个通道（比较反人类）

---

### 性能分析

GPA截帧:
![GPA截帧](Imgs/GPA%E5%88%86%E6%9E%90%E5%9B%BE.png)
![GPA截帧](Imgs/GPA%E5%88%86%E6%9E%90%E5%9B%BE00.png)

上图结果显示数据结构偏多,可以适当减少cpu端传递的数据。


---

## 资源

根据理解，各贴图的通道含义如下：

```
rock map
- diffuse,rgba
    - rgb:color
    - a:?
- normal,rgba
    - rgb:normal
    - a:null
```

```
grass map 
- diffuse
    - rgb:color
    - a:heightmask
- normal,rgba
    - r:x
    - g:y
    - b:heightmask
    - a:detailheightmask
- smbe,rgba
    - r:smoothness/specular/roughness
    - g:metalic
    - b:occlusion?
    - a:emission
```

```
rock detail map
- diffuse,rgba
    - rgb:color
    - a: heightmask
- smbe,rgba
    - r:smoothness/specular/roughness
    - g:metalic
    - b:occlusion?
    - a:emission
- normal,rgba
    - rgb:normal
    - a:?

```

---

### 以下是一些过程时笔记


```
- 实现URP pbr lit material
    - 先尝试simple lit 测试结果效果不好
    - lit shader
    - 定义输入项    
    - 完成两个材质测试
    - 混合两个材质
    - 修改lit forward pass
    - 修改lit input
    - 添加函数修改着色，在lit forward pass 中注入
        - 分支 先混合完surface data 再进入pbr的计算
        - 分支2 计算完surface data 直接进入pbr的计算后混合
    - _PBRMaskMap
        - R Metallic
        - G Smoothness
        - B occlusion
        - A Emission

- 重载材质编辑器界面
    - basemap tint(tootip("rgb color ,alpha:xxx"))
    - normal map
    - pbr mask map (smbe)
    - emission toggle
        - emission tint
    - metallic slider(0,1)
    - smoothness slider(0,1)
    - occlusion slider(0,1)
    - layer mask type
    - surface input 可以修改
    - layer lit 中的object filed将mat的属性传递过去

- material layer的功能 
    - shader replacement feature//相机渲染的替换着色器，无法做分层
    - 暂时预制两层属性
    - hdrp里的分层原理，预先做好四层的分层属性，通过配置材质面板后将各分层材质的字段同步到总的分层材质中，实现分层管理。但是只有匹配对应的字段才可以传递,也就是相同的lit shader才有可能被传递。
    - 参考hdrp 中有layeredlit的功能
    - sciptableobject 包络两个材质，并且自动生成layer，根据layer传递材质球参数.（过时）
    - 重写编辑器面板material block propertiy
    - 用object field 传递material propertiy
    - layer mask 
        - 输入源:vertex col
            - 使用vertex col作为layermask的时候，tex2d作为tint map
        - 输入源:texture 2d (uv map01)
            - 使用tex2d作为layermask的时候，vertex col作为tint map
        - 输入源：vertex col. alpha
            - vertex.rgb 作为tint map ， vertex.alpha 作为layermask
        - 通过输入的heightmap计算layermask
        - layermask 混合权重(min,max) 有一个minmaxslider控制
    - normal dot up normal layer mask

- 融合的功能
    - 融合法线
    - 法线融合- overlay算法
    - 融合度

- 高度可控融合输入源
    - vertex col
    - texure2d - mapping mode boxing mapping

- uv mapping
    - world space position uv
    - UV Triplane Mapping[具体实现](https://indienova.com/indie-game-development/unity-shader-triplanar-mapping/)


- properties:
    - mask weight
        - min
        - max
    - 

- keyword
    - up surface normal falt
    - enable layer
    - vertex  layer mask (rgb.a)
    - texture layer mask (rgb.a)
    
- BRDF 中还有surfacedata的引用
    - 修改surfacedata结构体需要递归到lighting.hlsl>brdf.hlsl中


- 初始化所有层级的surfacedata
    - 混合surfacedata

- 将材质面板中不可序列化的数据序列化到so中
    - 将so存放在和材质相同的目录，并且保持名字相同

- 将detailmap中的pbr map 补全


---

shader - 载入 8张贴图 

三个不同材质

detail
grass
rock-base

detail map - box mapping + worldposition

---

uv mapping
rock map
- uvmaps

detail map
- box mapping

grass map
- world position z uv mapping

---

vs input 
- color :rgba
    - rgb:color
    - a: heightblendfrac

rock map
- diffuse,rgba
    - rgb:color
    - a:?
- normal,rgba
    - rgb:normal
    - a:null

grass map 
- diffuse
    - rgb:color
    - a:heightmask
- normal,rgba
    - r:x
    - g:y
    - b:heightmask
    - a:detailheightmask
- smbe,rgba
    - r:smoothness/specular/roughness
    - g:metalic
    - b:occlusion?
    - a:emission

rock detail map
- diffuse,rgba
    - rgb:color
    - a: heightmask
- smbe,rgba
    - r:smoothness/specular/roughness
    - g:metalic
    - b:occlusion?
    - a:emission
- normal,rgba
    - rgb:normal
    - a:?

---
workflowMode;
basemap;
baseColor;
normalmap;
normalScale;
pbrMasMap;
metallic;
roughness;
occulsion;
emission;
emissionColor;
```