export default interface IMessage {
    create(payload: any): any;
    verify(payload: any): string;
    encode(payload: any): any;
    decode(payload: any): any;
}