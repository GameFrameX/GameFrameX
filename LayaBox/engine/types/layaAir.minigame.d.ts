declare module laya.wx.mini {
  class MiniAdpter {
    /**@private  包装对象**/
    static EnvConfig: any;
    /**@private **/
    /**全局window对象**/
    static window: any;
    /**@private **/
    private static _preCreateElement;
    /**@private 适配库是否初始化**/
    private static _inited;
    /**@private 获取手机系统信息**/
    static systemInfo: any;
    /**@private  是否是子域，默认为false**/
    static isZiYu: boolean;
    /**@private 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false**/
    static isPosMsgYu: boolean;
    /**是否自动缓存下载的图片跟声音文件，默认为true**/
    static autoCacheFile: boolean;
    /**缓存容量满时每次清理容量值,默认每次清理5M**/
    static minClearSize: number;
    /** 最大缓存 */
    static sizeLimit: number;
    /**本地资源列表**/
    static nativefiles: any[];
    /** 本地zip资源列表 **/
    static nativezipfiles: any[];
    /**zip资源外网请求头*/
    static zipRequestHead: string;
    /**zip请求头和本地路径映射表*/
    static zipHeadFiles: any;
    /**本地分包资源表**/
    static subNativeFiles: any;
    /**本地分包文件目录数组**/
    static subNativeheads: any[];
    /**本地分包文件目录映射表**/
    static subMaps: any[];
    /**@private 是否自动缓存非图片声音文件(这里要确保文件编码最好一致)**/
    static AutoCacheDownFile: boolean;
    static preDownloadFiles: any;
    /**@private **/
    static getJson(data: string): any;
    /**激活微信小游戏适配器*/
    static enable(): void;
    /**
     * 初始化回调
     * @param isPosMsg 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false
     * @param isSon 是否是子域，默认为false
     */
    static init(isPosMsg?: boolean, isSon?: boolean): void;
    private static _onMessage;
    /**
     * 获取url对应的encoding值
     * @param zipurl 压缩文件路径
     * @param nativeurl 压缩文件路径白名单路径
     * @param callBack 回调地址，返回参数{errCode,errMsg,wxData} errCode：0 解压成功 1解压失败 2 解压接口不存在；errMsg：报错信息；wxData:微信返回的数据信息
     * @param proCallBack 返回进度回调方法
     * @param isUpdateType 0 目录存在返回成功回调；1，覆盖；2增量更新
     * @return
     */
    static loadZip(
      zipurl: string,
      nativeurl: string,
      callBack: Handler,
      proCallBack: Handler,
      isUpdateType?: number,
    ): void;
    /**
     * 下载zip包
     * @param zipurl 压缩文件路径
     * @param path 压缩文件路径
     * @param fs 文件系统实例
     * @param callBack 回调地址，返回参数{errCode,errMsg,wxData} errCode：0 解压成功 1解压失败 2 解压接口不存在；errMsg：报错信息；wxData:微信返回的数据信息
     * @param proCallBack 返回进度回调方法
     * @return
     */
    static downZip(
      zipurl: string,
      path: string,
      fs: any,
      callBack: Handler,
      proCallBack: Handler,
    ): void;
    /**
     * 获取url对应的encoding值
     * @param url 文件路径
     * @param type 文件类型
     * @return
     */
    static getUrlEncode(url: string, type: string): string;
    /**
     * 下载文件
     * @param fileUrl 文件地址(全路径)
     * @param fileType 文件类型(image、text、json、xml、arraybuffer、sound、atlas、font)
     * @param callBack 文件加载回调,回调内容[errorCode码(0成功,1失败,2加载进度)
     * @param encoding 文件编码默认utf8，非图片文件加载需要设置相应的编码，二进制编码为空字符串
     */
    static downLoadFile(
      fileUrl: string,
      fileType?: string,
      callBack?: Handler,
      encoding?: string,
    ): void;
    /**
     * 从本地删除文件
     * @param fileUrl 文件地址(全路径)
     * @param callBack 回调处理，在存储图片时用到
     */
    static remove(fileUrl: string, callBack?: Handler): void;
    /**
     * 清空缓存空间文件内容
     */
    static removeAll(): void;
    /**
     * 判断是否是4M包文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static hasNativeFile(fileUrl: string): boolean;
    /**
     * 判断缓存里是否存在文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static getFileInfo(fileUrl: string): any;
    /**
     * 获取缓存文件列表
     * @return
     */
    static getFileList(): any;
    /**@private 退出小游戏**/
    static exitMiniProgram(): void;
    /**@private **/
    private static onMkdirCallBack;
    /**@private 设备像素比。*/
    static pixelRatio(): number;
    /**
     * @private
     * 将字符串解析成 XML 对象。
     * @param value 需要解析的字符串。
     * @return js原生的XML对象。
     */
    private static parseXMLFromString;
    /**@private **/
    private static idx;
    /**@private **/
    static createElement(type: string): any;
    /**@private **/
    private static onCreateInput;
    /**@private **/
    static createShaderCondition(conditionScript: string): Function;
    /**
     * 传递图集url地址到
     * @param url 为绝对地址
     */
    static sendAtlasToOpenDataContext(url: string): void;
    private static postInfoToContext;
    /**
     * 发送单张图片到开放数据域
     * @param url
     */
    static sendSinglePicToOpenDataContext(url: string): void;
    /**
     * 传递json配置数据到开放数据域
     * @param url 为绝对地址
     */
    static sendJsonDataToDataContext(url: string): void;
  }
}

