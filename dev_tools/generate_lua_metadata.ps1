# generate_lua_metadata.ps1
# 用于生成Ra3 Lua库的元数据JSON文件

param(
    [string]$LuaLibPath = "D:\tmp\Ra3CoronaMapLuaLib\lib",
    [string]$OutputPath = "..\Ra3MapUtils\data\ra3_lua_lib_metadata.json"
)

# 配置
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 正则表达式模式
$patterns = @{
    # --- 根据单位的命名获取unit table
    functionComment = '^---\s*(.+)$'

    # --- @param name string 单位的名字
    paramAnnotation = '^---\s*@param\s+(\w+)\s+(\S+)(?:\s+(.+))?$'

    # --- @return SystemUnitTable
    returnAnnotation = '^---\s*@return\s+(\S+)(?:\s+(.+))?$'

    # UnitModule.from_name = function(name)
    moduleFunction = '^(\w+)\.(\w+)\s*=\s*function\s*\(([^)]*)\)'

    # function Unit:set_name(name)
    classMethod = '^function\s+(\w+):(\w+)\s*\(([^)]*)\)'

    # --- @enum PlayerEnum
    enumAnnotation = '^---\s*@enum\s+(\w+)'

    # PlayerEnum = {
    enumDeclaration = '^(\w+)\s*=\s*\{'

    # Player_1 = "Player_1",
    enumValue = '^\s*(\w+)\s*=\s*"([^"]+)"'

    # --- @class Unit
    classAnnotation = '^---\s*@class\s+(\w+)'

    # --- @field id number
    fieldAnnotation = '^---\s*@field\s+(\w+)\s+(\S+)(?:\s+(.+))?$'
}

# 数据结构
$metadata = @{
    metadata = @{
        version = "1.0.0"
        lastUpdate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
        sourceLibPath = $LuaLibPath
        totalModules = 0
        totalFunctions = 0
    }
    categories = @{}
    modules = @()
    enums = @()
    classes = @()
    searchIndex = @{
        functionsByName = @{}
        modulesByName = @{}
    }
}

# 辅助函数：解析模块
function Parse-Module {
    param(
        [string[]]$Lines,
        [string]$FilePath,
        [string]$Category
    )

    $moduleName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    $module = @{
        name = $moduleName
        category = $Category
        filePath = "$Category/$FilePath"
        description = ""
        functions = @()
    }

    $currentComment = ""
    $currentParams = @()
    $currentReturn = $null

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]

        # 匹配函数注释（不包含@的注释行）
        if ($line -match $patterns.functionComment -and $line -notmatch '@') {
            $currentComment = $Matches[1].Trim()
        }
        # 匹配参数注解
        elseif ($line -match $patterns.paramAnnotation) {
            $currentParams += @{
                name = $Matches[1]
                type = $Matches[2]
                description = if ($Matches[3]) { $Matches[3].Trim() } else { "" }
            }
        }
        # 匹配返回值注解
        elseif ($line -match $patterns.returnAnnotation) {
            $currentReturn = @{
                type = $Matches[1]
                description = if ($Matches[2]) { $Matches[2].Trim() } else { "" }
            }
        }
        # 匹配函数定义
        elseif ($line -match $patterns.moduleFunction) {
            $modulePart = $Matches[1]
            $funcName = $Matches[2]
            $params = $Matches[3]

            # 只处理当前模块的函数
            if ($modulePart -eq $moduleName) {
                $function = @{
                    name = $funcName
                    signature = "$moduleName.$funcName($params)"
                    description = $currentComment
                    parameters = $currentParams
                    returns = $currentReturn
                }

                $module.functions += $function
            }

            # 重置状态
            $currentComment = ""
            $currentParams = @()
            $currentReturn = $null
        }
    }

    return $module
}

