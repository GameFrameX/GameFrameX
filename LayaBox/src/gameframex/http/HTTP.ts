import Singleton from "../basic/Singleton";

export default class HTTP extends Singleton<HTTP>() {
    private m_timeout = 10000;
    post_async(host: string, data?: any, header_parmas?: any): Promise<any> {
        return new Promise((resolve, reject) => {
            this.post(host, data, header_parmas, Laya.Handler.create(this, (data: any) => {
                resolve(data);
            }), Laya.Handler.create(this, (data: any) => {
                reject(data);
            }))
        });
    }
    /**
     * 发送post请求
     * @param host 请求地址
     * @param data 请求数据
     * @param header_parmas 头部信息
     * @param successCallback 请求成功回调
     * @param failedCallback 请求失败回调
     */
    post(host: string, data: any, header_parmas: any, successCallback: Laya.Handler, failedCallback?: Laya.Handler): void {
        this.request(host, data, "post", header_parmas, successCallback, failedCallback);
    }


    get_async(host: string, data?: any, header_parmas?: any): Promise<any> {
        return new Promise((resolve, reject) => {
            this.get(host, data, header_parmas, Laya.Handler.create(this, (data: any) => {
                resolve(data);
            }), Laya.Handler.create(this, (data: any) => {
                reject(data);
            }))
        });
    }

    /**
     * 发送get请求
     * @param host 请求地址
     * @param data 请求数据
     * @param header_parmas 头部信息
     * @param successCallback 请求成功回调
     * @param failedCallback 请求失败回调
     */
    get(host: string, data: any, header_parmas: any, successCallback: Laya.Handler, failedCallback?: Laya.Handler): void {
        this.request(host, data, "get", header_parmas, successCallback, failedCallback);
    }

    private request(host: string, data: any, method: any, header_parmas: any, successCallback: Laya.Handler, failedCallback?: Laya.Handler): void {
        let xhr: Laya.HttpRequest = new Laya.HttpRequest();
        xhr.http.timeout = this.m_timeout;
        xhr.once(Laya.Event.COMPLETE, null, (data: any) => {
            successCallback.runWith(data);
        });

        xhr.once(Laya.Event.ERROR, null, (data: any) => {
            if (failedCallback) {
                failedCallback.runWith(data);
            }
        });
        let header = [];
        if (header_parmas) {
            if (header_parmas.authToken) {
                header.push("AuthToken");
                header.push(header_parmas.authToken);
            }
        }
        xhr.send(host, data, method, "json", header);
    }
}