declare module laya.tt.mini {
  class TTMiniAdapter {
    static IGNORE: RegExp;
    static safeEncodeURI: (str: string) => string;
    /**@private  包装对象**/
    static EnvConfig: any;
    /**@private **/
    /**全局window对象**/
    static window: any;
    /**@private **/
    private static _preCreateElement;
    /**@private 适配库是否初始化**/
    private static _inited;
    /**@private 获取手机系统信息**/
    static systemInfo: any;
    /**@private  是否是子域，默认为false**/
    static isZiYu: boolean;
    /**@private 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false**/
    static isPosMsgYu: boolean;
    /**是否自动缓存下载的图片跟声音文件，默认为true**/
    static autoCacheFile: boolean;
    /**缓存容量满时每次清理容量值,默认每次清理5M**/
    static minClearSize: number;
    /** 最大缓存 */
    static sizeLimit: number;
    /**本地资源列表**/
    static nativefiles: any[];
    /**本地分包资源表**/
    static subNativeFiles: any;
    /**本地分包文件目录数组**/
    static subNativeheads: any[];
    /**本地分包文件目录映射表**/
    static subMaps: any[];
    /**@private 是否自动缓存非图片声音文件(这里要确保文件编码最好一致)**/
    static AutoCacheDownFile: boolean;
    /** 开放域 */
    static openCtx: any;
    /**@private **/
    static getJson(data: string): any;
    /**激活微信小游戏适配器*/
    static enable(): void;
    /**
     * 初始化回调
     * @param isPosMsg 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false
     * @param isSon 是否是子域，默认为false
     */
    static init(isPosMsg?: boolean, isSon?: boolean): void;
    private static _onMessage;
    /**
     * 获取url对应的encoding值
     * @param url 文件路径
     * @param type 文件类型
     * @return
     */
    static getUrlEncode(url: string, type: string): string;
    /**
     * 下载文件
     * @param fileUrl 文件地址(全路径)
     * @param fileType 文件类型(image、text、json、xml、arraybuffer、sound、atlas、font)
     * @param callBack 文件加载回调,回调内容[errorCode码(0成功,1失败,2加载进度)
     * @param encoding 文件编码默认utf8，非图片文件加载需要设置相应的编码，二进制编码为空字符串
     */
    static downLoadFile(
      fileUrl: string,
      fileType?: string,
      callBack?: Handler,
      encoding?: string,
    ): void;
    /**
     * 从本地删除文件
     * @param fileUrl 文件地址(全路径)
     * @param callBack 回调处理，在存储图片时用到
     */
    static remove(fileUrl: string, callBack?: Handler): void;
    /**
     * 清空缓存空间文件内容
     */
    static removeAll(): void;
    /**
     * 判断是否是4M包文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static hasNativeFile(fileUrl: string): boolean;
    /**
     * 判断缓存里是否存在文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static getFileInfo(fileUrl: string): any;
    /**
     * 获取缓存文件列表
     * @return
     */
    static getFileList(): any;
    /**@private 退出小游戏**/
    static exitMiniProgram(): void;
    /**@private **/
    private static onMkdirCallBack;
    /**@private 设备像素比。*/
    static pixelRatio(): number;
    /**
     * @private
     * 将字符串解析成 XML 对象。
     * @param value 需要解析的字符串。
     * @return js原生的XML对象。
     */
    private static parseXMLFromString;
    /**@private **/
    private static idx;
    /**@private **/
    static createElement(type: string): any;
    /**@private **/
    private static onCreateInput;
    /**@private **/
    static createShaderCondition(conditionScript: string): Function;
    /**
     * 传递图集url地址到
     * @param url 为绝对地址
     */
    static sendAtlasToOpenDataContext(url: string): void;
    private static postInfoToContext;
    /**
     * 发送单张图片到开放数据域
     * @param url
     */
    static sendSinglePicToOpenDataContext(url: string): void;
    /**
     * 传递json配置数据到开放数据域
     * @param url 为绝对地址
     */
    static sendJsonDataToDataContext(url: string): void;
  }
}

