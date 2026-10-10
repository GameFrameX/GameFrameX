import type IMessage from "./IMessage";
import ProtoMessageHelper from "./ProtoMessageHelper";
import ProtoMessageRegister from "../protobuf/ProtoMessageRegister";

/** protobufjs UMD 全局对象的最小结构(宿主以 script 标签或 require 注入 globalThis.protobuf)。 */
interface ProtobufGlobal {
    load(paths: string[], callback: (err: Error | null, root: unknown) => void): void;
}

/** protobufjs Root 的最小结构(结构化匹配 lookupType)。 */
export type ProtobufRoot = { lookupType: (name: string) => IMessage };

/**
 * 加载 .proto 列表并灌注消息注册表(ProtoMessageHelper.adopt)。
 *
 * 浏览器:全局 protobuf(assets/protobuf/protobuf.js 以 script 引入),路径相对页面 fetch;
 * node:require('./assets/protobuf/protobuf.js') 挂到 globalThis 后路径按本地文件读取。
 * @param paths .proto 路径列表,默认取生成清单
 */
export async function loadProtoRoot(paths: string[] = ProtoMessageRegister.getProtoBuffList()): Promise<void> {
    const protobuf = (globalThis as unknown as Record<string, ProtobufGlobal | undefined>).protobuf;
    if (protobuf == null) {
        throw new Error('globalThis.protobuf not found (include assets/protobuf/protobuf.js first)');
    }

    await new Promise<void>((resolve, reject) => {
        protobuf.load(paths, (err, root) => {
            if (err != null) {
                reject(err);
                return;
            }

            ProtoMessageHelper.adopt(root as ProtobufRoot);
            resolve();
        });
    });
}