# 辅助函数：解析枚举
function Parse-Enum {
    param(
        [string[]]$Lines,
        [string]$FilePath,
        [string]$Category
    )

    $enumName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    $enums = @()

    $currentEnum = $null
    $inEnumBody = $false

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]

        # 匹配枚举注解
        if ($line -match $patterns.enumAnnotation) {
            # 如果之前有未完成的枚举，先保存
            if ($currentEnum) {
                $enums += $currentEnum
            }

            $currentEnum = @{
                name = $Matches[1]
                category = $Category
                filePath = "$Category/$FilePath"
                description = ""
                values = @()
            }
            $inEnumBody = $false
        }
        # 匹配枚举声明
        elseif ($line -match $patterns.enumDeclaration -and $currentEnum) {
            $inEnumBody = $true
        }
        # 匹配枚举值
        elseif ($inEnumBody -and $line -match $patterns.enumValue) {
            $currentEnum.values += @{
                key = $Matches[1]
                value = $Matches[2]
                description = ""
            }
        }
        # 枚举体结束
        elseif ($inEnumBody -and $line -match '^\}') {
            $inEnumBody = $false
        }
    }

    # 保存最后一个枚举
    if ($currentEnum) {
        $enums += $currentEnum
    }

    return $enums
}

# 辅助函数：解析类
function Parse-Class {
    param(
        [string[]]$Lines,
        [string]$FilePath,
        [string]$Category
    )

    $className = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    $class = @{
        name = $className
        category = $Category
        filePath = "$Category/$FilePath"
        description = ""
        fields = @()
        methods = @()
    }

    $currentComment = ""
    $currentParams = @()
    $currentReturn = $null

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]

        # 匹配类注解
        if ($line -match $patterns.classAnnotation) {
            if ($Matches[1] -eq $className) {
                # 下一行可能是类描述
                if ($i + 1 -lt $Lines.Count -and $Lines[$i + 1] -match '^---\s*(.+)$' -and $Lines[$i + 1] -notmatch '@') {
                    $class.description = $Matches[1].Trim()
                }
            }
        }
        # 匹配字段注解
        elseif ($line -match $patterns.fieldAnnotation) {
            $class.fields += @{
                name = $Matches[1]
                type = $Matches[2]
                description = if ($Matches[3]) { $Matches[3].Trim() } else { "" }
            }
        }
        # 匹配函数注释
        elseif ($line -match $patterns.functionComment -and $line -notmatch '@') {
            $currentComment = $Matches[1].Trim()
        }
        # 匹配参数注解
        elseif ($line -match $patterns.paramAnnotation) {
            $currentParams += @{
                name = $Matches[1]
                type = $Matches[2]
                description = if ($Matches[3]) { $Matches[3].Trim() } else { "" }
            }
        }
        # 匹配返回值注解
        elseif ($line -match $patterns.returnAnnotation) {
            $currentReturn = @{
                type = $Matches[1]
                description = if ($Matches[2]) { $Matches[2].Trim() } else { "" }
            }
        }
        # 匹配方法定义
        elseif ($line -match $patterns.classMethod) {
            $classNamePart = $Matches[1]
            $methodName = $Matches[2]
            $params = $Matches[3]

            # 只处理当前类的方法
            if ($classNamePart -eq $className) {
                $method = @{
                    name = $methodName
                    signature = "$className`:$methodName($params)"
                    description = $currentComment
                    parameters = $currentParams
                    returns = $currentReturn
                }

                $class.methods += $method
            }

            # 重置状态
            $currentComment = ""
            $currentParams = @()
            $currentReturn = $null
        }
    }

    return $class
}

# 辅助函数：构建搜索索引
function Build-SearchIndex {
    param($Metadata)

    # 索引模块函数
    foreach ($module in $Metadata.modules) {
        foreach ($func in $module.functions) {
            $key = $func.name.ToLower()
            if (-not $Metadata.searchIndex.functionsByName.ContainsKey($key)) {
                $Metadata.searchIndex.functionsByName[$key] = @()
            }
            $Metadata.searchIndex.functionsByName[$key] += "$($module.name).$($func.name)"
        }

        # 索引模块名
        $Metadata.searchIndex.modulesByName[$module.name.ToLower()] = $module.name
    }

    # 索引类方法
    foreach ($class in $Metadata.classes) {
        foreach ($method in $class.methods) {
            $key = $method.name.ToLower()
            if (-not $Metadata.searchIndex.functionsByName.ContainsKey($key)) {
                $Metadata.searchIndex.functionsByName[$key] = @()
            }
            $Metadata.searchIndex.functionsByName[$key] += "$($class.name):$($method.name)"
        }
    }
}

# 主逻辑

Write-Host "开始生成Lua库元数据..." -ForegroundColor Cyan
Write-Host "源路径: $LuaLibPath" -ForegroundColor Gray
Write-Host "输出路径: $OutputPath" -ForegroundColor Gray

