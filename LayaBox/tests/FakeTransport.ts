import type ISocketTransport from "../src/gameframex/network/ISocketTransport";

/**
 * 测试用假传输层:记录写出的帧,允许测试注入服务端响应。
 */
export default class FakeTransport implements ISocketTransport {
    public sentFrames: ArrayBuffer[] = [];
    public connected = false;

    public onOpen: () => void = () => { };
    public onMessage: (data: ArrayBuffer) => void = () => { };
    public onClose: (reason?: string) => void = () => { };
    public onError: (reason?: string) => void = () => { };

    public connect(_url: string): void {
        this.connected = true;
        this.onOpen();
    }

    public send(buffer: ArrayBuffer): void {
        this.sentFrames.push(buffer);
    }

    public close(): void {
        this.connected = false;
        this.onClose('fake close');
    }

    /** 注入一帧服务端响应。 */
    public serverRespond(frame: ArrayBuffer): void {
        this.onMessage(frame);
    }
}
