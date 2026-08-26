import HTTP from "./http/HTTP";

export default class GameApp {
    private static _http: HTTP;
    public static get http(): HTTP {
        if (!this._http) {
            this._http = HTTP.instance;
        }
        return this._http;
    }

}