declare module laya.ali.mini {
  class ALIMiniAdapter {
    /**@private  包装对象**/
    static EnvConfig: any;
    /**@private **/
    /**全局window对象**/
    static window: any;
    /**@private **/
    private static _preCreateElement;
    /**@private 适配库是否初始化**/
    private static _inited;
    /**@private 获取手机系统信息**/
    static systemInfo: any;
    /**@private  是否是子域，默认为false**/
    static isZiYu: boolean;
    /**@private 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false**/
    static isPosMsgYu: boolean;
    /**是否自动缓存下载的图片跟声音文件，默认为true**/
    static autoCacheFile: boolean;
    /**缓存容量满时每次清理容量值,默认每次清理5M**/
    static minClearSize: number;
    /** 最大缓存 */
    static sizeLimit: number;
    /**本地资源列表**/
    static nativefiles: any[];
    /**本地分包资源表**/
    static subNativeFiles: any;
    /**本地分包文件目录数组**/
    static subNativeheads: any[];
    /**本地分包文件目录映射表**/
    static subMaps: any[];
    /**@private 是否自动缓存非图片声音文件(这里要确保文件编码最好一致)**/
    static AutoCacheDownFile: boolean;
    /**@private **/
    static getJson(data: string): any;
    /**激活微信小游戏适配器*/
    static enable(): void;
    /**
     * 初始化回调
     * @param isPosMsg 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false
     * @param isSon 是否是子域，默认为false
     */
    static init(isPosMsg?: boolean, isSon?: boolean): void;
    static aliPayCreateRender(): any;
    private static _onMessage;
    /**
     * 获取url对应的encoding值
     * @param url 文件路径
     * @param type 文件类型
     * @return
     */
    static getUrlEncode(url: string, type: string): string;
    /**
     * 下载文件
     * @param fileUrl 文件地址(全路径)
     * @param fileType 文件类型(image、text、json、xml、arraybuffer、sound、atlas、font)
     * @param callBack 文件加载回调,回调内容[errorCode码(0成功,1失败,2加载进度)
     * @param encoding 文件编码默认utf8，非图片文件加载需要设置相应的编码，二进制编码为空字符串
     */
    static downLoadFile(
      fileUrl: string,
      fileType?: string,
      callBack?: Handler,
      encoding?: string,
    ): void;
    /**
     * 从本地删除文件
     * @param fileUrl 文件地址(全路径)
     * @param callBack 回调处理，在存储图片时用到
     */
    static remove(fileUrl: string, callBack?: Handler): void;
    /**
     * 清空缓存空间文件内容
     */
    static removeAll(): void;
    /**
     * 判断是否是4M包文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static hasNativeFile(fileUrl: string): boolean;
    /**
     * 判断缓存里是否存在文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static getFileInfo(fileUrl: string): any;
    /**
     * 获取缓存文件列表
     * @return
     */
    static getFileList(): any;
    /**@private **/
    private static onMkdirCallBack;
    /**@private 设备像素比。*/
    static pixelRatio(): number;
    /**
     * @private
     * 将字符串解析成 XML 对象。
     * @param value 需要解析的字符串。
     * @return js原生的XML对象。
     */
    private static parseXMLFromString;
    /**@private **/
    private static idx;
    /**@private **/
    static createElement(type: string): any;
    /**@private **/
    private static onCreateInput;
    /**@private **/
    static createShaderCondition(conditionScript: string): Function;
    /**
     * 传递图集url地址到
     * @param url 为绝对地址
     */
    static sendAtlasToOpenDataContext(url: string): void;
    private static postInfoToContext;
    /**
     * 发送单张图片到开放数据域
     * @param url
     */
    static sendSinglePicToOpenDataContext(url: string): void;
    /**
     * 传递json配置数据到开放数据域
     * @param url 为绝对地址
     */
    static sendJsonDataToDataContext(url: string): void;
  }
}

