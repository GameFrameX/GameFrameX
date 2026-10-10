/**
 * 启动链 HTTP 适配。
 *
 * 对照 Unity WebComponent.PostToString + HttpJsonResult{Code,Data}:
 * 默认实现基于宿主 fetch(浏览器/node 通用),返回 Code<=0 视为成功(与 Unity 一致)。
 */
export interface StartupHttpClient {
    postJson(url: string, body: Record<string, string>): Promise<{ Code: number; Data: unknown }>;
}

/** fetch 实现(引擎无关)。 */
export class FetchStartupHttpClient implements StartupHttpClient {
    public async postJson(url: string, body: Record<string, string>): Promise<{ Code: number; Data: unknown }> {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body),
        });
        if (!response.ok) {
            throw new Error(`HTTP ${response.status} for ${url}`);
        }

        return await response.json() as { Code: number; Data: unknown };
    }
}