if (-not (Test-Path $LuaLibPath)) {
    Write-Host "错误: Lua库路径不存在: $LuaLibPath" -ForegroundColor Red
    exit 1
}

# 扫描目录
$categories = @(
    @{name="basic_module"; description="Basic modules with core game functions"},
    @{name="basic_util"; description="Utility classes for math, string, etc."},
    @{name="enums"; description="Enum type definitions"},
    @{name="helper"; description="Helper classes with high-level wrappers"},
    @{name="object"; description="Object classes with OOP encapsulation"}
)

foreach ($category in $categories) {
    $categoryPath = Join-Path $LuaLibPath $category.name

    if (-not (Test-Path $categoryPath)) {
        Write-Host "警告: 分类目录不存在: $categoryPath" -ForegroundColor Yellow
        continue
    }

    $luaFiles = Get-ChildItem -Path $categoryPath -Filter "*.lua" -File | Where-Object { $_.Name -notmatch '\.bak$' }

    $metadata.categories[$category.name] = @{
        description = $category.description
        fileCount = $luaFiles.Count
        modules = @()
    }

    Write-Host "`n处理分类: $($category.name) ($($luaFiles.Count) 个文件)" -ForegroundColor Green

    foreach ($file in $luaFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor Gray

        try {
            $lines = Get-Content -Path $file.FullName -Encoding UTF8 -ErrorAction Stop

            # 根据分类处理文件
            if ($category.name -eq "enums") {
                # 解析枚举
                $enumList = Parse-Enum -Lines $lines -FilePath $file.Name -Category $category.name
                foreach ($enumData in $enumList) {
                    if ($enumData -and $enumData.values.Count -gt 0) {
                        $metadata.enums += $enumData
                    }
                }
            }
            elseif ($category.name -eq "object") {
                # 解析类
                $classData = Parse-Class -Lines $lines -FilePath $file.Name -Category $category.name
                if ($classData -and ($classData.methods.Count -gt 0 -or $classData.fields.Count -gt 0)) {
                    $metadata.classes += $classData
                }
            }
            else {
                # 解析模块
                $moduleData = Parse-Module -Lines $lines -FilePath $file.Name -Category $category.name
                if ($moduleData -and $moduleData.functions.Count -gt 0) {
                    $metadata.modules += $moduleData
                    $metadata.categories[$category.name].modules += $moduleData.name
                }
            }
        }
        catch {
            Write-Host "    错误: 解析失败 - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# 统计信息
$metadata.metadata.totalModules = $metadata.modules.Count
$totalFunctions = ($metadata.modules | ForEach-Object { $_.functions.Count } | Measure-Object -Sum).Sum
$totalMethods = ($metadata.classes | ForEach-Object { $_.methods.Count } | Measure-Object -Sum).Sum
$metadata.metadata.totalFunctions = $totalFunctions + $totalMethods

Write-Host "`n统计信息:" -ForegroundColor Cyan
Write-Host "  模块数量: $($metadata.modules.Count)" -ForegroundColor White
Write-Host "  函数数量: $totalFunctions" -ForegroundColor White
Write-Host "  枚举数量: $($metadata.enums.Count)" -ForegroundColor White
Write-Host "  类数量: $($metadata.classes.Count)" -ForegroundColor White
Write-Host "  类方法数量: $totalMethods" -ForegroundColor White

# 构建搜索索引
Write-Host "`n构建搜索索引..." -ForegroundColor Cyan
Build-SearchIndex -Metadata $metadata

Write-Host "  索引函数数量: $($metadata.searchIndex.functionsByName.Count)" -ForegroundColor White
Write-Host "  索引模块数量: $($metadata.searchIndex.modulesByName.Count)" -ForegroundColor White

# 保存JSON
Write-Host "`n保存JSON文件..." -ForegroundColor Cyan

# 确保输出目录存在
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

try {
    $json = $metadata | ConvertTo-Json -Depth 10 -Compress:$false
    $json | Out-File -FilePath $OutputPath -Encoding UTF8 -Force

    $fileInfo = Get-Item $OutputPath
    Write-Host "`n成功! 元数据文件已生成:" -ForegroundColor Green
    Write-Host "  路径: $OutputPath" -ForegroundColor White
    Write-Host "  大小: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor White
}
catch {
    Write-Host "`n错误: 保存JSON文件失败 - $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n完成!" -ForegroundColor Green