declare module laya.mi.mini {
  class KGMiniAdapter {
    static IGNORE: RegExp;
    static safeEncodeURI: (str: string) => string;
    /**@private  包装对象**/
    static EnvConfig: any;
    /**@private **/
    /**全局window对象**/
    static window: any;
    /**@private **/
    private static _preCreateElement;
    /**@private 适配库是否初始化**/
    private static _inited;
    /**@private 获取手机系统信息**/
    static systemInfo: any;
    /**@private  是否是子域，默认为false**/
    static isZiYu: boolean;
    /**@private 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false**/
    static isPosMsgYu: boolean;
    /**是否自动缓存下载的图片跟声音文件，默认为true**/
    static autoCacheFile: boolean;
    /**50M缓存容量满时每次清理容量值,默认每次清理5M**/
    static minClearSize: number;
    /** 最大缓存 */
    static sizeLimit: number;
    /**本地资源列表**/
    static nativefiles: any[];
    /**本地分包资源表**/
    static subNativeFiles: any;
    /**本地分包文件目录数组**/
    static subNativeheads: any[];
    /**本地分包文件目录映射表**/
    static subMaps: any[];
    /**@private 是否自动缓存非图片声音文件(这里要确保文件编码最好一致)**/
    static AutoCacheDownFile: boolean;
    /**@private **/
    static getJson(data: string): any;
    /**激活微信小游戏适配器*/
    static enable(): void;
    /**
     * 初始化回调
     * @param isPosMsg 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false
     * @param isSon 是否是子域，默认为false
     */
    static init(isPosMsg?: boolean, isSon?: boolean): void;
    private static _onMessage;
    /**
     * 获取url对应的encoding值
     * @param url 文件路径
     * @param type 文件类型
     * @return
     */
    static getUrlEncode(url: string, type: string): string;
    /**
     * 下载文件
     * @param fileUrl 文件地址(全路径)
     * @param fileType 文件类型(image、text、json、xml、arraybuffer、sound、atlas、font)
     * @param callBack 文件加载回调,回调内容[errorCode码(0成功,1失败,2加载进度)
     * @param encoding 文件编码默认utf8，非图片文件加载需要设置相应的编码，二进制编码为空字符串
     */
    static downLoadFile(
      fileUrl: string,
      fileType?: string,
      callBack?: Handler,
      encoding?: string,
    ): void;
    /**
     * 从本地删除文件
     * @param fileUrl 文件地址(全路径)
     * @param callBack 回调处理，在存储图片时用到
     */
    static remove(fileUrl: string, callBack?: Handler): void;
    /**
     * 清空缓存空间文件内容
     */
    static removeAll(): void;
    /**
     * 判断是否是4M包文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static hasNativeFile(fileUrl: string): boolean;
    /**
     * 判断缓存里是否存在文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static getFileInfo(fileUrl: string): any;
    /**
     * 获取缓存文件列表
     * @return
     */
    static getFileList(): any;
    /**@private 退出小游戏**/
    static exitMiniProgram(): void;
    /**@private **/
    private static onMkdirCallBack;
    /**@private 设备像素比。*/
    static pixelRatio(): number;
    /**
     * @private
     * 将字符串解析成 XML 对象。
     * @param value 需要解析的字符串。
     * @return js原生的XML对象。
     */
    private static parseXMLFromString;
    /**@private **/
    private static idx;
    /**@private **/
    static createElement(type: string): any;
    /**@private **/
    private static onCreateInput;
    /**@private **/
    static createShaderCondition(conditionScript: string): Function;
    /**
     * 传递图集url地址到
     * @param url 为绝对地址
     */
    static sendAtlasToOpenDataContext(url: string): void;
    private static postInfoToContext;
    /**
     * 发送单张图片到开放数据域
     * @param url
     */
    static sendSinglePicToOpenDataContext(url: string): void;
    /**
     * 传递json配置数据到开放数据域
     * @param url 为绝对地址
     */
    static sendJsonDataToDataContext(url: string): void;
  }
}

