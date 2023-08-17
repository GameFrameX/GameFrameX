# Unity 中通过非 `WWW` 的方式读取 `StreamingAssets` 下的文件

>  `传入路径为相对目录`

> ReadBuffer  读取Byte[] 数组
```
 byte[] buffer = BlankReadAssets.Read(string path)
```
> IsFileExists   文件是否存在 
```
 bool isFileExists = BlankReadAssets.IsFileExists(string path)
```
