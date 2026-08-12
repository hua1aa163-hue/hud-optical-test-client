namespace HudOpticalTestClient.Protocol;

/// <summary>HUD 通信协议 V3.1 的 21 个光学测试和 5 个辅助指令目录。</summary>
public static class HudCommandCatalog
{
    public const string OpticalTestCategory = "光学测试";
    public const string AuxiliaryCategory = "辅助指令";

    private static readonly TestDefinition[] Definitions = CreateDefinitions();
    private static readonly IReadOnlyDictionary<string, TestDefinition> DefinitionsByCode =
        Definitions.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);

    /// <summary>全部 26 条定义，顺序为 t1～t21、c-、gin、pic-、bmp-、n-。</summary>
    public static IReadOnlyList<TestDefinition> All { get; } = Array.AsReadOnly(Definitions);

    /// <summary>t1～t21 共 21 个测试项目。</summary>
    public static IReadOnlyList<TestDefinition> Tests { get; } =
        Array.AsReadOnly(Definitions.Where(item => item.Category == OpticalTestCategory).ToArray());

    /// <summary>5 个辅助指令。</summary>
    public static IReadOnlyList<TestDefinition> AuxiliaryCommands { get; } =
        Array.AsReadOnly(Definitions.Where(item => item.Category == AuxiliaryCategory).ToArray());

    /// <summary>按目录代码查找定义。</summary>
    public static bool TryGet(string? code, out TestDefinition definition)
    {
        if (code is not null && DefinitionsByCode.TryGetValue(code.Trim(), out TestDefinition? found))
        {
            definition = found;
            return true;
        }

        definition = null!;
        return false;
    }

    /// <summary>按目录代码取得定义；不存在时抛出异常。</summary>
    public static TestDefinition Get(string code)
    {
        return TryGet(code, out TestDefinition definition)
            ? definition
            : throw new KeyNotFoundException($"协议目录中不存在命令“{code}”。");
    }

    private static TestDefinition[] CreateDefinitions()
    {
        ProtocolFieldDefinition[] geometry29 = CreateGeometry29Fields();
        ProtocolFieldDefinition[] color11 = CreateColorFields();
        ProtocolFieldDefinition[] resultField =
        [
            F("result", "执行结果", "OK 表示成功；Fail 表示失败。")
        ];

        return
        [
            new(
                OpticalTestCategory,
                "t1",
                "白画面测量",
                "测量九点亮度、平均值、均匀性、极值、FOV 和相机画面尺寸。规范称返回18项，但原文示例只有17项，疑似缺少“最小值”；字段表仍按规范18项列出。",
                "t1",
                "t1_Result:<18个空格分隔值>,%（兼容文档示例17项）",
                responseFields: CreateT1Fields()),

            new(
                OpticalTestCategory,
                "t2",
                "黑画面测量",
                "测量九点黑画面亮度、均匀性和对比度。规范称17项；原示例数值存在粘连且仅能可靠识别前13项，解析或人工对照时需保留警告。",
                "t2",
                "t2_Result:<17个空格分隔值>,%（兼容原文不完整示例）",
                responseFields: CreateT2Fields()),

            new(
                OpticalTestCategory,
                "t3",
                "虚像距离测量",
                "返回中心/平均距离、FOV、旋转与倾斜、畸变、分辨率、中心坐标和图像尺寸共29项。",
                "t3",
                "t3_Result 或 t3_Result(<点数>):<29个空格分隔值>,%",
                responseFields: geometry29),

            new(
                OpticalTestCategory,
                "t4",
                "畸变倾斜测量",
                "返回距离、FOV、旋转与倾斜、畸变、分辨率、中心坐标和图像尺寸共29项。",
                "t4",
                "t4_Result 或 t4_Result(<点数>):<29个空格分隔值>,%",
                responseFields: geometry29),

            new(
                OpticalTestCategory,
                "t5",
                "点阵畸变校正",
                "返回 m*n 个点的中心坐标与畸变，末组给出最大畸变。Error0 表示识别点数偏多/阈值偏低；Error1 表示识别点数偏少/阈值偏高。",
                "t5",
                "t5_Result:<x y distortion>,...<0 0 maxDistortion>,% 或 Error0%/Error1%",
                responseFields:
                [
                    F("pointCenterX", "点中心 X", "前 m*n 组的第1项。", repeated: true),
                    F("pointCenterY", "点中心 Y", "前 m*n 组的第2项。", repeated: true),
                    F("pointDistortion", "该点畸变", "前 m*n 组的第3项。", repeated: true),
                    F("maxDistortion", "最大畸变", "最后一组为 0 0 maxDistortion，取第3项。")
                ]),

            new(
                OpticalTestCategory,
                "t6",
                "MTF 清晰度测量",
                "测量黑白条纹画面中心和边缘九个区域的 MTF。",
                "t6",
                "t6_Result:<9区MTF min max avg>,%",
                responseFields: CreateMtfFields()),

            new(
                OpticalTestCategory,
                "t7",
                "白画面色度测量",
                "返回九点颜色、平均颜色和色差共11组。协议称每组三项，但没有给出末组色差三个数的逐项语义。",
                "t7",
                "t7_Result:<11个逗号分组；每组3值>,%",
                responseFields: color11),

            new(
                OpticalTestCategory,
                "t8",
                "字符完整度和镜像测试",
                "判断字符是否完整、镜像方向是否正常。",
                "t8",
                "t8_Result:1,%（正常）或 t8_Result:2,%（异常）",
                responseFields:
                [
                    F("status", "字符状态", "1=正常；2=异常。")
                ]),

            new(
                OpticalTestCategory,
                "t9",
                "快速眼盒测试",
                "快速判断当前画面是否处于眼盒范围。",
                "t9",
                "t9_Result:1 0 0,%（范围内）或 2 0 0,%（范围外）",
                responseFields:
                [
                    F("status", "眼盒状态", "1=范围内；2=范围外。"),
                    F("reserved1", "保留值1", "协议示例固定为0。"),
                    F("reserved2", "保留值2", "协议示例固定为0。")
                ]),

            new(
                OpticalTestCategory,
                "t10",
                "眼盒测量",
                "点数与设置一致时返回点阵中心对应的 x/y 角度；y 可用于下视角标定。协议没有写明单位。",
                "t10",
                "t10_Result:1 x y,%（范围内）或 2 0 0,%（范围外）",
                responseFields:
                [
                    F("status", "眼盒状态", "1=范围内；2=范围外。"),
                    F("centerAngleX", "中心点 X 对应角度", "范围内时返回；单位未定义。"),
                    F("centerAngleY", "中心点 Y 对应角度", "范围内时返回，可用于下视角标定；单位未定义。")
                ]),

            new(
                OpticalTestCategory,
                "t11",
                "双目视差测量",
                "无参数时发送 t11；需要上位机设置平移距离时，X/Y 必须成对填写并发送 t11/X/Y。文档未定义平移距离单位和范围。",
                "t11",
                "<原请求>_Result(<点数>):<x视差 y视差>,...,%；视差单位 mrad",
                fields:
                [
                    F("xTranslation", "X 平移距离", "可选；与 Y 平移距离必须同时填写。", example: "10"),
                    F("yTranslation", "Y 平移距离", "可选；与 X 平移距离必须同时填写。", example: "20")
                ],
                responseFields:
                [
                    F("xDisparity", "X 方向视差", "每个点一组，第1项。", unit: "mrad", repeated: true),
                    F("yDisparity", "Y 方向视差", "每个点一组，第2项。", unit: "mrad", repeated: true)
                ]),

            new(
                OpticalTestCategory,
                "t12",
                "红画面色度测量",
                "返回九点颜色、平均颜色和色差共11组；末组逐项语义未定义。",
                "t12",
                "t12_Result:<11个逗号分组；每组3值>,%",
                responseFields: color11),

            new(
                OpticalTestCategory,
                "t13",
                "绿画面色度测量",
                "返回九点颜色、平均颜色和色差共11组；末组逐项语义未定义。",
                "t13",
                "t13_Result:<11个逗号分组；每组3值>,%",
                responseFields: color11),

            new(
                OpticalTestCategory,
                "t14",
                "蓝画面色度测量",
                "返回九点颜色、平均颜色、色差、色域值和 NTSC 色域值。原示例是11个三元组后跟两个标量。",
                "t14",
                "t14_Result:<11个三元组>,<色域值>,<NTSC色域值%>",
                responseFields: CreateBlueColorFields()),

            new(
                OpticalTestCategory,
                "t15",
                "线重影测量",
                "返回 m*n 条线的重影、本体亮度、重影亮度，末组为三项最大值。",
                "t15",
                "t15_Result(<点数>):<m*n组三值>,<三项最大值>,%",
                responseFields:
                [
                    F("ghost", "重影", "前 m*n 组第1项。", repeated: true),
                    F("bodyLuminance", "本体亮度", "前 m*n 组第2项。", repeated: true),
                    F("ghostLuminance", "重影亮度", "前 m*n 组第3项。", repeated: true),
                    F("maxGhost", "重影最大值", "最后一组第1项。"),
                    F("maxBodyLuminance", "本体亮度最大值", "最后一组第2项。"),
                    F("maxGhostLuminance", "重影亮度最大值", "最后一组第3项。")
                ]),

            new(
                OpticalTestCategory,
                "t16",
                "白黑棋盘格测量",
                "返回25个区域亮度和当前画面的黑白对比度。",
                "t16",
                "t16_Result:<25区亮度 contrast>,%",
                responseFields: CreateCheckerboardFields()),

            new(
                OpticalTestCategory,
                "t17",
                "黑白棋盘格测量",
                "返回25个区域亮度和当前画面的黑白对比度。",
                "t17",
                "t17_Result:<25区亮度 contrast>,%",
                responseFields: CreateCheckerboardFields()),

            new(
                OpticalTestCategory,
                "t18",
                "横线 MTF 测量",
                "返回九个区域 MTF 及最小、最大、平均值。",
                "t18",
                "t18_Result:<9区MTF min max avg>,%",
                responseFields: CreateMtfFields()),

            new(
                OpticalTestCategory,
                "t19",
                "AR 靶标测量",
                "返回 m 个靶标/HUD 坐标组；最后一组仅第1项定义为偏移坐标最大值，其余4项应按保留值处理。",
                "t19",
                "t19_Result(<点数>):<m组五值>,<maxOffset及4个保留值>,%",
                responseFields:
                [
                    F("targetY", "靶标坐标 Y", "前 m 组第1项。", repeated: true),
                    F("targetZ", "靶标坐标 Z", "前 m 组第2项。", repeated: true),
                    F("hudY", "HUD 点坐标 Y", "前 m 组第3项。", repeated: true),
                    F("hudZ", "HUD 点坐标 Z", "前 m 组第4项。", repeated: true),
                    F("hudX", "HUD 点坐标 X", "前 m 组第5项。", repeated: true),
                    F("maxOffset", "偏移坐标最大值", "最后一组第1项，数据评判取此值。"),
                    F("reserved", "末组保留值", "最后一组其余4项，协议未定义含义。", repeated: true)
                ]),

            new(
                OpticalTestCategory,
                "t20",
                "综合畸变测试",
                "规范定义21项。原示例有多处缺失空格，目录仅保留规范顺序，不对粘连数值做静默修复。原文“4个型畸变”疑似缺字，按原文标注。",
                "t20",
                "t20_Result:<21个空格分隔值>,%",
                responseFields: CreateDistortion21Fields()),

            new(
                OpticalTestCategory,
                "t21",
                "FOV 测试",
                "返回与 t3/t4 同序的29项。正文示例前缀是 t21_Result，但末尾说明误写为 t3_Result；匹配器优先 t21，并兼容该文档笔误。",
                "t21",
                "t21_Result 或 t21_Result(<点数>):<29个空格分隔值>,%",
                responseFields: geometry29),

            new(
                AuxiliaryCategory,
                "c-",
                "切换配方",
                "切换到指定生产线配方。",
                "c-qwert",
                "OK%（成功）或 Fail%（失败）",
                fields:
                [
                    F("recipe", "配方名称", "拼接在 c- 之后。", "qwert", required: true, example: "qwert")
                ],
                responseFields: resultField),

            new(
                AuxiliaryCategory,
                "gin",
                "查询相机连接状态",
                "判断相机是否连接。原文说明句误复用了“切换配方”，命令名称与上下文明确为相机状态查询。",
                "gin",
                "OK%（连接/成功）或 Fail%（未连接/失败）",
                responseFields: resultField),

            new(
                AuxiliaryCategory,
                "pic-",
                "按完整文件名保存图片",
                "把图片保存到指定的完整文件路径，扩展名决定图片格式。",
                @"pic-D:\1\2.jpg",
                "OK%（成功）或 Fail%（失败）",
                fields:
                [
                    F("filePath", "完整图片路径", "拼接在 pic- 之后，需同时包含目录、文件名和扩展名。", @"D:\1\2.jpg", required: true, example: @"D:\1\2.jpg")
                ],
                responseFields: resultField),

            new(
                AuxiliaryCategory,
                "bmp-",
                "按目录和基名保存 BMP",
                "同时保存原图和测试图；格式为 bmp-目录|基名。文件名规则为“基名-测试项名称-原图/测试图-日期.bmp”。",
                @"bmp-D:\1|str",
                "OK%（成功）或 Fail%（失败）",
                fields:
                [
                    F("directory", "保存目录", "bmp- 之后、竖线之前的目录。", @"D:\1", required: true, example: @"D:\1"),
                    F("baseName", "文件基名", "竖线之后的名称前缀。", "str", required: true, example: "str")
                ],
                responseFields: resultField),

            new(
                AuxiliaryCategory,
                "n-",
                "保存测试数据",
                "把数据保存为“名称.xlsx”到光学软件设置的保存目录；请求末尾固定为“,%”。文档只定义成功返回 OK%，未定义失败返回。",
                "n-asdfg,%",
                "OK%（成功；失败格式未在 V3.1 中定义）",
                fields:
                [
                    F("workbookName", "工作簿名", "不含 .xlsx；构建时自动追加请求结尾“,%”。", "asdfg", required: true, example: "asdfg")
                ],
                responseFields:
                [
                    F("result", "执行结果", "V3.1 仅定义 OK=成功，未定义失败格式。")
                ])
        ];
    }

    private static ProtocolFieldDefinition F(
        string key,
        string name,
        string description,
        string defaultValue = "",
        bool required = false,
        string unit = "",
        string example = "",
        bool repeated = false) =>
        new(key, name, description, defaultValue, required, unit, example, repeated);

    private static ProtocolFieldDefinition[] CreateT1Fields()
    {
        var fields = Numbered("luminance", "亮度点", 9, "九点白画面亮度，第{0}项。");
        fields.AddRange(
        [
            F("averageLuminance", "平均亮度", "第10项。"),
            F("uniformity", "均匀性", "第11项。"),
            F("maxLuminance", "最大亮度", "第12项。"),
            F("minLuminance", "最小亮度", "第13项；原示例疑似缺少此项。"),
            F("fovH", "水平视场 FOV-H", "第14项。"),
            F("fovV", "竖直视场 FOV-V", "第15项。"),
            F("cameraPixelWidth", "相机水平方向像素宽度", "第16项。"),
            F("cameraPixelHeight", "相机竖直方向像素数", "第17项。"),
            F("totalCameraPixels", "白画面总相机像素数", "第18项。")
        ]);
        return fields.ToArray();
    }

    private static ProtocolFieldDefinition[] CreateT2Fields()
    {
        var fields = Numbered("luminance", "亮度点", 9, "九点黑画面亮度，第{0}项。");
        fields.AddRange(
        [
            F("averageLuminance", "平均亮度", "第10项。"),
            F("uniformity", "均匀性", "第11项。"),
            F("maxLuminance", "最大亮度", "第12项。"),
            F("minLuminance", "最小亮度", "第13项。"),
            F("averageContrast", "平均对比度", "第14项；原示例在此处存在粘连/缺项。"),
            F("maxContrast", "最大对比度", "第15项。"),
            F("minContrast", "最小对比度", "第16项。"),
            F("centerContrast", "中心对比度", "第17项。")
        ]);
        return fields.ToArray();
    }

    private static ProtocolFieldDefinition[] CreateGeometry29Fields() =>
    [
        F("distance", "中心点距离 (distance)", "第1项。"),
        F("meanDistance", "平均距离 (mean distance)", "第2项。"),
        F("fovH", "水平视场 (FOV-H)", "第3项。"),
        F("fovV", "竖直视场 (FOV-V)", "第4项。"),
        F("xRotation", "X 旋转 (X-Rotation)", "第5项。"),
        F("yRotation", "Y 旋转 (Y-Rotation)", "第6项。"),
        F("downAngle", "下视角 (Down angle)", "第7项。"),
        F("leftAngle", "左视角 (Left angle)", "第8项。"),
        F("ghost", "重影 (Ghost)", "第9项。"),
        F("dt", "平均光学畸变 (Dt)", "第10项。"),
        F("dmt", "最大光学畸变 (Dmt)", "第11项。"),
        F("tvHeight", "TV 水平畸变 (TV-Height)", "第12项。"),
        F("tvWidth", "TV 竖直畸变 (TV-Width)", "第13项。"),
        F("tvUp", "TV 上边畸变 (TV-UP)", "第14项。"),
        F("tvDown", "TV 下边畸变 (TV-Down)", "第15项。"),
        F("tvLeft", "TV 左边畸变 (TV-Left)", "第16项。"),
        F("tvRight", "TV 右边畸变 (TV-Right)", "第17项。"),
        F("tvDiagonal", "TV 菱形畸变 (TV-Diagonal)", "第18项。"),
        F("xUp", "X 上边倾斜 (X-UP)", "第19项。"),
        F("xDown", "X 下边倾斜 (X-Down)", "第20项。"),
        F("yLeft", "Y 左边倾斜 (Y-Left)", "第21项。"),
        F("yRight", "Y 右边倾斜 (Y-Right)", "第22项。"),
        F("resH", "水平角分辨率 (Res-H)", "第23项。"),
        F("resV", "竖直角分辨率 (Res-V)", "第24项。"),
        F("centerX", "中心点 X 坐标", "第25项。"),
        F("centerY", "中心点 Y 坐标", "第26项。"),
        F("width", "图像宽度", "第27项。"),
        F("height", "图像高度", "第28项。"),
        F("aspectRatio", "长宽比 (Length/width)", "第29项。")
    ];

    private static ProtocolFieldDefinition[] CreateMtfFields()
    {
        var fields = Numbered("regionMtf", "区域 MTF ", 9, "第{0}项。", compactName: true);
        fields.AddRange(
        [
            F("minMtf", "MTF 最小值", "第10项。"),
            F("maxMtf", "MTF 最大值", "第11项。"),
            F("averageMtf", "MTF 平均值", "第12项。")
        ]);
        return fields.ToArray();
    }

    private static ProtocolFieldDefinition[] CreateColorFields() =>
    [
        F("pointColorTemperature", "九点色温", "前9组第1项。", repeated: true),
        F("pointColorX", "九点色坐标 x", "前9组第2项。", repeated: true),
        F("pointColorY", "九点色坐标 y", "前9组第3项。", repeated: true),
        F("averageColorTemperature", "平均色温", "第10组第1项。"),
        F("averageColorX", "平均色坐标 x", "第10组第2项。"),
        F("averageColorY", "平均色坐标 y", "第10组第3项。"),
        F("colorDifferenceTuple", "色差三元组", "第11组的三个值；协议未逐项定义含义。", repeated: true)
    ];

    private static ProtocolFieldDefinition[] CreateBlueColorFields()
    {
        var fields = CreateColorFields().ToList();
        fields.Add(F("gamut", "色域值", "第12组，标量。"));
        fields.Add(F("ntscGamut", "NTSC 色域值", "第13组，示例带百分号。", unit: "%"));
        return fields.ToArray();
    }

    private static ProtocolFieldDefinition[] CreateCheckerboardFields()
    {
        var fields = Numbered("regionLuminance", "区域亮度 ", 25, "第{0}项。", compactName: true);
        fields.Add(F("contrast", "黑白对比度", "第26项。"));
        return fields.ToArray();
    }

    private static ProtocolFieldDefinition[] CreateDistortion21Fields() =>
    [
        F("horizontal1", "水平畸变1", "第1项。"),
        F("horizontal2", "水平畸变2", "第2项。"),
        F("horizontal3", "水平畸变3", "第3项。"),
        F("horizontalMax", "水平畸变最大值", "第4项。"),
        F("vertical1", "竖直畸变1", "第5项。"),
        F("vertical2", "竖直畸变2", "第6项。"),
        F("vertical3", "竖直畸变3", "第7项。"),
        F("verticalMax", "竖直畸变最大值", "第8项。"),
        F("typeDistortion1", "“型畸变”1", "第9项；名称照录原文，疑似缺字。"),
        F("typeDistortion2", "“型畸变”2", "第10项；名称照录原文，疑似缺字。"),
        F("typeDistortion3", "“型畸变”3", "第11项；名称照录原文，疑似缺字。"),
        F("typeDistortion4", "“型畸变”4", "第12项；名称照录原文，疑似缺字。"),
        F("imageDistortion1", "图像畸变1", "第13项。"),
        F("imageDistortion2", "图像畸变2", "第14项。"),
        F("imageDistortion3", "图像畸变3", "第15项。"),
        F("imageDistortion4", "图像畸变4", "第16项。"),
        F("diamondDistortion", "菱形畸变", "第17项。"),
        F("pointDistortionMax", "点畸变最大值", "第18项。"),
        F("pointDistortionAverage", "点畸变平均值", "第19项。"),
        F("widthTrapezoid", "宽度梯形畸变", "第20项。"),
        F("heightTrapezoid", "高度梯形畸变", "第21项。")
    ];

    private static List<ProtocolFieldDefinition> Numbered(
        string keyPrefix,
        string namePrefix,
        int count,
        string descriptionFormat,
        bool compactName = false)
    {
        var result = new List<ProtocolFieldDefinition>(count);
        for (int index = 1; index <= count; index++)
        {
            string name = compactName ? $"{namePrefix}{index}" : $"{namePrefix}{index}";
            result.Add(F(
                $"{keyPrefix}{index}",
                name,
                string.Format(descriptionFormat, index)));
        }

        return result;
    }
}