declare module laya.vv.mini {
  class VVMiniAdapter {
    static IGNORE: RegExp;
    static safeEncodeURI: (str: string) => string;
    /**@private  包装对象**/
    static EnvConfig: any;
    /**@private **/
    /**全局window对象**/
    static window: any;
    /**@private **/
    private static _preCreateElement;
    /**@private 适配库是否初始化**/
    private static _inited;
    /**@private 获取手机系统信息**/
    static systemInfo: any;
    /**@private  是否是子域，默认为false**/
    static isZiYu: boolean;
    /**@private 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false**/
    static isPosMsgYu: boolean;
    /**是否自动缓存下载的图片跟声音文件，默认为true**/
    static autoCacheFile: boolean;
    /**50M缓存容量满时每次清理容量值,默认每次清理5M**/
    static minClearSize: number;
    /** 最大缓存 */
    static sizeLimit: number;
    /**本地资源列表**/
    static nativefiles: any[];
    /**本地分包资源表**/
    static subNativeFiles: any;
    /**本地分包文件目录数组**/
    static subNativeheads: any[];
    /**本地分包文件目录映射表**/
    static subMaps: any[];
    /**@private 是否自动缓存非图片声音文件(这里要确保文件编码最好一致)**/
    static AutoCacheDownFile: boolean;
    /**@private **/
    static getJson(data: string): any;
    /**激活微信小游戏适配器*/
    static enable(): void;
    /**
     * 初始化回调
     * @param isPosMsg 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false
     * @param isSon 是否是子域，默认为false
     */
    static init(isPosMsg?: boolean, isSon?: boolean): void;
    private static _onMessage;
    /**
     * 获取url对应的encoding值
     * @param url 文件路径
     * @param type 文件类型
     * @return
     */
    static getUrlEncode(url: string, type: string): string;
    /**
     * 下载文件
     * @param fileUrl 文件地址(全路径)
     * @param fileType 文件类型(image、text、json、xml、arraybuffer、sound、atlas、font)
     * @param callBack 文件加载回调,回调内容[errorCode码(0成功,1失败,2加载进度)
     * @param encoding 文件编码默认utf8，非图片文件加载需要设置相应的编码，二进制编码为空字符串
     */
    static downLoadFile(
      fileUrl: string,
      fileType?: string,
      callBack?: Handler,
      encoding?: string,
    ): void;
    /**
     * 从本地删除文件
     * @param fileUrl 文件地址(全路径)
     * @param callBack 回调处理，在存储图片时用到
     */
    static remove(fileUrl: string, callBack?: Handler): void;
    /**
     * 清空缓存空间文件内容
     */
    static removeAll(): void;
    /**
     * 判断是否是4M包文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static hasNativeFile(fileUrl: string): boolean;
    /**
     * 判断缓存里是否存在文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static getFileInfo(fileUrl: string): any;
    /**
     * 获取缓存文件列表
     * @return
     */
    static getFileList(): any;
    /**@private 退出小游戏**/
    static exitMiniProgram(): void;
    /**@private **/
    private static onMkdirCallBack;
    /**@private 设备像素比。*/
    static pixelRatio(): number;
    /**
     * @private
     * 将字符串解析成 XML 对象。
     * @param value 需要解析的字符串。
     * @return js原生的XML对象。
     */
    private static parseXMLFromString;
    /**@private **/
    private static idx;
    /**@private **/
    static createElement(type: string): any;
    /**@private **/
    private static onCreateInput;
    /**@private **/
    static createShaderCondition(conditionScript: string): Function;
    /**
     * 传递图集url地址到
     * @param url 为绝对地址
     */
    static sendAtlasToOpenDataContext(url: string): void;
    private static postInfoToContext;
    /**
     * 发送单张图片到开放数据域
     * @param url
     */
    static sendSinglePicToOpenDataContext(url: string): void;
    /**
     * 传递json配置数据到开放数据域
     * @param url 为绝对地址
     */
    static sendJsonDataToDataContext(url: string): void;
  }
}

