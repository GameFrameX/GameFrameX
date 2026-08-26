export default class UtilityFairyGUI {
    
    /**
     * 获取节点
     * @param node
     * @param path
     * @returns
     */
    getChild(root: Laya.Node, path: string): Laya.Node {
        let arr = path.split("/");
        for (let i = 0; i < arr.length; i++) {
            let child = root.getChildByName(arr[i]);
            if (child == null) {
                return null;
            }
            root = child;
        }
        return root;
    }

    // private pathNode: string[] = [];
    // /**
    //  * 获取UI对象的全路径.如果中间有Group。名字则会为空。会忽略
    //  * @param uiObj UI对象
    //  * @returns 返回全路径
    //  */
    // public getUIFullPath(uiObj: fgui.GObject) {
    //     this.pathNode.length = 0;
    //     let current = uiObj;
    //     while (current.parent != null) {
    //         if (!this.isEmptyString(current.name) && current.name != UIMgr.ignoreName) {
    //             this.pathNode.push(current.name);
    //         }
    //         current = current.parent;
    //         if (current instanceof fgui.GRoot) {
    //             this.pathNode.push("GRoot");
    //         }
    //     }
    //     let path = this.pathNode.reverse().join(".");
    //     LogUtils.log(path);
    //     return path;
    // }

    // public getUIObjectPath(path: string): fgui.GComponent {
    //     let current: fgui.GComponent = fgui.GRoot.inst.asCom;
    //     let arr = path.split(".");
    //     for (let i = 0; i < arr.length; i++) {
    //         let currentName = arr[i];
    //         let currentCom = current.asCom;
    //         let child: fgui.GComponent = null
    //         for (let index = 0; index < currentCom.numChildren; index++) {
    //             const element = currentCom.getChildAt(index);
    //             if (element.name == UIMgr.ignoreName) {
    //                 child = element.asCom;
    //                 i--;
    //                 break;
    //             } else if (element.name == currentName) {
    //                 child = element.asCom;
    //                 break;
    //             }
    //         }

    //         if (child == null) {
    //             return null;
    //         }
    //         current = child;
    //     }

    //     return current;
    // }
}