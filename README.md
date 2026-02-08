# RusLatinizeHelper

一个使用 **.NET 10 / C#** 编写的命令行工具，用于将 **Lyricify Syllable** 格式歌词中的俄语音节转写为拉丁字母。

## 特性

- 支持 Lyricify Syllable 行（如 `[0]Пе(15956,123)...`）逐字符转写。
- 忽略常见源信息标签行（如 `[by:...]`、`[id:...]`、`[ti:...]`）。
- 支持 33 个俄文字母（大小写）映射的用户自定义。
- 保留空格、连字符、括号、时间戳与其它非俄文字符不变。

## 运行方式

```bash
dotnet run -- <input.lrc> <output.lrc> [--map <map.json>] [--write-default-map <path>]
```

### 参数

- `<input.lrc>`：输入歌词文件。
- `<output.lrc>`：输出歌词文件。
- `--map <map.json>`：指定自定义映射 JSON。
- `--write-default-map <path>`：导出默认映射 JSON，方便自行修改。

## 映射文件格式

映射文件是一个 JSON 对象，**key 必须是单个字符**，value 为替换后的拉丁字符串。示例：

```json
{
  "А": "A",
  "Б": "B",
  "В": "V",
  "Ё": "Yo",
  "Ж": "Zh",
  "Щ": "Shch",
  "Ъ": "",
  "Ь": "",
  "Я": "Ya",
  "а": "a",
  "б": "b",
  "в": "v",
  "ё": "yo",
  "ж": "zh",
  "щ": "shch",
  "ъ": "",
  "ь": "",
  "я": "ya"
}
```

## 示例

```bash
# 1) 导出默认映射
dotnet run -- --write-default-map map.json

# 2) 使用自定义映射转写
dotnet run -- input.lrc output.lrc --map map.json
```