declare module laya.qg.mini {
  class QGMiniAdapter {
    static IGNORE: RegExp;
    static safeEncodeURI: (str: string) => string;
    /**@private  包装对象**/
    static EnvConfig: any;
    /**@private **/
    /**全局window对象**/
    static window: any;
    /**@private **/
    private static _preCreateElement;
    /**@private 适配库是否初始化**/
    private static _inited;
    /**@private 获取手机系统信息**/
    static systemInfo: any;
    /**@private  是否是子域，默认为false**/
    static isZiYu: boolean;
    /**@private 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false**/
    static isPosMsgYu: boolean;
    /**是否自动缓存下载的图片跟声音文件，默认为true**/
    static autoCacheFile: boolean;
    /**50M缓存容量满时每次清理容量值,默认每次清理5M**/
    static minClearSize: number;
    /** 最大缓存 */
    static sizeLimit: number;
    /**本地资源列表**/
    static nativefiles: any[];
    /**本地分包资源表**/
    static subNativeFiles: any;
    /**本地分包文件目录数组**/
    static subNativeheads: any[];
    /**本地分包文件目录映射表**/
    static subMaps: any[];
    /**@private 是否自动缓存非图片声音文件(这里要确保文件编码最好一致)**/
    static AutoCacheDownFile: boolean;
    /**@private **/
    static getJson(data: string): any;
    /**激活微信小游戏适配器*/
    static enable(): void;
    /**
     * 初始化回调
     * @param isPosMsg 是否需要在主域中自动将加载的文本数据自动传递到子域，默认 false
     * @param isSon 是否是子域，默认为false
     */
    static init(isPosMsg?: boolean, isSon?: boolean): void;
    private static _onMessage;
    /**
     * 获取url对应的encoding值
     * @param url 文件路径
     * @param type 文件类型
     * @return
     */
    static getUrlEncode(url: string, type: string): string;
    /**
     * 下载文件
     * @param fileUrl 文件地址(全路径)
     * @param fileType 文件类型(image、text、json、xml、arraybuffer、sound、atlas、font)
     * @param callBack 文件加载回调,回调内容[errorCode码(0成功,1失败,2加载进度)
     * @param encoding 文件编码默认utf8，非图片文件加载需要设置相应的编码，二进制编码为空字符串
     */
    static downLoadFile(
      fileUrl: string,
      fileType?: string,
      callBack?: Handler,
      encoding?: string,
    ): void;
    /**
     * 从本地删除文件
     * @param fileUrl 文件地址(全路径)
     * @param callBack 回调处理，在存储图片时用到
     */
    static remove(fileUrl: string, callBack?: Handler): void;
    /**
     * 清空缓存空间文件内容
     */
    static removeAll(): void;
    /**
     * 判断是否是4M包文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static hasNativeFile(fileUrl: string): boolean;
    /**
     * 判断缓存里是否存在文件
     * @param fileUrl 文件地址(全路径)
     * @return
     */
    static getFileInfo(fileUrl: string): any;
    /**
     * 获取缓存文件列表
     * @return
     */
    static getFileList(): any;
    /**@private 退出小游戏**/
    static exitMiniProgram(): void;
    /**@private **/
    private static onMkdirCallBack;
    /**@private 设备像素比。*/
    static pixelRatio(): number;
    /**
     * @private
     * 将字符串解析成 XML 对象。
     * @param value 需要解析的字符串。
     * @return js原生的XML对象。
     */
    private static parseXMLFromString;
    /**@private **/
    private static idx;
    /**@private **/
    static createElement(type: string): any;
    /**@private **/
    private static onCreateInput;
    /**@private **/
    static createShaderCondition(conditionScript: string): Function;
    /**
     * 传递图集url地址到
     * @param url 为绝对地址
     */
    static sendAtlasToOpenDataContext(url: string): void;
    private static postInfoToContext;
    /**
     * 发送单张图片到开放数据域
     * @param url
     */
    static sendSinglePicToOpenDataContext(url: string): void;
    /**
     * 传递json配置数据到开放数据域
     * @param url 为绝对地址
     */
    static sendJsonDataToDataContext(url: string): void;
  }
}

declare module Laya {
  class MiniAdpter extends laya.wx.mini.MiniAdpter {}
  class TTMiniAdapter extends laya.tt.mini.TTMiniAdapter {}
  class ALIMiniAdapter extends laya.ali.mini.ALIMiniAdapter {}
  class KGMiniAdapter extends laya.mi.mini.KGMiniAdapter {}
  class VVMiniAdapter extends laya.vv.mini.VVMiniAdapter {}
  class QGMiniAdapter extends laya.qg.mini.QGMiniAdapter {}
}
