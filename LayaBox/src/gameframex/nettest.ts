import Network from "./network/Network";

export default class nettest {
    private static net: Network;
    public static S() {
        this.net.connect("ws://127.0.0.1", 21100);
    }
    public static D() {
        this.net.send("test", { a: 1, b: 2 });
    }
    public static F() {

    }
    public static A() {
        this.net = new Network();
    }
}