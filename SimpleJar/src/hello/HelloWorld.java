package hello;

public class HelloWorld {
    private String name;

    public HelloWorld() {
        this.name = "World";
    }

    public HelloWorld(String name) {
        this.name = name;
    }

    public String Say(String value) {
        return this.name + " " + value;
    }

    @Override
    public String toString() {
        return "Hello " + name;
    }
}
