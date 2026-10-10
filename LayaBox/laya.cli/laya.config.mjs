import path from 'node:path';
import os from 'node:os';

/**
 * layacli (npm: layacli@0.6.1) 配置——驱动 `laya dev`:
 * esbuild 编译 src/ → laya.cli/resources/js/bundles/bundle.js;
 * 自带 HTTP server (serverPort);用内嵌入口页加载 bundle + 引擎;
 * CDP (cdpPort) 打开 Chrome 用于热重载推送。
 */
const isMac = os.platform() === 'darwin';
const engineDir = isMac
    ? '/Applications/LayaAirIDE.app/Contents'
    : 'C:\\Program Files\\LayaAirIDE';

const config = {
    tsProjectSrcDir: './src',
    engineDir,
    engineVersion: '3.0.0',
    chromePath: undefined,
    enginePort: 18090,
    serverPort: 43990,
    cdpPort: 9222,
};

export default config;
