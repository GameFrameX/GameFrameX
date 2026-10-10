export default class UtilityIdGenerator {

    private static _id: number = Math.floor((new Date().getTime() - new Date(2000, 1, 1, 0, 0, 0).getTime()) / 1000_0000);

    private static readonly MAX_ID: number = 2 ** 32 - 1;

    public static getUniqueIntId(): number {
        if (this._id >= this.MAX_ID) {
            this._id = 0;
        }

        return ++this._id;
    }
}
