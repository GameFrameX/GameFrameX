
export default class UtilityIdGenerator {

    private static _id: number = Math.floor((new Date().getTime() - new Date(2000, 1, 1, 0, 0, 0).getTime()) / 1000_0000);

    public static getUniqueIntId(): number {
        if (this._id > (2 ^ 32 - 1)) {
            this._id = 1;
        }

        return ++this._id;
    }
}
